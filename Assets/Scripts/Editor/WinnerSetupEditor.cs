using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor script untuk setup WinnerUI dan efek visual di scene Winner.
/// Akses melalui menu: Tools > Dungeon Escape > Setup Winner Scene
/// </summary>
public class WinnerSetupEditor : EditorWindow
{
    [MenuItem("Tools/Dungeon Escape/Setup Winner Scene")]
    public static void ShowWindow()
    {
        GetWindow<WinnerSetupEditor>("Winner Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Winner Scene Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Tool ini akan membuat:\n" +
            "1. GameObject 'WinnerManager' dengan script WinnerUI.cs\n" +
            "2. Canvas 'FadeCanvas' dengan Image hitam full screen (alpha = 1)\n" +
            "3. Auto-assign FadeOverlay ke WinnerUI\n\n" +
            "PENTING: Jalankan ini HANYA di scene Winner!",
            MessageType.Info);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Setup Winner + Fade UI", GUILayout.Height(40)))
        {
            SetupWinner();
        }

        EditorGUILayout.Space(20);
        
        // ==================== DARK ATMOSPHERE SECTION ====================
        GUILayout.Label("Dark Atmosphere", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Menambahkan efek gelap:\n" +
            "• Background kamera hitam\n" +
            "• Vignette (gelap di ujung-ujung layar)",
            MessageType.Info);

        if (GUILayout.Button("Apply Dark Atmosphere", GUILayout.Height(35)))
        {
            ApplyDarkAtmosphere();
        }
    }

    void SetupWinner()
    {
        // 1. Cari atau buat WinnerManager
        WinnerUI existingUI = FindObjectOfType<WinnerUI>();
        GameObject managerObject;

        if (existingUI != null)
        {
            managerObject = existingUI.gameObject;
            Debug.Log("WinnerSetup: WinnerUI sudah ada, menggunakan yang existing.");
        }
        else
        {
            managerObject = new GameObject("WinnerManager");
            managerObject.AddComponent<WinnerUI>();
            Debug.Log("WinnerSetup: WinnerManager baru dibuat.");
        }

        WinnerUI winnerUI = managerObject.GetComponent<WinnerUI>();

        // 2. Cari atau buat Fade Canvas
        Canvas fadeCanvas = null;
        GameObject canvasObj = GameObject.Find("FadeCanvas");
        
        if (canvasObj != null)
        {
            fadeCanvas = canvasObj.GetComponent<Canvas>();
            Debug.Log("WinnerSetup: FadeCanvas sudah ada.");
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
            
            Debug.Log("WinnerSetup: FadeCanvas baru dibuat.");
        }

        // 3. Cari atau buat FadeOverlay Image
        Image fadeOverlay = null;
        Transform existingOverlay = canvasObj.transform.Find("FadeOverlay");
        
        if (existingOverlay != null)
        {
            fadeOverlay = existingOverlay.GetComponent<Image>();
            Debug.Log("WinnerSetup: FadeOverlay sudah ada.");
        }
        else
        {
            // Buat Image baru
            GameObject imageObj = new GameObject("FadeOverlay");
            imageObj.transform.SetParent(canvasObj.transform, false);
            
            fadeOverlay = imageObj.AddComponent<Image>();
            fadeOverlay.color = new Color(0, 0, 0, 1); // Hitam penuh (alpha = 1)
            
            // Stretch ke full screen
            RectTransform rt = fadeOverlay.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            
            // Nonaktifkan raycast agar tidak menghalangi input
            fadeOverlay.raycastTarget = false;
            
            Debug.Log("WinnerSetup: FadeOverlay Image dibuat (alpha = 1, hitam penuh).");
        }

        // 4. Assign FadeOverlay ke WinnerUI
        winnerUI.fadeOverlay = fadeOverlay;

        // 5. Mark scene sebagai dirty agar perubahan tersimpan
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // Pilih WinnerManager di Hierarchy
        Selection.activeGameObject = managerObject;

        EditorUtility.DisplayDialog(
            "Setup Complete", 
            "WinnerManager dan FadeCanvas berhasil dibuat!\n\n" +
            "Jangan lupa:\n" +
            "1. Save scene (Ctrl+S)\n" +
            "2. FadeOverlay sudah diset hitam penuh (alpha = 1)\n" +
            "3. Saat scene dimulai, akan fade out untuk menampilkan Winner",
            "OK");

        Debug.Log("WinnerSetup: Setup selesai!");
    }

    /// <summary>
    /// Apply dark atmosphere: black camera background + vignette overlay
    /// </summary>
    void ApplyDarkAtmosphere()
    {
        // 1. Set camera background to black
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = Color.black;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            Debug.Log("WinnerSetup: Camera background diset ke hitam.");
        }
        else
        {
            Debug.LogWarning("WinnerSetup: Main Camera tidak ditemukan!");
        }

        // 2. Cari atau buat Vignette Canvas (terpisah dari FadeCanvas)
        GameObject vignetteCanvasObj = GameObject.Find("VignetteCanvas");
        Canvas vignetteCanvas;
        
        if (vignetteCanvasObj == null)
        {
            vignetteCanvasObj = new GameObject("VignetteCanvas");
            vignetteCanvas = vignetteCanvasObj.AddComponent<Canvas>();
            vignetteCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            vignetteCanvas.sortingOrder = 10; // Di bawah FadeCanvas (999) tapi di atas konten
            
            vignetteCanvasObj.AddComponent<CanvasScaler>();
            vignetteCanvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            vignetteCanvas = vignetteCanvasObj.GetComponent<Canvas>();
        }

        // 3. Buat Vignette overlay menggunakan 4 gradient panels di setiap tepi
        CreateVignetteEdges(vignetteCanvasObj);

        // 4. Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Dark Atmosphere Applied",
            "✓ Camera background: Hitam\n" +
            "✓ Vignette overlay: Ditambahkan\n\n" +
            "Jangan lupa Save scene (Ctrl+S)!",
            "OK");

        Debug.Log("WinnerSetup: Dark atmosphere applied!");
    }

    /// <summary>
    /// Membuat vignette dengan panel gelap di setiap tepi layar
    /// </summary>
    void CreateVignetteEdges(GameObject parentCanvas)
    {
        // Hapus vignette lama jika ada
        Transform existingVignette = parentCanvas.transform.Find("VignetteContainer");
        if (existingVignette != null)
        {
            DestroyImmediate(existingVignette.gameObject);
        }

        // Container untuk semua vignette edges
        GameObject container = new GameObject("VignetteContainer");
        container.transform.SetParent(parentCanvas.transform, false);
        
        RectTransform containerRT = container.AddComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero;
        containerRT.anchorMax = Vector2.one;
        containerRT.offsetMin = Vector2.zero;
        containerRT.offsetMax = Vector2.zero;

        // Warna vignette (hitam semi-transparan)
        Color vignetteColor = new Color(0, 0, 0, 0.7f);
        float edgeSize = 150f; // Ukuran area gelap di tepi

        // Top edge
        CreateVignettePanel(container, "VignetteTop", vignetteColor,
            new Vector2(0, 1), new Vector2(1, 1), // Anchor di atas
            new Vector2(0, -edgeSize), Vector2.zero); // Offset

        // Bottom edge
        CreateVignettePanel(container, "VignetteBottom", vignetteColor,
            new Vector2(0, 0), new Vector2(1, 0), // Anchor di bawah
            Vector2.zero, new Vector2(0, edgeSize)); // Offset

        // Left edge
        CreateVignettePanel(container, "VignetteLeft", vignetteColor,
            new Vector2(0, 0), new Vector2(0, 1), // Anchor di kiri
            Vector2.zero, new Vector2(edgeSize, 0)); // Offset

        // Right edge
        CreateVignettePanel(container, "VignetteRight", vignetteColor,
            new Vector2(1, 0), new Vector2(1, 1), // Anchor di kanan
            new Vector2(-edgeSize, 0), Vector2.zero); // Offset

        // Corner overlays untuk efek lebih kuat di sudut
        float cornerSize = 200f;
        Color cornerColor = new Color(0, 0, 0, 0.5f);
        
        CreateCornerVignette(container, "VignetteCornerTL", cornerColor, cornerSize, 
            new Vector2(0, 1), new Vector2(0, -1)); // Top-Left
        CreateCornerVignette(container, "VignetteCornerTR", cornerColor, cornerSize, 
            new Vector2(1, 1), new Vector2(-1, -1)); // Top-Right
        CreateCornerVignette(container, "VignetteCornerBL", cornerColor, cornerSize, 
            new Vector2(0, 0), new Vector2(0, 1)); // Bottom-Left
        CreateCornerVignette(container, "VignetteCornerBR", cornerColor, cornerSize, 
            new Vector2(1, 0), new Vector2(-1, 1)); // Bottom-Right

        Debug.Log("WinnerSetup: Vignette edges dibuat.");
    }

    void CreateVignettePanel(GameObject parent, string name, Color color, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);

        Image img = panel.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    void CreateCornerVignette(GameObject parent, string name, Color color, float size, 
        Vector2 anchor, Vector2 pivotDir)
    {
        GameObject corner = new GameObject(name);
        corner.transform.SetParent(parent.transform, false);

        Image img = corner.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        RectTransform rt = corner.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = Vector2.zero;
    }
}
