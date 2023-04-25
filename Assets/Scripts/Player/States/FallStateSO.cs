using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/States/FallState")]
public class FallStateSO : IStateSO
{
    public FallState fall;
    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new FallState(brain, transitions);
            fall = (FallState)instance;

            MovementDataSO data = brain.movementData;

            fall.stateType = stateType;
            fall.maxFallSpeed = data.maxFallSpeed < 0f ? data.maxFallSpeed : -data.maxFallSpeed;
            fall.maxBoostedFallSpeed = data.maxBoostedFallSpeed < 0f ? data.maxBoostedFallSpeed : -data.maxBoostedFallSpeed;
            fall.strafeSpeed = data.strafeSpeed;
            fall.timeToMaxFromRest = data.timeToMaxFallFromRest;
            fall.timeToBoostFromMax = data.timeToBoostFromMaxFall;
            fall.timeToMaxFromBoost = data.timeToMaxFallFromBoost;
            fall.turnBounciness = data.turnBounciness;
            fall.timeToMaxStrafeFromRest = data.timeToMaxStrafeFromRest;
            fall.timeToRestFromMaxStrafe = data.timeToRestFromMaxStrafe;

            ClearStates += ClearState;
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
    public float turnBounciness;
    public float timeToMaxStrafeFromRest;
    public float timeToRestFromMaxStrafe;

    float normAccel;
    float boostAccel;
    float boostDecel;

    float strafeAccel;
    float strafeDecel;

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

        strafeAccel = strafeSpeed / timeToMaxStrafeFromRest;
        strafeDecel = strafeSpeed / timeToRestFromMaxStrafe;

        initialStrafeSpeed = Mathf.Max(strafeSpeed, Mathf.Abs(brain.GetVelocity().x));

    }

    public override void OnStateExit(StateType nextState)
    {
        base.OnStateExit(nextState);

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

        float goalVel = inputs.x * initialStrafeSpeed;

        if (vel.x < -0.001f && goalVel > 0.001f || vel.x > 0.001f && goalVel < -0.001f)
        {
            vel.x *= -1f * turnBounciness;
        }

        float diffSign = Mathf.Sign(goalVel - vel.x);

        if (Mathf.Approximately(vel.x, goalVel))
        {
            vel.x = goalVel;
        }
        else if (Mathf.Abs(vel.x) < Mathf.Abs(goalVel))
        {
            vel.x += diffSign * strafeAccel * dt;
            vel.x = (Mathf.Sign(goalVel - vel.x) != diffSign) ? goalVel : vel.x;
        }
        else
        {
            vel.x += diffSign * strafeDecel * dt;
            vel.x = (Mathf.Sign(goalVel - vel.x) != diffSign) ? goalVel : vel.x;
        }

        if (Mathf.Abs(vel.x) < initialStrafeSpeed)
        {
            initialStrafeSpeed = Mathf.Max(initialStrafeSpeed, strafeSpeed);
        }

        brain.SetVelocity(vel);
    }
}

