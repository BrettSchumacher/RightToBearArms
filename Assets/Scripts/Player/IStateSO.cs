using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public static UnityAction ClearStates;

    public StateType stateType;
    public bool resetJumps = false;

    public List<Transition> transitions;

    protected IState instance;

    public abstract IState GetStateInstance(BearControllerSM brain);

    public virtual void ClearState()
    {
        instance = null;
        ClearStates -= ClearState;
    }
}
