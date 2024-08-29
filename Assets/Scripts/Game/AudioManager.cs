using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Ensure only one instance exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
    }

    // Assign these in the Unity Inspector
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup voiceMixerGroup;

    public int poolSize = 10; // Number of AudioSources to pool
    private Queue<AudioSource> sfxPool;
    private AudioSource musicSource;
    private List<AudioSource> pausedSources = new();

    private void InitializePools()
    {
        sfxPool = new Queue<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; // Avoid playing on awake
            sfxPool.Enqueue(source);
        }
    }

    public void Play(AudioClip clip, SoundCategory category = SoundCategory.SFX, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null! Cannot play sound.");
            return;
        }

        AudioSource source = null;

        if (category == SoundCategory.SFX)
        {
            // Use pooled AudioSource
            if (sfxPool.Count > 0)
            {
                source = sfxPool.Dequeue();
            }
            else
            {
                source = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            // Create a new AudioSource for music or other categories
            source = gameObject.AddComponent<AudioSource>();
        }

        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;

        switch (category)
        {
            case SoundCategory.Music:
                source.outputAudioMixerGroup = musicMixerGroup;
                musicSource = source; // Store the current music source
                break;
            case SoundCategory.Voice:
                source.outputAudioMixerGroup = voiceMixerGroup;
                break;
            case SoundCategory.SFX:
            default:
                source.outputAudioMixerGroup = sfxMixerGroup;
                break;
        }

        source.Play();

        if (category == SoundCategory.SFX)
        {
            // Return the AudioSource to the pool after it's done playing
            StartCoroutine(ReturnToPoolAfterPlayback(source, clip.length / Mathf.Abs(pitch)));
        }
    }

    public void Stop(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null! Cannot stop sound.");
            return;
        }

        foreach (var source in GetComponents<AudioSource>())
        {
            if (source.clip == clip)
            {
                source.Stop();
                if (sfxPool.Contains(source) == false && source != musicSource)
                {
                    sfxPool.Enqueue(source);
                }
            }
        }
    }

    public bool IsPlaying(AudioClip clip)
    {
        foreach (var source in GetComponents<AudioSource>())
        {
            if (source.clip == clip && source.isPlaying)
            {
                return true;
            }
        }
        return false;
    }

    public void PauseAll()
    {
        foreach (var source in GetComponents<AudioSource>())
        {
            if (source.isPlaying)
            {
                source.Pause();
                if (!pausedSources.Contains(source))
                {
                    pausedSources.Add(source);
                }
            }
        }
    }

    public void ResumeAll()
    {
        foreach (var source in pausedSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
        pausedSources.Clear();
    }

    public void PauseCategory(SoundCategory category)
    {
        foreach (var source in GetComponents<AudioSource>())
        {
            if (source.isPlaying && GetCategory(source) == category)
            {
                source.Pause();
                if (!pausedSources.Contains(source))
                {
                    pausedSources.Add(source);
                }
            }
        }
    }

    public void ResumeCategory(SoundCategory category)
    {
        foreach (var source in pausedSources)
        {
            if (source != null && GetCategory(source) == category)
            {
                source.UnPause();
            }
        }
    }

    public void StopAll()
    {
        foreach (var source in GetComponents<AudioSource>())
        {
            source.Stop();
            if (sfxPool.Contains(source) == false && source != musicSource)
            {
                sfxPool.Enqueue(source);
            }
        }
    }

    private IEnumerator ReturnToPoolAfterPlayback(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.Stop();
        sfxPool.Enqueue(source);
    }

    public void PlayBackgroundMusic(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = true)
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic(musicSource, 1f)); // Fade out current music
        }

        Play(clip, SoundCategory.Music, volume, pitch, loop);
    }

    private IEnumerator FadeOutMusic(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
        source.volume = 0;
        source.Stop();
    }

    private SoundCategory GetCategory(AudioSource source)
    {
        if (source.outputAudioMixerGroup == musicMixerGroup) return SoundCategory.Music;
        if (source.outputAudioMixerGroup == voiceMixerGroup) return SoundCategory.Voice;
        return SoundCategory.SFX;
    }
}

// Enum for sound categories
public enum SoundCategory
{
    SFX,
    Music,
    Voice
}
