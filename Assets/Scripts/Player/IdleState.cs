using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState
{
    public float speedThreshold = 0.05f;

    public IdleState(BearControllerSM brain) : base(brain)
    {
    }

    public override bool StateTriggered()
    {
        return brain.IsGrounded() && brain.GetVelocity().magnitude < speedThreshold;
    }
}

public class IdleStateSO : IStateSO
{
    public float speedThreshold = 0.05f;
    public IdleState idle;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new IdleState(brain);
            idle = (IdleState)instance;

            idle.speedThreshold = speedThreshold;
            idle.stateType = stateType;
        }

        return instance;
    }
}
