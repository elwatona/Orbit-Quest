using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OrbSettingsBinder : MonoBehaviour
{
    // Ranges must match OrbiterSettings [Range] in RigidbodyOrbiter.cs
    const float ImpulseForceMin = 0f, ImpulseForceMax = 30f;
    const float ThrustForceMin = 0f, ThrustForceMax = 20f;
    const float InertiaDampTimeMin = 0.5f, InertiaDampTimeMax = 5f;
    const float StabilizerMaxThrustSpeedMin = 1f, StabilizerMaxThrustSpeedMax = 25f;

    [SerializeField] Orb _orb;
    [SerializeField] TMP_Dropdown _escapeModeDropdown;
    [FormerlySerializedAs("_escapeForceSlider")] [SerializeField] Slider _impulseForceSlider;
    [FormerlySerializedAs("_escapeForceLabel")] [SerializeField] TMP_Text _impulseForceLabel;
    [SerializeField] TMP_Text _orbStatusLabel;
    [Tooltip("Shows 'Impulse Ready' (green) or 'Impulse Charging' (white) based on impulse energy.")]
    [SerializeField] TMP_Text _impulseStatusLabel;

    [Header("Thrust")]
    [SerializeField] Slider _thrustForceSlider;
    [SerializeField] TMP_Text _thrustForceLabel;

    [Header("Inertia")]
    [SerializeField] Toggle _inertiaStabilizerToggle;
    [SerializeField] Slider _inertiaDampTimeSlider;
    [SerializeField] TMP_Text _inertiaDampTimeLabel;
    [SerializeField] Slider _stabilizerMaxThrustSpeedSlider;
    [SerializeField] TMP_Text _stabilizerMaxThrustSpeedLabel;

    void OnEnable()
    {
        if (_orb == null) return;

        if (_escapeModeDropdown != null)
        {
            _escapeModeDropdown.SetValueWithoutNotify((int)_orb.EscapeMode);
            _escapeModeDropdown.onValueChanged.AddListener(OnEscapeModeChanged);
        }

        if (_impulseForceSlider != null)
        {
            _impulseForceSlider.minValue = ImpulseForceMin;
            _impulseForceSlider.maxValue = ImpulseForceMax;
            _impulseForceSlider.SetValueWithoutNotify(_orb.ImpulseForce);
            _impulseForceSlider.onValueChanged.AddListener(OnImpulseForceChanged);
            RefreshImpulseForceLabel(_orb.ImpulseForce);
        }

        if (_thrustForceSlider != null)
        {
            _thrustForceSlider.minValue = ThrustForceMin;
            _thrustForceSlider.maxValue = ThrustForceMax;
            _thrustForceSlider.SetValueWithoutNotify(_orb.ThrustForce);
            _thrustForceSlider.onValueChanged.AddListener(OnThrustForceChanged);
            RefreshThrustForceLabel(_orb.ThrustForce);
        }

        if (_inertiaStabilizerToggle != null)
        {
            _inertiaStabilizerToggle.SetIsOnWithoutNotify(_orb.InertiaStabilizer);
            _inertiaStabilizerToggle.onValueChanged.AddListener(OnInertiaStabilizerChanged);
        }

        if (_inertiaDampTimeSlider != null)
        {
            _inertiaDampTimeSlider.minValue = InertiaDampTimeMin;
            _inertiaDampTimeSlider.maxValue = InertiaDampTimeMax;
            _inertiaDampTimeSlider.SetValueWithoutNotify(_orb.InertiaDampTime);
            _inertiaDampTimeSlider.onValueChanged.AddListener(OnInertiaDampTimeChanged);
            RefreshInertiaDampTimeLabel(_orb.InertiaDampTime);
        }

        if (_stabilizerMaxThrustSpeedSlider != null)
        {
            _stabilizerMaxThrustSpeedSlider.minValue = StabilizerMaxThrustSpeedMin;
            _stabilizerMaxThrustSpeedSlider.maxValue = StabilizerMaxThrustSpeedMax;
            _stabilizerMaxThrustSpeedSlider.SetValueWithoutNotify(_orb.StabilizerMaxThrustSpeed);
            _stabilizerMaxThrustSpeedSlider.onValueChanged.AddListener(OnStabilizerMaxThrustSpeedChanged);
            RefreshStabilizerMaxThrustSpeedLabel(_orb.StabilizerMaxThrustSpeed);
        }

        Orb.OnSpawn += UpdateOrbStatusLabel;
        Orb.OnDespawn += UpdateOrbStatusLabel;
        Orb.OnInertiaStabilizerChanged += SyncInertiaStabilizerToggle;
        Orb.OnImpulseReadyChanged += UpdateImpulseStatusLabel;

        if (_impulseStatusLabel != null && _orb != null)
            UpdateImpulseStatusLabel(_orb.IsImpulseEnergyFull);
    }

    void OnDisable()
    {
        Orb.OnImpulseReadyChanged -= UpdateImpulseStatusLabel;
        Orb.OnInertiaStabilizerChanged -= SyncInertiaStabilizerToggle;
        if (_escapeModeDropdown != null)
            _escapeModeDropdown.onValueChanged.RemoveListener(OnEscapeModeChanged);
        if (_impulseForceSlider != null)
            _impulseForceSlider.onValueChanged.RemoveListener(OnImpulseForceChanged);
        if (_thrustForceSlider != null)
            _thrustForceSlider.onValueChanged.RemoveListener(OnThrustForceChanged);
        if (_inertiaStabilizerToggle != null)
            _inertiaStabilizerToggle.onValueChanged.RemoveListener(OnInertiaStabilizerChanged);
        if (_inertiaDampTimeSlider != null)
            _inertiaDampTimeSlider.onValueChanged.RemoveListener(OnInertiaDampTimeChanged);
        if (_stabilizerMaxThrustSpeedSlider != null)
            _stabilizerMaxThrustSpeedSlider.onValueChanged.RemoveListener(OnStabilizerMaxThrustSpeedChanged);
        Orb.OnSpawn -= UpdateOrbStatusLabel;
        Orb.OnDespawn -= UpdateOrbStatusLabel;
    }
    void Start()
    {
        UpdateOrbStatusLabel();
    }
    void OnEscapeModeChanged(int index)
    {
        if (_orb != null)
            _orb.SetEscapeMode((EscapeMode)index);
    }

    void OnImpulseForceChanged(float value)
    {
        if (_orb != null)
        {
            _orb.SetImpulseForce(value);
            RefreshImpulseForceLabel(value);
        }
    }

    void RefreshImpulseForceLabel(float value)
    {
        if (_impulseForceLabel != null)
            _impulseForceLabel.text = value.ToString("0.##");
    }

    void OnThrustForceChanged(float value)
    {
        if (_orb != null)
        {
            _orb.SetThrustForce(value);
            RefreshThrustForceLabel(value);
        }
    }

    void RefreshThrustForceLabel(float value)
    {
        if (_thrustForceLabel != null)
            _thrustForceLabel.text = value.ToString("0.##");
    }

    void OnInertiaStabilizerChanged(bool value)
    {
        if (_orb != null)
            _orb.SetInertiaStabilizer(value);
    }

    void SyncInertiaStabilizerToggle(bool value)
    {
        if (_inertiaStabilizerToggle != null)
            _inertiaStabilizerToggle.SetIsOnWithoutNotify(value);
    }

    void OnInertiaDampTimeChanged(float value)
    {
        if (_orb != null)
        {
            _orb.SetInertiaDampTime(value);
            RefreshInertiaDampTimeLabel(value);
        }
    }

    void OnStabilizerMaxThrustSpeedChanged(float value)
    {
        if (_orb != null)
        {
            _orb.SetStabilizerMaxThrustSpeed(value);
            RefreshStabilizerMaxThrustSpeedLabel(value);
        }
    }

    void RefreshInertiaDampTimeLabel(float value)
    {
        if (_inertiaDampTimeLabel != null)
            _inertiaDampTimeLabel.text = value.ToString("0.#");
    }

    void RefreshStabilizerMaxThrustSpeedLabel(float value)
    {
        if (_stabilizerMaxThrustSpeedLabel != null)
            _stabilizerMaxThrustSpeedLabel.text = value.ToString("0.#");
    }

    void UpdateOrbStatusLabel()
    {
        if (_orbStatusLabel == null) return;
        _orbStatusLabel.text = GetOrbStatusText(_orb.gameObject.activeSelf);
        if (_impulseStatusLabel != null && _orb != null)
            UpdateImpulseStatusLabel(_orb.IsImpulseEnergyFull);
    }

    void UpdateImpulseStatusLabel(bool ready)
    {
        if (_impulseStatusLabel == null) return;
        _impulseStatusLabel.text = ready ? "Impulse Ready" : "Impulse Charging";
        _impulseStatusLabel.color = ready ? Color.green : Color.white;
    }
    string GetOrbStatusText(bool isActive)
    {
        return isActive ? "<color=green>Alive" : "<color=red>Dead";
    }
}
