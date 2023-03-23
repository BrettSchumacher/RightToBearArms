using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/GrappleEngaged")]
public class GrappleEngagedSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return GrappleHookManager.GrappleInUse();
    }
}
