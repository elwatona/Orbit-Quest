using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;

public class OrbitalCamera : Camera
{
    const string LookOrbitX = "Look Orbit X";
    const string LookOrbitY = "Look Orbit Y";

    readonly CinemachineOrbitalFollow _follow;
    readonly CinemachineInputAxisController _axisController;
    readonly Cinemachine3OrbitRig.Settings _baseOrbits;

    public OrbitalCamera(CinemachineCamera camera, SharedCameraZoom sharedZoom, SharedCameraOrbit sharedOrbit)
        : base(camera, sharedZoom, sharedOrbit)
    {
        _follow = _cameraTransform.GetComponent<CinemachineOrbitalFollow>();
        _axisController = _cameraTransform.GetComponent<CinemachineInputAxisController>();
        if (_follow != null)
        {
            var tracker = _follow.TrackerSettings;
            tracker.BindingMode = BindingMode.WorldSpace;
            tracker.QuaternionDamping = 0f;
            tracker.RotationDamping = Vector3.zero;
            _follow.TrackerSettings = tracker;
            _baseOrbits = _follow.Orbits;
        }
        SetLookEnabled(false);
        EnableAssignedLookActions();
    }

    void EnableAssignedLookActions()
    {
        if (_axisController == null) return;

        foreach (var controller in _axisController.Controllers)
        {
            if (controller.Name != LookOrbitX && controller.Name != LookOrbitY)
                continue;
            controller.Input.InputAction?.action?.Enable();
        }
    }

    public override void SetLookEnabled(bool enabled)
    {
        if (_axisController == null) return;

        foreach (var controller in _axisController.Controllers)
        {
            if (controller.Name == LookOrbitX || controller.Name == LookOrbitY)
                controller.Enabled = enabled;
        }
    }

    public override void Zoom(float delta)
    {
        float nextOrtho = Mathf.Clamp(
            _sharedZoom.OrthographicSize + delta * 5f,
            _sharedZoom.Limits.x,
            _sharedZoom.Limits.y);
        _sharedZoom.OrthographicSize = nextOrtho;
        _camera.Lens.OrthographicSize = nextOrtho;
        ApplyOrbitScale();
    }

    protected override void ApplySharedZoom()
    {
        base.ApplySharedZoom();
        ApplyOrbitScale();
    }

    void ApplyOrbitScale()
    {
        if (_follow == null) return;

        float scale = _sharedZoom.OrthographicSize / _sharedZoom.Limits.x;
        var orbits = _baseOrbits;
        orbits.Top.Height = _baseOrbits.Top.Height * scale;
        orbits.Top.Radius = _baseOrbits.Top.Radius * scale;
        orbits.Center.Height = _baseOrbits.Center.Height * scale;
        orbits.Center.Radius = _baseOrbits.Center.Radius * scale;
        orbits.Bottom.Height = _baseOrbits.Bottom.Height * scale;
        orbits.Bottom.Radius = _baseOrbits.Bottom.Radius * scale;
        _follow.Orbits = orbits;
    }

    protected override void CommitOrbit()
    {
        if (_follow == null) return;
        Quaternion orientation = _camera.State.GetFinalOrientation();
        float worldYaw = orientation.eulerAngles.y;
        if (worldYaw > 180f) worldYaw -= 360f;
        _sharedOrbit.Set(worldYaw, _follow.VerticalAxis.Value, orientation);
    }

    protected override void ApplySharedOrbit()
    {
        if (!_sharedOrbit.HasValue || _follow == null) return;

        _follow.HorizontalAxis.Value = _sharedOrbit.Horizontal;
        _follow.VerticalAxis.Value = _sharedOrbit.Vertical;
        _camera.ForceCameraPosition(_cameraTransform.position, _sharedOrbit.Rotation);
    }
}
