using System;
using UnityEngine;

public sealed class FloatParameter
{
    public string Id { get; }
    public string DisplayName { get; }
    public float Min { get; }
    public float Max { get; }
    public float Value { get; private set; }

    public event Action<FloatParameter> Changed;

    public FloatParameter(string id, string displayName, float min, float max, float defaultValue)
    {
        Id = id;
        DisplayName = displayName;
        Min = min;
        Max = max;
        Value = Mathf.Clamp(defaultValue, min, max);
    }

    public void SetValue(float value)
    {
        float clamped = Mathf.Clamp(value, Min, Max);
        if (Mathf.Approximately(Value, clamped))
            return;

        Value = clamped;
        Changed?.Invoke(this);
    }
}
