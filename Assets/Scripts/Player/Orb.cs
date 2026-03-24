using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    public static event Action OnOrbitEnter, OnOrbitExit, OnSpawn, OnDespawn;
    public static event Action<float> OnDebugUpdate;
    public static event Action<bool> OnInertiaStabilizerChanged;
    public static event Action<bool> OnImpulseReadyChanged;

    [SerializeField] RigidbodyOrbiter _orbiterController;
    [SerializeField] OrbiterSettings _orbiterSettings = OrbiterSettings.Default;
    [SerializeField] LineRenderer _trajectoryRenderer;
    [SerializeField] LineRenderer _directionRenderer;
    private LineRendererController _lineRendererController;
    private Rigidbody _rb;
    private Vector2 _thrustInput;
    private Vector2 _aimDirection;
    private Vector3 _screenPosition;
    private bool _isAiming;
    private bool _isInScreen => _screenPosition.x > 0 & _screenPosition.x < 1 & _screenPosition.y > 0 & _screenPosition.y < 1;

    /// <summary>PLACEHOLDER: time in seconds to refill impulse energy from 0 to 1. Remove or replace with proper recharge logic.</summary>
    const float ImpulseRechargeDuration = 5f;

    [Header("Impulse resource")]
    [Tooltip("Energy required to use Impulse (0-1). Must be full (>= 1) to execute impulse.")]
    [SerializeField, Range(0f, 1f)] float _impulseEnergy = 1f;
    private bool _wasImpulseEnergyFull = true;

    public bool IsImpulseEnergyFull => _impulseEnergy >= 1f;

    public EscapeMode EscapeMode => _orbiterSettings.escapeMode;
    public float ImpulseForce => _orbiterSettings.impulseForce;
    public float ThrustForce => _orbiterSettings.thrustForce;
    public bool InertiaStabilizer => _orbiterSettings.inertiaStabilizer;
    public float InertiaDampTime => _orbiterSettings.inertiaDampTime;
    public float StabilizerMaxThrustSpeed => _orbiterSettings.stabilizerMaxThrustSpeed;

    public void SetInertiaStabilizer(bool value)
    {
        _orbiterSettings.inertiaStabilizer = value;
        _orbiterController.UpdateSettings(_orbiterSettings);
        OnInertiaStabilizerChanged?.Invoke(_orbiterSettings.inertiaStabilizer);
    }

    public void ToggleInertiaStabilizer()
    {
        SetInertiaStabilizer(!_orbiterSettings.inertiaStabilizer);
    }

    public void SetInertiaDampTime(float value)
    {
        _orbiterSettings.inertiaDampTime = Mathf.Clamp(value, 0.5f, 5f);
        _orbiterController.UpdateSettings(_orbiterSettings);
    }

    public void SetStabilizerMaxThrustSpeed(float value)
    {
        _orbiterSettings.stabilizerMaxThrustSpeed = Mathf.Clamp(value, 1f, 25f);
        _orbiterController.UpdateSettings(_orbiterSettings);
    }

    public void SetEscapeMode(EscapeMode mode)
    {
        _orbiterSettings.escapeMode = mode;
        _orbiterController.UpdateSettings(_orbiterSettings);
    }

    public void SetImpulseForce(float force)
    {
        _orbiterSettings.impulseForce = Mathf.Clamp(force, 0f, 30f);
        _orbiterController.UpdateSettings(_orbiterSettings);
    }

    public void SetThrustForce(float value)
    {
        _orbiterSettings.thrustForce = Mathf.Clamp(value, 0f, 20f);
        _orbiterController.UpdateSettings(_orbiterSettings);
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _orbiterController = new RigidbodyOrbiter(_rb, transform, OnOrbitEnter, OnOrbitExit, _orbiterSettings);
        _lineRendererController = new LineRendererController(_trajectoryRenderer, _directionRenderer, _orbiterController, transform.localScale.x);
    }
    void OnEnable()
    {
        _thrustInput = Vector2.zero;
        _aimDirection = Vector2.zero;
        _orbiterController.OnEnable();
        _impulseEnergy = 1f;
        _wasImpulseEnergyFull = true;
        OnSpawn?.Invoke();
    }
    void FixedUpdate()
    {
        _orbiterController?.FixedUpdate(_thrustInput);
        _orbiterController.ApplyThrust(_thrustInput);
    }

    void Update()
    {
        // PLACEHOLDER: recharge impulse energy over ImpulseRechargeDuration. Remove or replace later.
        if (_impulseEnergy < 1f)
            _impulseEnergy = Mathf.Clamp01(_impulseEnergy + Time.deltaTime / ImpulseRechargeDuration);

        if (_wasImpulseEnergyFull != IsImpulseEnergyFull)
        {
            _wasImpulseEnergyFull = IsImpulseEnergyFull;
            OnImpulseReadyChanged?.Invoke(IsImpulseEnergyFull);
        }
    }

    /// <summary>Called by the Move input action (e.g. from PlayerController). Passes the current movement direction.</summary>
    public void SetThrustInput(Vector2 value)
    {
        _thrustInput = value;
    }

    /// <summary>Sets the aim direction (world space, e.g. from mouse). Used to orient the orb and for thrust direction.</summary>
    public void SetAimDirection(Vector2 worldDirection)
    {
        _aimDirection = worldDirection;
    }

    /// <summary>Applies a single thrust impulse in the given world-space direction. Called e.g. on right click.</summary>
    public void ApplyThrustImpulse(Vector2 worldDirection)
    {
        if (worldDirection.sqrMagnitude < 0.0001f) return;
        _orbiterController.ApplyThrust(worldDirection);
    }
    void LateUpdate()
    {
        _screenPosition = Camera.main.WorldToViewportPoint(transform.position);
        if(!_isInScreen) gameObject.SetActive(false);

        if (_isAiming)
        {
            Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorWorldPosition.z = 0;

            _lineRendererController.UpdateTrajectory(cursorWorldPosition);
        }

        _lineRendererController.UpdateDirection(transform.position, _rb.linearVelocity);

        if (_aimDirection.sqrMagnitude > 0.0001f)
        {
            float angleDeg = -Mathf.Atan2(_aimDirection.x, _aimDirection.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }

        OnDebugUpdate?.Invoke(_orbiterController.Speed);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IOrbitable orbit))
            _orbiterController.AddGravitySource(orbit);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IOrbitable orbit))
            _orbiterController.RemoveGravitySource(orbit);
    }
    void OnValidate()
    {
        _orbiterController?.UpdateSettings(_orbiterSettings);
    }
    void OnCollisionEnter(Collision collision)
    {
        gameObject.SetActive(false);
    }
    void OnDisable()
    {
        _thrustInput = Vector2.zero;
        SetAiming(false);
        _orbiterController.OnDisable();
        OnDespawn?.Invoke();
    }
    public void SetAiming(bool active)
    {
        _isAiming = active;
        _lineRendererController.SetAiming(active && _orbiterSettings.escapeMode == EscapeMode.Cursor);
    }
    public void Impulse(Vector3 cursorWorldPosition)
    {
        if (!IsImpulseEnergyFull)
            return;

        _impulseEnergy = 0f;
        _wasImpulseEnergyFull = false;
        OnImpulseReadyChanged?.Invoke(false);

        _orbiterController.Impulse(cursorWorldPosition);
    }
}
