public class GameState : IController
{
    public LevelSignals LevelSignals { get; }
    public State CurrentState { get; private set; }
    public GameState(LevelSignals levelSignals, State initialState)
    {
        LevelSignals = levelSignals;
        CurrentState = initialState;
    }
    public bool TrySetState(State next)
    {
        if (!CanTransitionTo(CurrentState, next)) return false;

        LevelSignals.RaiseStateExited(CurrentState);
        CurrentState = next;
        LevelSignals.RaiseStateEntered(CurrentState);
        return true;
    }

    static bool CanTransitionTo(State from, State next)
    {
        if(from == next) return false;
        return true;
    }
}