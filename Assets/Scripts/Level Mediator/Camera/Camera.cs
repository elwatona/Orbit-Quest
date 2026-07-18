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

public abstract class Camera
{
    protected CameraAxisValues _zoom;
    protected CameraAxisValues _rotation;
    protected readonly SharedCameraZoom _sharedZoom;
    protected readonly CinemachineCamera _camera;
    protected readonly Transform _cameraTransform;

    public Camera(CinemachineCamera camera, SharedCameraZoom sharedZoom)
    {
        _camera = camera;
        _cameraTransform = camera.transform;
        _sharedZoom = sharedZoom;
        _zoom = sharedZoom.Zoom;
        _rotation = new CameraAxisValues(10f);
    }

    public virtual void SetActive(bool active)
    {
        _camera.enabled = active;
        if (active)
            ApplySharedZoom();
    }

    protected virtual void ApplySharedZoom()
    {
        _camera.Lens.OrthographicSize = _sharedZoom.OrthographicSize;
    }

    public abstract void Zoom(float delta);
    public abstract void Rotate(float delta);

    public virtual void Update() {}
    public virtual void LateUpdate() {}
}
