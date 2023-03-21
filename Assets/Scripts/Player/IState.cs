using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IState
{
    public State stateType;

    protected List<Transition> transitions;
    protected BearControllerSM brain;

    public IState(BearControllerSM brain, List<Transition> transitions)
    {
        this.brain = brain;
        this.transitions = transitions;

        foreach (Transition transition in transitions)
        {
            foreach (TransitionCondition condition in transition.conditions)
            {
                condition.condition.Initialize(brain);
            }
        }
    }

    public virtual void OnStateEnter() { }
    public virtual void OnStateUpdate(float dt) { }
    public virtual void OnStateFixedUpdate(float fixedDt) { }
    public virtual void OnStateExit() { }

    public virtual bool CanTransition() { return true; }

    public List<Transition> GetTransitions() { return transitions; }
}
