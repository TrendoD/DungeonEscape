using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor Tool untuk setup Volume Settings dengan mudah.
/// Akses via menu: Tools > Dungeon Escape > Volume Settings Setup
/// </summary>
public class VolumeSettingsSetupEditor : EditorWindow
{
    private GameObject settingsPanel;
    private bool createLabels = true;
    private bool useExistingSlider = false;
    private Slider existingSlider;

    [MenuItem("Tools/Dungeon Escape/Volume Settings Setup")]
    public static void ShowWindow()
    {
        GetWindow<VolumeSettingsSetupEditor>("Volume Settings Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Volume Settings Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // ==================== SECTION 1: AudioManager ====================
        EditorGUILayout.LabelField("1. Setup AudioManager", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("AudioManager akan dibuat otomatis saat game mulai. Klik tombol ini jika ingin membuatnya manual di scene.", MessageType.Info);
        
        if (GUILayout.Button("Create AudioManager in Scene"))
        {
            CreateAudioManager();
        }
        
        EditorGUILayout.Space(15);

        // ==================== SECTION 2: BackgroundMusicManager ====================
        EditorGUILayout.LabelField("2. Setup Background Music", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Pilih GameObject dengan AudioSource background music, lalu klik tombol di bawah.", MessageType.Info);
        
        if (GUILayout.Button("Add BackgroundMusicManager to Selected"))
        {
            AddBackgroundMusicManager();
        }
        
        EditorGUILayout.Space(15);

        // ==================== SECTION 3: Volume Sliders ====================
        EditorGUILayout.LabelField("3. Setup Volume Sliders", EditorStyles.boldLabel);
        
        settingsPanel = (GameObject)EditorGUILayout.ObjectField("Settings Panel", settingsPanel, typeof(GameObject), true);
        createLabels = EditorGUILayout.Toggle("Create Labels", createLabels);
        
        EditorGUILayout.Space(5);
        
        useExistingSlider = EditorGUILayout.Toggle("Use Existing Slider as Template", useExistingSlider);
        if (useExistingSlider)
        {
            existingSlider = (Slider)EditorGUILayout.ObjectField("Template Slider", existingSlider, typeof(Slider), true);
        }
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Akan membuat 3 slider (Master, Music, SFX) di dalam Settings Panel yang dipilih.", MessageType.Info);
        
        if (GUILayout.Button("Create Volume Sliders"))
        {
            CreateVolumeSliders();
        }
        
        EditorGUILayout.Space(15);

        // ==================== SECTION 4: Auto-Assign ====================
        EditorGUILayout.LabelField("4. Auto-Assign Sliders", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Otomatis assign slider ke SettingsMenu atau PauseMenuUI yang ada di scene.", MessageType.Info);
        
        if (GUILayout.Button("Auto-Assign Sliders to Scripts"))
        {
            AutoAssignSliders();
        }
    }

    private void CreateAudioManager()
    {
        // Cek apakah sudah ada AudioManager di scene
        AudioManager existing = FindObjectOfType<AudioManager>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Info", "AudioManager sudah ada di scene!", "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        GameObject audioManager = new GameObject("AudioManager");
        audioManager.AddComponent<AudioManager>();
        
        Undo.RegisterCreatedObjectUndo(audioManager, "Create AudioManager");
        Selection.activeGameObject = audioManager;
        
        EditorUtility.DisplayDialog("Success", "AudioManager berhasil dibuat!", "OK");
    }

    private void AddBackgroundMusicManager()
    {
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Error", "Pilih GameObject dengan AudioSource terlebih dahulu!", "OK");
            return;
        }

        AudioSource audioSource = selected.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            EditorUtility.DisplayDialog("Error", "GameObject yang dipilih tidak memiliki AudioSource!", "OK");
            return;
        }

        // Cek apakah sudah ada BackgroundMusicManager
        BackgroundMusicManager existing = selected.GetComponent<BackgroundMusicManager>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Info", "BackgroundMusicManager sudah ada di GameObject ini!", "OK");
            return;
        }

        Undo.AddComponent<BackgroundMusicManager>(selected);
        EditorUtility.DisplayDialog("Success", $"BackgroundMusicManager ditambahkan ke {selected.name}!", "OK");
    }

    private void CreateVolumeSliders()
    {
        if (settingsPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "Pilih Settings Panel terlebih dahulu!", "OK");
            return;
        }

        // Buat container untuk sliders
        GameObject sliderContainer = new GameObject("VolumeSliders");
        sliderContainer.transform.SetParent(settingsPanel.transform, false);
        
        RectTransform containerRect = sliderContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(300, 200);
        containerRect.anchoredPosition = Vector2.zero;

        // Tambah Vertical Layout Group
        var layoutGroup = sliderContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 20;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        // Buat 3 slider
        CreateSliderWithLabel(sliderContainer.transform, "MasterSlider", "Master Volume");
        CreateSliderWithLabel(sliderContainer.transform, "MusicSlider", "Music Volume");
        CreateSliderWithLabel(sliderContainer.transform, "SFXSlider", "SFX Volume");

        Undo.RegisterCreatedObjectUndo(sliderContainer, "Create Volume Sliders");
        Selection.activeGameObject = sliderContainer;

        EditorUtility.DisplayDialog("Success", "3 Volume Sliders berhasil dibuat!\n\nJangan lupa jalankan 'Auto-Assign Sliders to Scripts' untuk menghubungkan ke SettingsMenu/PauseMenuUI.", "OK");
    }

    private void CreateSliderWithLabel(Transform parent, string sliderName, string labelText)
    {
        // Container untuk label + slider
        GameObject container = new GameObject(sliderName + "Container");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(280, 50);

        // Horizontal layout
        var hLayout = container.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10;
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;

        if (createLabels)
        {
            // Label
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(container.transform, false);
            
            var labelRect = labelGO.AddComponent<RectTransform>();
            var layoutElement = labelGO.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 100;
            layoutElement.flexibleWidth = 0;
            
            var text = labelGO.AddComponent<TextMeshProUGUI>();
            text.text = labelText;
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Left;
            text.color = Color.white;
        }

        // Slider
        GameObject sliderGO;
        
        if (useExistingSlider && existingSlider != null)
        {
            // Duplicate existing slider
            sliderGO = Instantiate(existingSlider.gameObject);
            sliderGO.name = sliderName;
            sliderGO.transform.SetParent(container.transform, false);
        }
        else
        {
            // Create new slider from scratch (using Unity's default slider)
            sliderGO = CreateDefaultSlider(sliderName);
            sliderGO.transform.SetParent(container.transform, false);
        }

        // Add layout element ke slider
        var sliderLayout = sliderGO.GetComponent<LayoutElement>();
        if (sliderLayout == null)
            sliderLayout = sliderGO.AddComponent<LayoutElement>();
        sliderLayout.flexibleWidth = 1;
        sliderLayout.preferredHeight = 30;

        // Setup slider value
        Slider slider = sliderGO.GetComponent<Slider>();
        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
        }
    }

    private GameObject CreateDefaultSlider(string name)
    {
        // Gunakan DefaultControls untuk membuat slider standar Unity
        var resources = new DefaultControls.Resources();
        GameObject sliderGO = DefaultControls.CreateSlider(resources);
        sliderGO.name = name;
        return sliderGO;
    }

    private void AutoAssignSliders()
    {
        // Cari slider-slider
        Slider masterSlider = FindSliderByName("MasterSlider");
        Slider musicSlider = FindSliderByName("MusicSlider");
        Slider sfxSlider = FindSliderByName("SFXSlider");

        if (masterSlider == null && musicSlider == null && sfxSlider == null)
        {
            EditorUtility.DisplayDialog("Error", "Tidak menemukan slider dengan nama MasterSlider, MusicSlider, atau SFXSlider di scene!", "OK");
            return;
        }

        int assignedCount = 0;

        // Assign ke SettingsMenu
        SettingsMenu settingsMenu = FindObjectOfType<SettingsMenu>();
        if (settingsMenu != null)
        {
            SerializedObject so = new SerializedObject(settingsMenu);
            
            if (masterSlider != null)
            {
                so.FindProperty("masterSlider").objectReferenceValue = masterSlider;
            }
            if (musicSlider != null)
            {
                so.FindProperty("musicSlider").objectReferenceValue = musicSlider;
            }
            if (sfxSlider != null)
            {
                so.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            }
            
            so.ApplyModifiedProperties();
            assignedCount++;
            Debug.Log("Sliders assigned to SettingsMenu");
        }

        // Assign ke PauseMenuUI
        PauseMenuUI pauseMenu = FindObjectOfType<PauseMenuUI>();
        if (pauseMenu != null)
        {
            SerializedObject so = new SerializedObject(pauseMenu);
            
            if (masterSlider != null)
            {
                so.FindProperty("masterSlider").objectReferenceValue = masterSlider;
            }
            if (musicSlider != null)
            {
                so.FindProperty("musicSlider").objectReferenceValue = musicSlider;
            }
            if (sfxSlider != null)
            {
                so.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            }
            
            so.ApplyModifiedProperties();
            assignedCount++;
            Debug.Log("Sliders assigned to PauseMenuUI");
        }

        if (assignedCount > 0)
        {
            EditorUtility.DisplayDialog("Success", $"Sliders berhasil di-assign ke {assignedCount} script(s)!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", "Tidak menemukan SettingsMenu atau PauseMenuUI di scene!", "OK");
        }
    }

    private Slider FindSliderByName(string name)
    {
        Slider[] allSliders = FindObjectsOfType<Slider>(true);
        foreach (Slider slider in allSliders)
        {
            if (slider.gameObject.name == name)
            {
                return slider;
            }
        }
        return null;
    }
}
