public class CameraManager : IManager
{

    public enum Type
    {
        Isometric,
        Cenital
    }
    public LevelSignals LevelSignals { get; }
    private CameraManagerDependencies _dependencies;
    Type _currentCameraType;
    private IsometricCamera _isometricController;
    private CenitalCamera _cenitalController;
    private float _inputRotation;

    public CameraManager(CameraManagerDependencies dependencies)
    {
        _dependencies = dependencies;
        _isometricController = new IsometricCamera(_dependencies.IsometricCamera);
        _cenitalController = new CenitalCamera(_dependencies.CenitalCamera);
        
        _currentCameraType = _dependencies.PlayerData.IsInEditMode ? Type.Isometric : Type.Cenital;
        UpdateCameras(_currentCameraType);
    }
    public void Subscribe()
    {
        CameraInputController.OnCameraInput += OnCameraInput;
    }
    public void Unsubscribe()
    {
        CameraInputController.OnCameraInput -= OnCameraInput;
    }
    public void Update() => OnRotate(_inputRotation);
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
        _currentCameraType = _currentCameraType == Type.Isometric ? Type.Cenital : Type.Isometric;
        UpdateCameras(_currentCameraType);
    }
    private void OnZoom(float delta)
    {
        switch(_currentCameraType)
        {
            case Type.Isometric:
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
            case Type.Isometric:
                _isometricController.Rotate(delta);
                break;
            default:
                break;
        }
    }
    private void UpdateCameras(Type cameraType)
    {
        _isometricController.SetActive(cameraType == Type.Isometric);
        _cenitalController.SetActive(cameraType == Type.Cenital);
    }
}
