using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{

    public enum CameraType
    {
        Isometric,
        Cenital
    }
    [SerializeField] PlayerData _playerData;
    [SerializeField] CinemachineCamera _isometricCamera;
    [SerializeField] CinemachineCamera _cenitalCamera;
    [SerializeField] CameraType _currentCameraType;

    private IsometricCamera _isometricController;
    private CenitalCamera _cenitalController;

    private float _inputRotation;

    void Awake()
    {
        _isometricController = new IsometricCamera(_isometricCamera);
        _cenitalController = new CenitalCamera(_cenitalCamera);
    }
    void OnEnable()
    {
        CameraInputController.OnCameraInput += OnCameraInput;
    }
    void OnDisable()
    {
        CameraInputController.OnCameraInput -= OnCameraInput;
    }
    void Start()
    {
        _currentCameraType = _playerData.IsInEditMode ? CameraType.Isometric : CameraType.Cenital;
        UpdateCameras(_currentCameraType);
    }
    void Update() => OnRotate(_inputRotation);
    private void OnCameraInput(CameraInputController.InputType inputType, float value)
    {
        switch(inputType)
        {
            case CameraInputController.InputType.Zoom:
                OnZoom(value);
                break;
            case CameraInputController.InputType.Rotate:
                _inputRotation = value;
                break;
            case CameraInputController.InputType.SwitchCameraType:
                ToggleCameraType();
                break;
        }
    }
    public void ToggleCameraType()
    {
        _currentCameraType = _currentCameraType == CameraType.Isometric ? CameraType.Cenital : CameraType.Isometric;
        UpdateCameras(_currentCameraType);
    }
    private void OnZoom(float delta)
    {
        switch(_currentCameraType)
        {
            case CameraType.Isometric:
                _isometricController.Zoom(delta);
                break;
            default:
                break;
        }
    }
    private void OnRotate(float delta)
    {
        switch(_currentCameraType)
        {
            case CameraType.Isometric:
                _isometricController.Rotate(delta);
                break;
            default:
                break;
        }
    }
    private void UpdateCameras(CameraType cameraType)
    {
        _isometricController.SetActive(cameraType == CameraType.Isometric);
        _cenitalController.SetActive(cameraType == CameraType.Cenital);
    }
}
