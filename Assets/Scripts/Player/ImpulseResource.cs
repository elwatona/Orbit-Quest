using System;
using UnityEngine;

public enum ImpulseSettingsChangeType
{
    ImpulseForce,
    RechargeDuration,
    ImpulseMode,
    EnergyChanged
}
[CreateAssetMenu(fileName = "ImpulseResource", menuName = "Proto-Pablo/Impulse Resource", order = 2)]
public class ImpulseResource : ScriptableObject
{
    [Header("Impulse")]
    [SerializeField] ImpulseMode _impulseMode = ImpulseMode.Cursor;

    [SerializeField, Range(OrbiterTuning.ImpulseForceMin, OrbiterTuning.ImpulseForceMax)]
    float _impulseForce = 5f;
    [SerializeField, Min(0.01f)] float _rechargeDuration = 5f;
    [SerializeField, Range(0f, 1f)] float _energy = 1f;

    public ImpulseMode ImpulseMode => _impulseMode;
    public float ImpulseForce => _impulseForce;
    public float NormalizedEnergy => _energy;
    public bool IsReady => _energy >= 1f;
    public float RechargeDuration => _rechargeDuration;
    public event Action<ImpulseSettingsChangeType> ImpulseSettingsChanged;

    public void SetRechargeDuration(float seconds)
    {
        _rechargeDuration = Mathf.Max(0.01f, seconds);
        ImpulseSettingsChanged?.Invoke(ImpulseSettingsChangeType.RechargeDuration);
    }

    public void ResetForSpawn()
    {
        _energy = 1f;
        ImpulseSettingsChanged?.Invoke(ImpulseSettingsChangeType.EnergyChanged);
    }

    public void Tick(float deltaTime)
    {
        if (_energy >= 1f)
            return;

        float previous = _energy;
        _energy = Mathf.Clamp01(_energy + deltaTime / _rechargeDuration);
        if (!Mathf.Approximately(previous, _energy))
            ImpulseSettingsChanged?.Invoke(ImpulseSettingsChangeType.EnergyChanged);
    }

    /// <summary>Drains energy when an impulse is executed. Returns false if not ready.</summary>
    public bool TryConsumeImpulse()
    {
        if (!IsReady)
            return false;

        _energy = 0f;
        ImpulseSettingsChanged?.Invoke(ImpulseSettingsChangeType.EnergyChanged);
        return true;
    }
    public void UpdateImpulseForce(float value)
    {
        _impulseForce = Mathf.Clamp(value, OrbiterTuning.ImpulseForceMin, OrbiterTuning.ImpulseForceMax);
        ImpulseSettingsChanged?.Invoke(ImpulseSettingsChangeType.ImpulseForce);
    }
    public void UpdateImpulseMode(int mode)
    {
        _impulseMode = (ImpulseMode)mode;
        ImpulseSettingsChanged?.Invoke(ImpulseSettingsChangeType.ImpulseMode);
    }
    public ImpulseResourceSettings ToImpulseResourceSettings()
    {
        return new ImpulseResourceSettings {
            ImpulseMode = _impulseMode,
            ImpulseForce = _impulseForce,
            RechargeDuration = _rechargeDuration,
        };
    }
}
public struct ImpulseResourceSettings
{
    public ImpulseMode ImpulseMode;
    public float ImpulseForce;
    public float RechargeDuration;
    public static ImpulseResourceSettings Default => new()
    {
        ImpulseMode = ImpulseMode.Cursor,
        ImpulseForce = 5f,
        RechargeDuration = 5f,
    };
}
