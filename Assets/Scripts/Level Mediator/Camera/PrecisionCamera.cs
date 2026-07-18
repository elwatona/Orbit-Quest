using UnityEngine;
using Unity.Cinemachine;

public class PrecisionCamera : Camera
{
    private CinemachineOrbitalFollow _follow;
    public PrecisionCamera(CinemachineCamera camera) : base(camera)
    {
        _follow = _cameraTransform.GetComponent<CinemachineOrbitalFollow>();
        _zoom = new Values(10);
        _rotation = new Values(10);
    }
    public override void Zoom(float delta)
    {
        _zoom.Set(Mathf.Clamp(_zoom.value + delta, 0, _zoom.maxStep));
    
        _follow.VerticalAxis.Value = Mathf.Lerp(_follow.VerticalAxis.Range.x, _follow.VerticalAxis.Range.y, _zoom.lerp);
    }
    public override void Rotate(float delta)
    {
        Debug.Log("Rotate: " + delta);
        _rotation.Set(delta);

        _follow.HorizontalAxis.Value = Mathf.Lerp(_follow.HorizontalAxis.Range.x, _follow.HorizontalAxis.Range.y, _rotation.lerp);
    }
}