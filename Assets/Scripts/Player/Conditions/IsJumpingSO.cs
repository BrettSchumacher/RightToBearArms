using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/IsJumping")]
public class IsJumpingSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return !brain.grounded && !brain.leftWall && !brain.rightWall && brain.GetVelocity().y > 0.1f;
    }
}
