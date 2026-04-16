using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    public static event Action OnOrbitEnter, OnOrbitExit, OnSpawn, OnDespawn;

    [SerializeField] PlayerData _playerData;
    [SerializeField] LineRenderer _trajectoryRenderer;
    [SerializeField] LineRenderer _directionRenderer;
    private RigidbodyOrbiter _orbiterController;
    private LineRendererController _lineRendererController;
    private Rigidbody _rb;
    private Vector2 _thrustInput;
    private Vector2 _aimDirection;
    private Vector3 _screenPosition;
    private bool _isAiming;
    private bool _isInScreen => _screenPosition.x > 0 & _screenPosition.x < 1 & _screenPosition.y > 0 & _screenPosition.y < 1;

    public void SetInertiaStabilizer(bool value)
    {
        if (_playerData.InertiaResource == null) return;
        _playerData.UpdateInertiaStabilizer(value);
        SyncOrbiterFromConfig();
    }

    public void ToggleInertiaStabilizer()
    {
        if (_playerData.InertiaResource == null) return;
        SetInertiaStabilizer(!_playerData.InertiaResource.InertiaStabilizer);
    }

    void Awake()
    {
        if (_playerData == null || _playerData.ImpulseResource == null)
            Debug.LogError("Orb requires OrbiterConfig and ImpulseResource references.", this);

        _rb = GetComponent<Rigidbody>();
        _orbiterController = new RigidbodyOrbiter(_rb, transform, OnOrbitEnter, OnOrbitExit,
            _playerData != null ? _playerData.ToOrbiterSettings() : OrbiterSettings.Default);
        _lineRendererController = new LineRendererController(_trajectoryRenderer, _directionRenderer, _orbiterController, transform.localScale.x);
    }

    void OnEnable()
    {
        _thrustInput = Vector2.zero;
        _aimDirection = Vector2.zero;
        _orbiterController?.OnEnable();
        _playerData.ImpulseResource?.ResetForSpawn();
        OnSpawn?.Invoke();

        _playerData.OrbiterConfigUpdated += SyncOrbiterFromConfig;
        _playerData.SetPlayerStatus(PlayerStatus.Alive);
    }

    void FixedUpdate()
    {
        _orbiterController?.FixedUpdate(_thrustInput);
        _orbiterController?.ApplyThrust(_thrustInput);
    }

    void Update()
    {
        _playerData.ImpulseResource?.Tick(Time.deltaTime);
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
        _playerData.ThrusterResource.UpdateSpeed(_orbiterController.Speed);
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
        if (!Application.isPlaying || _orbiterController == null || _playerData == null)
            return;
        SyncOrbiterFromConfig();
    }

    void OnCollisionEnter(Collision collision)
    {
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        _thrustInput = Vector2.zero;
        SetAiming(false);
        _orbiterController?.OnDisable();
        OnDespawn?.Invoke();

        _playerData.OrbiterConfigUpdated -= SyncOrbiterFromConfig;
        _playerData.SetPlayerStatus(PlayerStatus.Dead);
    }

    public void SetAiming(bool active)
    {
        _isAiming = active;
        bool cursorMode = _playerData != null && _playerData.ImpulseResource.ImpulseMode == ImpulseMode.Cursor;
        _lineRendererController.SetAiming(active && cursorMode);
    }

    public void Impulse(Vector3 cursorWorldPosition)
    {
        if (_playerData.ImpulseResource == null || !_playerData.ImpulseResource.TryConsumeImpulse())
            return;

        _orbiterController.Impulse(cursorWorldPosition);
    }

    void SyncOrbiterFromConfig()
    {
        if (_playerData == null) return;
        _orbiterController?.UpdateSettings(_playerData.ToOrbiterSettings());
    }
}
