using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script sederhana untuk tombol HUD yang membuka menu pause.
/// Attach script ini ke tombol UI (ikon gear/settings) yang sudah dibuat.
/// Script akan otomatis mencari PauseMenuUI dan memanggil PauseGame().
/// </summary>
[RequireComponent(typeof(Button))]
public class PauseHudButton : MonoBehaviour
{
    [Header("Referensi (Opsional - akan dicari otomatis)")]
    [Tooltip("Referensi ke PauseMenuUI. Jika kosong, akan dicari otomatis.")]
    [SerializeField] private PauseMenuUI pauseMenuUI;

    private Button button;

    void Awake()
    {
        // Ambil komponen Button
        button = GetComponent<Button>();

        // Cari PauseMenuUI jika belum di-assign
        if (pauseMenuUI == null)
        {
            pauseMenuUI = FindObjectOfType<PauseMenuUI>();
        }

        // Setup listener
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    /// <summary>
    /// Dipanggil saat tombol diklik
    /// </summary>
    void OnButtonClick()
    {
        if (pauseMenuUI != null)
        {
            // Jika game belum di-pause, buka menu
            if (!pauseMenuUI.IsPaused)
            {
                pauseMenuUI.PauseGame();
            }
            else
            {
                // Jika sudah di-pause, resume game
                pauseMenuUI.ResumeGame();
            }
        }
        else
        {
            Debug.LogWarning("PauseHudButton: Tidak menemukan PauseMenuUI di scene!");
        }
    }

    void OnDestroy()
    {
        // Hapus listener saat destroyed
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}
