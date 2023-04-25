using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/States/IdleState")]
public class IdleStateSO : IStateSO
{
    public IdleState idle;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new IdleState(brain, transitions);
            idle = (IdleState)instance;

            MovementDataSO data = brain.movementData;

            idle.stateType = stateType;
            idle.timeTorestFromMaxWalk = data.timeToRestFromMaxWalk;
            idle.walkSpeed = data.walkSpeed;

            ClearStates += ClearState;
        }

        return instance;
    }
}

public class IdleState : IState
{
    public float timeTorestFromMaxWalk;
    public float walkSpeed;
    float decel;

    public IdleState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        decel = -walkSpeed / timeTorestFromMaxWalk;
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        float vel = brain.GetVelocity().x;

        if (Mathf.Approximately(vel, 0f))
        {
            vel = 0f;
        }
        else if (vel < 0f)
        {
            vel -= decel * dt;
            vel = vel > 0f ? 0f : vel;
        }
        else
        {
            vel += decel * dt;
            vel = vel < 0f ? 0f : vel;
        }

        brain.SetXVelocity(vel);
    }
}


