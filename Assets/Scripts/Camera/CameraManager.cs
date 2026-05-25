using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
public class CameraManager : MonoBehaviour
{
    public enum CameraType
    {
        Isometric,
        Cenital
    }

    [SerializeField] CinemachineCamera _isometricCamera;
    [SerializeField] CinemachineCamera _cenitalCamera;
    [SerializeField] CameraType _currentCameraType;

    private IsometricCamera _isometricController;
    private CenitalCamera _cenitalController;

    void Awake()
    {
        _isometricController = new IsometricCamera(_isometricCamera);
        _cenitalController = new CenitalCamera(_cenitalCamera);
    }
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
                _isometricController.Zoom(delta);
                break;
            case CameraType.Cenital:
                _cenitalController.Zoom(delta);
                break;
        }
    }
    public void Rotate(float delta)
    {
        switch(_currentCameraType)
        {
            case CameraType.Isometric:
                _isometricController.Rotate(delta);
                break;
            case CameraType.Cenital:
                _cenitalController.Rotate(delta);
                break;
        }
    }
    private void UpdateCameras(CameraType cameraType)
    {
        _isometricController.SetActive(cameraType == CameraType.Isometric);
        _cenitalController.SetActive(cameraType == CameraType.Cenital);
    }
}
