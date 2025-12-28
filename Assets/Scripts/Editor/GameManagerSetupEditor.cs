using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor script untuk setup GameManager dan UI Fade Overlay secara otomatis.
/// Akses melalui menu: Tools > Dungeon Escape > Setup GameManager
/// </summary>
public class GameManagerSetupEditor : EditorWindow
{
    [MenuItem("Tools/Dungeon Escape/Setup GameManager")]
    public static void ShowWindow()
    {
        GetWindow<GameManagerSetupEditor>("GameManager Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("GameManager Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Tool ini akan membuat:\n" +
            "1. GameObject 'GameManager' dengan script GameManager.cs\n" +
            "2. Canvas 'FadeCanvas' dengan Image hitam full screen\n" +
            "3. Auto-assign FadeOverlay ke GameManager",
            MessageType.Info);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Setup GameManager + Fade UI", GUILayout.Height(40)))
        {
            SetupGameManager();
        }

        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Catatan:\n" +
            "- Jalankan tool ini di setiap scene game (Map1, Map2, dll)\n" +
            "- Pastikan scene GameOver sudah ada di Build Settings",
            MessageType.Warning);
    }

    void SetupGameManager()
    {
        // 1. Cari atau buat GameManager
        GameManager existingGM = FindObjectOfType<GameManager>();
        GameObject gmObject;

        if (existingGM != null)
        {
            gmObject = existingGM.gameObject;
            Debug.Log("GameManagerSetup: GameManager sudah ada, menggunakan yang existing.");
        }
        else
        {
            gmObject = new GameObject("GameManager");
            gmObject.AddComponent<GameManager>();
            Debug.Log("GameManagerSetup: GameManager baru dibuat.");
        }

        GameManager gm = gmObject.GetComponent<GameManager>();

        // 2. Cari atau buat Fade Canvas
        Canvas fadeCanvas = null;
        GameObject canvasObj = GameObject.Find("FadeCanvas");
        
        if (canvasObj != null)
        {
            fadeCanvas = canvasObj.GetComponent<Canvas>();
            Debug.Log("GameManagerSetup: FadeCanvas sudah ada.");
        }
        else
        {
            // Buat Canvas baru
            canvasObj = new GameObject("FadeCanvas");
            fadeCanvas = canvasObj.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 999; // Di atas semua UI lain
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("GameManagerSetup: FadeCanvas baru dibuat.");
        }

        // 3. Cari atau buat FadeOverlay Image
        Image fadeOverlay = null;
        Transform existingOverlay = canvasObj.transform.Find("FadeOverlay");
        
        if (existingOverlay != null)
        {
            fadeOverlay = existingOverlay.GetComponent<Image>();
            Debug.Log("GameManagerSetup: FadeOverlay sudah ada.");
        }
        else
        {
            // Buat Image baru
            GameObject imageObj = new GameObject("FadeOverlay");
            imageObj.transform.SetParent(canvasObj.transform, false);
            
            fadeOverlay = imageObj.AddComponent<Image>();
            fadeOverlay.color = new Color(0, 0, 0, 0); // Hitam transparan
            
            // Stretch ke full screen
            RectTransform rt = fadeOverlay.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            // Nonaktifkan raycast agar tidak menghalangi input
            fadeOverlay.raycastTarget = false;
            
            Debug.Log("GameManagerSetup: FadeOverlay Image dibuat.");
        }

        // 4. Assign FadeOverlay ke GameManager
        gm.fadeOverlay = fadeOverlay;

        // 5. Mark scene sebagai dirty agar perubahan tersimpan
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // Pilih GameManager di Hierarchy
        Selection.activeGameObject = gmObject;

        EditorUtility.DisplayDialog(
            "Setup Complete", 
            "GameManager dan FadeCanvas berhasil dibuat!\n\n" +
            "Jangan lupa:\n" +
            "1. Save scene (Ctrl+S)\n" +
            "2. Pastikan scene 'GameOver' ada di Build Settings",
            "OK");

        Debug.Log("GameManagerSetup: Setup selesai!");
    }
}
