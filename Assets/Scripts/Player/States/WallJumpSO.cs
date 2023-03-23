using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/States/WallJump")]
public class WallJumpSO : IStateSO
{
    WallJumpState jump;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new WallJumpState(brain, transitions);
            jump = (WallJumpState)instance;

            MovementDataSO data = brain.movementData;

            jump.stateType = stateType;
            jump.jumpSpeed = data.jumpSpeed;
            jump.maxBoostLength = data.wallJumpBoostLength;
            jump.decelTime = data.decelTime;
            jump.strafeSpeed = data.strafeSpeed;
        }

        return instance;
    }
}

public class WallJumpState : IState
{
    public float jumpSpeed;
    public float maxBoostLength;
    public float decelTime;
    public float strafeSpeed;

    float initialStrafeSpeed;
    float decel;

    bool holdingJump;
    float boostExpire;
    bool left;

    public WallJumpState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {

    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        brain.UseJump(false);
        brain.grounded = false;

        left = brain.rightWall;

        decel = -jumpSpeed / decelTime;
        initialStrafeSpeed = Mathf.Max(strafeSpeed, Mathf.Abs(brain.GetVelocity().x));
        holdingJump = brain.jumpHeld;
        boostExpire = Time.time + maxBoostLength;

        brain.SetYVelocity(jumpSpeed);
        brain.SetXVelocity(left ? -strafeSpeed : strafeSpeed);
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        Vector2 vel = brain.GetVelocity();

        holdingJump = holdingJump && brain.jumpHeld;

        if (holdingJump && Time.time < boostExpire)
        {
            vel.y = jumpSpeed;
        }
        else
        {
            vel.y += decel * dt;
        }

        vel.x = left ? -strafeSpeed : strafeSpeed;

        if (Mathf.Abs(vel.x) < initialStrafeSpeed)
        {
            initialStrafeSpeed = Mathf.Max(initialStrafeSpeed, strafeSpeed);
        }

        brain.SetVelocity(vel);
    }
}