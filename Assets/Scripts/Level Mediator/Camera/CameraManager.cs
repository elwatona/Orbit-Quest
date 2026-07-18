using System;
using Unity.Cinemachine;
public class CameraManager
{
    readonly LevelData _levelData;
    readonly EditorCamera _editorCamera;
    readonly PrecisionCamera _precisionCamera;
    readonly PrecisionCamera _contemplativeCamera;

    private float _rotationInput;

    public CameraManager(CameraManagerDependencies dependencies)
    {
        _levelData = dependencies.LevelData;
        _editorCamera = new EditorCamera(dependencies.EditorCamera);
        _precisionCamera = new PrecisionCamera(dependencies.PrecisionCamera);
        _contemplativeCamera = new PrecisionCamera(dependencies.ContemplativeCamera);
    }
    public void Subscribe()
    {
        CameraInputController.CameraInput += OnCameraInput;
        _levelData.StateEntered += UpdateCameras;
    }
    public void Unsubscribe()
    {
        CameraInputController.CameraInput -= OnCameraInput;
        _levelData.StateEntered -= UpdateCameras;
    }
    public void Update() => OnRotate(_rotationInput);
    private void OnCameraInput(CameraInputController.InputType inputType, float value)
    {
        switch(inputType)
        {
            case CameraInputController.InputType.Zoom:
                OnZoom(value);
                break;
            case CameraInputController.InputType.Rotate:
                _rotationInput = value;
                break;
        }
    }
    private void OnZoom(float delta)
    {
        switch(_levelData.CurrentState)
        {
            case GameState.Edition:
                _editorCamera.Zoom(delta);
                break;
        }
    }
    private void OnRotate(float delta)
    {
        switch(_levelData.CurrentState)
        {
            case GameState.Edition:
                _editorCamera.Rotate(delta);
                break;
        }
    }
    private void UpdateCameras(GameState cameraType)
    {
        _editorCamera.SetActive(cameraType == GameState.Edition);
        _precisionCamera.SetActive(cameraType == GameState.Precision);
        _contemplativeCamera.SetActive(cameraType == GameState.Contemplative);
    }
}

[Serializable]
public class CameraManagerDependencies
{
    public CinemachineCamera EditorCamera;
    public CinemachineCamera PrecisionCamera;
    public CinemachineCamera ContemplativeCamera;
    public LevelData LevelData;
}
