using UnityEngine;
using Unity.Cinemachine;

public class PrecisionCamera : Camera
{
    private CinemachineOrbitalFollow _follow;

    public PrecisionCamera(CinemachineCamera camera, SharedCameraZoom sharedZoom) : base(camera, sharedZoom)
    {
        _follow = _cameraTransform.GetComponent<CinemachineOrbitalFollow>();
    }

    public override void Zoom(float delta)
    {
        const float epsilon = 0.05f;
        float ortho = _sharedZoom.OrthographicSize;
        bool orthoAtMin = ortho <= _sharedZoom.Limits.x + epsilon;
        bool vertAtMax = _follow.VerticalAxis.Value >= _follow.VerticalAxis.Range.y - epsilon;
        bool orthoPhase = !orthoAtMin || (vertAtMax && delta > 0f);

        if (orthoPhase)
        {
            float remaining = _zoom.maxStep - _zoom.value;
            if (remaining > 0f)
                _zoom.Set(remaining);

            _follow.VerticalAxis.Value = _follow.VerticalAxis.Range.y;
            float nextOrtho = Mathf.Clamp(ortho + delta * 5f, _sharedZoom.Limits.x, _sharedZoom.Limits.y);
            _sharedZoom.OrthographicSize = nextOrtho;
            _camera.Lens.OrthographicSize = nextOrtho;
        }
        else
        {
            if (orthoAtMin)
            {
                _sharedZoom.OrthographicSize = _sharedZoom.Limits.x;
                _camera.Lens.OrthographicSize = _sharedZoom.Limits.x;
            }

            _zoom.Set(delta);
            _follow.VerticalAxis.Value = Mathf.Lerp(
                _follow.VerticalAxis.Range.x,
                _follow.VerticalAxis.Range.y,
                _zoom.lerp);
        }
    }

    public override void Rotate(float delta)
    {
        _rotation.Set(delta);

        _follow.HorizontalAxis.Value = Mathf.Lerp(
            _follow.HorizontalAxis.Range.x,
            _follow.HorizontalAxis.Range.y,
            _rotation.lerp);
    }

    protected override void ApplySharedZoom()
    {
        base.ApplySharedZoom();

        const float epsilon = 0.05f;
        bool orthoAtMin = _sharedZoom.OrthographicSize <= _sharedZoom.Limits.x + epsilon;

        if (orthoAtMin)
        {
            _follow.VerticalAxis.Value = Mathf.Lerp(
                _follow.VerticalAxis.Range.x,
                _follow.VerticalAxis.Range.y,
                _zoom.lerp);
        }
        else
        {
            _follow.VerticalAxis.Value = _follow.VerticalAxis.Range.y;
        }
    }
}
