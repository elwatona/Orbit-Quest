using UnityEngine;
using Unity.Cinemachine;

public class CenitalCamera : CameraController
{
    private CinemachineOrbitalFollow _follow;
    public CenitalCamera(CinemachineCamera camera) : base(camera)
    {
        _follow = _cameraTransform.GetComponent<CinemachineOrbitalFollow>();
    }
    public override void Zoom(float delta)
    {
        _zoomStep = Mathf.Clamp(_zoomStep += delta, 0, _zoomMaxStep);
        _zoomLerp = _zoomStep / _zoomMaxStep;
    
        _follow.VerticalAxis.Value = Mathf.Lerp(_follow.VerticalAxis.Range.x, _follow.VerticalAxis.Range.y, _zoomLerp);
        _follow.RadialAxis.Value = Mathf.Lerp(_follow.RadialAxis.Range.x, _follow.RadialAxis.Range.y, _zoomLerp);
    }
    public override void Rotate(float delta)
    {
        _rotateStep = Mathf.Clamp(_rotateStep += delta, 0, _rotateMaxStep);
        _rotateLerp = _rotateStep / _rotateMaxStep;
    }
}