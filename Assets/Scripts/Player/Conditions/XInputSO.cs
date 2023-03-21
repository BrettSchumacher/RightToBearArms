using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterStates/Conditions/XInput")]
public class XInputSO : IConditionSO
{
    public float deadzone = 0.05f;

    public override bool IsConditionMet()
    {
        return Mathf.Abs(brain.moveInput.x) > deadzone;
    }
}
