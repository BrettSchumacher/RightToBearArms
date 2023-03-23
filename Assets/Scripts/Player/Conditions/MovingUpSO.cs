using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/MovingUp")]
public class MovingUpSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return brain.GetVelocity().y > 0f;
    }
}
