using System;

public sealed class BoolParameter
{
    public string Id { get; }
    public string DisplayName { get; }
    public bool Value { get; private set; }

    public event Action<BoolParameter> Changed;

    public BoolParameter(string id, string displayName, bool defaultValue)
    {
        Id = id;
        DisplayName = displayName;
        Value = defaultValue;
    }

    public void SetValue(bool value)
    {
        if (Value == value)
            return;

        Value = value;
        Changed?.Invoke(this);
    }
}
