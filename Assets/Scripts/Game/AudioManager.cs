using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles Background Music (BGM) and Sound Effects (SFX) playback.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Default Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Audio Clips")]
    public List<AudioClip> bgmClips;  // List of background music tracks
    public List<AudioClip> sfxClips;  // List of sound effects

    [Header("Combo Clips")]
    public List<AudioClip> comboClips;

    private Dictionary<string, AudioClip> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        // Prepare lookup dictionaries
        bgmDict = new Dictionary<string, AudioClip>();
        foreach (var clip in bgmClips)
        {
            if (!bgmDict.ContainsKey(clip.name))
                bgmDict.Add(clip.name, clip);
        }

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var clip in sfxClips)
        {
            if (!sfxDict.ContainsKey(clip.name))
                sfxDict.Add(clip.name, clip);
        }

        // Set initial volumes
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    private void Start()
    {
        PlayBGM("BgSound",true);
        SetBGMVolume(0.7f);
    }
    #region  BGM Functions
    public void PlayBGM(string bgmName, bool loop = true)
    {
        if (bgmDict.TryGetValue(bgmName, out AudioClip clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"[AudioManager] BGM '{bgmName}' not found!");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }
    #endregion

    #region  SFX Functions
    public void PlaySFX(string sfxName)
    {
        if (sfxDict.TryGetValue(sfxName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX '{sfxName}' not found!");
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }
    public void PlayComboSFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
    public void PlayComboSound(int comboCount)
    {
        if (comboCount < 2) return; // Only play sound for combos 2+
        int index = comboCount - 2; // combo 2 => index 0, combo 3 => index 1
        if (index < comboClips.Count && comboClips[index] != null)
        {
            PlayComboSFX(comboClips[index]);
        }
    }
    #endregion
}
