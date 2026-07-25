using UnityEngine;

public static class Audio
{
    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
