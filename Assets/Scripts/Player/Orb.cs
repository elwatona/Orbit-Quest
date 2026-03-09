using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    public static event Action OnOrbitEnter, OnOrbitExit, OnSpawn, OnDespawn;
    public static event Action<float, EscapeMode> OnDebugUpdate;

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
    private bool _pendingLooseOncePerDeath;
    private bool _isInScreen => _screenPosition.x > 0 & _screenPosition.x < 1 & _screenPosition.y > 0 & _screenPosition.y < 1;

    const float LooseOncePerDeathSpeed = 20f;

    public EscapeMode EscapeMode => _orbiterSettings.escapeMode;
    public float EscapeForce => _orbiterSettings.escapeForce;
    public OrbitMode OrbitMode => _orbiterSettings.orbitMode;

    public void SetOrbitMode(OrbitMode mode)
    {
        _orbiterSettings.orbitMode = mode;
        _orbiterController.UpdateSettings(_orbiterSettings);
    }

    public void SetEscapeMode(EscapeMode mode)
    {
        _orbiterSettings.escapeMode = mode;
        _orbiterController.UpdateSettings(_orbiterSettings);
    }

    public void SetEscapeForce(float force)
    {
        _orbiterSettings.escapeForce = Mathf.Clamp(force, 0f, 30f);
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
        _orbiterController.OnEnable();
        _pendingLooseOncePerDeath = true;
        OnSpawn?.Invoke();
    }
    void FixedUpdate()
    {
        _orbiterController?.FixedUpdate();
        _orbiterController.ApplyThrust(_thrustInput);
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

        OnDebugUpdate?.Invoke(_orbiterController.Speed, _orbiterController.EscapeMode);
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
    public void Loose(Vector3 cursorWorldPosition)
    {
        if (_pendingLooseOncePerDeath)
        {
            _orbiterController.LooseWithFixedSpeed(cursorWorldPosition, LooseOncePerDeathSpeed);
            _pendingLooseOncePerDeath = false;
        }
        else
        {
            _orbiterController.Loose(cursorWorldPosition);
        }
    }
}
