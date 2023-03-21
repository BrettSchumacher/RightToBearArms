using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/States/WalkState")]
public class WalkStateSO : IStateSO
{
    public float walkSpeed = 2f;
    public float accel = 2f;
    public float inputDeadzone = 0.05f;
    public float turnBounciness = 1f;

    WalkState walk;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new WalkState(brain, transitions);
            walk = (WalkState)instance;

            walk.stateType = stateType;
            walk.walkSpeed = walkSpeed;
            walk.accel = accel;
            walk.inputDeadzone = inputDeadzone;
            walk.turnBounciness = turnBounciness;
        }

        return instance;
    }
}

public class WalkState : IState
{
    public float walkSpeed;
    public float accel;
    public float inputDeadzone;
    public float turnBounciness;

    public WalkState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {

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

        if (Mathf.Approximately(vel, goalVel))
        {
            vel = goalVel;
        }
        else
        {
            vel = (vel - goalVel) * Mathf.Exp(-accel * dt) + goalVel;
        }

        brain.SetXVelocity(vel);
    }
}
