using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class BearControllerSM : MonoBehaviour
{
    public List<IStateSO> startingStates;

    public UnityAction<State> OnStateEnter;

    List<IStateSO> availableStates;
    IState currentState;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        availableStates = new List<IStateSO>(startingStates);
        currentState = availableStates[0].GetStateInstance(this);
        currentState.OnStateEnter();
        OnStateEnter.Invoke(currentState.stateType);

        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
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

    void TryTransition()
    {
        foreach (IStateSO stateSO in availableStates)
        {
            IState state = stateSO.GetStateInstance(this);

            if (state != currentState && state.StateTriggered())
            {
                TransitionStates(state);
                return; // do we only want max of 1 state transition per update?
            }
        }
    }

    void TransitionStates(IState newState)
    {
        currentState.OnStateExit();
        currentState = newState;
        currentState.OnStateEnter();
        OnStateEnter.Invoke(currentState.stateType);
    }

    public bool IsGrounded()
    {
        return true;
    }

    public Vector2 GetVelocity()
    {
        return rb.velocity;
    }
}
