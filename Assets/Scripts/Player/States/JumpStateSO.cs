using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/States/JumpState")]
public class JumpStateSO : IStateSO
{
    JumpState jump;

    public bool decrementJumps = true;
    public bool variableHeight = true;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new JumpState(brain, transitions);
            jump = (JumpState)instance;

            MovementDataSO data = brain.movementData;

            jump.stateType = stateType;
            jump.jumpSpeed = data.jumpSpeed;
            jump.maxBoostLength = data.maxBoostLength;
            jump.jumpReleaseSpeed = data.jumpReleaseSpeed;
            jump.decelTime = data.decelTime;
            jump.strafeSpeed = data.strafeSpeed;
            jump.decrementJumps = decrementJumps;
            jump.variableHeight = variableHeight;

            ClearStates += ClearState;
        }

        return instance;
    }
}

public class JumpState : IState
{
    public float jumpSpeed;
    public float jumpReleaseSpeed;
    public float maxBoostLength;
    public float decelTime;
    public float strafeSpeed;
    public float cooldown;
    public bool decrementJumps;
    public bool variableHeight;

    float initialStrafeSpeed;
    float decel;

    bool holdingJump;
    float boostExpire;

    public JumpState(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {

    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        brain.UseJump(decrementJumps);
        brain.grounded = false;
      
        decel = -jumpSpeed / decelTime;
        initialStrafeSpeed = Mathf.Max(strafeSpeed, Mathf.Abs(brain.GetVelocity().x));
        holdingJump = brain.jumpHeld;
        boostExpire = Time.time + maxBoostLength;

        brain.SetYVelocity(jumpSpeed);
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        Vector2 inputs = brain.moveInput;
        Vector2 vel = brain.GetVelocity();

        bool wasHoldingJump = holdingJump;
        holdingJump = holdingJump && (brain.jumpHeld || !variableHeight);

        if (holdingJump && Time.time < boostExpire)
        {
            vel.y = jumpSpeed;
        }
        else if (!holdingJump && wasHoldingJump) // released jump early
        {
            vel.y = jumpReleaseSpeed;
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
}
