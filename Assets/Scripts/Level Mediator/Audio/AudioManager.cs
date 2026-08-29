using UnityEngine;

public class AudioManager
{
    readonly AudioDependencies _dependencies;
    readonly float[] _baseVolumes;
    float _masterVolume = AudioTuning.MasterVolumeDefault;

    public AudioManager(AudioDependencies dependencies)
    {
        _dependencies = dependencies;
        AudioSource[] sources = _dependencies.OrbAudios;
        _baseVolumes = new float[sources != null ? sources.Length : 0];
        for (int i = 0; i < _baseVolumes.Length; i++)
            _baseVolumes[i] = sources[i] != null ? sources[i].volume : 1f;
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp(volume, AudioTuning.MasterVolumeMin, AudioTuning.MasterVolumeMax);
        ApplyVolume();
    }

    public void Subscribe()
    {
        Orb.OnSpawn += HandleSpawn;
        Orb.OnDespawn += HandleDespawn;
        Orb.OnOrbitEnter += HandleOrbitEnter;
        Orb.OnOrbitExit += HandleOrbitExit;
    }

    public void Unsubscribe()
    {
        Orb.OnSpawn -= HandleSpawn;
        Orb.OnDespawn -= HandleDespawn;
        Orb.OnOrbitEnter -= HandleOrbitEnter;
        Orb.OnOrbitExit -= HandleOrbitExit;
    }

    void HandleSpawn() => PlayOrbSound(0);
    void HandleDespawn() => PlayOrbSound(1);
    void HandleOrbitEnter() => PlayOrbSound(2);
    void HandleOrbitExit() => PlayOrbSound(3);

    void ApplyVolume()
    {
        AudioSource[] sources = _dependencies.OrbAudios;
        if (sources == null)
            return;

        int count = Mathf.Min(sources.Length, _baseVolumes.Length);
        for (int i = 0; i < count; i++)
        {
            if (sources[i] == null)
                continue;
            sources[i].volume = _baseVolumes[i] * _masterVolume;
        }
    }

    void PlayOrbSound(int index)
    {
        AudioSource[] sources = _dependencies.OrbAudios;
        if (sources == null || index < 0 || index >= sources.Length)
            return;

        AudioSource desiredAudio = sources[index];
        if (desiredAudio == null)
            return;

        if (desiredAudio.isPlaying)
            desiredAudio.PlayOneShot(desiredAudio.clip, desiredAudio.volume);
        else
            desiredAudio.Play();
    }
}
