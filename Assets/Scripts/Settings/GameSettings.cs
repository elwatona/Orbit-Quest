using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Proto-Pablo/Game Settings", order = 2)]
public class GameSettings : ScriptableObject
{
    const string PlayerPrefsKey = "GameSettings";

    public GraphicsSettings Graphics { get; private set; }
    public AudioSettings Audio { get; private set; }
    public EditionSettings Edition { get; private set; }

    public event Action Changed;

    bool _initialized;
    bool _suppressSave;

    public void Initialize()
    {
        if (_initialized && Graphics != null && Audio != null && Edition != null)
            return;

        Graphics = new GraphicsSettings();
        Audio = new AudioSettings();
        Edition = new EditionSettings();
        SubscribeParameters();
        Load();
        Edition.ApplyToTuning();
        _initialized = true;
        Changed?.Invoke();
    }

    public void Save()
    {
        if (Graphics == null || Audio == null || Edition == null)
            return;

        var data = new GameSettingsData
        {
            GridThickness = Graphics.GridThickness.Value,
            SkyboxColdness = Graphics.SkyboxColdness.Value,
            SkyboxDensity = Graphics.SkyboxDensity.Value,
            SkyboxEnabled = Graphics.SkyboxEnabled.Value,
            MasterVolume = Audio.MasterVolume.Value,
            Editor = Edition.ToData()
        };
        PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        Initialize();
        _suppressSave = true;
        Graphics.ApplyData(GameSettingsData.Defaults);
        Audio.ApplyData(GameSettingsData.Defaults);
        Edition.ApplyData(EditorRangesData.Defaults);
        _suppressSave = false;
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    void Load()
    {
        _suppressSave = true;
        string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            GameSettingsData data = JsonUtility.FromJson<GameSettingsData>(json);
            if (!json.Contains("\"SkyboxEnabled\""))
                data.SkyboxEnabled = GraphicsTuning.SkyboxEnabledDefault;
            Graphics.ApplyData(data);
            Audio.ApplyData(data);
            Edition.ApplyData(json.Contains("\"Editor\"") ? data.Editor : EditorRangesData.Defaults);
        }
        else
        {
            Edition.ApplyData(EditorRangesData.Defaults);
        }
        _suppressSave = false;
    }

    void SubscribeParameters()
    {
        foreach (FloatParameter parameter in Graphics.Parameters)
            parameter.Changed += HandleParameterChanged;
        Graphics.SkyboxEnabled.Changed += HandleBoolChanged;
        foreach (FloatParameter parameter in Audio.Parameters)
            parameter.Changed += HandleParameterChanged;
        foreach (RangeParameter parameter in Edition.Parameters)
            parameter.Changed += HandleRangeChanged;
    }

    void HandleParameterChanged(FloatParameter _)
    {
        NotifyChanged();
    }

    void HandleBoolChanged(BoolParameter _)
    {
        NotifyChanged();
    }

    void HandleRangeChanged(RangeParameter _)
    {
        Edition.ApplyToTuning();
        NotifyChanged();
    }

    void NotifyChanged()
    {
        if (!_suppressSave)
            Save();
        Changed?.Invoke();
    }
}
