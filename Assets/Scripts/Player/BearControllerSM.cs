using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class BearControllerSM : MonoBehaviour
{
    public List<IStateSO> startingStates;
    public float jumpBufferTime = 0.1f;
    public float coyoteTimeLength = 0.1f;
    public int startingNumJumps = 1;

    public UnityAction<State> OnStateEnter;

    List<IStateSO> availableStates;
    List<Transition> currentTransitions;
    IState currentState;
    Rigidbody2D rb;
    int numJumps;
    int numAvailableJumps;
    bool coyoteTime;

    [HideInInspector] public bool grounded = true;
    [HideInInspector] public bool rightWall = false;
    [HideInInspector] public bool leftWall = false;
    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public bool jumpInput;
    [HideInInspector] public bool climbHeld;
    [HideInInspector] public bool grappleInput;
    Coroutine resetJump;
    Coroutine resetCoyoteTime;
    public Vector2 moveInput { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        availableStates = new List<IStateSO>(startingStates);
        currentState = availableStates[0].GetStateInstance(this);
        currentState.OnStateEnter();
        currentTransitions = currentState.GetTransitions();

        OnStateEnter?.Invoke(currentState.stateType);

        rb = GetComponent<Rigidbody2D>();
        numJumps = startingNumJumps;
        numAvailableJumps = numJumps;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateContacts();

        currentState.OnStateUpdate(Time.deltaTime);

        if (currentState.CanTransition())
        {
            TryTransition();
        }
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

        foreach (ContactPoint2D contact in contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                grounded = true;
            }
            if (contact.normal.x > 0.5f)
            {
                leftWall = true;
            }
            if (contact.normal.x < -0.5f)
            {
                rightWall = true;
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
            numAvailableJumps = numJumps;
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
        currentState.OnStateExit();
        currentState = newState;
        currentState.OnStateEnter();
        OnStateEnter?.Invoke(currentState.stateType);
    }

    public Vector2 GetVelocity()
    {
        return rb.velocity;
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
        return numAvailableJumps > 0 && nearGround;
    }

    public void UseJump()
    {
        numAvailableJumps--;

        coyoteTime = false;
        grounded = false;
        if (resetCoyoteTime != null)
        {
            StopCoroutine(resetCoyoteTime);
        }
    }

    public void OnMove(InputValue val)
    {
        moveInput = val.Get<Vector2>();
    }

    public void OnJump(InputValue val)
    {
        bool jumpWasHeld = jumpHeld;
        jumpHeld = val.Get<bool>();

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

    public void OnClimb(InputValue val)
    {
        climbHeld = val.Get<bool>();
    }

    public void OnGrapple(InputValue val)
    {
        grappleInput = val.Get<bool>();
    }

    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(jumpBufferTime);

        resetJump = null;
        jumpInput = false;
    }
    
    IEnumerator StopCoyoteTime()
    {
        yield return new WaitForSeconds(coyoteTimeLength);

        coyoteTime = false;
        resetCoyoteTime = null;
    }
}
