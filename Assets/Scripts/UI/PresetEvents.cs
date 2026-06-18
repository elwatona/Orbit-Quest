using System;

public static class PresetEvents
{
    public static event Action<Preset> OnPresetSelected;
    public static event Action<PresetData> OnPresetLoaded;
    public static event Action<string[]> OnPresetNamesLoaded;
    public static void RaisePresetSelectedEvent(Preset preset)
    {
        OnPresetSelected?.Invoke(preset);
    }
    public static void RaisePresetLoadedEvent(PresetData presetData)
    {
        OnPresetLoaded?.Invoke(presetData);
    }
    public static void RaisePresetNamesLoadedEvent(string[] presetNames)
    {
        OnPresetNamesLoaded?.Invoke(presetNames);
    }
}