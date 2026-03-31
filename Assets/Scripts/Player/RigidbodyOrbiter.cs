using System;
using System.Collections.Generic;
using UnityEngine;

public enum ImpulseMode
{
    Cursor,
    Velocity
}

[Serializable]
public class RigidbodyOrbiter
{
    readonly Action _orbitEnter, _orbitExit;
    readonly Transform _transform;
    readonly Rigidbody _rb;

    private readonly List<IOrbitable> _gravitySources = new();
    private IOrbitable _capturedOrbit;
    private IOrbitable _lastReleasedOrbit;
    private float _graceTimer;
    private OrbiterSettings _settings;

    // Trajectory prediction cache
    private readonly Vector3[] _trajectoryPoints = new Vector3[2];

    public RigidbodyOrbiter(Rigidbody rb, Transform transform, Action orbitEnter, Action orbitExit, OrbiterSettings settings)
    {
        _rb = rb;
        _transform = transform;
        _orbitEnter = orbitEnter;
        _orbitExit = orbitExit;
        _settings = settings;

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // ---------------------------------------------------------
    // Lifecycle
    // ---------------------------------------------------------

    /// <param name="thrustInput">Current frame thrust direction. Inertia stabilizer only applies when this is released (zero).</param>
    public void FixedUpdate(Vector2 thrustInput)
    {
        if (_graceTimer > 0f)
        {
            _graceTimer -= Time.fixedDeltaTime;
            if (_graceTimer <= 0f)
                _lastReleasedOrbit = null;
        }

        ApplyGravitySources();

        if (_capturedOrbit != null)
            ApplyMinThrustAssist(_capturedOrbit.Data);
        else if (_settings.InertiaResourceSettings.InertiaStabilizer && thrustInput.sqrMagnitude < 0.0001f)
        {
            ApplyFreeFlightDamping();
        }
    }

    /// <summary>Applies thrust in the given 2D direction using ForceMode.Acceleration (mass-independent). When inertia stabilizer is ON, does not add force in thrust direction if velocity in that direction already exceeds stabilizerMaxThrustSpeed.</summary>
    public void ApplyThrust(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;
        Vector3 worldDirection = new Vector3(direction.x, direction.y, 0f).normalized;
        float magnitude = _settings.ThrusterResourceSettings.ThrustForce;
        if (magnitude < 0.0001f)
            return;

        if (_settings.InertiaResourceSettings.InertiaStabilizer)
        {
            float velocityInThrustDir = Vector3.Dot(_rb.linearVelocity, worldDirection);
            if (velocityInThrustDir >= _settings.InertiaResourceSettings.StabilizerMaxThrustSpeed)
                return;
        }

        _rb.AddForce(worldDirection * magnitude, ForceMode.Acceleration);
    }

    public void OnEnable()
    {
        _capturedOrbit = null;
        _lastReleasedOrbit = null;
        _graceTimer = 0f;

        _gravitySources.Clear();
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    public void OnDisable()
    {
        ReleaseCapturedOrbit();
        _lastReleasedOrbit = null;
        _graceTimer = 0f;
        _gravitySources.Clear();
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    // ---------------------------------------------------------
    // Gravity source tracking (called by Orb via triggers)
    // ---------------------------------------------------------

    public void AddGravitySource(IOrbitable source)
    {
        if (source.Data.type == AstroType.None)
        {
            Debug.LogWarning("Astro type not registered");
            return;
        }

        if (!_gravitySources.Contains(source))
            _gravitySources.Add(source);

        GrabOrbit(source);
    }

    public void RemoveGravitySource(IOrbitable source)
    {
        if (_capturedOrbit == source)
        {
            ReleaseCapturedOrbit();
            _lastReleasedOrbit = null;
            _graceTimer = 0f;
            _gravitySources.Remove(source);
            return;
        }

        _gravitySources.Remove(source);
    }

    // ---------------------------------------------------------
    // Phase 1 — Free-fall gravity from every nearby source
    // ---------------------------------------------------------

    void ApplyGravitySources()
    {
        for (int i = _gravitySources.Count - 1; i >= 0; i--)
        {
            IOrbitable source = _gravitySources[i];

            if (!IsValidSource(source))
            {
                if (_capturedOrbit == source)
                    ReleaseCapturedOrbit();

                _gravitySources.RemoveAt(i);
                continue;
            }

            // Skip gravity from the recently released orbit during grace period
            if (source == _lastReleasedOrbit && _graceTimer > 0f)
                continue;

            ApplyGravity(source.Data);
        }
    }

    void ApplyGravity(OrbitData data)
    {
        Vector3 toCenter = data.transform.position - _transform.position;
        float distance = toCenter.magnitude;

        if (distance < 0.01f)
            return;

        float gravityMagnitude = data.gravity / (distance * distance);
        _rb.AddForce(toCenter.normalized * gravityMagnitude, ForceMode.Acceleration);

        Debug.DrawLine(_transform.position, data.transform.position, Color.grey);
    }

    // ---------------------------------------------------------
    // Captured orbit — gravity + minimum tangential assist to stay in orbit
    // ---------------------------------------------------------

    void ApplyMinThrustAssist(OrbitData data)
    {
        Vector3 relativeVelocity = _rb.linearVelocity - data.velocity;
        Vector3 toCenter = data.transform.position - _transform.position;
        float distance = toCenter.magnitude;

        if (distance < 0.01f)
            return;

        Vector3 centerDir = toCenter.normalized;
        Vector3 tangentialVelocity = relativeVelocity - centerDir * Vector3.Dot(relativeVelocity, centerDir);
        float tangentialSpeed = tangentialVelocity.magnitude;

        Vector3 tangentDir = tangentialSpeed > 0.001f
            ? tangentialVelocity.normalized
            : Vector3.Cross(centerDir, Vector3.forward).normalized;

        float idealSpeed = Mathf.Sqrt(data.gravity / Mathf.Max(distance, 0.01f));
        if (tangentialSpeed >= idealSpeed || _settings.ThrusterResourceSettings.MinThrustAssist < 0.0001f)
            return;

        float deficit = idealSpeed - tangentialSpeed;
        float assist = Mathf.Min(deficit * 2f, _settings.ThrusterResourceSettings.MinThrustAssist);
        _rb.AddForce(tangentDir * assist, ForceMode.Acceleration);

        Debug.DrawLine(_transform.position, data.transform.position, Color.cyan);
    }

    // ---------------------------------------------------------
    // Inertia stabilizer — damp velocity when free (no captured orbit)
    // ---------------------------------------------------------

    void ApplyFreeFlightDamping()
    {
        Vector3 referenceVelocity = GetNearestReferenceVelocity();
        Vector3 relativeVelocity = _rb.linearVelocity - referenceVelocity;

        if (relativeVelocity.sqrMagnitude < 0.01f)
        {
            _rb.linearVelocity = referenceVelocity;
            return;
        }

        float dampTime = Mathf.Max(_settings.InertiaResourceSettings.InertiaDampTime, 0.1f);
        float dampFactor = 1f / dampTime;
        _rb.AddForce(-relativeVelocity * dampFactor, ForceMode.Acceleration);
    }

    Vector3 GetNearestReferenceVelocity()
    {
        if (_gravitySources.Count == 0)
            return Vector3.zero;

        IOrbitable nearest = null;
        float nearestDistSq = float.MaxValue;
        for (int i = 0; i < _gravitySources.Count; i++)
        {
            IOrbitable source = _gravitySources[i];
            if (!IsValidSource(source))
                continue;
            float distSq = (_transform.position - source.Data.transform.position).sqrMagnitude;
            if (distSq < nearestDistSq)
            {
                nearestDistSq = distSq;
                nearest = source;
            }
        }
        return nearest != null ? nearest.Data.velocity : Vector3.zero;
    }

    // ---------------------------------------------------------
    // Grab — orbit captures the orb on trigger enter
    // ---------------------------------------------------------

    void GrabOrbit(IOrbitable orbit)
    {
        if (_capturedOrbit == orbit)
            return;

        if (orbit == _lastReleasedOrbit && _graceTimer > 0f)
            return;

        if (_capturedOrbit != null)
        {
            _capturedOrbit.ExitOrbit();
            _orbitExit?.Invoke();
        }

        _capturedOrbit = orbit;
        orbit.EnterOrbit();

        _orbitEnter?.Invoke();
    }

    // ---------------------------------------------------------
    // Release
    // ---------------------------------------------------------

    void ReleaseCapturedOrbit()
    {
        if (_capturedOrbit == null)
            return;

        _lastReleasedOrbit = _capturedOrbit;
        _graceTimer = 1f;

        _capturedOrbit.ExitOrbit();
        _capturedOrbit = null;

        _orbitExit?.Invoke();
    }

    // ---------------------------------------------------------
    // Public API
    // ---------------------------------------------------------

    public void Impulse(Vector3 cursorWorldPosition)
    {
        float currentSpeed = _rb.linearVelocity.magnitude;

        Vector3 impulseDirection = _settings.ImpulseResourceSettings.ImpulseMode switch
        {
            ImpulseMode.Velocity => _rb.linearVelocity.normalized,
            _ => (cursorWorldPosition - _transform.position).normalized,
        };

        ReleaseCapturedOrbit();

        _rb.linearVelocity = impulseDirection * currentSpeed;
        _rb.AddForce(impulseDirection * _settings.ImpulseResourceSettings.ImpulseForce, ForceMode.VelocityChange);
    }

    public int PredictTrajectory(Vector3 cursorWorldPosition)
    {
        Vector3 origin = _transform.position;
        Vector3 direction = cursorWorldPosition - origin;
        float lineLength = direction.magnitude;

        _trajectoryPoints[0] = origin;
        _trajectoryPoints[1] = cursorWorldPosition;

        if (lineLength < 0.001f)
            return 1;

        // Check if the line segment intersects any non-attached orbit
        Vector3 normalized = direction / lineLength;

        for (int i = 0; i < _gravitySources.Count; i++)
        {
            IOrbitable source = _gravitySources[i];
            if (source == _capturedOrbit || !IsValidSource(source))
                continue;

            OrbitData data = source.Data;
            Vector3 toCenter = data.transform.position - origin;
            float projection = Vector3.Dot(toCenter, normalized);

            if (projection < 0f || projection > lineLength)
                continue;

            Vector3 closestPoint = origin + normalized * projection;
            float distanceToAxis = Vector3.Distance(closestPoint, data.transform.position);

            if (distanceToAxis > data.radius)
                continue;

            // Line enters this orbit — find the entry point along the segment
            float offset = Mathf.Sqrt(data.radius * data.radius - distanceToAxis * distanceToAxis);
            float entryDistance = projection - offset;

            if (entryDistance > 0f && entryDistance < lineLength)
            {
                Vector3 entryPoint = origin + normalized * entryDistance;

                // Keep the closest intersection
                if (Vector3.Distance(origin, entryPoint) < Vector3.Distance(origin, _trajectoryPoints[1]))
                    _trajectoryPoints[1] = entryPoint;
            }
        }

        return 2;
    }

    public Vector3[] TrajectoryPoints => _trajectoryPoints;
    public float Speed => _rb.linearVelocity.magnitude;

    public void UpdateSettings(OrbiterSettings settings)
    {
        _settings = settings;
    }

    // ---------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------

    static bool IsValidSource(IOrbitable source)
    {
        if (source is UnityEngine.Object obj && !obj)
            return false;

        Transform t = source.Data.transform;
        return t && t.parent && t.parent.gameObject.activeSelf;
    }
}

[Serializable]
public struct OrbiterSettings
{
    public ThrusterResourceSettings ThrusterResourceSettings;
    public ImpulseResourceSettings ImpulseResourceSettings;
    public InertiaResourceSettings InertiaResourceSettings;

    public static OrbiterSettings Default => new()
    {
        ThrusterResourceSettings = ThrusterResourceSettings.Default,
        ImpulseResourceSettings = ImpulseResourceSettings.Default,
        InertiaResourceSettings = InertiaResourceSettings.Default,
    };
}
