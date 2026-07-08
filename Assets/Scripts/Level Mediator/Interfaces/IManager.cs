public interface IManager
{
    public LevelSignals LevelSignals { get; }
    public void Subscribe();
    public void Unsubscribe();
}