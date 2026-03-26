using UnityEngine;

/// <summary>
/// Authoring asset: single source of truth for values passed to <see cref="RigidbodyOrbiter"/> via <see cref="OrbiterSettings"/>.
/// </summary>
[CreateAssetMenu(fileName = "OrbiterConfig", menuName = "Proto-Pablo/Orbiter Config", order = 1)]
public class OrbiterConfig : ScriptableObject
{
    [Header("Thrust")]
    [SerializeField, Range(OrbiterTuning.ThrustForceMin, OrbiterTuning.ThrustForceMax)]
    float _thrustForce = 5f;

    [SerializeField, Range(OrbiterTuning.MinThrustAssistMin, OrbiterTuning.MinThrustAssistMax)]
    float _minThrustAssist = 2f;

    [Header("Impulse")]
    [SerializeField] EscapeMode _escapeMode = EscapeMode.Cursor;

    [SerializeField, Range(OrbiterTuning.ImpulseForceMin, OrbiterTuning.ImpulseForceMax)]
    float _impulseForce = 5f;

    [Header("Inertia")]
    [SerializeField] bool _inertiaStabilizer = true;

    [SerializeField, Range(OrbiterTuning.InertiaDampTimeMin, OrbiterTuning.InertiaDampTimeMax)]
    float _inertiaDampTime = 2f;

    [SerializeField, Range(OrbiterTuning.StabilizerMaxThrustSpeedMin, OrbiterTuning.StabilizerMaxThrustSpeedMax)]
    float _stabilizerMaxThrustSpeed = 10f;

    public EscapeMode EscapeMode => _escapeMode;
    public float ThrustForce => _thrustForce;
    public float MinThrustAssist => _minThrustAssist;
    public float ImpulseForce => _impulseForce;
    public bool InertiaStabilizer => _inertiaStabilizer;
    public float InertiaDampTime => _inertiaDampTime;
    public float StabilizerMaxThrustSpeed => _stabilizerMaxThrustSpeed;

    public OrbiterSettings ToOrbiterSettings()
    {
        return new OrbiterSettings
        {
            thrustForce = _thrustForce,
            minThrustAssist = _minThrustAssist,
            escapeMode = _escapeMode,
            impulseForce = _impulseForce,
            inertiaStabilizer = _inertiaStabilizer,
            inertiaDampTime = _inertiaDampTime,
            stabilizerMaxThrustSpeed = _stabilizerMaxThrustSpeed
        };
    }

    public void ApplyThrustForce(float value)
    {
        _thrustForce = Mathf.Clamp(value, OrbiterTuning.ThrustForceMin, OrbiterTuning.ThrustForceMax);
    }

    public void ApplyMinThrustAssist(float value)
    {
        _minThrustAssist = Mathf.Clamp(value, OrbiterTuning.MinThrustAssistMin, OrbiterTuning.MinThrustAssistMax);
    }

    public void ApplyImpulseForce(float value)
    {
        _impulseForce = Mathf.Clamp(value, OrbiterTuning.ImpulseForceMin, OrbiterTuning.ImpulseForceMax);
    }

    public void ApplyEscapeMode(EscapeMode mode)
    {
        _escapeMode = mode;
    }

    public void ApplyInertiaStabilizer(bool value)
    {
        _inertiaStabilizer = value;
    }

    public void ApplyInertiaDampTime(float value)
    {
        _inertiaDampTime = Mathf.Clamp(value, OrbiterTuning.InertiaDampTimeMin, OrbiterTuning.InertiaDampTimeMax);
    }

    public void ApplyStabilizerMaxThrustSpeed(float value)
    {
        _stabilizerMaxThrustSpeed = Mathf.Clamp(value, OrbiterTuning.StabilizerMaxThrustSpeedMin, OrbiterTuning.StabilizerMaxThrustSpeedMax);
    }
}
