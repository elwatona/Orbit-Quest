using UnityEngine;
using System.Collections.Generic;
public class LevelMediator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] LevelData _levelData;
    [SerializeField] Limits _limits;
    [SerializeField] GameSettings _gameSettings;
    [SerializeField] Material _skyboxMaterial;
    [Header("Dependencies")]
    [SerializeField] AudioDependencies _audioDependencies;
    [SerializeField] CameraManagerDependencies _cameraDependencies;
    [Header("Level Bounds")]
    [SerializeField] GameObject[] _levelBoundsGO;
    private CameraManager _cameraManager;
    private AudioManager _audioManager;
    private SkyboxShaderController _skyboxShaderController;
    private List<ILimitable> _levelBounds = new List<ILimitable>();
    void Awake()
    {
        _levelData.Initialize();
        if (_gameSettings != null)
            _gameSettings.Initialize();
        _cameraManager = new CameraManager(_cameraDependencies);
        _audioManager = new AudioManager(_audioDependencies);
        _skyboxShaderController = new SkyboxShaderController(
            _skyboxMaterial != null ? _skyboxMaterial : RenderSettings.skybox);
        ApplyGameSettings();
        UpdateLimitables();
    }
    void OnEnable()
    {
        _cameraManager.Subscribe();
        _audioManager.Subscribe();
        if (_gameSettings != null)
            _gameSettings.Changed += ApplyGameSettings;
    }
    void OnDisable()
    {
        _cameraManager.Unsubscribe();
        _audioManager.Unsubscribe();
        if (_gameSettings != null)
            _gameSettings.Changed -= ApplyGameSettings;
        if (_levelData != null)
            _levelData.SetPaused(false);
    }
    void Start() => _levelData.Start();

    void ApplyGameSettings()
    {
        if (_gameSettings == null)
            return;

        _audioManager.SetMasterVolume(_gameSettings.Audio.MasterVolume.Value);
        _skyboxShaderController?.Apply(_gameSettings.Graphics);
    }

    private void UpdateLimitables()
    {
        foreach (GameObject levelBoundGO in _levelBoundsGO)
        {
            levelBoundGO.TryGetComponent(out ILimitable levelBound);
            if (levelBound != null)
            {
                _levelBounds.Add(levelBound);
            }
        }
        foreach (ILimitable levelBound in _levelBounds)
        {
            levelBound.SetLimits(_limits);
        }
        Shader.SetGlobalVector("_Limits_Min", _limits.Min);
        Shader.SetGlobalVector("_Limits_Max", _limits.Max);
    }
}
