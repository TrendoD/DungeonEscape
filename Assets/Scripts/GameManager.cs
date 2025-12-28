using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Singleton GameManager untuk mengelola kondisi Game Over.
/// Menampilkan animasi fade to black lalu transisi ke scene GameOver.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton Instance
    public static GameManager Instance { get; private set; }

    [Header("Fade Settings")]
    [Tooltip("UI Image hitam untuk efek fade (harus full screen, alpha awal 0)")]
    public Image fadeOverlay;
    
    [Tooltip("Durasi fade ke hitam (detik)")]
    public float fadeInDuration = 0.5f;
    
    [Tooltip("Delay sebelum pindah scene setelah fade selesai")]
    public float delayBeforeSceneLoad = 0.3f;

    [Header("Scene Settings")]
    [Tooltip("Nama scene Game Over")]
    public string gameOverSceneName = "GameOver";

    [Header("Debug")]
    [Tooltip("Tampilkan log debug di Console")]
    public bool showDebugLogs = true;

    // Status internal
    private bool isGameOver = false;
    private CanvasGroup fadeCanvasGroup;

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-find fade overlay jika belum di-assign
        AutoFindFadeOverlay();
    }

    void Start()
    {
        // Pastikan fade overlay transparan di awal
        if (fadeOverlay != null)
        {
            SetFadeAlpha(0f);
        }
    }

    /// <summary>
    /// Mencari FadeOverlay secara otomatis jika belum di-assign
    /// </summary>
    void AutoFindFadeOverlay()
    {
        if (fadeOverlay == null)
        {
            // Coba cari berdasarkan nama
            GameObject found = GameObject.Find("FadeOverlay");
            if (found != null)
            {
                fadeOverlay = found.GetComponent<Image>();
                if (showDebugLogs) Debug.Log("GameManager: Auto-assigned FadeOverlay");
            }
        }
    }

    /// <summary>
    /// Trigger kondisi Game Over dengan animasi fade
    /// </summary>
    /// <param name="reason">Alasan kalah (untuk logging/UI nanti)</param>
    public void TriggerGameOver(string reason = "")
    {
        // Cegah multiple trigger
        if (isGameOver)
        {
            if (showDebugLogs) Debug.Log("GameManager: Game Over sudah aktif, mengabaikan trigger baru.");
            return;
        }

        isGameOver = true;
        
        if (showDebugLogs) Debug.Log($"GameManager: GAME OVER - {reason}");

        // Mulai sequence Game Over
        StartCoroutine(GameOverSequence(reason));
    }

    /// <summary>
    /// Coroutine untuk menjalankan animasi fade dan transisi scene
    /// </summary>
    IEnumerator GameOverSequence(string reason)
    {
        // Hentikan pergerakan player (opsional - bisa diexpand)
        Time.timeScale = 1f; // Pastikan time scale normal

        // 1. Langsung hitam (instant, tanpa fade in)
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            SetFadeAlpha(1f); // Langsung gelap total
        }

        // 2. Tunggu sebentar di layar hitam
        yield return new WaitForSeconds(delayBeforeSceneLoad);

        // 3. Simpan scene saat ini untuk Retry nanti
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastPlayedScene", currentSceneName);
        
        // 4. Simpan alasan kalah untuk ditampilkan di scene GameOver (opsional)
        PlayerPrefs.SetString("GameOverReason", reason);
        PlayerPrefs.Save();

        // 5. Pindah ke scene GameOver (fade out akan dilakukan di scene GameOver)
        SceneManager.LoadScene(gameOverSceneName);
    }

    // FadeIn tidak lagi digunakan, tapi dipertahankan untuk keperluan lain
    /// <summary>
    /// Animasi fade in (dari transparan ke hitam) - TIDAK DIGUNAKAN
    /// </summary>
    IEnumerator FadeIn()
    {
        if (fadeOverlay == null)
        {
            Debug.LogWarning("GameManager: FadeOverlay belum di-assign! Langsung pindah scene.");
            yield break;
        }

        float elapsed = 0f;
        
        // Pastikan overlay aktif
        fadeOverlay.gameObject.SetActive(true);

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        // Pastikan alpha = 1 di akhir
        SetFadeAlpha(1f);
    }

    /// <summary>
    /// Set nilai alpha pada fade overlay
    /// </summary>
    void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay != null)
        {
            Color color = fadeOverlay.color;
            color.a = alpha;
            fadeOverlay.color = color;
        }
    }

    /// <summary>
    /// Reset status game over (dipanggil saat restart level)
    /// </summary>
    public void ResetGameOver()
    {
        isGameOver = false;
        if (fadeOverlay != null)
        {
            SetFadeAlpha(0f);
        }
    }

    /// <summary>
    /// Cek apakah game sudah dalam kondisi Game Over
    /// </summary>
    public bool IsGameOver => isGameOver;

    // ==================== DEBUG ====================

    void Update()
    {
        // Debug: Tekan G untuk test Game Over (hapus di build final)
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.G) && !isGameOver)
        {
            TriggerGameOver("Debug Test");
        }
        #endif
    }
}
