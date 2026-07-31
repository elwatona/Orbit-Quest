using System;
public class GameStateController
{
    public GameState CurrentState { get; private set; }
    public GameState LastPlayMode { get; private set; } = GameState.Precision;
    private event Action<GameState> _stateExited, _stateEntered;
    public GameStateController(GameState initialState, Action<GameState> onStateExited, Action<GameState> onStateEntered)
    {
        CurrentState = initialState;
        if (IsPlayMode(initialState))
            LastPlayMode = initialState;
        _stateExited = onStateExited;
        _stateEntered = onStateEntered;
    }
    public void SetState(GameState state)
    {
        if (!CanTransitionTo(CurrentState, state)) return;
        _stateExited?.Invoke(CurrentState);
        CurrentState = state;
        if (IsPlayMode(state))
            LastPlayMode = state;
        _stateEntered?.Invoke(CurrentState);
    }
    static bool IsPlayMode(GameState state)
        => state == GameState.Precision || state == GameState.Contemplative;
    static bool CanTransitionTo(GameState from, GameState next)
    {
        if(from == next) return false;
        return true;
    }
}
