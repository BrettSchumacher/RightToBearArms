using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IStateSO : ScriptableObject
{
    public State stateType;

    protected IState instance;

    public abstract IState GetStateInstance(BearControllerSM brain);
}
