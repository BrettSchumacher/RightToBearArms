using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/GrappleStuck")]
public class GrappleStuckSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return brain.grappleSuccess;
    }
}
