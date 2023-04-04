using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public static RespawnPoint currentPoint;

    public Vector2 respawnPoint;
    public bool showGizmos;

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

        currentPoint = this;
    }

    private void OnDestroy()
    {
        if (currentPoint == this)
        {
            currentPoint = null;
        }
    }

    public static Vector2 GetSpawnPoint()
    {
        if (currentPoint == null)
        {
            return Vector2.zero;
        }

        return (Vector2)currentPoint.transform.position + currentPoint.respawnPoint;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
        {
            return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + (Vector3)respawnPoint, 0.25f);

    }
}
