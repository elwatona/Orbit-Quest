using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Proto-Pablo/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [SerializeField] GameState _initialState;
    public event Action<GameState> StateExited, StateEntered;
    public event Action<bool> PausedChanged;

    private GameStateController _gameStateController;
    private bool _isPaused;
    public GameState CurrentState
    {
        get
        {
            EnsureInitialized();
            return _gameStateController.CurrentState;
        }
    }
    public bool IsInEditMode => CurrentState == GameState.Edition;
    public bool IsPaused => _isPaused;
    public GameState LastPlayMode
    {
        get
        {
            EnsureInitialized();
            return _gameStateController.LastPlayMode;
        }
    }
    public void SetState(GameState state)
    {
        EnsureInitialized();
        _gameStateController.SetState(state);
    }
    public void SetPaused(bool paused)
    {
        if (_isPaused == paused)
            return;

        _isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        PausedChanged?.Invoke(_isPaused);
    }
    public void Initialize()
    {
        if (_gameStateController == null)
        {
            _gameStateController = new GameStateController(
                _initialState,
                state => StateExited?.Invoke(state),
                state => StateEntered?.Invoke(state));
        }
        SetPaused(false);
    }
    void EnsureInitialized() => Initialize();
    public void Start()
    {
        StateEntered?.Invoke(CurrentState);
    }
}
