using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/OnClimbable")]
public class OnClimbableSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        LayerMask mask = brain.movementData.climbableMask;
        bool met = false;

        if (brain.rightWallObj != null)
        {
            met |= (((1 << brain.rightWallObj.layer) & mask.value) != 0);
        }

        if (brain.leftWallObj != null)
        {
            met |= (((1 << brain.leftWallObj.layer) & mask.value) != 0);
        }

        return met;
    }
}
