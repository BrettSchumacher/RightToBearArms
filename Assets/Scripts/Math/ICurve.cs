using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICurve
{
    public float Update(float dt);
    public float GetValue();
}
