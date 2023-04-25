using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraSwapper : MonoBehaviour
{
    public static List<CameraSwapper> cameraStack = new List<CameraSwapper>();
    public static CameraSwapper activeZone;
    public static UnityAction<CameraSwapper> SetPriority;
    
    static Vector2 transitionDirection;

    CinemachineVirtualCamera vcam;
    Collider2D camBounds;
    Collider2D player;
    bool hasPriority = false;

    private void Awake()
    {
        SetPriority += SetCamPriority;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = BearControllerSM.instance.GetComponent<Collider2D>();
        vcam = GetComponent<CinemachineVirtualCamera>();
        camBounds = GetComponent<Collider2D>();

        if (player.IsTouching(camBounds))
        {
            OnTriggerEnter2D(player);
        }
    }

    private void OnDestroy()
    {
        if (cameraStack.Contains(this))
        {
            cameraStack.Remove(this);
        }

        SetPriority -= SetCamPriority;
    }

    void SetCamPriority(CameraSwapper swap)
    {
        bool hadPriority = hasPriority;
        hasPriority = swap == this;
        vcam.Priority = (hasPriority) ? 1 : 0;

        if (hasPriority && !hadPriority)
        {
            StartCoroutine(PauseForSec(GameManager.data.screenTransitionTime));
            if (GameManager.data.clearGrappleOnTransition)
            {
                GrappleHookManager.ResetRope();
            }

            if (transitionDirection.y > 0.5f)
            {
                print("UP");
                BearControllerSM.instance.UpTransition();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != player)
        {
            return;
        }

        Vector2 playerPos = player.transform.position;
        Vector2 surfacePoint = camBounds.ClosestPoint(playerPos);
        bool insideCam = camBounds.OverlapPoint(playerPos);

        transitionDirection = (surfacePoint - playerPos).normalized;
        if (insideCam)
        {
            transitionDirection *= -1f;
        }

        // player touching the zone
        SetPriority?.Invoke(this);
        if (!cameraStack.Contains(this))
        {
            cameraStack.Add(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other != player)
        {
            return;
        }

        if (cameraStack.Contains(this))
        {
            cameraStack.Remove(this);
        }

        if (hasPriority)
        {
            if (cameraStack.Count < 1)
            {
                Debug.LogWarning("no cam found!");
                return;
            }

            transitionDirection *= -1f;

            CameraSwapper newCam = cameraStack[0];
            SetPriority?.Invoke(newCam);
        }
    }

    IEnumerator PauseForSec(float seconds)
    {
        GameManager.FreezeGame();
        yield return new WaitForSecondsRealtime(seconds);
        GameManager.UnfreezeGame();
    }
}
