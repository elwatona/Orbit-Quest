using System;
using UnityEngine;

/// <summary>
/// Mutable balance + runtime impulse energy (0–1). HUD and <see cref="Orb"/> share the same asset reference.
/// </summary>
[CreateAssetMenu(fileName = "ImpulseResource", menuName = "Proto-Pablo/Impulse Resource", order = 2)]
public class ImpulseResource : ScriptableObject
{
    [SerializeField, Min(0.01f)] float _rechargeDuration = 5f;
    [SerializeField, Range(0f, 1f)] float _energy = 1f;

    public event Action<float> OnEnergyChanged;
    public event Action<bool> OnReadyChanged;

    public float NormalizedEnergy => _energy;
    public bool IsReady => _energy >= 1f;
    public float RechargeDuration => _rechargeDuration;

    public void SetRechargeDuration(float seconds)
    {
        _rechargeDuration = Mathf.Max(0.01f, seconds);
    }

    public void ResetForSpawn()
    {
        _energy = 1f;
        OnEnergyChanged?.Invoke(_energy);
        OnReadyChanged?.Invoke(true);
    }

    public void Tick(float deltaTime)
    {
        if (_energy >= 1f)
            return;

        float previous = _energy;
        _energy = Mathf.Clamp01(_energy + deltaTime / _rechargeDuration);
        if (!Mathf.Approximately(previous, _energy))
            OnEnergyChanged?.Invoke(_energy);

        if (previous < 1f && _energy >= 1f)
            OnReadyChanged?.Invoke(true);
    }

    /// <summary>Drains energy when an impulse is executed. Returns false if not ready.</summary>
    public bool TryConsumeImpulse()
    {
        if (!IsReady)
            return false;

        _energy = 0f;
        OnEnergyChanged?.Invoke(_energy);
        OnReadyChanged?.Invoke(false);
        return true;
    }
}
