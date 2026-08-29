using System;
using UnityEngine;

public sealed class RangeParameter
{
    public string Id { get; }
    public string DisplayName { get; }
    public float AbsoluteMin { get; }
    public float AbsoluteMax { get; }
    public float Min { get; private set; }
    public float Max { get; private set; }

    public event Action<RangeParameter> Changed;

    public RangeParameter(string id, string displayName, float absoluteMin, float absoluteMax)
    {
        Id = id;
        DisplayName = displayName;
        AbsoluteMin = absoluteMin;
        AbsoluteMax = absoluteMax;
        Min = absoluteMin;
        Max = absoluteMax;
    }

    public void SetMin(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, Max);
        if (Mathf.Approximately(Min, clamped))
            return;

        Min = clamped;
        Changed?.Invoke(this);
    }

    public void SetMax(float value)
    {
        float clamped = Mathf.Max(Min, value);
        if (clamped < 0f)
            clamped = 0f;
        if (Mathf.Approximately(Max, clamped))
            return;

        Max = clamped;
        Changed?.Invoke(this);
    }

    public void SetRange(float min, float max, bool notify = true)
    {
        float newMin = Mathf.Max(0f, min);
        float newMax = Mathf.Max(newMin, max);
        if (newMax < 0f)
            newMax = 0f;

        if (Mathf.Approximately(Min, newMin) && Mathf.Approximately(Max, newMax))
            return;

        Min = newMin;
        Max = newMax;
        if (notify)
            Changed?.Invoke(this);
    }

    public void ResetToDefaults()
    {
        SetRange(AbsoluteMin, AbsoluteMax);
    }
}
