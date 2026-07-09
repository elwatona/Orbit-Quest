using UnityEngine;
using Unity.Cinemachine;

public abstract class CameraController
{
    protected class Values
    {
        public Values(float maxStep)
        {
            this.maxStep = maxStep;
        }
        public void Set(float value)
        {
            this.value = Mathf.Clamp(this.value + value, 0, maxStep);
        }
        public float maxStep { get; }
        public float value { get; private set; }
        public float lerp { get => value / maxStep; }
    }
    protected Values _zoom;
    protected Values _rotation;
    protected readonly CinemachineCamera _camera;
    protected readonly Transform _cameraTransform;

    public CameraController(CinemachineCamera camera)
    {
        _camera = camera;
        _cameraTransform = camera.transform;
    }
    public virtual void SetActive(bool active)
    {
        _camera.enabled = active;
    }
    public abstract void Zoom(float delta);
    public abstract void Rotate(float delta);

    public virtual void Update() {}
    public virtual void LateUpdate() {}
}