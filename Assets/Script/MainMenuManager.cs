using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Nama scene yang akan dimuat saat tombol Play ditekan. Jika kosong, akan memuat scene berikutnya di Build Settings.")]
    [SerializeField] private string nextSceneName;

    /// <summary>
    /// Fungsi untuk memulai permainan.
    /// Panggil fungsi ini dari event OnClick tombol Play.
    /// </summary>
    public void PlayGame()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Memuat scene selanjutnya berdasarkan urutan di Build Settings
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            
            // Cek apakah index selanjutnya valid
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("Tidak ada scene selanjutnya di Build Settings!");
            }
        }
    }

    /// <summary>
    /// Fungsi untuk keluar dari aplikasi.
    /// Panggil fungsi ini dari event OnClick tombol Exit.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Game Exited");
        
        // Keluar dari aplikasi (hanya bekerja pada build aplikasi, bukan di Editor)
        Application.Quit();

        // Jika dijalankan di dalam Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
