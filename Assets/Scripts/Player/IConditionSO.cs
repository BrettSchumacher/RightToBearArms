using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IConditionSO : ScriptableObject
{
    protected BearControllerSM brain;

    public void Initialize(BearControllerSM brain)
    {
        this.brain = brain;
    }

    public abstract bool IsConditionMet();
}
