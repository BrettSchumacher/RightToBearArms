using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/XInput")]
public class XInputSO : IConditionSO
{
    public float min;
    public float max;

    public override bool IsConditionMet()
    {
        return brain.moveInput.x >= min && brain.moveInput.x <= max;
    }
}
