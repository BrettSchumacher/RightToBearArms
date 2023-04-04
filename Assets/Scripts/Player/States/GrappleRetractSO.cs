using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/States/GrappleRetract")]
public class GrappleRetractSO : IStateSO
{
    GrappleRetract grappleRetract;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new GrappleRetract(brain, transitions);
            grappleRetract = (GrappleRetract)instance;

            MovementDataSO data = brain.movementData;
            grappleRetract.grappleShotTimescale = data.grappleRetractTimescale;
            grappleRetract.fallSpeed = data.maxFallSpeed < 0f ? data.maxFallSpeed : -data.maxFallSpeed;
            grappleRetract.fallAccelTime = data.timeToMaxFallFromRest;
            grappleRetract.jumpSpeed = data.jumpSpeed;
            grappleRetract.jumpDecelTime = data.decelTime;
        }

        return instance;
    }
}

public class GrappleRetract : IState
{
    public float grappleShotTimescale;
    public float jumpSpeed;
    public float jumpDecelTime;
    public float fallSpeed;
    public float fallAccelTime;

    float jumpDecel;
    float fallAccel;

    bool retracted = false;
    float initialTimeScale;

    public GrappleRetract(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        Debug.Log("RETRACT");

        retracted = false;

        initialTimeScale = Time.timeScale;
        Time.timeScale = grappleShotTimescale;

        jumpDecel = -jumpSpeed / jumpDecelTime;
        fallAccel = fallSpeed / fallAccelTime;

        GrappleHookManager.RetractGrapplingHook();
        GrappleHookManager.OnGrappleRetracted += OnRetracted;
    }

    void OnRetracted()
    {
        retracted = true;
    }


    public override void OnStateExit(StateType nextState)
    {
        base.OnStateExit(nextState);

        Time.timeScale = initialTimeScale;
        brain.grappling = false;
        // GrappleHookManager.instance.ClearRope();

        GrappleHookManager.OnGrappleRetracted -= OnRetracted;
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        GrappleHookManager.instance.GrappleUpdate(dt);

        if (!brain.grounded)
        {
            UpdateVelocity(dt);
        }
    }

    void UpdateVelocity(float dt)
    {
        float vel = brain.GetVelocity().y;

        if (vel > 0f)
        {
            vel += dt * jumpDecel;
        }
        else if (vel > fallSpeed)
        {
            vel += dt * fallAccel;
        }

        if (vel <= fallSpeed)
        {
            vel = fallSpeed;
        }

        brain.SetYVelocity(vel);
    }

    public override bool CanTransition()
    {
        return true;
    }
}

