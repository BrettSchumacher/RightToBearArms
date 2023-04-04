using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    Collider2D player;

    private void Start()
    {
        player = BearControllerSM.instance.GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != player)
        {
            return;
        }

        GameManager.instance.KillPlayer();
    }
}
