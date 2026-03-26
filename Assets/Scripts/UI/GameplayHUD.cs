using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Read-only HUD for the player: subscribes to Orb events and does not write simulation state.
/// Impulse fill reads from <see cref="ImpulseResource"/> when assigned.
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [SerializeField] Orb _orb;
    [SerializeField] ImpulseResource _impulseResource;
    [SerializeField] TextMeshProUGUI _speedText;
    [Tooltip("Filled Image (Image Type: Filled). fillAmount mirrors impulse recharge 0–1.")]
    [SerializeField] Image _impulseFillImage;
    [SerializeField] TextMeshProUGUI _impulseStatusText;
    [SerializeField] Color _impulseReadyColor = new(0.35f, 1f, 0.45f, 1f);
    [SerializeField] Color _impulseChargingColor = new(0.85f, 0.85f, 0.9f, 1f);
    [SerializeField] TextMeshProUGUI _inertiaStabilizerText;

    void OnEnable()
    {
        Orb.OnDebugUpdate += OnSpeedUpdate;
        Orb.OnImpulseReadyChanged += OnImpulseReadyChanged;
        Orb.OnInertiaStabilizerChanged += OnInertiaStabilizerChanged;
        if (_impulseResource != null)
            _impulseResource.OnEnergyChanged += OnImpulseEnergyChanged;
    }

    void OnDisable()
    {
        Orb.OnDebugUpdate -= OnSpeedUpdate;
        Orb.OnImpulseReadyChanged -= OnImpulseReadyChanged;
        Orb.OnInertiaStabilizerChanged -= OnInertiaStabilizerChanged;
        if (_impulseResource != null)
            _impulseResource.OnEnergyChanged -= OnImpulseEnergyChanged;
    }

    void Start()
    {
        if (_orb != null)
        {
            RefreshImpulseFill();
            if (IsUiAlive(_impulseStatusText))
                OnImpulseReadyChanged(GetImpulseReady());
            if (IsUiAlive(_inertiaStabilizerText))
                OnInertiaStabilizerChanged(_orb.InertiaStabilizer);
        }
    }

    void Update()
    {
        if (!isActiveAndEnabled) return;
        RefreshImpulseFill();
    }

    static bool IsUiAlive(UnityEngine.Object ui) => ui != null;

    bool GetImpulseReady()
    {
        if (_impulseResource != null)
            return _impulseResource.IsReady;
        return _orb != null && _orb.IsImpulseEnergyFull;
    }

    float GetImpulseNormalized()
    {
        if (_impulseResource != null)
            return _impulseResource.NormalizedEnergy;
        return _orb != null ? _orb.ImpulseEnergyNormalized : 0f;
    }

    void RefreshImpulseFill()
    {
        if (!IsUiAlive(_impulseFillImage)) return;
        float t = GetImpulseNormalized();
        _impulseFillImage.fillAmount = t;
        _impulseFillImage.color = t >= 1f ? _impulseReadyColor : _impulseChargingColor;
    }

    void OnImpulseEnergyChanged(float _)
    {
        RefreshImpulseFill();
    }

    void OnSpeedUpdate(float speed)
    {
        if (!isActiveAndEnabled || !IsUiAlive(_speedText)) return;
        _speedText.text = $"Speed: {speed:0.##}";
    }

    void OnImpulseReadyChanged(bool ready)
    {
        if (!isActiveAndEnabled) return;
        if (IsUiAlive(_impulseStatusText))
        {
            _impulseStatusText.text = ready ? "Impulse Ready" : "Impulse Charging";
            _impulseStatusText.color = ready ? _impulseReadyColor : _impulseChargingColor;
        }
        RefreshImpulseFill();
    }

    void OnInertiaStabilizerChanged(bool on)
    {
        if (!isActiveAndEnabled || !IsUiAlive(_inertiaStabilizerText)) return;
        _inertiaStabilizerText.text = on ? "Stabilizer On" : "Stabilizer Off";
        _inertiaStabilizerText.color = on ? Color.cyan : Color.gray;
    }
}
