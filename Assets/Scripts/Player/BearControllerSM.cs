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
    public IStateSO upTransitionState;
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
    public UnityAction<StateType> OnStateTypeUpdate;

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
    bool paused;

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
    [HideInInspector] public float grappleExtend = 0f;
    [HideInInspector] public float grappleShorten = 0f;
    [HideInInspector] public bool invertSprite = false;

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

        GameManager.OnGameFreeze -= OnGameFreeze;
        GameManager.OnGameUnfreeze -= OnGameUnfreeze;

        IStateSO.ClearStates?.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();

        availableStates = new List<IStateSO>(startingStates);
        currentState = availableStates[0].GetStateInstance(this);
        currentState.OnStateEnter();
        currentTransitions = currentState.GetTransitions();

        OnStateEnter?.Invoke(currentState.stateType);

        numJumps = movementData.startingAirJumps;
        numAvailableJumps = numJumps;
        startingXScale = sprite.transform.localScale.x;

        GameManager.OnGameFreeze += OnGameFreeze;
        GameManager.OnGameUnfreeze += OnGameUnfreeze;
    }

    // Update is called once per frame
    void Update()
    {
        if (paused)
        {
            return;
        }

        // Debug.Log("--------------------------------");
        int frame = Time.frameCount;

        // Debug.Log("Bear update start: " + frame);
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

        sprite.flipX = invertSprite;

        sprite.transform.localScale = new Vector3((spriteFlipped ? -1f : 1f) * startingXScale, sprite.transform.localScale.y, sprite.transform.localScale.z);

        // Debug.Log("Bear update end: " + frame);
    }

    private void FixedUpdate()
    {
        if (paused)
        {
            return;
        }

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
                TransitionStates(transition.targetState);
                break;
            }
        }
    }

    void TransitionStates(IStateSO newStateSO)
    {
        IState newState = newStateSO.GetStateInstance(this);

        currentState.OnStateExit(newState.stateType);
        currentState = newState;
        currentState.OnStateEnter();
        currentTransitions = newState.GetTransitions();
        OnStateEnter?.Invoke(currentState.stateType);

        if (newStateSO.resetJumps)
        {
            RefreshJumps();
        }
    }

    void OnGameFreeze()
    {
        paused = true;
    }

    void OnGameUnfreeze()
    {
        paused = false;
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

    public void SetRBVelocity(Vector2 newVel)
    {
        velocity = newVel;
        rb.velocity = newVel;
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

    public void AddMovementStates(List<IStateSO> states)
    {
        foreach (IStateSO state in states)
        {
            AddMovementState(state);
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

    public void UpTransition()
    {
        TransitionStates(upTransitionState);
    }

    public void ResetController(Vector2 respawnPoint)
    {
        TransitionStates(availableStates[0]);

        rb.velocity = Vector2.zero;
        velocity = Vector2.zero;

        GrappleHookManager.instance.ClearRope();

        transform.position = respawnPoint;
    }

    public void InvokeStateTypeUpdate(StateType newState)
    {
        OnStateTypeUpdate?.Invoke(newState);
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

    public void OnRopeShorten(InputAction.CallbackContext obj)
    {
        float val = obj.ReadValue<Vector2>().y;

        print("VAL: " + val);

        if (Mathf.Abs(val) < movementData.inputDeadzone) return;

        grappleShorten = 0f;
        grappleExtend = 0f;

        if (val > 0f)
        {
            grappleShorten = Time.time + movementData.grappleManualRetruactDuration;
        }
        else
        {
            grappleExtend = Time.time + movementData.grappleManualRetruactDuration;
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
