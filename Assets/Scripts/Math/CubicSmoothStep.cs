using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicSmoothStep : ICurve
{
    float value;
    float time;
    float start;
    float end;
    float duration;

    public CubicSmoothStep(float start, float end, float duration)
    {
        time = 0;
        value = start;

        this.start = start;
        this.end = end;
        this.duration = duration;
    }

    public float Update(float dt)
    {
        time += dt;

        if (time > duration)
        {
            value = end;
            return value;
        }

        float x = time / duration;
        float x2 = x * x;
        float x3 = x2 * x;

        value = (end - start) * (3f * x2 - 2f * x3) + start;
        return value;
    }

    public float GetValue()
    {
        return value;
    }
}
