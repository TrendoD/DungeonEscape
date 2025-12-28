using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Script untuk scene Winner.
/// Menampilkan fade out dari hitam untuk memunculkan tampilan kemenangan.
/// Menangani tombol Back to Main Menu dan Quit.
/// </summary>
public class WinnerUI : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("UI Image hitam untuk efek fade out (harus full screen, alpha awal 1)")]
    public Image fadeOverlay;
    
    [Tooltip("Durasi fade out (dari hitam ke transparan)")]
    public float fadeOutDuration = 1.0f;
    
    [Tooltip("Delay sebelum fade out dimulai")]
    public float delayBeforeFadeOut = 0.3f;

    [Header("Button References")]
    [Tooltip("Tombol untuk kembali ke Main Menu")]
    public Button backToMenuButton;
    
    [Tooltip("Tombol untuk keluar dari game")]
    public Button quitButton;

    [Header("Scene Settings")]
    [Tooltip("Nama scene Main Menu")]
    public string mainMenuSceneName = "MainMenu";

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

        // Mulai fade out untuk menampilkan Winner screen
        StartCoroutine(FadeOutSequence());
    }

    /// <summary>
    /// Mencari tombol secara otomatis berdasarkan nama
    /// </summary>
    void AutoFindButtons()
    {
        if (backToMenuButton == null)
        {
            // Coba beberapa nama yang umum
            GameObject found = GameObject.Find("ButtonNext");
            if (found == null) found = GameObject.Find("BackToMenuButton");
            if (found == null) found = GameObject.Find("Button_BackToMenu");
            if (found != null) backToMenuButton = found.GetComponent<Button>();
        }

        if (quitButton == null)
        {
            GameObject found = GameObject.Find("ButtonQuit");
            if (found == null) found = GameObject.Find("QuitButton");
            if (found == null) found = GameObject.Find("Button_Quit");
            if (found != null) quitButton = found.GetComponent<Button>();
        }
    }

    /// <summary>
    /// Setup listener untuk tombol-tombol
    /// </summary>
    void SetupButtons()
    {
        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(BackToMainMenu);
            Debug.Log("WinnerUI: Back to Menu button connected.");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
            Debug.Log("WinnerUI: Quit button connected.");
        }
    }

    // ==================== BUTTON FUNCTIONS ====================

    /// <summary>
    /// Kembali ke Main Menu
    /// </summary>
    public void BackToMainMenu()
    {
        Debug.Log("WinnerUI: Back to Main Menu");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Keluar dari game sepenuhnya
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("WinnerUI: Quitting Game...");
        
        // Keluar dari aplikasi
        Application.Quit();

        // Jika di Unity Editor, hentikan play mode
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ==================== FADE ANIMATION ====================

    /// <summary>
    /// Sequence fade out untuk memunculkan tampilan Winner
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
            Debug.LogWarning("WinnerUI: FadeOverlay belum di-assign!");
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
