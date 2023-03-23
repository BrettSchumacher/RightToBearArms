using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/States/GrappleSwing")]
public class GrappleSwingSO : IStateSO
{
    GrappleSwing grappleSwing;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new GrappleSwing(brain, transitions);
            grappleSwing = (GrappleSwing)instance;

            MovementDataSO data = brain.movementData;
            grappleSwing.drag = data.drag;
            grappleSwing.xSwingInfluence = data.xSwingInfluence;
            grappleSwing.minSwingBoost = data.minSwingBoost;
            grappleSwing.maxAngularVel = data.maxAngularVel;
            grappleSwing.walkSpeed = data.walkSpeed;
            grappleSwing.timeToWalkMax = data.timeToMaxWalkFromRest;
            grappleSwing.timeToWalkRest = data.timeToMaxWalkFromRest;
            grappleSwing.inputDeadzone = data.inputDeadzone;
            grappleSwing.turnBounciness = data.turnBounciness;
            grappleSwing.fallSpeed = data.maxFallSpeed;
            grappleSwing.jumpSpeed = data.jumpSpeed;
            grappleSwing.timeToMaxFall = data.timeToMaxFallFromRest;
            grappleSwing.timeToJumpRest = data.decelTime;
            grappleSwing.strafeSpeed = data.strafeSpeed;
            grappleSwing.timeToMaxStrafe = data.timeToMaxStrafeFromRest;
            grappleSwing.timeToRestStrafe = data.timeToRestFromMaxStrafe;
            grappleSwing.ropePullSnapiness = data.ropePullSnapiness;
            grappleSwing.grappleLengthRachetAmt = data.grappleLengthRachetAmt;
            grappleSwing.grappleUpGravity = data.grappleUpGravity;
            grappleSwing.grappleDownGravity = data.grappleDownGravity;
            grappleSwing.grappleRetractSpeed = data.grappleManualRetractSpeed;
        }

        return instance;
    }
}

public class GrappleSwing : IState
{
    public float drag = 0.1f;
    public float xSwingInfluence = 0.15f;
    public float minSwingBoost = 0.15f;
    public float maxAngularVel = 2f;
    public float walkSpeed;
    public float timeToWalkMax;
    public float timeToWalkRest;
    public float inputDeadzone;
    public float turnBounciness;
    public float fallSpeed;
    public float jumpSpeed;
    public float timeToMaxFall;
    public float timeToJumpRest;
    public float strafeSpeed;
    public float timeToMaxStrafe;
    public float timeToRestStrafe;
    public float ropePullSnapiness = 1f;
    public float grappleLengthRachetAmt = 0.1f;
    public float grappleUpGravity;
    public float grappleDownGravity;
    public float grappleRetractSpeed;

    float walkAccel;
    float walkDecel;
    float fallAccel;
    float jumpDecel;
    float strafeAccel;
    float strafeDecel;

    float ropeLength;
    Vector2 prevPosition;
    float prevTime;
    float rachetShorten;

    public GrappleSwing(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        prevPosition = brain.transform.position;
        prevTime = Time.time;

        walkAccel = walkSpeed / timeToWalkMax;
        walkDecel = walkSpeed / timeToWalkRest;
        fallAccel = Mathf.Abs(fallSpeed) / timeToMaxFall;
        jumpDecel = jumpSpeed / timeToJumpRest;
        strafeAccel = strafeSpeed / timeToMaxStrafe;
        strafeDecel = strafeSpeed / timeToRestStrafe;

        ropeLength = GrappleHookManager.GetRopeLength();
        rachetShorten = 0f;
        brain.swingJoint.enabled = true;
        brain.swingJoint.distance = GrappleHookManager.GetBearSegmentLength();
        brain.swingJoint.connectedAnchor = GrappleHookManager.GetBearRopePivot();
        brain.swingJoint.anchor = GrappleHookManager.instance.grappleOrigin.position - brain.transform.position;
        brain.GetComponent<Rigidbody2D>().gravityScale = grappleDownGravity;
    }

    public override void OnStateExit(StateType nextState)
    {
        base.OnStateExit(nextState);
        brain.swingJoint.enabled = false;
        brain.GetComponent<Rigidbody2D>().gravityScale = 0f;
    }

    public override void OnStateUpdate(float dt)
    {
        base.OnStateUpdate(dt);

        GrappleHookManager.instance.GrappleUpdate(dt);

        Vector2 pivot = GrappleHookManager.GetBearRopePivot();
        Vector2 origin = GrappleHookManager.instance.grappleOrigin.position;

        float actualRopeLength = GrappleHookManager.GetRopeLength();
        float prevRopeLength = ropeLength;
        ropeLength = Mathf.Min(ropeLength, grappleLengthRachetAmt * Mathf.Ceil(actualRopeLength / grappleLengthRachetAmt));
        if (ropeLength < prevRopeLength - 0.01f)
        {
            rachetShorten = 0f;
        }

        Debug.Log("Rope length: " + ropeLength);

        float segLength = GrappleHookManager.GetBearSegmentLength();
        float idealSegLength = ropeLength - (GrappleHookManager.GetRopeLength() - segLength) - rachetShorten;

        Debug.Log("Segment: " + segLength + ", ideal: " + idealSegLength);

        Vector2 rope = origin - pivot;
        Vector2 vel = brain.GetVelocity();

        if (brain.grounded)
        {
            vel.x = WalkUpdate(dt);
        }
        else
        {
            vel = AirUpdate(dt);
            vel /= 1f + drag * dt;
        }


        if (brain.moveInput.y > inputDeadzone)
        {
            float retractAmt = brain.moveInput.y * grappleRetractSpeed * dt;
            if (idealSegLength > retractAmt)
            {
                rachetShorten += retractAmt;
                idealSegLength -= retractAmt;
            }
        }
        else if (brain.moveInput.y < -inputDeadzone)
        {
            float releaseAmt = -brain.moveInput.y * grappleRetractSpeed * dt;
            if (releaseAmt > rachetShorten)
            {
                releaseAmt = rachetShorten;
            }
            rachetShorten -= releaseAmt;
            idealSegLength += releaseAmt;
        }

        brain.swingJoint.distance = idealSegLength;
        brain.swingJoint.connectedAnchor = pivot;
        brain.SetVelocity(vel);
    }

    Vector2 ClampVel(float radius, Vector2 vel)
    {
        float angularVel = vel.magnitude / radius;

        if (angularVel > maxAngularVel)
        {
            return vel.normalized * maxAngularVel * radius;
        }

        return vel;
    }

    float WalkUpdate(float dt)
    {
        float input = brain.moveInput.x;
        float vel = brain.GetVelocity().x;

        // clamp input with deadzone
        if (Mathf.Abs(input) < inputDeadzone)
        {
            input = 0f;
        }

        // allow snappy turns by checking if player reversed the input direction
        if ((input < 0f && vel > 0f) || (input > 0f && vel < 0f))
        {
            vel *= -turnBounciness;
        }

        float goalVel = input * walkSpeed;
        float diffSign = Mathf.Sign(goalVel - vel);

        if (Mathf.Approximately(vel, goalVel))
        {
            vel = goalVel;
        }
        else if (Mathf.Abs(vel) < Mathf.Abs(goalVel))
        {
            vel += Mathf.Sign(diffSign) * walkAccel * dt;
            vel = (Mathf.Sign(goalVel - vel) != diffSign) ? goalVel : vel;
        }
        else
        {
            vel += Mathf.Sign(diffSign) * walkDecel * dt;
            vel = (Mathf.Sign(goalVel - vel) != diffSign) ? goalVel : vel;
        }

        return vel;
    }

    bool inputUsed = false;
    bool inputInUse = false;
    bool inputRight = false;
    float builtInfluence = 0f;

    Vector2 AirUpdate(float dt)
    {
        Vector2 vel = brain.GetVelocity();
        Vector2 inputs = brain.moveInput;

        if (Mathf.Abs(inputs.x) < inputDeadzone)
        {
            inputUsed = false;
            inputInUse = false;
        }

        if (vel.x > 0) // swinging right
        {
            if (inputs.x > inputDeadzone) // holding right
            {
                if (inputUsed && inputRight && !inputInUse) // we already used this right input
                {
                    builtInfluence = 0f;
                    return vel;
                }
                else if (!inputRight) // we were holding a left input
                {
                    // now let's change to hold the new right input
                    inputUsed = true;
                    inputRight = true;
                    inputInUse = true;
                }
                else if (!inputInUse) //input hasn't been registered yet
                {
                    inputRight = true;
                    inputInUse = true;
                    inputUsed = true;
                }
            }
            else if (inputs.x < -inputDeadzone) // holding left
            {
                if (inputRight) // we had been hodling a right input
                {
                    // register the left input
                    inputUsed = false;
                    inputRight = false;
                    inputInUse = false;
                }
                else if (!inputRight && inputUsed) // already used this left input
                {
                    inputInUse = false;
                }
            }
        }
        else if (vel.x < 0) // swinging left
        {
            if (inputs.x < -inputDeadzone) // holding left
            {
                if (inputUsed && !inputRight && !inputInUse) // we already used this left input
                {
                    builtInfluence = 0f;
                    return vel;
                }
                else if (inputRight) // we were holding a right input
                {
                    // now let's change to hold the new left input
                    inputUsed = true;
                    inputRight = false;
                    inputInUse = true;
                }
                else if (!inputInUse) //input hasn't been registered yet
                {
                    inputRight = false;
                    inputInUse = true;
                    inputUsed = true;
                }
            }
            else if (inputs.x > inputDeadzone) // holding right
            {
                if (!inputRight) // we had been holding a left input
                {
                    // register the right input
                    inputUsed = false;
                    inputRight = true;
                    inputInUse = false;
                }
                else if (inputRight && inputUsed) // already used this right input
                {
                    inputInUse = false;
                }
            }
        }

        if (!inputInUse)
        {
            builtInfluence = 0f;
            return vel;
        }

        bool rightInput = inputs.x > 0f;
        if (Mathf.Abs(inputs.x) > inputDeadzone && ((rightInput && vel.x > 0.01f) || (!rightInput && vel.x < -0.01f)))
        {
            vel.x += Mathf.Max(minSwingBoost, Mathf.Abs(vel.x)) * inputs.x * xSwingInfluence * dt;
        }

        return vel;
    }
}
