using UnityEngine;

/// <summary>
/// Mengelola background music yang persistent antar scene.
/// Pasang di GameObject dengan AudioSource yang berisi background music.
/// Volume akan otomatis menyesuaikan dengan Music Volume dari AudioManager.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    // ==================== SINGLETON ====================
    public static BackgroundMusicManager Instance { get; private set; }

    // ==================== REFERENCES ====================
    private AudioSource audioSource;
    
    [Header("Settings")]
    [Tooltip("Volume dasar musik (sebelum dikalikan Music Volume)")]
    [Range(0f, 1f)]
    [SerializeField] private float baseVolume = 1f;

    // ==================== LIFECYCLE ====================
    void Awake()
    {
        // Singleton pattern - hanya satu background music manager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            audioSource = GetComponent<AudioSource>();
            
            // Pastikan music loop
            audioSource.loop = true;
            audioSource.playOnAwake = true;
        }
        else
        {
            // Jika sudah ada instance, hancurkan duplikat
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Subscribe ke event perubahan volume
        AudioManager.OnMusicVolumeChanged += UpdateVolume;
        
        // Apply volume awal
        ApplyVolume();
    }

    void OnDestroy()
    {
        // Unsubscribe untuk mencegah memory leak
        AudioManager.OnMusicVolumeChanged -= UpdateVolume;
    }

    /// <summary>
    /// Callback saat Music Volume berubah dari AudioManager
    /// </summary>
    private void UpdateVolume(float newVolume)
    {
        ApplyVolume();
    }

    /// <summary>
    /// Apply volume dari AudioManager ke AudioSource
    /// </summary>
    private void ApplyVolume()
    {
        if (audioSource != null && AudioManager.Instance != null)
        {
            audioSource.volume = baseVolume * AudioManager.Instance.MusicVolume;
        }
    }

    // ==================== PUBLIC METHODS ====================

    /// <summary>
    /// Ganti track musik
    /// </summary>
    public void ChangeMusic(AudioClip newClip, bool playImmediately = true)
    {
        if (audioSource != null && newClip != null)
        {
            audioSource.clip = newClip;
            if (playImmediately)
            {
                audioSource.Play();
            }
        }
    }

    /// <summary>
    /// Pause musik
    /// </summary>
    public void PauseMusic()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// Resume musik
    /// </summary>
    public void ResumeMusic()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    /// <summary>
    /// Stop musik
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
