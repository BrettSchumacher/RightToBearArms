using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BearControllerSM))]
public class BearAnimManager : MonoBehaviour
{
    public static BearAnimManager instance;

    public GameObject hotdogBear;
    public GameObject hamburgerBear;

    BearControllerSM brain;
    /*Collider2D hotdogCollider;
    Collider2D hamburgerCollider;
    Animator hotdogAnimator;
    Animator hamburgerAnimator;*/

    Collider2D bearCollider;
    Animator animator;

    bool inHotdog = true;

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

    void OnStateChange(StateType newState)
    {

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
