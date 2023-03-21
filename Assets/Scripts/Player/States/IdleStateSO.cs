using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/States/IdleState")]
public class IdleStateSO : IStateSO
{
    public float maxStopTime = 0.2f;
    public float walkSpeed = 2f;
    public IdleState idle;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new IdleState(brain, transitions);
            idle = (IdleState)instance;

            idle.stateType = stateType;
            idle.maxStopTime = maxStopTime;
            idle.walkSpeed = walkSpeed;
        }

        return instance;
    }
}

public class IdleState : IState
{
    public float maxStopTime;
    public float walkSpeed;
    float startVel;
    float startTime;
    float endTime;

    public IdleState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        startVel = brain.GetVelocity().x;
        startTime = Time.time;

        if (Mathf.Abs(startVel) < 0.001)
        {
            endTime = startTime - 0.1f;
        }
        else
        {
            endTime = startTime + maxStopTime * Mathf.Min(Mathf.Abs(startVel) / walkSpeed, 1f);
        }
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        float vel;

        if (Time.time > endTime)
        {
            vel = 0f;
        }
        else
        {
            float t = (Time.time - startTime) / (endTime - startTime);
            vel = (1f - t) * startVel;
        }

        brain.SetXVelocity(vel);
    }
}


