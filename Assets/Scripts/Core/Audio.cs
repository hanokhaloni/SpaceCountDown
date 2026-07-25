using UnityEngine;

public static class Audio
{
    public static float MusicVolume = 1f;
    public static float SfxVolume = 1f;

    public static void Play(AudioClip clip, float volume = 1f)
    {
        PlayInternal(clip, volume * SfxVolume);
    }

    public static void PlayMusic(AudioClip clip, float volume = 1f)
    {
        PlayInternal(clip, volume * MusicVolume);
    }

    static void PlayInternal(AudioClip clip, float volume)
    {
        if (clip == null) return;
        Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
