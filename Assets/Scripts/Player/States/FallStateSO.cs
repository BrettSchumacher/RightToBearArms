using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/States/FallState")]
public class FallStateSO : IStateSO
{
    public float maxFallSpeed = 1f;
    public float maxBoostedFallSpeed = 2f;
    public float strafeSpeed = 1f;
    public float timeToMaxFromRest = 1f;
    public float timeToBoostFromMax = 0.2f;
    public float timeToMaxFromBoost = 0.15f;

    public FallState fall;
    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new FallState(brain, transitions);
            fall = (FallState)instance;

            fall.stateType = stateType;
            fall.maxFallSpeed = maxFallSpeed < 0f ? maxFallSpeed : -maxFallSpeed;
            fall.maxBoostedFallSpeed = maxBoostedFallSpeed < 0f ? maxBoostedFallSpeed : -maxBoostedFallSpeed;
            fall.strafeSpeed = strafeSpeed;
            fall.timeToMaxFromRest = timeToMaxFromRest;
            fall.timeToBoostFromMax = timeToBoostFromMax;
            fall.timeToMaxFromBoost = timeToMaxFromBoost;
        }

        return instance;
    }
}

public class FallState : IState
{
    public float maxFallSpeed;
    public float maxBoostedFallSpeed;
    public float strafeSpeed;
    public float timeToMaxFromRest;
    public float timeToBoostFromMax;
    public float timeToMaxFromBoost;

    float normAccel;
    float boostAccel;
    float boostDecel;

    float initialStrafeSpeed;

    public FallState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        normAccel = maxFallSpeed / timeToMaxFromRest;
        boostAccel = (maxBoostedFallSpeed - maxFallSpeed) / timeToBoostFromMax;
        boostDecel = (maxFallSpeed - maxBoostedFallSpeed) / timeToMaxFromBoost;

        initialStrafeSpeed = Mathf.Max(strafeSpeed, Mathf.Abs(brain.GetVelocity().x));
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        Vector2 inputs = brain.moveInput;
        Vector2 vel = brain.GetVelocity();

        float boostFac = -Mathf.Min(inputs.y, 0f);
        float maxSpeed = Mathf.Lerp(maxFallSpeed, maxBoostedFallSpeed, boostFac);

        if (Mathf.Approximately(vel.y, maxSpeed))
        {
            vel.y = maxSpeed;
        }
        else if (vel.y > maxSpeed)
        {
            float accel = Mathf.Lerp(normAccel, boostAccel, boostFac);
            vel.y += accel * dt;
            vel.y = Mathf.Clamp(vel.y, maxSpeed, 0f);
        }
        else
        {
            vel.y += boostDecel * dt;
            vel.y = vel.y > maxSpeed ? maxSpeed : vel.y;
        }

        vel.x = inputs.x * initialStrafeSpeed;

        if (Mathf.Abs(vel.x) < initialStrafeSpeed)
        {
            initialStrafeSpeed = Mathf.Max(initialStrafeSpeed, strafeSpeed);
        }

        brain.SetVelocity(vel);
    }
}

