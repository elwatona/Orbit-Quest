using UnityEngine;
using Unity.Cinemachine;

public class IsometricCamera : CameraController
{
    private Vector2 _zoomLimits;
    public IsometricCamera(CinemachineCamera camera) : base(camera)
    {
        _zoomLimits = new Vector2(10, 20);
    }

    public override void Zoom(float delta)
    {
        _zoomStep = Mathf.Clamp(_zoomStep += delta, 0, _zoomMaxStep);
        _zoomLerp = _zoomStep / _zoomMaxStep;
    
        _camera.Lens.OrthographicSize = Mathf.Lerp(_camera.Lens.OrthographicSize, _camera.Lens.OrthographicSize + delta, _zoomLerp);
        _camera.Lens.OrthographicSize = Mathf.Clamp(_camera.Lens.OrthographicSize, _zoomLimits.x, _zoomLimits.y);
    }
    public override void Rotate(float delta)
    {
        _rotateStep = Mathf.Clamp(_rotateStep += delta, 0, _rotateMaxStep);
        _rotateLerp = _rotateStep / _rotateMaxStep;
    }
}