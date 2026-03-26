using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    public static event Action OnOrbitEnter, OnOrbitExit, OnSpawn, OnDespawn;
    public static event Action<float> OnDebugUpdate;
    public static event Action<bool> OnInertiaStabilizerChanged;
    public static event Action<bool> OnImpulseReadyChanged;

    [SerializeField] OrbiterConfig _orbiterConfig;
    [SerializeField] ImpulseResource _impulseResource;
    RigidbodyOrbiter _orbiterController;
    [SerializeField] LineRenderer _trajectoryRenderer;
    [SerializeField] LineRenderer _directionRenderer;
    private LineRendererController _lineRendererController;
    private Rigidbody _rb;
    private Vector2 _thrustInput;
    private Vector2 _aimDirection;
    private Vector3 _screenPosition;
    private bool _isAiming;
    private bool _isInScreen => _screenPosition.x > 0 & _screenPosition.x < 1 & _screenPosition.y > 0 & _screenPosition.y < 1;

    public bool IsImpulseEnergyFull => _impulseResource != null && _impulseResource.IsReady;

    /// <summary>Impulse recharge progress 0–1 for HUD (read from <see cref="ImpulseResource"/>).</summary>
    public float ImpulseEnergyNormalized => _impulseResource != null ? _impulseResource.NormalizedEnergy : 0f;

    public EscapeMode EscapeMode => _orbiterConfig != null ? _orbiterConfig.EscapeMode : EscapeMode.Cursor;
    public float ImpulseForce => _orbiterConfig != null ? _orbiterConfig.ImpulseForce : 0f;
    public float ThrustForce => _orbiterConfig != null ? _orbiterConfig.ThrustForce : 0f;
    public bool InertiaStabilizer => _orbiterConfig != null && _orbiterConfig.InertiaStabilizer;
    public float InertiaDampTime => _orbiterConfig != null ? _orbiterConfig.InertiaDampTime : 0f;
    public float StabilizerMaxThrustSpeed => _orbiterConfig != null ? _orbiterConfig.StabilizerMaxThrustSpeed : 0f;

    public ImpulseResource ImpulseResourceAsset => _impulseResource;

    public void SetInertiaStabilizer(bool value)
    {
        if (_orbiterConfig == null) return;
        _orbiterConfig.ApplyInertiaStabilizer(value);
        SyncOrbiterFromConfig();
        OnInertiaStabilizerChanged?.Invoke(_orbiterConfig.InertiaStabilizer);
    }

    public void ToggleInertiaStabilizer()
    {
        if (_orbiterConfig == null) return;
        SetInertiaStabilizer(!_orbiterConfig.InertiaStabilizer);
    }

    public void SetInertiaDampTime(float value)
    {
        if (_orbiterConfig == null) return;
        _orbiterConfig.ApplyInertiaDampTime(value);
        SyncOrbiterFromConfig();
    }

    public void SetStabilizerMaxThrustSpeed(float value)
    {
        if (_orbiterConfig == null) return;
        _orbiterConfig.ApplyStabilizerMaxThrustSpeed(value);
        SyncOrbiterFromConfig();
    }

    public void SetEscapeMode(EscapeMode mode)
    {
        if (_orbiterConfig == null) return;
        _orbiterConfig.ApplyEscapeMode(mode);
        SyncOrbiterFromConfig();
    }

    public void SetImpulseForce(float force)
    {
        if (_orbiterConfig == null) return;
        _orbiterConfig.ApplyImpulseForce(force);
        SyncOrbiterFromConfig();
    }

    public void SetThrustForce(float value)
    {
        if (_orbiterConfig == null) return;
        _orbiterConfig.ApplyThrustForce(value);
        SyncOrbiterFromConfig();
    }

    void Awake()
    {
        if (_orbiterConfig == null || _impulseResource == null)
            Debug.LogError("Orb requires OrbiterConfig and ImpulseResource references.", this);

        _rb = GetComponent<Rigidbody>();
        _orbiterController = new RigidbodyOrbiter(_rb, transform, OnOrbitEnter, OnOrbitExit,
            _orbiterConfig != null ? _orbiterConfig.ToOrbiterSettings() : OrbiterSettings.Default);
        _lineRendererController = new LineRendererController(_trajectoryRenderer, _directionRenderer, _orbiterController, transform.localScale.x);
    }

    void OnEnable()
    {
        _thrustInput = Vector2.zero;
        _aimDirection = Vector2.zero;
        if (_impulseResource != null)
            _impulseResource.OnReadyChanged += HandleImpulseReadyFromResource;
        _orbiterController?.OnEnable();
        _impulseResource?.ResetForSpawn();
        OnSpawn?.Invoke();
    }

    void FixedUpdate()
    {
        _orbiterController?.FixedUpdate(_thrustInput);
        _orbiterController?.ApplyThrust(_thrustInput);
    }

    void Update()
    {
        _impulseResource?.Tick(Time.deltaTime);
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
        if (!_isInScreen) gameObject.SetActive(false);

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
        if (!Application.isPlaying || _orbiterController == null || _orbiterConfig == null)
            return;
        SyncOrbiterFromConfig();
    }

    void OnCollisionEnter(Collision collision)
    {
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (_impulseResource != null)
            _impulseResource.OnReadyChanged -= HandleImpulseReadyFromResource;
        _thrustInput = Vector2.zero;
        SetAiming(false);
        _orbiterController?.OnDisable();
        OnDespawn?.Invoke();
    }

    public void SetAiming(bool active)
    {
        _isAiming = active;
        bool cursorMode = _orbiterConfig != null && _orbiterConfig.EscapeMode == EscapeMode.Cursor;
        _lineRendererController.SetAiming(active && cursorMode);
    }

    public void Impulse(Vector3 cursorWorldPosition)
    {
        if (_impulseResource == null || !_impulseResource.TryConsumeImpulse())
            return;

        _orbiterController.Impulse(cursorWorldPosition);
    }

    void SyncOrbiterFromConfig()
    {
        if (_orbiterConfig == null) return;
        _orbiterController?.UpdateSettings(_orbiterConfig.ToOrbiterSettings());
    }

    void HandleImpulseReadyFromResource(bool ready)
    {
        OnImpulseReadyChanged?.Invoke(ready);
    }
}
