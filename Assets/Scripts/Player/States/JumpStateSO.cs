using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/States/JumpState")]
public class JumpStateSO : IStateSO
{
    JumpState jump;

    public float jumpSpeed = 5f;
    public float maxBoostLength = 1f;
    public float decelTime = 1f;
    public float strafeSpeed = 3f;
    public float cooldown = 0.02f;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new JumpState(brain, transitions);
            jump = (JumpState)instance;

            jump.stateType = stateType;
            jump.jumpSpeed = jumpSpeed;
            jump.maxBoostLength = maxBoostLength;
            jump.decelTime = decelTime;
            jump.strafeSpeed = strafeSpeed;
            jump.cooldown = cooldown;
        }

        return instance;
    }
}

public class JumpState : IState
{
    public float jumpSpeed;
    public float maxBoostLength;
    public float decelTime;
    public float strafeSpeed;
    public float cooldown;

    float initialStrafeSpeed;
    float decel;

    bool holdingJump;
    float boostExpire;
    float cooldownDone;

    public JumpState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {

    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        brain.UseJump();
        brain.grounded = false;

        decel = -jumpSpeed / decelTime;
        initialStrafeSpeed = Mathf.Max(strafeSpeed, Mathf.Abs(brain.GetVelocity().x));
        holdingJump = brain.jumpHeld;
        boostExpire = Time.time + maxBoostLength;
        cooldownDone = Time.time + cooldown;

        brain.SetYVelocity(jumpSpeed);
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        Vector2 inputs = brain.moveInput;
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

        vel.x = inputs.x * initialStrafeSpeed;

        if (Mathf.Abs(vel.x) < initialStrafeSpeed)
        {
            initialStrafeSpeed = Mathf.Max(initialStrafeSpeed, strafeSpeed);
        }

        brain.SetVelocity(vel);
    }

    public override bool CanTransition()
    {
        return Time.time > cooldownDone;
    }
}
