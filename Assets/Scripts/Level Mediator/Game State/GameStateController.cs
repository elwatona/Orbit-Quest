public class GameStateController : IController
{
    public LevelSignals LevelSignals { get; }
    public GameState CurrentState { get; private set; }
    public GameStateController(LevelSignals levelSignals, GameState initialState)
    {
        LevelSignals = levelSignals;
        TrySetState(initialState);
    }
    public bool TrySetState(GameState next)
    {
        if (!CanTransitionTo(CurrentState, next)) return false;

        LevelSignals.RaiseStateExited(CurrentState);
        CurrentState = next;
        LevelSignals.RaiseStateEntered(CurrentState);
        return true;
    }
    static bool CanTransitionTo(GameState from, GameState next)
    {
        if(from == next) return false;
        return true;
    }
}