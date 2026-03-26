/// <summary>
/// Contract for dev-only tools (inspector, orb editing). Used so gameplay code can check availability without referencing concrete UI types.
/// </summary>
public interface IDeveloperTools
{
    bool IsDeveloperModeActive { get; }
    bool IsAvailable { get; }
}
