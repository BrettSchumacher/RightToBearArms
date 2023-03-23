using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/GrappleHeld")]
public class GrappleHeldSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return brain.grappleHeld;
    }
}
