using System;

public sealed class LevelSignals
{
    public event Action<PresetData> PresetLoaded;
    public event Action<string> PresetSaved;
    // public event Action<GameState> GameStateChanged;

    internal void RaisePresetLoaded(PresetData presetData) => PresetLoaded?.Invoke(presetData);
    internal void RaisePresetSaved(string presetName) => PresetSaved?.Invoke(presetName);
    // internal void RaiseGameStateChanged(GameState gameState) => GameStateChanged?.Invoke(gameState);
}