using UnityEngine;
using System;

public enum InertiaSettingsChangeType
{
    InertiaStabilizer,
    InertiaDampTime,
    StabilizerMaxThrustSpeed
}
[CreateAssetMenu(fileName = "InertiaResource", menuName = "Proto-Pablo/Inertia Resource", order = 4)]
public class InertiaResource : ScriptableObject
{
    [Header("Inertia")]
    [SerializeField] bool _inertiaStabilizer = true;

    [SerializeField, Range(OrbiterTuning.InertiaDampTimeMin, OrbiterTuning.InertiaDampTimeMax)]
    float _inertiaDampTime = 2f;

    [SerializeField, Range(OrbiterTuning.StabilizerMaxThrustSpeedMin, OrbiterTuning.StabilizerMaxThrustSpeedMax)]
    float _stabilizerMaxThrustSpeed = 10f;

    public bool InertiaStabilizer => _inertiaStabilizer;
    public float InertiaDampTime => _inertiaDampTime;
    public float StabilizerMaxThrustSpeed => _stabilizerMaxThrustSpeed;
    public event Action<InertiaSettingsChangeType> InertiaSettingsChanged;

    public void UpdateInertiaStabilizer(bool value)
    {
        _inertiaStabilizer = value;
        InertiaSettingsChanged?.Invoke(InertiaSettingsChangeType.InertiaStabilizer);
    }
    public void UpdateInertiaDampTime(float value)
    {
        _inertiaDampTime = Mathf.Clamp(value, OrbiterTuning.InertiaDampTimeMin, OrbiterTuning.InertiaDampTimeMax);
        InertiaSettingsChanged?.Invoke(InertiaSettingsChangeType.InertiaDampTime);
    }
    public void UpdateStabilizerMaxThrustSpeed(float value)
    {
        _stabilizerMaxThrustSpeed = Mathf.Clamp(value, OrbiterTuning.StabilizerMaxThrustSpeedMin, OrbiterTuning.StabilizerMaxThrustSpeedMax);
        InertiaSettingsChanged?.Invoke(InertiaSettingsChangeType.StabilizerMaxThrustSpeed);
    }
    public InertiaResourceSettings ToInertiaResourceSettings()
    {
        return new InertiaResourceSettings {
            InertiaStabilizer = _inertiaStabilizer,
            InertiaDampTime = _inertiaDampTime,
            StabilizerMaxThrustSpeed = _stabilizerMaxThrustSpeed,
        };
    }
}
public struct InertiaResourceSettings
{
    public bool InertiaStabilizer;
    public float InertiaDampTime;
    public float StabilizerMaxThrustSpeed;
    public static InertiaResourceSettings Default => new()
    {
        InertiaStabilizer = true,
        InertiaDampTime = 2f,
        StabilizerMaxThrustSpeed = 10f,
    };
}