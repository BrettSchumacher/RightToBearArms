using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/States/IdleState")]
public class IdleStateSO : IStateSO
{
    public float speedThreshold = 0.05f;
    public IdleState idle;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new IdleState(brain, transitions);
            idle = (IdleState)instance;

            idle.speedThreshold = speedThreshold;
            idle.stateType = stateType;
        }

        return instance;
    }
}

public class IdleState : IState
{
    public float speedThreshold = 0.05f;

    public IdleState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {
    }
}


