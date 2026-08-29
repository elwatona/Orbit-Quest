using System.Collections.Generic;

public class AudioSettings
{
    public FloatParameter MasterVolume { get; }

    public AudioSettings()
    {
        MasterVolume = new FloatParameter(
            "masterVolume",
            "Master Volume",
            AudioTuning.MasterVolumeMin,
            AudioTuning.MasterVolumeMax,
            AudioTuning.MasterVolumeDefault);
    }

    public IReadOnlyList<FloatParameter> Parameters => new[] { MasterVolume };

    public void ApplyData(GameSettingsData data)
    {
        MasterVolume.SetValue(data.MasterVolume);
    }
}
