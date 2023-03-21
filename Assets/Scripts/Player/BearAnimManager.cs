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
    Collider2D hotdogCollider;
    Collider2D hamburgerCollider;
    Animator hotdogAnimator;
    Animator hamburgerAnimator;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        brain = GetComponent<BearControllerSM>();
        brain.OnStateEnter += OnStateChange;

        hotdogCollider = hotdogBear.GetComponent<Collider2D>();
        hamburgerCollider = hamburgerBear.GetComponent<Collider2D>();

        hotdogAnimator = hotdogBear.GetComponent<Animator>();
        hamburgerAnimator = hamburgerBear.GetComponent<Animator>();
    }

    void SwapMode()
    {
        SetMode(!inHotdog);
    }

    void SetMode(bool inHotdog)
    {
        this.inHotdog = inHotdog;

        hotdogBear.SetActive(inHotdog);
        hamburgerBear.SetActive(!inHotdog);
    }

    private void OnDestroy()
    {
        brain.OnStateEnter -= OnStateChange;
    }

    void OnStateChange(StateType newState)
    {

    }

    public static Collider2D GetActiveCollider()
    {
        return instance.inHotdog ? instance.hotdogCollider : instance.hamburgerCollider;
    }
}
