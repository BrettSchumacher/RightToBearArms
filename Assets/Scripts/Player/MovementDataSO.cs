using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/MovementData")]
public class MovementDataSO : ScriptableObject
{
    [Header("Walk Values")]
    public float walkSpeed = 8f;
    public float inputDeadzone = 0.05f;
    public float turnBounciness = 1f;
    public float timeToMaxWalkFromRest = 0.1f;
    public float timeToRestFromMaxWalk = 0.05f;

    [Header("Jumping Values")]
    public int startingAirJumps = 0;
    public float jumpSpeed = 7f;
    public float maxBoostLength = 0.17f;
    public float decelTime = 0.12f;
    public float strafeSpeed = 7f;
    public float timeToMaxStrafeFromRest = 0.12f;
    public float timeToRestFromMaxStrafe = 0.07f;

    [Header("Wall Jump Values")]
    public float wallJumpBoostLength = 0.1f;

    [Header("Falling Values")]
    public float maxFallSpeed = -15f;
    public float maxBoostedFallSpeed = -25f;
    public float timeToMaxFallFromRest = 0.3f;
    public float timeToBoostFromMaxFall = 0.05f;
    public float timeToMaxFallFromBoost = 0.05f;

    [Header("Wall Slide Values")]
    public float maxSlideSpeed = -6f;
    public float maxBoostedSlideSpeed = -10f;
    public float timeToMaxSlideFromRest = 0.5f;
    public float timeToBoostFromMaxSlide = 0.15f;
    public float timeToMaxSlideFromBoost = 0.1f;

    [Header("Climb Values")]
    public float climbSpeed = 4f;
    public float timeToMaxClimbFromRest = 0.05f;
    public float timeToRestFromMaxClimb = 0.05f;
    public float climbAroundBoost = 5f;

    [Header("Grapple Values")]
    public float grappleRange = 10f;
    public float grappleShotTimescale = 1f; // in case I want to do a slowdown
    public float grappleRetractTimescale = 1f;
    public float grappleShotDuration = 0.3f;
    public float grappleRetractDuration = 0.3f;
    public float grappleHeadLength = 0.5f;
    public float drag = 0.1f;
    public float xSwingInfluence = 0.15f;
    public float minSwingBoost = 0.15f;
    public float ropeWidth = 0.1f;
    public float maxAngularVel = 2f;
    public float ropePullSnapiness = 1f;
    public float grappleLengthRachetAmt = 0.1f;
    public float grappleUpGravity = 1f;
    public float grappleDownGravity = 2f;
    public float grappleManualRetractSpeed = 1f;
    public LayerMask grappleInteractMask;
    public LayerMask grappleBlockMask;
    public GameObject grappleHeadPrefab;
    public GameObject ropeSegmentPrefab;

    [Header("Misc")]
    public float spriteFlipThreshold = 0.02f;
}
