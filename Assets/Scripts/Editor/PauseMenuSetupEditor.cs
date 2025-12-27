using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor Script untuk membuat Pause Menu UI secara otomatis.
/// Jalankan via Menu: Tools > Dungeon Escape > Setup Pause Menu UI
/// </summary>
public class PauseMenuSetupEditor : Editor
{
    [MenuItem("Tools/Dungeon Escape/Setup Pause Menu UI")]
    public static void CreatePauseMenuUI()
    {
        // ===== 1. BUAT CANVAS (jika belum ada) =====
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        GameObject canvasGO;

        if (existingCanvas != null)
        {
            canvasGO = existingCanvas.gameObject;
            Debug.Log("PauseMenuSetup: Menggunakan Canvas yang sudah ada.");
        }
        else
        {
            canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Di atas UI lain
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("PauseMenuSetup: Canvas baru dibuat.");
        }

        // Pastikan ada EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ===== 2. BUAT PANEL PAUSE MENU =====
        GameObject pausePanel = CreatePanel(canvasGO.transform, "PauseMenuPanel", new Color(0, 0, 0, 0.85f));
        pausePanel.SetActive(false); // Default hidden

        // Container untuk tombol (Vertical Layout)
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(pausePanel.transform);
        RectTransform containerRT = buttonContainer.AddComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.anchoredPosition = Vector2.zero;
        containerRT.sizeDelta = new Vector2(300, 350);

        VerticalLayoutGroup vlg = buttonContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        // Judul "PAUSED"
        GameObject titleGO = CreateText(buttonContainer.transform, "TitleText", "PAUSED", 48, Color.white, TextAnchor.MiddleCenter);
        titleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);

        // Tombol-tombol
        GameObject resumeBtn = CreateButton(buttonContainer.transform, "ResumeButton", "RESUME", new Color(0.2f, 0.8f, 0.2f));
        GameObject restartBtn = CreateButton(buttonContainer.transform, "RestartButton", "RESTART", new Color(0.9f, 0.6f, 0.1f));
        GameObject settingsBtn = CreateButton(buttonContainer.transform, "SettingsButton", "SETTINGS", new Color(0.3f, 0.5f, 0.9f));
        GameObject exitBtn = CreateButton(buttonContainer.transform, "ExitButton", "EXIT", new Color(0.9f, 0.2f, 0.2f));

        // ===== 3. BUAT PANEL SETTINGS =====
        GameObject settingsPanel = CreatePanel(canvasGO.transform, "SettingsPanel", new Color(0, 0, 0, 0.9f));
        settingsPanel.SetActive(false);

        // Container Settings
        GameObject settingsContainer = new GameObject("SettingsContainer");
        settingsContainer.transform.SetParent(settingsPanel.transform);
        RectTransform settingsContainerRT = settingsContainer.AddComponent<RectTransform>();
        settingsContainerRT.anchorMin = new Vector2(0.5f, 0.5f);
        settingsContainerRT.anchorMax = new Vector2(0.5f, 0.5f);
        settingsContainerRT.pivot = new Vector2(0.5f, 0.5f);
        settingsContainerRT.anchoredPosition = Vector2.zero;
        settingsContainerRT.sizeDelta = new Vector2(400, 300);

        VerticalLayoutGroup settingsVLG = settingsContainer.AddComponent<VerticalLayoutGroup>();
        settingsVLG.spacing = 20;
        settingsVLG.childAlignment = TextAnchor.MiddleCenter;
        settingsVLG.childControlHeight = false;
        settingsVLG.childControlWidth = true;
        settingsVLG.childForceExpandHeight = false;
        settingsVLG.childForceExpandWidth = true;

        // Judul Settings
        GameObject settingsTitleGO = CreateText(settingsContainer.transform, "SettingsTitleText", "SETTINGS", 42, Color.white, TextAnchor.MiddleCenter);
        settingsTitleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);

        // Volume Label
        GameObject volumeLabelGO = CreateText(settingsContainer.transform, "VolumeLabel", "Volume", 24, Color.white, TextAnchor.MiddleCenter);
        volumeLabelGO.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 30);

        // Volume Slider
        GameObject sliderGO = CreateSlider(settingsContainer.transform, "VolumeSlider");

        // Back Button
        GameObject backBtn = CreateButton(settingsContainer.transform, "BackButton", "BACK", new Color(0.5f, 0.5f, 0.5f));

        // ===== 4. BUAT PAUSE MENU MANAGER (Script Holder) =====
        GameObject managerGO = new GameObject("PauseMenuManager");
        PauseMenuUI pauseMenuScript = managerGO.AddComponent<PauseMenuUI>();

        // Assign referensi via SerializedObject
        SerializedObject so = new SerializedObject(pauseMenuScript);
        so.FindProperty("pauseMenuPanel").objectReferenceValue = pausePanel;
        so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        so.FindProperty("resumeButton").objectReferenceValue = resumeBtn.GetComponent<Button>();
        so.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        so.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
        so.FindProperty("exitButton").objectReferenceValue = exitBtn.GetComponent<Button>();
        so.FindProperty("backFromSettingsButton").objectReferenceValue = backBtn.GetComponent<Button>();
        so.FindProperty("volumeSlider").objectReferenceValue = sliderGO.GetComponent<Slider>();
        so.ApplyModifiedProperties();

        // ===== 5. REGISTER UNDO & SELECT =====
        Undo.RegisterCreatedObjectUndo(managerGO, "Create Pause Menu UI");
        Undo.RegisterCreatedObjectUndo(pausePanel, "Create Pause Menu Panel");
        Undo.RegisterCreatedObjectUndo(settingsPanel, "Create Settings Panel");

        Selection.activeGameObject = managerGO;

        Debug.Log("✅ PAUSE MENU UI BERHASIL DIBUAT!\n" +
                  "• Panel Menu: PauseMenuPanel\n" +
                  "• Panel Settings: SettingsPanel\n" +
                  "• Manager: PauseMenuManager\n" +
                  "Tekan ESC saat Play untuk membuka menu.");
    }

    // ==================== HELPER FUNCTIONS ====================

    static GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject panelGO = new GameObject(name);
        panelGO.transform.SetParent(parent, false); // worldPositionStays = false

        RectTransform rt = panelGO.AddComponent<RectTransform>();
        
        // Reset local transform terlebih dahulu
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
        
        // Set anchor ke stretch-stretch (full screen)
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        
        // Set offset ke 0 (tidak ada margin)
        rt.offsetMin = Vector2.zero; // left, bottom
        rt.offsetMax = Vector2.zero; // right, top (negated)
        
        // Pivot di tengah
        rt.pivot = new Vector2(0.5f, 0.5f);

        Image img = panelGO.AddComponent<Image>();
        img.color = bgColor;

        return panelGO;
    }

    static GameObject CreateButton(Transform parent, string name, string buttonText, Color buttonColor)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(250, 50);
        rt.localScale = Vector3.one;

        Image img = btnGO.AddComponent<Image>();
        img.color = buttonColor;

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(buttonColor.r + 0.1f, buttonColor.g + 0.1f, buttonColor.b + 0.1f);
        colors.pressedColor = new Color(buttonColor.r - 0.1f, buttonColor.g - 0.1f, buttonColor.b - 0.1f);
        btn.colors = colors;

        // Text child
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        textRT.localScale = Vector3.one;

        Text text = textGO.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;

        return btnGO;
    }

    static GameObject CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);

        RectTransform rt = textGO.AddComponent<RectTransform>();
        rt.localScale = Vector3.one;

        Text text = textGO.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = FontStyle.Bold;

        return textGO;
    }

    static GameObject CreateSlider(Transform parent, string name)
    {
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(parent, false);

        RectTransform sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.sizeDelta = new Vector2(300, 30);
        sliderRT.localScale = Vector3.one;

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        // Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.25f);
        bgRT.anchorMax = new Vector2(1, 0.75f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bgRT.localScale = Vector3.one;
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        // Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1, 0.75f);
        fillAreaRT.offsetMin = new Vector2(5, 0);
        fillAreaRT.offsetMax = new Vector2(-5, 0);
        fillAreaRT.localScale = Vector3.one;

        // Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        RectTransform fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        fillRT.localScale = Vector3.one;
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.7f, 0.9f);

        // Handle Slide Area
        GameObject handleAreaGO = new GameObject("Handle Slide Area");
        handleAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform handleAreaRT = handleAreaGO.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = new Vector2(10, 0);
        handleAreaRT.offsetMax = new Vector2(-10, 0);
        handleAreaRT.localScale = Vector3.one;

        // Handle
        GameObject handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(handleAreaGO.transform, false);
        RectTransform handleRT = handleGO.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20, 0);
        handleRT.anchorMin = new Vector2(0, 0);
        handleRT.anchorMax = new Vector2(0, 1);
        handleRT.localScale = Vector3.one;
        Image handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Color.white;

        // Assign ke Slider
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;

        return sliderGO;
    }
}
