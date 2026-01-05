using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsWindow; // Panel utama window settings
    
    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;       // Slider untuk Master Volume
    [SerializeField] private Slider musicSlider;        // Slider untuk Music Volume
    [SerializeField] private Slider sfxSlider;          // Slider untuk SFX Volume

    void Start()
    {
        // 1. Pastikan window tertutup saat game mulai
        if (settingsWindow != null)
            settingsWindow.SetActive(false);

        // 2. Pastikan AudioManager sudah ada
        EnsureAudioManager();

        // 3. Setup semua slider
        SetupSliders();
    }

    /// <summary>
    /// Memastikan AudioManager ada di scene
    /// </summary>
    private void EnsureAudioManager()
    {
        if (AudioManager.Instance == null)
        {
            // Buat AudioManager jika belum ada
            GameObject audioManager = new GameObject("AudioManager");
            audioManager.AddComponent<AudioManager>();
        }
    }

    /// <summary>
    /// Setup slider dengan nilai tersimpan dan listener
    /// </summary>
    private void SetupSliders()
    {
        // Master Volume Slider
        if (masterSlider != null)
        {
            masterSlider.value = AudioManager.Instance.MasterVolume;
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        // Music Volume Slider
        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.Instance.MusicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // SFX Volume Slider
        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.SFXVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // Dipanggil oleh Tombol "Settings" di Menu Utama
    public void OpenSettings()
    {
        settingsWindow.SetActive(true);
        
        // Refresh slider values saat membuka settings
        RefreshSliderValues();
    }

    // Dipanggil oleh Tombol "Close" / "Back" di dalam Window Settings
    public void CloseSettings()
    {
        settingsWindow.SetActive(false);
        
        // Simpan settingan saat menu ditutup
        if (AudioManager.Instance != null)
            AudioManager.Instance.SaveSettings();
    }

    /// <summary>
    /// Refresh nilai slider dari AudioManager
    /// </summary>
    private void RefreshSliderValues()
    {
        if (AudioManager.Instance == null) return;

        if (masterSlider != null)
            masterSlider.value = AudioManager.Instance.MasterVolume;
        
        if (musicSlider != null)
            musicSlider.value = AudioManager.Instance.MusicVolume;
        
        if (sfxSlider != null)
            sfxSlider.value = AudioManager.Instance.SFXVolume;
    }

    // ==================== VOLUME SETTERS ====================

    public void SetMasterVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.MasterVolume = value;
    }

    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.MusicVolume = value;
    }

    public void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SFXVolume = value;
    }
}
