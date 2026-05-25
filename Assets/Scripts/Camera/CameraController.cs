using UnityEngine;
using Unity.Cinemachine;

public abstract class CameraController
{
    protected readonly CinemachineCamera _camera;
    protected readonly Transform _cameraTransform;
    
    protected float _zoomStep, _zoomMaxStep = 10, _zoomLerp;
    protected float _rotateStep, _rotateMaxStep = 10, _rotateLerp;

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
}