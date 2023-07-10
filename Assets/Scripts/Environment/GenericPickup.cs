using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class GenericPickup : MonoBehaviour
{
    public UnityEvent OnPickup;
    public bool oneTime;

    Collider2D player;

    bool used = false;

    // Start is called before the first frame update
    void Start()
    {
        player = BearControllerSM.instance.GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (used) return;

        if (collision == player)
        {
            OnPickup?.Invoke();
            if (oneTime)
            {
                used = true;
            }
        }
    }
}
