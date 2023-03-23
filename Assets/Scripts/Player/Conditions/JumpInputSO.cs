using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/JumpInput")]
public class JumpInputSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return brain.jumpInput;
    }
}
