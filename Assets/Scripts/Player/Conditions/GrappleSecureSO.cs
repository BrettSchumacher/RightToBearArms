using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Conditions/GrappleSecure")]
public class GrappleSecureSO : IConditionSO
{
    public override bool IsConditionMet()
    {
        return GrappleHookManager.GrappleSecured();
    }
}
