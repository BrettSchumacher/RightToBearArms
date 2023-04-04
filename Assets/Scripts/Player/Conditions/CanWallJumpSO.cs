using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/CanWallJump")]
public class CanWallJumpSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        if (!brain.jumpInput)
        {
            return false;
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(brain.obstacleMask);

        List<Collider2D> colliders = new List<Collider2D>();

        brain.wallJumpCollider.OverlapCollider(filter, colliders);

        return colliders.Count > 0;
    }
}
