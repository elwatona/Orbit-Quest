using UnityEngine;

public class LevelMediator : MonoBehaviour
{
    [SerializeField] private GameState _initialState;
    [SerializeField, Space(10)] private AudioDependencies _audioDependencies;

    private LevelSignals _levelSignals;
    private GameStateController _gameStateController;
    private AudioManager _audioManager;
    private void Awake()
    {
        _levelSignals = new LevelSignals();
        InitializeManagers();
        InitializeControllers();
    }
    private void InitializeManagers()
    {

    }
    private void InitializeControllers()
    {
        _gameStateController = new GameStateController(_levelSignals, _initialState);
        _audioManager = new AudioManager(_audioDependencies);
    }
}