using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Proto-Pablo/Player Data", order = 1)]
public class PlayerData : ScriptableObject
{
    public event Action OrbiterConfigUpdated;
    public event Action IsInEditModeUpdated;
    [SerializeField] ImpulseResource _impulseResource;
    [SerializeField] ThrusterResource _thrusterResource;
    [SerializeField] InertiaResource _inertiaResource;
    [SerializeField] bool _isInEditMode = false;
    public ImpulseResource ImpulseResource => _impulseResource;
    public ThrusterResource ThrusterResource => _thrusterResource;
    public InertiaResource InertiaResource => _inertiaResource;
    public bool IsInEditMode => _isInEditMode;
    public OrbiterSettings ToOrbiterSettings()
    {
        return new OrbiterSettings {
            ThrusterResourceSettings = _thrusterResource.ToThrusterResourceSettings(),
            ImpulseResourceSettings = _impulseResource.ToImpulseResourceSettings(),
            InertiaResourceSettings = _inertiaResource.ToInertiaResourceSettings(),
        };
    }

    public void UpdateThrustForce(float value)
    {
        _thrusterResource.UpdateThrustForce(value);
        OrbiterConfigUpdated?.Invoke();
    }

    public void UpdateMinThrustAssist(float value)
    {
        _thrusterResource.UpdateMinThrustAssist(value);
        OrbiterConfigUpdated?.Invoke();
    }

    public void UpdateImpulseForce(float value)
    {
        _impulseResource.UpdateImpulseForce(value);
        OrbiterConfigUpdated?.Invoke();
    }

    public void UpdateImpulseMode(int mode)
    {
        _impulseResource.UpdateImpulseMode(mode);
        OrbiterConfigUpdated?.Invoke();
    }
    public void UpdateImpuseCooldown(float value)
    {
        _impulseResource.SetRechargeDuration(value);
        OrbiterConfigUpdated?.Invoke();
    }

    public void UpdateInertiaStabilizer(bool value)
    {
        _inertiaResource.UpdateInertiaStabilizer(value);
        OrbiterConfigUpdated?.Invoke();
    }

    public void UpdateInertiaDampTime(float value)
    {
        _inertiaResource.UpdateInertiaDampTime(value);
        OrbiterConfigUpdated?.Invoke();
    }

    public void UpdateStabilizerMaxThrustSpeed(float value)
    {
        _inertiaResource.UpdateStabilizerMaxThrustSpeed(value);
        OrbiterConfigUpdated?.Invoke();
    }
    public void SetIsInEditMode(bool value)
    {
        _isInEditMode = value;
        IsInEditModeUpdated?.Invoke();
    }
}
