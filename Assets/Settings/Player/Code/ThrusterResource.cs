using UnityEngine;
using System;

public enum ThrusterSettingsChangeType
{
    ThrustForce,
    MinThrustAssist,
    SpeedChanged
}
[CreateAssetMenu(fileName = "ThrusterResource", menuName = "Proto-Pablo/Thruster Resource", order = 3)]
public class ThrusterResource : ScriptableObject
{
    [SerializeField, Range(OrbiterTuning.ThrustForceMin, OrbiterTuning.ThrustForceMax)]
    float _thrustForce = 5f;

    [SerializeField, Range(OrbiterTuning.MinThrustAssistMin, OrbiterTuning.MinThrustAssistMax)]
    float _minThrustAssist = 2f;

    public float ThrustForce => _thrustForce;
    public float MinThrustAssist => _minThrustAssist;
    public float Speed {get; private set;}
    public event Action<ThrusterSettingsChangeType> ThrusterSettingsChanged;
    public void UpdateThrustForce(float value)
    {
        _thrustForce = Mathf.Clamp(value, OrbiterTuning.ThrustForceMin, OrbiterTuning.ThrustForceMax);
        ThrusterSettingsChanged?.Invoke(ThrusterSettingsChangeType.ThrustForce);
    }
    public void UpdateMinThrustAssist(float value)
    {
        _minThrustAssist = Mathf.Clamp(value, OrbiterTuning.MinThrustAssistMin, OrbiterTuning.MinThrustAssistMax);
        ThrusterSettingsChanged?.Invoke(ThrusterSettingsChangeType.MinThrustAssist);
    }
    public void UpdateSpeed(float speed)
    {
        Speed = speed;
        ThrusterSettingsChanged?.Invoke(ThrusterSettingsChangeType.SpeedChanged);
    }
    public ThrusterResourceSettings ToThrusterResourceSettings()
    {
        return new ThrusterResourceSettings {
            ThrustForce = _thrustForce,
            MinThrustAssist = _minThrustAssist,
        };
    }
}
public struct ThrusterResourceSettings
{
    public float ThrustForce;
    public float MinThrustAssist;
    public static ThrusterResourceSettings Default => new()
    {
        ThrustForce = 5f,
        MinThrustAssist = 2f,
    };
}