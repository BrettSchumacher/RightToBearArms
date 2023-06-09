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

        brain.headCollider.OverlapCollider(filter, colliders);

        return colliders.Count > 0;
    }
}
