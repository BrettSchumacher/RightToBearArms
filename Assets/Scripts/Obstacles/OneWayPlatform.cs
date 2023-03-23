using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OneWayPlatform : MonoBehaviour
{
    public LayerMask playerMask;
    public int normalLayer;
    public int playerIgnoreLayer;

    BearControllerSM bear;
    BoxCollider2D platform;

    bool isEnabled = true;
    Vector2 boxPoint;
    Vector2 boxSize;
    float boxAngle;

    Vector2 prevBearPoint;

    // Start is called before the first frame update
    void Start()
    {
        bear = BearControllerSM.instance;
        platform = GetComponent<BoxCollider2D>();

        boxPoint = platform.bounds.center;
        boxSize = platform.bounds.size;
        boxAngle = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        bool wasEnabled = isEnabled;
        Vector2 bearPoint = bear.transform.position;

        isEnabled = (bearPoint.y - prevBearPoint.y) < 0.001f;

        prevBearPoint = bearPoint;

        if (!wasEnabled && isEnabled)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(playerMask);
            List<Collider2D> colliders = new List<Collider2D>();

            // don't reenable if player is inside the collider area
            Physics2D.OverlapBox(boxPoint, boxSize, boxAngle, filter, colliders);
            isEnabled = colliders.Count == 0;
        }

        platform.gameObject.layer = isEnabled ? normalLayer : playerIgnoreLayer;
    }
}
