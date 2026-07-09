using System;

public sealed class LevelSignals
{
    public event Action<PresetData> PresetLoaded;
    public event Action<string> PresetSaved;
    public event Action<GameState> StateExited, StateEntered;

    internal void RaisePresetLoaded(PresetData presetData) => PresetLoaded?.Invoke(presetData);
    internal void RaisePresetSaved(string presetName) => PresetSaved?.Invoke(presetName);
    internal void RaiseStateExited(GameState state) => StateExited?.Invoke(state);
    internal void RaiseStateEntered(GameState state) => StateEntered?.Invoke(state);
}