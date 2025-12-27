using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Script untuk menampilkan Menu Pause saat bermain game.
/// Tekan tombol ESC untuk membuka/menutup menu.
/// Fitur: Resume, Restart, Settings, Exit
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panel Referensi")]
    [Tooltip("Panel utama menu pause (yang berisi tombol-tombol)")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Panel Settings (opsional, bisa dikosongkan jika mau pakai SettingsMenu.cs terpisah)")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Tombol Referensi (Opsional - untuk setup via script)")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button backFromSettingsButton;

    [Header("Settings - Volume")]
    [SerializeField] private Slider volumeSlider;

    [Header("Pengaturan Scene")]
    [Tooltip("Nama scene Menu Utama. Jika kosong, akan kembali ke scene index 0")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // Status apakah game sedang di-pause
    private bool isPaused = false;

    void Awake()
    {
        // Auto-find panels jika belum di-assign
        AutoFindReferences();
    }

    /// <summary>
    /// Mencari referensi secara otomatis berdasarkan nama GameObject
    /// </summary>
    void AutoFindReferences()
    {
        // Cari PauseMenuPanel
        if (pauseMenuPanel == null)
        {
            GameObject found = GameObject.Find("PauseMenuPanel");
            if (found != null)
            {
                pauseMenuPanel = found;
                Debug.Log("PauseMenuUI: Auto-assigned PauseMenuPanel");
            }
        }

        // Cari SettingsPanel
        if (settingsPanel == null)
        {
            GameObject found = GameObject.Find("SettingsPanel");
            if (found != null)
            {
                settingsPanel = found;
                Debug.Log("PauseMenuUI: Auto-assigned SettingsPanel");
            }
        }

        // Cari tombol-tombol
        if (resumeButton == null)
        {
            GameObject found = GameObject.Find("ResumeButton");
            if (found != null) resumeButton = found.GetComponent<Button>();
        }
        if (restartButton == null)
        {
            GameObject found = GameObject.Find("RestartButton");
            if (found != null) restartButton = found.GetComponent<Button>();
        }
        if (settingsButton == null)
        {
            GameObject found = GameObject.Find("SettingsButton");
            if (found != null) settingsButton = found.GetComponent<Button>();
        }
        if (exitButton == null)
        {
            GameObject found = GameObject.Find("ExitButton");
            if (found != null) exitButton = found.GetComponent<Button>();
        }
        if (backFromSettingsButton == null)
        {
            GameObject found = GameObject.Find("BackButton");
            if (found != null) backFromSettingsButton = found.GetComponent<Button>();
        }

        // Cari Volume Slider
        if (volumeSlider == null)
        {
            GameObject found = GameObject.Find("VolumeSlider");
            if (found != null) volumeSlider = found.GetComponent<Slider>();
        }
    }

    void Start()
    {
        // Pastikan menu tertutup saat game mulai
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Setup tombol-tombol jika referensi sudah di-assign
        SetupButtons();

        // Load volume yang tersimpan
        SetupVolumeSlider();
    }

    void Update()
    {
        // Deteksi tombol ESC untuk toggle menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                // Jika settings terbuka, tutup settings dulu
                CloseSettings();
            }
            else if (isPaused)
            {
                // Jika menu pause terbuka, resume game
                ResumeGame();
            }
            else
            {
                // Jika tidak, buka menu pause
                PauseGame();
            }
        }
    }

    /// <summary>
    /// Setup listener untuk tombol-tombol
    /// </summary>
    void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMainMenu);

        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(CloseSettings);
    }

    /// <summary>
    /// Setup slider volume
    /// </summary>
    void SetupVolumeSlider()
    {
        if (volumeSlider != null)
        {
            // Load volume tersimpan (default 1.0)
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;

            // Daftarkan listener
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // ==================== FUNGSI PUBLIK (Untuk OnClick di Inspector) ====================

    /// <summary>
    /// Membuka menu pause dan menghentikan waktu game
    /// </summary>
    public void PauseGame()
    {
        if (pauseMenuPanel == null)
        {
            Debug.LogError("PauseMenuUI: pauseMenuPanel belum di-assign! Drag PauseMenuPanel ke Inspector.");
            return;
        }

        isPaused = true;
        pauseMenuPanel.SetActive(true);
        
        // Hentikan waktu game
        Time.timeScale = 0f;
        
        // Opsional: Tampilkan cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Melanjutkan game dan menutup menu pause
    /// </summary>
    public void ResumeGame()
    {
        if (pauseMenuPanel == null) return;

        isPaused = false;
        pauseMenuPanel.SetActive(false);
        
        // Lanjutkan waktu game
        Time.timeScale = 1f;
        
        // Opsional: Sembunyikan cursor (tergantung jenis game)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    /// <summary>
    /// Restart level saat ini
    /// </summary>
    public void RestartLevel()
    {
        // Pastikan time scale kembali normal
        Time.timeScale = 1f;
        isPaused = false;

        // Muat ulang scene yang sedang aktif
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Membuka panel settings
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel != null && pauseMenuPanel != null)
        {
            // Sembunyikan menu pause, tampilkan settings
            pauseMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Menutup panel settings dan kembali ke menu pause
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            pauseMenuPanel.SetActive(true);
        }

        // Simpan settings
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Keluar ke Main Menu
    /// </summary>
    public void ExitToMainMenu()
    {
        // Pastikan time scale kembali normal
        Time.timeScale = 1f;
        isPaused = false;

        // Muat scene Main Menu
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            // Jika kosong, muat scene pertama di Build Settings
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Keluar dari aplikasi sepenuhnya (Quit Game)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Game Exited");
        
        // Keluar dari aplikasi
        Application.Quit();

        // Jika di Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    /// <summary>
    /// Mengubah volume game
    /// </summary>
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    // ==================== PROPERTI ====================

    /// <summary>
    /// Cek apakah game sedang di-pause
    /// </summary>
    public bool IsPaused => isPaused;
}
