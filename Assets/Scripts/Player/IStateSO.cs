using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TransitionCondition
{
    public IConditionSO condition;

    public bool inverted;
}

[System.Serializable]
public struct Transition
{
    public IStateSO targetState;

    public List<TransitionCondition> conditions;
}

public abstract class IStateSO : ScriptableObject
{
    public State stateType;

    public List<Transition> transitions;

    protected IState instance;

    public abstract IState GetStateInstance(BearControllerSM brain);
}
