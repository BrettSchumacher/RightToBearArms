using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/WalLContact")]
public class WallContactSO : IConditionSO
{
    public bool left;
    public bool right;

    public override bool IsConditionMet()
    {
        bool result = false;

        if (left)
        {
            result |= brain.leftWall;
        }

        if (right)
        {
            result |= brain.rightWall;
        }

        return result;
    }
}
