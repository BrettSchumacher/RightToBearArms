using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/Conditions/Grounded")]
public class GroundedSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return brain.grounded;
    }
}
