using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BearControllerSM))]
public class BearAnimManager : MonoBehaviour
{
    public static BearAnimManager instance;

    public Animator animator;
    public List<StateType> flippedAnims;

    BearControllerSM brain;
    /*Collider2D hotdogCollider;
    Collider2D hamburgerCollider;
    Animator hotdogAnimator;
    Animator hamburgerAnimator;*/

    Collider2D bearCollider;

    StateType lastState = StateType.IDLE;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Duplicate bear anim manager found, deleting self");
            Destroy(gameObject);
            return;
        }

        instance = this;

        brain = GetComponent<BearControllerSM>();

        /*hotdogCollider = hotdogBear.GetComponent<Collider2D>();
        hamburgerCollider = hamburgerBear.GetComponent<Collider2D>();

        hotdogAnimator = hotdogBear.GetComponent<Animator>();
        hamburgerAnimator = hamburgerBear.GetComponent<Animator>();*/

        bearCollider = GetComponent<Collider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        brain.OnStateEnter += OnStateChange;
        brain.OnStateTypeUpdate += OnStateChange;
    }

    private void OnDestroy()
    {
        brain.OnStateEnter -= OnStateChange;
        brain.OnStateTypeUpdate -= OnStateChange;
    }

    /*
    void SwapMode()
    {
        SetMode(!inHotdog);
    }

    void SetMode(bool inHotdog)
    {
        bool wasInHotdog = this.inHotdog;
        this.inHotdog = inHotdog;

        hotdogBear.SetActive(inHotdog);
        hamburgerBear.SetActive(!inHotdog);
    }

    private void OnDestroy()
    {
        brain.OnStateEnter -= OnStateChange;
    }
    */

    string StateToParam(StateType state)
    {
        string param;

        switch (state)
        {
            case StateType.IDLE:
                param = "Idle";
                break;
            case StateType.RUN:
                param = "Walk";
                break;
            case StateType.JUMP:
                param = "Jump";
                break;
            case StateType.WALL_GRAB:
                param = "Wall Grab";
                break;
            case StateType.CLIMB:
                param = "Climb";
                break;
            case StateType.WALL_SLIDE:
                param = "Wall Slide";
                break;
            case StateType.WALL_JUMP:
                param = "Wall Jump";
                break;
            case StateType.GRAPPLE:
            case StateType.GRAPPLE_SHOOT:
            case StateType.GRAPPLE_RELEASE:
                param = "Grapple";
                break;
            case StateType.GRAPPLE_SWING:
                param = "Grapple Swing";
                break;
            case StateType.GRAPPLE_CLIMB:
                param = "Grapple Climb";
                break;
            case StateType.GRAPPLE_WALK:
                param = "Grapple Walk";
                break;
            default:
                param = "Fall";
                break;
        }

        return param;
    }

    void OnStateChange(StateType newState)
    {
        string lastParam = StateToParam(lastState);
        string newParam = StateToParam(newState);

        if (lastParam == newParam)
        {
            return;
        }

        animator.SetBool(lastParam, false);
        animator.SetBool(newParam, true);

        lastState = newState;

        if (flippedAnims.Contains(newState))
        {
            brain.invertSprite = true;
        }
        else
        {
            brain.invertSprite = false;
        }
    }

    public static Collider2D GetActiveCollider()
    {
        return instance.bearCollider;
    }

/*    public static void SetOrientation(bool inHotdog)
    {
        instance.SetMode(inHotdog);
    }*/
}
