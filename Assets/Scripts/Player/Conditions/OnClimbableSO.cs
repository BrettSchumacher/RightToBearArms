using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/OnClimbable")]
public class OnClimbableSO : IConditionSO
{
    public LayerMask climbableMask;

    public override bool IsConditionMet()
    {
        bool met = false;

        if (brain.rightWallObj != null)
        {
            met |= (((1 << brain.rightWallObj.layer) & climbableMask.value) != 0);
        }

        if (brain.leftWallObj != null)
        {
            met |= (((1 << brain.leftWallObj.layer) & climbableMask.value) != 0);
        }

        return met;
    }
}
