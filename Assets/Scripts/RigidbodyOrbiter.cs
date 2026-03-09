using System;
using System.Collections.Generic;
using UnityEngine;

public enum EscapeMode
{
    Cursor,
    Velocity
}

public enum MovementMode
{
    Auto,
    Manual
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

    // Detach spin tracking
    private bool _isDetaching;
    private float _accumulatedAngle;
    private float _previousAngle;

    // Settle-in: OrbiterSettings derived from orbit, blended in over 1s
    private float _orbitEnterTime;
    private float _captureDistance;
    private float _captureTangentialSpeed;
    private OrbiterSettings _effectiveSettingsAtCapture;
    private OrbiterSettings _effectiveSettingsTarget;
    private OrbiterSettings _effectiveSettings;

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
        {
            if (_settings.movementMode == MovementMode.Auto)
                ApplyStabilization(_capturedOrbit.Data);
            else
                ApplyMinThrustAssist(_capturedOrbit.Data);

            if (_isDetaching)
                UpdateDetach(_capturedOrbit.Data);
        }
        else if (_settings.inertiaStabilizer && thrustInput.sqrMagnitude < 0.0001f)
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
        float magnitude = _settings.thrustForce;
        if (magnitude < 0.0001f)
            return;

        if (_settings.inertiaStabilizer)
        {
            float velocityInThrustDir = Vector3.Dot(_rb.linearVelocity, worldDirection);
            if (velocityInThrustDir >= _settings.stabilizerMaxThrustSpeed)
                return;
        }

        _rb.AddForce(worldDirection * magnitude, ForceMode.Acceleration);
    }

    public void OnEnable()
    {
        _capturedOrbit = null;
        _lastReleasedOrbit = null;
        _graceTimer = 0f;

        _isDetaching = false;
        _accumulatedAngle = 0f;
        _previousAngle = 0f;
        _orbitEnterTime = 0f;
        _captureDistance = 0f;
        _captureTangentialSpeed = 0f;
        _effectiveSettings = _settings;

        _gravitySources.Clear();
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    public void OnDisable()
    {
        _isDetaching = false;
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

        // Re-entering the captured orbit's zone cancels detach
        if (source == _capturedOrbit && _isDetaching)
        {
            _isDetaching = false;
            return;
        }

        GrabOrbit(source);
    }

    public void RemoveGravitySource(IOrbitable source)
    {
        if (_capturedOrbit == source)
        {
            if (_settings.movementMode == MovementMode.Manual)
            {
                _isDetaching = false;
                ReleaseCapturedOrbit();
                _lastReleasedOrbit = null;
                _graceTimer = 0f;
                _gravitySources.Remove(source);
            }
            else
            {
                if (!_isDetaching)
                    BeginDetach();
            }
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
                {
                    _isDetaching = false;
                    ReleaseCapturedOrbit();
                }

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
    // Phase 2 — Stabilization forces (only on the captured orbit)
    // ---------------------------------------------------------

    void ApplyStabilization(OrbitData data)
    {
        Vector3 relativeVelocity = _rb.linearVelocity - data.velocity;
        Vector3 toCenter = data.transform.position - _transform.position;
        float distance = toCenter.magnitude;

        if (distance < 0.01f)
            return;

        Vector3 centerDir = toCenter.normalized;

        // Single gradual ramp over 1s (smooth curve: gentle at start, full at end)
        float rawT = (Time.fixedTime - _orbitEnterTime) / 1f;
        float settleT = Mathf.Clamp01(rawT);
        float settleStrength = Mathf.SmoothStep(0f, 1f, settleT);

        _effectiveSettings = LerpOrbiterSettingsForSettle(_effectiveSettingsAtCapture, _effectiveSettingsTarget, settleStrength);

        // Decompose velocity into radial and tangential components
        Vector3 radialVelocity = Vector3.Project(relativeVelocity, centerDir);
        Vector3 tangentialVelocity = relativeVelocity - radialVelocity;
        float tangentialSpeed = tangentialVelocity.magnitude;

        Vector3 tangentDir = tangentialSpeed > 0.001f
            ? tangentialVelocity.normalized
            : Vector3.Cross(centerDir, Vector3.forward).normalized;

        // Target radius: from capture distance to orbit radius over 1s (same smooth ramp)
        float targetRadius = Mathf.Lerp(_captureDistance, data.radius, settleStrength);

        // -------------------
        // RADIUS CORRECTION (gradual within 1s)
        // -------------------
        float radiusError = distance - targetRadius;
        float correctionForce = radiusError * _effectiveSettings.radiusCorrection * settleStrength;
        _rb.AddForce(centerDir * correctionForce, ForceMode.Acceleration);

        // -------------------
        // TANGENTIAL MAINTENANCE (gradual within 1s)
        // -------------------
        float speedScale = _effectiveSettings.orbitSpeedScale > 0.01f ? _effectiveSettings.orbitSpeedScale : 2f;
        float idealSpeedAtTargetRadius = Mathf.Sqrt(data.gravity / Mathf.Max(targetRadius, 0.1f)) * speedScale;
        float targetSpeed = Mathf.Lerp(_captureTangentialSpeed, Mathf.Min(idealSpeedAtTargetRadius, _effectiveSettings.maxSpeed), settleStrength);
        float speedError = targetSpeed - tangentialSpeed;
        float tangentialAssist = speedError * data.tangentialForce * _effectiveSettings.stabilization * settleStrength;
        if (speedError < 0f && settleT < 1f)
            tangentialAssist *= 0.25f;
        _rb.AddForce(tangentDir * tangentialAssist, ForceMode.Acceleration);

        // -------------------
        // SPEED LIMITING (gradual within 1s)
        // -------------------
        if (tangentialSpeed > _effectiveSettings.maxSpeed)
        {
            float excess = tangentialSpeed - _effectiveSettings.maxSpeed;
            float limitStrength = settleT < 1f ? Mathf.Lerp(0.2f, 1f, settleStrength) : 1f;
            _rb.AddForce(-tangentDir * excess * _effectiveSettings.speedDamping * limitStrength, ForceMode.Acceleration);
        }

        // -------------------
        // RADIAL DAMPING (gradual within 1s)
        // -------------------
        float normalizedDeviation = Mathf.Abs(radiusError) / Mathf.Max(targetRadius, 0.1f);
        float dampingStrength = data.radialDamping * (1f + normalizedDeviation * _effectiveSettings.stabilization * 2f) * settleStrength;
        _rb.AddForce(-radialVelocity * dampingStrength, ForceMode.Acceleration);

        Debug.DrawLine(_transform.position, data.transform.position, Color.yellow);
    }

    // ---------------------------------------------------------
    // Manual mode — gravity only + minimum tangential assist to stay in orbit
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
        if (tangentialSpeed >= idealSpeed || _settings.minThrustAssist < 0.0001f)
            return;

        float deficit = idealSpeed - tangentialSpeed;
        float assist = Mathf.Min(deficit * 2f, _settings.minThrustAssist);
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

        float dampTime = Mathf.Max(_settings.inertiaDampTime, 0.1f);
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
            _isDetaching = false;
            _capturedOrbit.ExitOrbit();
            _orbitExit?.Invoke();
        }

        _capturedOrbit = orbit;
        _orbitEnterTime = Time.fixedTime;
        orbit.EnterOrbit();

        _orbitEnter?.Invoke();

        OrbitData data = orbit.Data;

        // OrbiterSettings from orbit only (escape/detach from _settings), applied gradually over 1s
        _effectiveSettingsTarget = ComputeOrbiterSettingsFromOrbit(data, _settings);
        _effectiveSettingsAtCapture = SoftenForCapture(_effectiveSettingsTarget);

        Vector3 toCenter = data.transform.position - _transform.position;
        float distance = toCenter.magnitude;

        if (distance < 0.01f)
            return;

        Vector3 centerDir = toCenter.normalized;
        Vector3 relativeVel = _rb.linearVelocity - data.velocity;
        Vector3 tangentialVelocity = relativeVel - centerDir * Vector3.Dot(relativeVel, centerDir);

        _captureDistance = distance;
        _captureTangentialSpeed = tangentialVelocity.magnitude;

        float speedScale = _effectiveSettingsTarget.orbitSpeedScale;
        if (_captureTangentialSpeed < 0.1f)
            _captureTangentialSpeed = Mathf.Min(Mathf.Sqrt(data.gravity / Mathf.Max(distance, 0.1f)) * speedScale, _effectiveSettingsTarget.maxSpeed);

        Vector3 tangentDir = Vector3.Cross(centerDir, Vector3.forward).normalized;
        if (tangentialVelocity.sqrMagnitude > 0.001f && Vector3.Dot(tangentialVelocity.normalized, tangentDir) < 0f)
            tangentDir = -tangentDir;

        if (_settings.movementMode == MovementMode.Auto)
        {
            float effectiveDistance = Mathf.Lerp(distance, data.radius, _effectiveSettingsTarget.stabilization);
            float orbitalSpeed = Mathf.Min(Mathf.Sqrt(data.gravity / effectiveDistance) * speedScale, _effectiveSettingsTarget.maxSpeed);

            Vector3 idealVelocity = tangentDir * orbitalSpeed + data.velocity;
            float currentSpeed = _rb.linearVelocity.magnitude;
            float idealSpeed = idealVelocity.magnitude;
            Vector3 blended = Vector3.Lerp(_rb.linearVelocity, idealVelocity, 0.2f);
            float blendedSpeed = blended.magnitude;
            float minSpeed = Mathf.Lerp(currentSpeed, idealSpeed, 0.15f);
            if (blendedSpeed < minSpeed && blendedSpeed > 0.001f)
                blended = blended.normalized * Mathf.Min(minSpeed, _effectiveSettingsTarget.maxSpeed);
            _rb.linearVelocity = blended;
        }
    }

    // ---------------------------------------------------------
    // Detach — spin countdown before releasing from orbit
    // ---------------------------------------------------------

    void BeginDetach()
    {
        _isDetaching = true;
        _accumulatedAngle = 0f;

        Vector3 toOrb = _transform.position - _capturedOrbit.Data.transform.position;
        _previousAngle = Mathf.Atan2(toOrb.y, toOrb.x);
    }

    void UpdateDetach(OrbitData data)
    {
        Vector3 toOrb = _transform.position - data.transform.position;
        float currentAngle = Mathf.Atan2(toOrb.y, toOrb.x);

        float delta = currentAngle - _previousAngle;

        // Wrap to [-PI, PI] to handle the ±180° boundary
        if (delta > Mathf.PI) delta -= 2f * Mathf.PI;
        if (delta < -Mathf.PI) delta += 2f * Mathf.PI;

        _accumulatedAngle += Mathf.Abs(delta);
        _previousAngle = currentAngle;

        if (_accumulatedAngle >= _settings.detachSpins * Mathf.PI * 2f)
            CompleteDetach();
    }

    void CompleteDetach()
    {
        _isDetaching = false;

        IOrbitable detachedOrbit = _capturedOrbit;
        ReleaseCapturedOrbit();

        _gravitySources.Remove(detachedOrbit);
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
        _isDetaching = false;

        _orbitExit?.Invoke();
    }

    // ---------------------------------------------------------
    // Public API
    // ---------------------------------------------------------

    public void Loose(Vector3 cursorWorldPosition)
    {
        if (_capturedOrbit == null)
            return;

        float currentSpeed = _rb.linearVelocity.magnitude;

        Vector3 escapeDirection = _settings.escapeMode switch
        {
            EscapeMode.Velocity => _rb.linearVelocity.normalized,
            _ => (cursorWorldPosition - _transform.position).normalized,
        };

        _isDetaching = false;
        ReleaseCapturedOrbit();

        _rb.linearVelocity = escapeDirection * currentSpeed;
        _rb.AddForce(escapeDirection * _settings.escapeForce, ForceMode.VelocityChange);
    }

    /// <summary>Applies a fixed speed toward cursor (e.g. first loose after respawn, when velocity is 0 and there is no orbit).</summary>
    public void LooseWithFixedSpeed(Vector3 cursorWorldPosition, float speed)
    {
        Vector3 toCursor = cursorWorldPosition - _transform.position;
        Vector3 direction = toCursor.sqrMagnitude > 0.0001f ? toCursor.normalized : Vector3.right;

        if (_capturedOrbit != null)
        {
            _isDetaching = false;
            ReleaseCapturedOrbit();
        }

        _rb.linearVelocity = direction * speed;
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
    public EscapeMode EscapeMode => _settings.escapeMode;

    public void UpdateSettings(OrbiterSettings settings)
    {
        _settings = settings;
    }

    // ---------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------

    /// <summary>Orbit-related values are derived only from the orbit; baseSettings is used only for escape and detach.</summary>
    static OrbiterSettings ComputeOrbiterSettingsFromOrbit(OrbitData data, OrbiterSettings baseSettings)
    {
        float orbitalSpeed = Mathf.Sqrt(data.gravity / Mathf.Max(data.radius, 0.1f));

        return new OrbiterSettings
        {
            movementMode = baseSettings.movementMode,
            maxSpeed = Mathf.Clamp(orbitalSpeed * 2f, 5f, 20f),
            orbitSpeedScale = 2f,
            stabilization = Mathf.Clamp(0.2f + (data.tangentialForce - 2f) / 3f * 0.6f, 0f, 1f),
            radiusCorrection = Mathf.Clamp(2f + data.gravity / 15f, 0f, 10f),
            speedDamping = Mathf.Clamp(0.5f + data.radialDamping, 0f, 5f),
            thrustForce = baseSettings.thrustForce,
            minThrustAssist = baseSettings.minThrustAssist,
            escapeMode = baseSettings.escapeMode,
            escapeForce = baseSettings.escapeForce,
            detachSpins = baseSettings.detachSpins,
            inertiaStabilizer = baseSettings.inertiaStabilizer,
            inertiaDampTime = baseSettings.inertiaDampTime,
            stabilizerMaxThrustSpeed = baseSettings.stabilizerMaxThrustSpeed,
        };
    }

    static OrbiterSettings SoftenForCapture(OrbiterSettings target)
    {
        var soft = target;
        soft.radiusCorrection = target.radiusCorrection * 0.2f;
        soft.stabilization = target.stabilization * 0.2f;
        soft.speedDamping = target.speedDamping * 0.5f;
        return soft;
    }

    static OrbiterSettings LerpOrbiterSettingsForSettle(OrbiterSettings from, OrbiterSettings to, float t)
    {
        return new OrbiterSettings
        {
            movementMode = to.movementMode,
            maxSpeed = Mathf.Lerp(from.maxSpeed, to.maxSpeed, t),
            orbitSpeedScale = Mathf.Lerp(from.orbitSpeedScale, to.orbitSpeedScale, t),
            stabilization = Mathf.Lerp(from.stabilization, to.stabilization, t),
            radiusCorrection = Mathf.Lerp(from.radiusCorrection, to.radiusCorrection, t),
            speedDamping = Mathf.Lerp(from.speedDamping, to.speedDamping, t),
            thrustForce = Mathf.Lerp(from.thrustForce, to.thrustForce, t),
            minThrustAssist = Mathf.Lerp(from.minThrustAssist, to.minThrustAssist, t),
            escapeMode = to.escapeMode,
            escapeForce = to.escapeForce,
            detachSpins = to.detachSpins,
            inertiaStabilizer = to.inertiaStabilizer,
            inertiaDampTime = Mathf.Lerp(from.inertiaDampTime, to.inertiaDampTime, t),
            stabilizerMaxThrustSpeed = Mathf.Lerp(from.stabilizerMaxThrustSpeed, to.stabilizerMaxThrustSpeed, t),
        };
    }

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
    [Header("Movement")]
    [Tooltip("Auto: full orbit stabilization. Manual: free direction from orbit impulses + boost toward cursor.")]
    public MovementMode movementMode;
    [Range(5f, 20f)] public float maxSpeed;
    [Range(0.5f, 3f)] public float orbitSpeedScale;
    [Range(0f, 1f)] public float stabilization;
    [Range(0f, 10f)] public float radiusCorrection;
    [Range(0f, 5f)] public float speedDamping;

    [Header("Thrust")]
    [Range(0f, 20f)] public float thrustForce;
    [Tooltip("Manual mode only: max tangential acceleration to maintain orbit when below orbital speed.")]
    [Range(0f, 10f)] public float minThrustAssist;

    [Header("Escape")]
    public EscapeMode escapeMode;
    [Range(0f, 30f)] public float escapeForce;

    [Header("Detach")]
    [Range(1, 5)] public int detachSpins;

    [Header("Inertia")]
    [Tooltip("When ON, dampens velocity to reference (nearest orbit or world) over inertiaDampTime seconds when not captured.")]
    public bool inertiaStabilizer;
    [Tooltip("Time in seconds to bring relative velocity to zero when stabilizer is ON and free.")]
    [Range(0.5f, 5f)] public float inertiaDampTime;
    [Tooltip("When stabilizer is ON: no thrust is applied in the thrust direction if velocity in that direction already exceeds this value.")]
    [Range(1f, 25f)] public float stabilizerMaxThrustSpeed;

    public static OrbiterSettings Default => new()
    {
        movementMode = MovementMode.Auto,
        maxSpeed = 14f,
        orbitSpeedScale = 2f,
        stabilization = 0.4f,
        radiusCorrection = 6f,
        speedDamping = 2f,
        thrustForce = 5f,
        minThrustAssist = 2f,
        escapeForce = 5f,
        detachSpins = 1,
        inertiaStabilizer = true,
        inertiaDampTime = 2f,
        stabilizerMaxThrustSpeed = 10f,
    };
}
