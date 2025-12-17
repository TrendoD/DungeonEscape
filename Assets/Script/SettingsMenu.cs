using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsWindow; // Panel utama window settings
    [SerializeField] private Slider volumeSlider;       // Slider untuk mengatur volume

    void Start()
    {
        // 1. Pastikan window tertutup saat game mulai
        if (settingsWindow != null)
            settingsWindow.SetActive(false);

        // 2. Load volume yang tersimpan (Default 1.0 atau 100%)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        
        // 3. Terapkan ke AudioListener (Global Volume)
        AudioListener.volume = savedVolume;

        // 4. Update posisi slider agar sesuai dengan volume sekarang
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            // Daftarkan fungsi SetVolume agar dipanggil saat slider digeser
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // Dipanggil oleh Tombol "Settings" di Menu Utama
    public void OpenSettings()
    {
        settingsWindow.SetActive(true);
    }

    // Dipanggil oleh Tombol "Close" / "Back" di dalam Window Settings
    public void CloseSettings()
    {
        settingsWindow.SetActive(false);
        
        // Simpan settingan saat menu ditutup
        PlayerPrefs.Save();
    }

    // Dipanggil otomatis oleh Slider
    public void SetVolume(float value)
    {
        // Mengubah volume global Unity
        AudioListener.volume = value;
        
        // Simpan nilai ke memory (PlayerPrefs)
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
}
