using UnityEngine;
using Unity.Cinemachine;

public class CameraAxisValues
{
    public CameraAxisValues(float maxStep)
    {
        this.maxStep = maxStep;
    }

    public void Set(float delta)
    {
        value = Mathf.Clamp(value + delta, 0, maxStep);
    }

    public void SetAbsolute(float absolute)
    {
        value = Mathf.Clamp(absolute, 0, maxStep);
    }

    public float maxStep { get; }
    public float value { get; private set; }
    public float lerp => maxStep > 0f ? value / maxStep : 0f;
}

public class SharedCameraZoom
{
    public SharedCameraZoom(float maxStep = 10f, float initialOrtho = 10f)
    {
        Zoom = new CameraAxisValues(maxStep);
        OrthographicSize = initialOrtho;
        Limits = new Vector2(10f, 100f);
    }

    public CameraAxisValues Zoom { get; }
    public float OrthographicSize { get; set; }
    public Vector2 Limits { get; }
}

public class SharedCameraOrbit
{
    public bool HasValue { get; private set; }
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public Quaternion Rotation { get; private set; } = Quaternion.identity;

    public void Set(float horizontal, float vertical, Quaternion rotation)
    {
        Horizontal = horizontal;
        Vertical = vertical;
        Rotation = rotation;
        HasValue = true;
    }
}

public abstract class Camera
{
    protected CameraAxisValues _zoom;
    protected readonly SharedCameraZoom _sharedZoom;
    protected readonly SharedCameraOrbit _sharedOrbit;
    protected readonly CinemachineCamera _camera;
    protected readonly Transform _cameraTransform;

    public Camera(CinemachineCamera camera, SharedCameraZoom sharedZoom, SharedCameraOrbit sharedOrbit)
    {
        _camera = camera;
        _cameraTransform = camera.transform;
        _sharedZoom = sharedZoom;
        _sharedOrbit = sharedOrbit;
        _zoom = sharedZoom.Zoom;
    }

    public virtual void SetActive(bool active, bool commitOrbit = true)
    {
        if (!active && _camera.enabled && commitOrbit)
            CommitOrbit();

        if (!active)
            SetLookEnabled(false);

        _camera.enabled = active;

        if (active)
        {
            ApplySharedOrbit();
            ApplySharedZoom();
        }
    }

    protected virtual void ApplySharedZoom()
    {
        _camera.Lens.OrthographicSize = _sharedZoom.OrthographicSize;
    }

    protected virtual void CommitOrbit() { }

    protected virtual void ApplySharedOrbit() { }

    public abstract void Zoom(float delta);
    public virtual void SetLookEnabled(bool enabled) { }
}
