using UnityEngine;

public class LevelMediator : MonoBehaviour
{
    [SerializeField] private GameState _initialState;
    [SerializeField, Space(10)] private AudioDependencies _audioDependencies;
    [SerializeField] private CameraManagerDependencies _cameraDependencies;
    private LevelSignals _levelSignals;
    private GameStateController _gameStateController;
    private CameraManager _cameraManager;
    private AudioManager _audioManager;
    private void Awake()
    {
        _levelSignals = new LevelSignals();
        InitializeManagers();
        InitializeControllers();
    }
    private void InitializeManagers()
    {
        _cameraManager = new CameraManager(_cameraDependencies);
        _audioManager = new AudioManager(_audioDependencies);
    }
    private void InitializeControllers()
    {
        _gameStateController = new GameStateController(_levelSignals, _initialState);
    }
    private void Update()
    {
        _cameraManager.Update();
    }
}