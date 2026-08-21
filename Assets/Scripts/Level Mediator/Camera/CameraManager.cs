using System;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager
{
    readonly LevelData _levelData;
    readonly OrbitalCamera _editorCamera;
    readonly OrbitalCamera _precisionCamera;
    readonly OrbitalCamera _contemplativeCamera;

    OrbitalCamera _activeCamera;
    bool _lookHeld;

    public CameraManager(CameraManagerDependencies dependencies)
    {
        _levelData = dependencies.LevelData;
        var sharedZoom = new SharedCameraZoom();
        var sharedOrbit = new SharedCameraOrbit();
        _editorCamera = new OrbitalCamera(dependencies.EditorCamera, sharedZoom, sharedOrbit);
        _precisionCamera = new OrbitalCamera(dependencies.PrecisionCamera, sharedZoom, sharedOrbit);
        _contemplativeCamera = new OrbitalCamera(dependencies.ContemplativeCamera, sharedZoom, sharedOrbit);
        _editorCamera.SetActive(false, commitOrbit: false);
        _precisionCamera.SetActive(false, commitOrbit: false);
        _contemplativeCamera.SetActive(false, commitOrbit: false);
    }

    public void Subscribe()
    {
        CameraInputController.CameraInput += OnCameraInput;
        CameraInputController.LookHeld += OnLookHeld;
        _levelData.StateEntered += UpdateCameras;
    }

    public void Unsubscribe()
    {
        CameraInputController.CameraInput -= OnCameraInput;
        CameraInputController.LookHeld -= OnLookHeld;
        _levelData.StateEntered -= UpdateCameras;
    }

    void OnCameraInput(CameraInputController.InputType inputType, float value)
    {
        if (inputType != CameraInputController.InputType.Zoom) return;
        OnZoom(value);
    }

    void OnLookHeld(bool held)
    {
        _lookHeld = held;
        _activeCamera?.SetLookEnabled(held);
    }

    void OnZoom(float delta)
    {
        _activeCamera?.Zoom(delta);
    }

    void UpdateCameras(GameState cameraType)
    {
        _activeCamera?.SetActive(false);

        _activeCamera = cameraType switch
        {
            GameState.Edition => _editorCamera,
            GameState.Precision => _precisionCamera,
            GameState.Contemplative => _contemplativeCamera,
            _ => null
        };

        if (_activeCamera == null) return;
        _activeCamera.SetActive(true);
        _activeCamera.SetLookEnabled(_lookHeld);
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
