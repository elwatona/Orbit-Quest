using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Proto-Pablo/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [SerializeField] GameState _initialState;
    public event Action<GameState> StateExited, StateEntered;

    private GameStateController _gameStateController;
    public GameState CurrentState
    {
        get
        {
            EnsureInitialized();
            return _gameStateController.CurrentState;
        }
    }
    public bool IsInEditMode => CurrentState == GameState.Edition;
    public void SetState(GameState state)
    {
        EnsureInitialized();
        _gameStateController.SetState(state);
    }
    public void Initialize()
    {
        if (_gameStateController != null) return;
        _gameStateController = new GameStateController(
            _initialState,
            state => StateExited?.Invoke(state),
            state => StateEntered?.Invoke(state));
    }
    void EnsureInitialized() => Initialize();
    public void Start()
    {
        StateEntered?.Invoke(CurrentState);
    }
}
