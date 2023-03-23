using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/GrappleSuccess")]
public class GrappleSuccessSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return brain.grappling;
    }
}
