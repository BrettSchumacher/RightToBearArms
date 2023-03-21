using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/Conditions/CanJump")]
public class CanJumpSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return brain.CanJump() && brain.jumpInput;
    }
}
