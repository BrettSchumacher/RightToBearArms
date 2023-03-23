using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/States/WalkState")]
public class WalkStateSO : IStateSO
{
    WalkState walk;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new WalkState(brain, transitions);
            walk = (WalkState)instance;

            MovementDataSO data = brain.movementData;

            walk.stateType = stateType;
            walk.walkSpeed = data.walkSpeed;
            walk.timeToMaxFromRest = data.timeToMaxWalkFromRest;
            walk.timeToRestFromMax = data.timeToRestFromMaxWalk;
            walk.inputDeadzone = data.inputDeadzone;
            walk.turnBounciness = data.turnBounciness;
        }

        return instance;
    }
}

public class WalkState : IState
{
    public float walkSpeed;
    public float timeToMaxFromRest;
    public float timeToRestFromMax;
    public float inputDeadzone;
    public float turnBounciness;

    float accel;
    float decel;

    public WalkState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {

    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        accel = walkSpeed / timeToMaxFromRest;
        decel = walkSpeed / timeToRestFromMax;
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        // get variables from brain
        float input = brain.moveInput.x;
        float vel = brain.GetVelocity().x;

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

        float goalVel = input * walkSpeed;
        float diffSign = Mathf.Sign(goalVel - vel);

        if (Mathf.Approximately(vel, goalVel))
        {
            vel = goalVel;
        }
        else if (Mathf.Abs(vel) < Mathf.Abs(goalVel))
        {
            vel += Mathf.Sign(diffSign) * accel * dt;
            vel = (Mathf.Sign(goalVel - vel) != diffSign) ? goalVel : vel;
        }
        else
        {
            vel += Mathf.Sign(diffSign) * decel * dt;
            vel = (Mathf.Sign(goalVel - vel) != diffSign) ? goalVel : vel;
        }

        brain.SetXVelocity(vel);
    }
}
