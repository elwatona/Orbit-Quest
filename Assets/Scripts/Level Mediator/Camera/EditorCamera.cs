using UnityEngine;
using Unity.Cinemachine;

public class EditorCamera : Camera
{
    readonly Vector2 _zoomLimits = new Vector2(10, 100);
    public EditorCamera(CinemachineCamera camera) : base(camera)
    {
        _zoom = new Values(10);
        _rotation = new Values(10);
    }

    public override void Zoom(float delta)
    {
        _zoom.Set(Mathf.Clamp(_zoom.value + delta, 0, _zoom.maxStep));
    
        _camera.Lens.OrthographicSize = Mathf.Lerp(_camera.Lens.OrthographicSize, _camera.Lens.OrthographicSize + delta * 5f, _zoom.lerp);
        _camera.Lens.OrthographicSize = Mathf.Clamp(_camera.Lens.OrthographicSize, _zoomLimits.x, _zoomLimits.y);
    }
    public override void Rotate(float delta)
    {
        _cameraTransform.RotateAround(_cameraTransform.position, Vector3.up, delta);
    }
}