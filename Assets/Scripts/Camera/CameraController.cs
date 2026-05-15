using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    public enum CameraType
    {
        Isometric,
        Cenital
    }

    [SerializeField] Transform _worldUp;
    [SerializeField] CinemachineCamera _isometricCamera;
    [SerializeField] CinemachineCamera _cenitalCamera;
    [SerializeField] CinemachineOrbitalFollow _cenitalFollow;
    [SerializeField] CinemachineBrain _brain;
    [SerializeField] CameraType _currentCameraType;
    [SerializeField] Vector2 _isometricZoomLimits;

    private float _zoomStep, _zoomMaxStep = 10, _zoomLerp;
    private float _rotateStep, _rotateMaxStep = 10, _rotateLerp;
    void OnValidate() => UpdateCameras(_currentCameraType);
    void Start() => UpdateCameras(_currentCameraType);
    public void ToggleCameraType()
    {
        _currentCameraType = _currentCameraType == CameraType.Isometric ? CameraType.Cenital : CameraType.Isometric;
        UpdateCameras(_currentCameraType);
    }
    public void Zoom(float delta)
    {
        switch(_currentCameraType)
        {
            case CameraType.Isometric:
                UpdateIsometricZoom(delta);
                break;
            case CameraType.Cenital:
                UpdateCenitalZoom(delta);
                break;
        }
    }
    private void UpdateCameras(CameraType cameraType)
    {
        _isometricCamera.enabled = cameraType == CameraType.Isometric;
        _cenitalCamera.enabled = cameraType == CameraType.Cenital;

        _brain.WorldUpOverride = _worldUp;
    }
    private void UpdateIsometricZoom(float delta)
    {
        _isometricCamera.Lens.OrthographicSize += delta;
        _isometricCamera.Lens.OrthographicSize = Mathf.Clamp(_isometricCamera.Lens.OrthographicSize, _isometricZoomLimits.x, _isometricZoomLimits.y);
    }
    private void UpdateCenitalZoom(float delta)
    {
        _zoomStep = Mathf.Clamp(_zoomStep += delta, 0, _zoomMaxStep);
        _zoomLerp = _zoomStep / _zoomMaxStep;
    
        _cenitalFollow.VerticalAxis.Value = Mathf.Lerp(_cenitalFollow.VerticalAxis.Range.x, _cenitalFollow.VerticalAxis.Range.y, _zoomLerp);
        _cenitalFollow.RadialAxis.Value = Mathf.Lerp(_cenitalFollow.RadialAxis.Range.x, _cenitalFollow.RadialAxis.Range.y, _zoomLerp);
    }
    public void RotateCenital(float delta)
    {
        if(_currentCameraType == CameraType.Isometric || _zoomLerp > 0.5f) return;

        _rotateStep = Mathf.Clamp(_rotateStep += delta, 0, _rotateMaxStep);
        _rotateLerp = _rotateStep / _rotateMaxStep;
        
    }
}
