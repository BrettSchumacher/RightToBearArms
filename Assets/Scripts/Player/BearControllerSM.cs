using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BearAnimManager))]
public class BearControllerSM : MonoBehaviour
{
    public static BearControllerSM instance;

    public MovementDataSO movementData;
    public List<IStateSO> startingStates;
    public float jumpBufferTime = 0.1f;
    public float coyoteTimeLength = 0.1f;
    public float groundRaycastDist = 0.01f;
    public float wallRaycastDist = 0.05f;
    public LayerMask obstacleMask;
    public Collider2D headCollider;
    public Collider2D wallJumpCollider;
    public DistanceJoint2D swingJoint;
    public SpriteRenderer sprite;

    public UnityAction<StateType> OnStateEnter;

    List<IStateSO> availableStates;
    List<Transition> currentTransitions;
    IState currentState;
    Rigidbody2D rb;
    int numJumps;
    int numAvailableJumps;
    bool coyoteTime;
    bool spriteFlipped;
    Vector2 velocity;
    float startingXScale;

    [HideInInspector] public bool grounded = true;
    [HideInInspector] public bool rightWall = false;
    [HideInInspector] public bool leftWall = false;
    [HideInInspector] public bool ceiling = false;
    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public bool jumpInput;
    [HideInInspector] public bool climbHeld;
    [HideInInspector] public bool grappleHeld;
    [HideInInspector] public bool grappleInput;
    [HideInInspector] public bool grappling = false;
    [HideInInspector] public bool grappleSuccess = false;

    [HideInInspector] public GameObject rightWallObj;
    [HideInInspector] public GameObject leftWallObj;

    Coroutine resetJump;
    Coroutine resetGrapple;
    Coroutine resetCoyoteTime;
    public Vector2 moveInput { get; set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Duplicate BearController found! Deleteing self");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        availableStates = new List<IStateSO>(startingStates);
        currentState = availableStates[0].GetStateInstance(this);
        currentState.OnStateEnter();
        currentTransitions = currentState.GetTransitions();

        OnStateEnter?.Invoke(currentState.stateType);

        numJumps = movementData.startingAirJumps;
        numAvailableJumps = numJumps;
        startingXScale = sprite.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateContacts();

        GrappleHookManager.instance.GrappleUpdate(Time.deltaTime);
        currentState.OnStateUpdate(Time.deltaTime);

        if (currentState.CanTransition())
        {
            TryTransition();
        }

        rb.velocity = velocity;

        if (rb.velocity.x < -movementData.spriteFlipThreshold && !spriteFlipped)
        {
            spriteFlipped = true;
        }
        if (rb.velocity.x > movementData.spriteFlipThreshold && spriteFlipped)
        {
            spriteFlipped = false;
        }

        sprite.transform.localScale = new Vector3((spriteFlipped ? -1f : 1f) * startingXScale, sprite.transform.localScale.y, sprite.transform.localScale.z);
    }

    private void FixedUpdate()
    {
        currentState.OnStateFixedUpdate(Time.fixedDeltaTime);
    }

    void UpdateContacts()
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        rb.GetContacts(contacts);

        bool wasGrounded = grounded;

        grounded = false;
        leftWall = false;
        rightWall = false;
        ceiling = false;

        leftWallObj = null;
        rightWallObj = null;

        foreach (ContactPoint2D contact in contacts)
        {
            if (rb.velocity.y < 0.001f && contact.normal.y > 0.5f)
            {
                grounded = true;
            }
            if (rb.velocity.y > -0.001f && contact.normal.y < -0.5f)
            {
                ceiling = true;
            }
            if (rb.velocity.x < 0.001f && contact.normal.x > 0.5f)
            {
                leftWall = true;
                leftWallObj = contact.collider.gameObject;
            }
            if (rb.velocity.x > -0.001f && contact.normal.x < -0.5f)
            {
                rightWall = true;
                rightWallObj = contact.collider.gameObject;
            }
        }

        if (wasGrounded && !grounded) // left the ground
        {
            coyoteTime = true;
            if (resetCoyoteTime != null)
            {
                StopCoroutine(resetCoyoteTime);
            }
            resetCoyoteTime = StartCoroutine(StopCoyoteTime());
        }
        else if (!wasGrounded && grounded) // hit the ground
        {
            RefreshJumps();
        }
    }

    void TryTransition()
    {
        foreach (Transition transition in currentTransitions)
        {
            if (!availableStates.Contains(transition.targetState))
            {
                continue;
            }

            bool transitionTriggered = true;
            foreach (TransitionCondition cond in transition.conditions)
            {
                if (cond.inverted == cond.condition.IsConditionMet())
                {
                    transitionTriggered = false;
                    break;
                }
            }

            if (transitionTriggered)
            {
                TransitionStates(transition.targetState.GetStateInstance(this));
                break;
            }
        }
    }

    void TransitionStates(IState newState)
    {
        currentState.OnStateExit(newState.stateType);
        currentState = newState;
        currentState.OnStateEnter();
        currentTransitions = newState.GetTransitions();
        OnStateEnter?.Invoke(currentState.stateType);
    }

    public Vector2 GetVelocity()
    {
        return rb.velocity;
    }

    public void SetXVelocity(float xVel)
    {
        velocity.x = xVel;
    }

    public void SetYVelocity(float yVel)
    {
        velocity.y = yVel;
    }

    public void SetVelocity(Vector2 newVel)
    {
        velocity = newVel;
    }

    public void SetPosition(Vector2 pos)
    {
        rb.MovePosition(pos);
    }

    public void AddMovementState(IStateSO newState)
    {
        if (!availableStates.Contains(newState))
        {
            availableStates.Add(newState);
        }
    }

    public void SetNumJumps(int jumps)
    {
        numJumps = jumps;
        if (grounded)
        {
            numAvailableJumps = numJumps;
        }
    }

    public void RefreshJumps()
    {
        numAvailableJumps = numJumps;
    }

    public bool CanJump()
    {
        bool nearGround = grounded || coyoteTime;
        return nearGround || numAvailableJumps > 0;
    }

    public void UseJump(bool decrement = true)
    {
        if (decrement && !(grounded || coyoteTime))
        {
            numAvailableJumps--;
        }

        coyoteTime = false;
        grounded = false;
        jumpInput = false;
        if (resetCoyoteTime != null)
        {
            StopCoroutine(resetCoyoteTime);
            resetCoyoteTime = null;
        }
    }

    public void UseGrapple()
    {
        grappleInput = false;
        if (resetGrapple != null)
        {
            StopCoroutine(resetGrapple);
            resetGrapple = null;
        }
    }

    public void ResetController(Vector2 respawnPoint)
    {
        currentState.OnStateExit(availableStates[0].stateType);
        currentState = availableStates[0].GetStateInstance(this);
        currentState.OnStateEnter();

        rb.velocity = Vector2.zero;
        velocity = Vector2.zero;

        GrappleHookManager.instance.ClearRope();

        transform.position = respawnPoint;
    }

    public void OnMove(InputAction.CallbackContext obj)
    {
        moveInput = obj.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext obj)
    {
        bool jumpWasHeld = jumpHeld;
        jumpHeld = obj.performed;

        if (jumpHeld && !jumpWasHeld) // just pressed jump
        {
            jumpInput = true;
            if (resetJump != null)
            {
                StopCoroutine(resetJump);
            }
            resetJump = StartCoroutine(ResetJump());
        }
    }

    public void OnClimb(InputAction.CallbackContext obj)
    {
        climbHeld = obj.performed;
    }

    public void OnGrapple(InputAction.CallbackContext obj)
    {
        bool grappleWasHeld = grappleHeld;
        grappleHeld = obj.performed;

        if (grappleHeld && !grappleWasHeld) // just pressed grapple
        {
            grappleInput = true;
            if (resetGrapple != null)
            {
                StopCoroutine(resetGrapple);
            }
            resetGrapple = StartCoroutine(ResetGrapple());
        }
    }

    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(jumpBufferTime);

        resetJump = null;
        jumpInput = false;
    }

    IEnumerator ResetGrapple()
    {
        yield return new WaitForSeconds(jumpBufferTime);

        resetGrapple = null;
        grappleInput = false;
    }

    IEnumerator StopCoyoteTime()
    {
        yield return new WaitForSeconds(coyoteTimeLength);

        coyoteTime = false;
        resetCoyoteTime = null;
    }
}
