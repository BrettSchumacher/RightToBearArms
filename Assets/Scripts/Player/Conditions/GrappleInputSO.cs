using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/GrappleInput")]
public class GrappleInputSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return brain.grappleInput && !GrappleHookManager.GrappleInUse();
    }
}
