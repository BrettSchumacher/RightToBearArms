using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IState
{
    public State stateType;

    protected BearControllerSM brain;

    public IState(BearControllerSM brain)
    {
        this.brain = brain;
    }

    public virtual void OnStateEnter() { }
    public virtual void OnStateUpdate(float dt) { }
    public virtual void OnStateFixedUpdate(float fixedDt) { }
    public virtual void OnStateExit() { }

    public virtual bool StateTriggered() { return false; }
    public virtual bool CanTransition() { return true; }
}
