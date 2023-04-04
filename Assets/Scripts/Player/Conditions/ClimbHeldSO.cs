using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/ClimbHeld")]
public class ClimbHeldSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return true; // brain.climbHeld;
    }
}
