using UnityEngine;
using System;

/// <summary>
/// Singleton untuk mengelola semua pengaturan volume game.
/// Menyimpan 3 kategori: Master, Music, dan SFX.
/// Menggunakan PlayerPrefs untuk persistensi antar sesi.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static AudioManager Instance { get; private set; }

    // ==================== EVENTS ====================
    // Event untuk memberitahu script lain saat volume berubah
    public static event Action<float> OnMasterVolumeChanged;
    public static event Action<float> OnMusicVolumeChanged;
    public static event Action<float> OnSFXVolumeChanged;

    // ==================== VOLUME VALUES ====================
    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    // ==================== PLAYERPREFS KEYS ====================
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    // ==================== PUBLIC PROPERTIES ====================
    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            AudioListener.volume = masterVolume;
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
            OnMasterVolumeChanged?.Invoke(masterVolume);
        }
    }

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
            OnMusicVolumeChanged?.Invoke(musicVolume);
        }
    }

    public float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
            OnSFXVolumeChanged?.Invoke(sfxVolume);
        }
    }

    // ==================== LIFECYCLE ====================
    void Awake()
    {
        // Singleton pattern dengan DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load semua volume settings dari PlayerPrefs
    /// </summary>
    private void LoadVolumeSettings()
    {
        // Load dengan default value 1.0 (100%)
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        // Apply master volume ke AudioListener
        AudioListener.volume = masterVolume;
    }

    /// <summary>
    /// Simpan semua settings ke PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Helper method untuk mendapatkan volume SFX yang sudah di-scale.
    /// Gunakan ini untuk AudioSource.PlayOneShot() atau PlayClipAtPoint().
    /// </summary>
    public float GetScaledSFXVolume(float baseVolume = 1f)
    {
        return baseVolume * sfxVolume;
    }

    /// <summary>
    /// Helper method untuk mendapatkan volume Music yang sudah di-scale.
    /// </summary>
    public float GetScaledMusicVolume(float baseVolume = 1f)
    {
        return baseVolume * musicVolume;
    }
}
