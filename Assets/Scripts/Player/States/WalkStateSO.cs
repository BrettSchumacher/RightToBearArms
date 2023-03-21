using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/States/WalkState")]
public class WalkStateSO : IStateSO
{
    public float speedThreshold = 0.05f;
    public float inputThreshold = 0.1f;

    WalkState walk;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new WalkState(brain, transitions);
            walk = (WalkState)instance;

            walk.speedThreshold = speedThreshold;
            walk.inputThreshold = inputThreshold;
        }

        return instance;
    }
}

public class WalkState : IState
{
    public float speedThreshold = 0.05f;
    public float inputThreshold = 0.1f;

    public WalkState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {

    }
}
