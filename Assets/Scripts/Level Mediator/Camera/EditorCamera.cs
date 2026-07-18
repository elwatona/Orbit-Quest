using UnityEngine;
using Unity.Cinemachine;

public class EditorCamera : Camera
{
    public EditorCamera(CinemachineCamera camera, SharedCameraZoom sharedZoom) : base(camera, sharedZoom)
    {
    }

    public override void Zoom(float delta)
    {
        _zoom.Set(delta);

        float nextOrtho = Mathf.Clamp(
            _sharedZoom.OrthographicSize + delta * 5f,
            _sharedZoom.Limits.x,
            _sharedZoom.Limits.y);

        _sharedZoom.OrthographicSize = nextOrtho;
        _camera.Lens.OrthographicSize = nextOrtho;
    }

    public override void Rotate(float delta)
    {
        _cameraTransform.RotateAround(_cameraTransform.position, Vector3.up, delta);
    }
}
