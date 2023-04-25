using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/States/WallSlide")]
public class WallSlideSO : IStateSO
{

    public WallSlide wallSlide;
    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new WallSlide(brain, transitions);
            wallSlide = (WallSlide)instance;

            MovementDataSO data = brain.movementData;

            wallSlide.stateType = stateType;
            wallSlide.maxFallSpeed = data.maxSlideSpeed < 0f ? data.maxSlideSpeed : -data.maxSlideSpeed;
            wallSlide.maxBoostedFallSpeed = data.maxBoostedSlideSpeed < 0f ? data.maxBoostedSlideSpeed : -data.maxBoostedSlideSpeed;
            wallSlide.timeToMaxFromRest = data.timeToMaxSlideFromRest;
            wallSlide.timeToBoostFromMax = data.timeToBoostFromMaxSlide;
            wallSlide.timeToMaxFromBoost = data.timeToMaxSlideFromBoost;

            ClearStates += ClearState;
        }

        return instance;
    }
}

public class WallSlide : IState
{
    public float maxFallSpeed;
    public float maxBoostedFallSpeed;
    public float timeToMaxFromRest;
    public float timeToBoostFromMax;
    public float timeToMaxFromBoost;

    float normAccel;
    float boostAccel;
    float boostDecel;

    public WallSlide(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        normAccel = maxFallSpeed / timeToMaxFromRest;
        boostAccel = (maxBoostedFallSpeed - maxFallSpeed) / timeToBoostFromMax;
        boostDecel = (maxFallSpeed - maxBoostedFallSpeed) / timeToMaxFromBoost;
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

        brain.SetVelocity(vel);
    }
}
