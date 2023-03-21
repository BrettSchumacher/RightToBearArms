using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/States/FallState")]
public class FallStateSO : IStateSO
{
    public FallState fall;
    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new FallState(brain, transitions);
            fall = (FallState)instance;

            fall.stateType = stateType;
        }

        return instance;
    }
}

public class FallState : IState
{
    public FallState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {
    }
}

