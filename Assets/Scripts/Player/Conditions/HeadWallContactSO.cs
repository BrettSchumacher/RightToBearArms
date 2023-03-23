using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/HeadWallContact")]
public class HeadWallContactSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(brain.obstacleMask);

        List<Collider2D> colliders = new List<Collider2D>();
        Vector2 min = brain.headCollider.bounds.min;
        Vector2 max = brain.headCollider.bounds.max;

        Physics2D.OverlapArea(min, max, filter, colliders);

        return colliders.Count > 0;
    }
}
