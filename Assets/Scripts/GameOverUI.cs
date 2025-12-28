using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Script untuk scene GameOver.
/// Menampilkan fade out dari hitam untuk memunculkan tampilan Game Over.
/// Juga menangani tombol Retry dan Quit.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("UI Image hitam untuk efek fade out (harus full screen, alpha awal 1)")]
    public Image fadeOverlay;
    
    [Tooltip("Durasi fade out (dari hitam ke transparan)")]
    public float fadeOutDuration = 0.8f;
    
    [Tooltip("Delay sebelum fade out dimulai")]
    public float delayBeforeFadeOut = 0.2f;

    [Header("Button References")]
    [Tooltip("Tombol Retry untuk kembali ke level sebelumnya")]
    public Button retryButton;
    
    [Tooltip("Tombol Quit untuk kembali ke Main Menu")]
    public Button quitButton;

    [Header("Scene Settings")]
    [Tooltip("Nama scene Main Menu")]
    public string mainMenuSceneName = "MainMenu";

    [Header("UI References (Opsional)")]
    [Tooltip("Text untuk menampilkan alasan kalah")]
    public UnityEngine.UI.Text reasonText;
    
    [Tooltip("Atau gunakan TextMeshPro")]
    public TMPro.TextMeshProUGUI reasonTextTMP;

    void Start()
    {
        // Auto-find fade overlay jika belum di-assign
        if (fadeOverlay == null)
        {
            GameObject found = GameObject.Find("FadeOverlay");
            if (found != null)
            {
                fadeOverlay = found.GetComponent<Image>();
            }
        }

        // Auto-find buttons jika belum di-assign
        AutoFindButtons();

        // Setup button listeners
        SetupButtons();

        // Pastikan overlay hitam di awal
        if (fadeOverlay != null)
        {
            SetFadeAlpha(1f);
        }

        // Tampilkan alasan kalah jika ada
        DisplayGameOverReason();

        // Mulai fade out
        StartCoroutine(FadeOutSequence());
    }

    /// <summary>
    /// Mencari tombol secara otomatis berdasarkan nama
    /// </summary>
    void AutoFindButtons()
    {
        if (retryButton == null)
        {
            GameObject found = GameObject.Find("Button_Retry");
            if (found != null) retryButton = found.GetComponent<Button>();
        }

        if (quitButton == null)
        {
            GameObject found = GameObject.Find("Button_quit");
            if (found != null) quitButton = found.GetComponent<Button>();
        }
    }

    /// <summary>
    /// Setup listener untuk tombol-tombol
    /// </summary>
    void SetupButtons()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryGame);
            Debug.Log("GameOverUI: Retry button connected.");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitToMainMenu);
            Debug.Log("GameOverUI: Quit button connected.");
        }
    }

    // ==================== BUTTON FUNCTIONS ====================

    /// <summary>
    /// Retry - Kembali ke scene yang sedang dimainkan sebelumnya
    /// </summary>
    public void RetryGame()
    {
        // Ambil nama scene terakhir yang dimainkan
        string lastScene = PlayerPrefs.GetString("LastPlayedScene", "");

        if (!string.IsNullOrEmpty(lastScene))
        {
            Debug.Log($"GameOverUI: Retry - Loading scene: {lastScene}");
            SceneManager.LoadScene(lastScene);
        }
        else
        {
            // Fallback: jika tidak ada scene tersimpan, kembali ke MainMenu
            Debug.LogWarning("GameOverUI: LastPlayedScene tidak ditemukan, kembali ke MainMenu.");
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    /// <summary>
    /// Quit - Kembali ke Main Menu
    /// </summary>
    public void QuitToMainMenu()
    {
        Debug.Log("GameOverUI: Quit to Main Menu");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ==================== DISPLAY & FADE ====================

    /// <summary>
    /// Menampilkan alasan kalah dari PlayerPrefs
    /// </summary>
    void DisplayGameOverReason()
    {
        string reason = PlayerPrefs.GetString("GameOverReason", "");
        
        if (!string.IsNullOrEmpty(reason))
        {
            if (reasonTextTMP != null)
            {
                reasonTextTMP.text = reason;
            }
            else if (reasonText != null)
            {
                reasonText.text = reason;
            }
            
            Debug.Log($"GameOver Reason: {reason}");
        }
    }

    /// <summary>
    /// Sequence fade out untuk memunculkan tampilan GameOver
    /// </summary>
    IEnumerator FadeOutSequence()
    {
        // Tunggu sebentar sebelum fade out
        yield return new WaitForSeconds(delayBeforeFadeOut);

        // Fade out (dari hitam ke transparan)
        yield return StartCoroutine(FadeOut());
    }

    /// <summary>
    /// Animasi fade out (dari hitam ke transparan)
    /// </summary>
    IEnumerator FadeOut()
    {
        if (fadeOverlay == null)
        {
            Debug.LogWarning("GameOverUI: FadeOverlay belum di-assign!");
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        // Pastikan alpha = 0 di akhir dan nonaktifkan overlay
        SetFadeAlpha(0f);
        fadeOverlay.gameObject.SetActive(false);
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
}
