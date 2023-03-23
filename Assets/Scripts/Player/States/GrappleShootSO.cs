using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Character/States/GrappleShoot")]
public class GrappleShootSO : IStateSO
{
    GrappleShoot grappleShoot;

    public override IState GetStateInstance(BearControllerSM brain)
    {
        if (instance == null)
        {
            instance = new GrappleShoot(brain, transitions);
            grappleShoot = (GrappleShoot)instance;

            MovementDataSO data = brain.movementData;

            grappleShoot.stateType = stateType;
            grappleShoot.grappleRange = data.grappleRange;
            grappleShoot.grappleShotTimescale = data.grappleShotTimescale;
            grappleShoot.grappleShotDuration = data.grappleShotDuration;
            grappleShoot.grappleHeadLength = data.grappleHeadLength;
            grappleShoot.drag = data.drag;
            grappleShoot.ropeWidth = data.ropeWidth;
            grappleShoot.grappleHeadPrefab = data.grappleHeadPrefab;
            grappleShoot.ropeSegmentPrefab = data.ropeSegmentPrefab;
            grappleShoot.grappleInteractMask = data.grappleInteractMask;
            grappleShoot.grappleBlockMask = data.grappleBlockMask;
            grappleShoot.fallSpeed = data.maxFallSpeed < 0f ? data.maxFallSpeed : -data.maxFallSpeed;
            grappleShoot.fallAccelTime = data.timeToMaxFallFromRest;
            grappleShoot.jumpSpeed = data.jumpSpeed;
            grappleShoot.jumpDecelTime = data.decelTime;
        }

        return instance;
    }
}

public class GrappleShoot : IState
{
    public float grappleRange = 10f;
    public float grappleShotTimescale = 0.2f;
    public float grappleShotDuration = 0.3f;
    public float grappleHeadLength;
    public float drag = 0.1f;
    public float ropeWidth = 0.1f;
    public GameObject grappleHeadPrefab;
    public GameObject ropeSegmentPrefab;
    public LayerMask grappleInteractMask;
    public LayerMask grappleBlockMask;
    public float fallSpeed;
    public float fallAccelTime;
    public float jumpSpeed;
    public float jumpDecelTime;

    float fallAccel;
    float jumpDecel;

    float initialTimeScale;
    float endTimer;

    bool grappleHit;

    public GrappleShoot(BearControllerSM brain, List<Transition> transitions) : base(brain, transitions)
    {

    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        brain.UseGrapple();
        endTimer = Time.unscaledTime + grappleShotDuration;

        initialTimeScale = Time.timeScale;
        Time.timeScale = grappleShotTimescale;

        jumpDecel = -jumpSpeed / jumpDecelTime;
        fallAccel = fallSpeed / fallAccelTime;

        grappleHit = false;
        Vector2 goal = GetGrapplePoint(GrappleHookManager.instance.grappleOrigin.position);

        GrappleHookManager.DeployGrappleHook(goal);
        GrappleHookManager.OnGrappleFailure += OnFailure;
        GrappleHookManager.OnGrappleSuccess += OnSuccess;
    }

    Vector2 GetGrapplePoint(Vector2 start)
    {
        Vector2 dir;
        // handle the case of controller
        if (InputManager.GetCurrentControlScheme() == ControlScheme.CONTROLLER)
        {
            if (brain.moveInput.magnitude < 0.05f)
            {
                dir = Vector2.right;
            }
            else
            {
                dir = brain.moveInput.normalized;
            }

            RaycastHit2D hit = Physics2D.CircleCast(start, ropeWidth / 2f, dir, grappleRange, grappleInteractMask);
            if (hit)
            {
                return start + dir * hit.distance;
            }

            return start + dir * grappleRange;
        }

        // now time for the case of keyboard
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPos);

        dir = (worldPoint - start).normalized;

        if (Vector2.Distance(start, worldPoint) > grappleRange)
        {
            return start + dir * grappleRange;
        }
        return worldPoint;
    }

    void OnSuccess()
    {
        grappleHit = true;
        brain.grappling = true;
        endTimer = Time.unscaledTime - 0.1f;
    }

    void OnFailure()
    {
        grappleHit = false;
        endTimer = Time.unscaledTime - 0.1f;
    }

    public override void OnStateExit(StateType nextState)
    {
        base.OnStateExit(nextState);

        Time.timeScale = initialTimeScale;

        GrappleHookManager.OnGrappleFailure -= OnFailure;
        GrappleHookManager.OnGrappleSuccess -= OnSuccess;
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
        return grappleHit || Time.unscaledTime > endTimer;
    }
}

