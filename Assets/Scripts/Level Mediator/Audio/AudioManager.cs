using UnityEngine;

public class AudioManager : IManager
{
    public LevelSignals LevelSignals { get; }
    private AudioDependencies _dependencies;
    public AudioManager(AudioDependencies dependencies)
    {
        _dependencies = dependencies;
    }
    void PlayOrbSound(int index)
    {
        AudioSource desiredAudio = _dependencies.OrbAudios[index];
        if(desiredAudio.isPlaying) desiredAudio.PlayOneShot(desiredAudio.clip, desiredAudio.volume);
        else desiredAudio.Play();
    }

    public void Subscribe()
    {
        Orb.OnSpawn += () => PlayOrbSound(0);
        Orb.OnDespawn += () => PlayOrbSound(1);
        Orb.OnOrbitEnter += () => PlayOrbSound(2);
        Orb.OnOrbitExit += () => PlayOrbSound(3);
    }

    public void Unsubscribe()
    {
        Orb.OnSpawn += () => PlayOrbSound(0);
        Orb.OnDespawn += () => PlayOrbSound(1);
        Orb.OnOrbitEnter += () => PlayOrbSound(2);
        Orb.OnOrbitExit += () => PlayOrbSound(3);
    }
}
