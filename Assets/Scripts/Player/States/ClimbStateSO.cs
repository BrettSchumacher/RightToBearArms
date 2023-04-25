using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/States/Climb")]
public class ClimbStateSO : IStateSO
{
    ClimbState climb;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new ClimbState(brain, transitions);
            climb = (ClimbState)instance;

            MovementDataSO data = brain.movementData;

            climb.stateType = stateType;
            climb.climbSpeed = data.climbSpeed;
            climb.timeToMaxFromRest = data.timeToMaxClimbFromRest;
            climb.timeToRestFromMax = data.timeToRestFromMaxClimb;
            climb.inputDeadzone = data.inputDeadzone;
            climb.turnBounciness = data.turnBounciness;
            climb.climbAroundBoost = data.climbAroundBoost;

            ClearStates += ClearState;
        }

        return instance;
    }
}

public class ClimbState : IState
{
    public float climbSpeed;
    public float timeToMaxFromRest;
    public float timeToRestFromMax;
    public float inputDeadzone;
    public float turnBounciness;
    public float climbAroundBoost;

    float accel;
    float decel;

    bool leftClimb;

    public ClimbState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {

    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        brain.SetXVelocity(0f);

        accel = climbSpeed / timeToMaxFromRest;
        decel = climbSpeed / timeToRestFromMax;

        leftClimb = brain.leftWall;
    }

    public override void OnStateExit(StateType nextState)
    {
        base.OnStateExit(nextState);

        if (nextState == StateType.FALL && !(brain.leftWall || brain.rightWall) && brain.GetVelocity().y > 0f)
        {
            brain.SetXVelocity(leftClimb ? -climbAroundBoost : climbAroundBoost);
        }
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        // get variables from brain
        float input = brain.moveInput.y;
        float vel = brain.GetVelocity().y;

        // clamp input with deadzone
        if (Mathf.Abs(input) < inputDeadzone)
        {
            input = 0f;
        }

        // allow snappy turns by checking if player reversed the input direction
        if ((input < 0f && vel > 0f) || (input > 0f && vel < 0f))
        {
            vel *= -turnBounciness;
        }

        float goalVel = input * climbSpeed;
        float diffSign = Mathf.Sign(goalVel - vel);

        if (Mathf.Approximately(vel, goalVel))
        {
            vel = goalVel;
        }
        else if (Mathf.Abs(vel) < Mathf.Abs(goalVel))
        {
            vel += diffSign * accel * dt;
            vel = (Mathf.Sign(goalVel - vel) != diffSign) ? goalVel : vel;
        }
        else
        {
            vel += diffSign * decel * dt;
            vel = (Mathf.Sign(goalVel - vel) != diffSign) ? goalVel : vel;
        }

        brain.SetYVelocity(vel);
    }
}

