using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor untuk setup PathGrid dengan mudah
/// </summary>
public class PathGridSetupEditor : EditorWindow
{
    [MenuItem("Tools/Setup PathGrid")]
    public static void ShowWindow()
    {
        GetWindow<PathGridSetupEditor>("PathGrid Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("PathGrid Setup Helper", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create PathGrid Manager"))
        {
            CreatePathGridManager();
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Setelah create PathGrid Manager:\n" +
            "1. Adjust Grid Width & Height sesuai ukuran map\n" +
            "2. Set Grid Origin ke pojok kiri bawah map\n" +
            "3. Assign WALL layer ke 'Wall Layer'\n" +
            "4. Enable 'Show Grid' untuk lihat visualisasi",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Auto-Detect Map Bounds (Experimental)"))
        {
            AutoDetectMapBounds();
        }
    }
    
    void CreatePathGridManager()
    {
        // Cek apakah sudah ada
        PathGrid existing = FindFirstObjectByType<PathGrid>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("PathGrid", "PathGrid Manager sudah ada di scene!", "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }
        
        // Buat GameObject baru
        GameObject pathGridObj = new GameObject("PathGrid Manager");
        pathGridObj.AddComponent<PathGrid>();
        pathGridObj.AddComponent<Pathfinding>();
        
        // Set default values yang reasonable
        PathGrid grid = pathGridObj.GetComponent<PathGrid>();
        grid.gridWidth = 50;
        grid.gridHeight = 50;
        grid.cellSize = 1f;
        grid.gridOrigin = new Vector3(-25, -25, 0);
        
        // Coba set WALL layer
        int wallLayerIndex = LayerMask.NameToLayer("WALL");
        if (wallLayerIndex >= 0)
        {
            grid.wallLayer = 1 << wallLayerIndex;
        }
        
        Selection.activeGameObject = pathGridObj;
        
        EditorUtility.DisplayDialog("PathGrid", 
            "PathGrid Manager berhasil dibuat!\n\n" +
            "Jangan lupa:\n" +
            "1. Adjust ukuran grid sesuai map\n" +
            "2. Set Grid Origin ke pojok map\n" +
            "3. Assign WALL layer", 
            "OK");
    }
    
    void AutoDetectMapBounds()
    {
        // Cari semua Tilemap di scene
        UnityEngine.Tilemaps.Tilemap[] tilemaps = FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(FindObjectsSortMode.None);
        
        if (tilemaps.Length == 0)
        {
            EditorUtility.DisplayDialog("Auto Detect", "Tidak ditemukan Tilemap di scene!", "OK");
            return;
        }
        
        // Hitung bounds total
        Bounds totalBounds = new Bounds();
        bool first = true;
        
        foreach (var tilemap in tilemaps)
        {
            tilemap.CompressBounds();
            Bounds localBounds = tilemap.localBounds;
            
            // Convert ke world bounds
            Vector3 min = tilemap.transform.TransformPoint(localBounds.min);
            Vector3 max = tilemap.transform.TransformPoint(localBounds.max);
            Bounds worldBounds = new Bounds();
            worldBounds.SetMinMax(min, max);
            
            if (first)
            {
                totalBounds = worldBounds;
                first = false;
            }
            else
            {
                totalBounds.Encapsulate(worldBounds);
            }
        }
        
        // Update PathGrid jika ada
        PathGrid grid = FindFirstObjectByType<PathGrid>();
        if (grid != null)
        {
            Undo.RecordObject(grid, "Auto Detect Bounds");
            
            grid.gridOrigin = totalBounds.min;
            grid.gridWidth = Mathf.CeilToInt(totalBounds.size.x / grid.cellSize) + 2;
            grid.gridHeight = Mathf.CeilToInt(totalBounds.size.y / grid.cellSize) + 2;
            
            EditorUtility.SetDirty(grid);
            
            EditorUtility.DisplayDialog("Auto Detect", 
                $"Bounds detected!\n\n" +
                $"Origin: {grid.gridOrigin}\n" +
                $"Size: {grid.gridWidth} x {grid.gridHeight}\n\n" +
                "PathGrid sudah di-update. Silakan verify!", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Auto Detect", 
                $"Bounds detected!\n\n" +
                $"Min: {totalBounds.min}\n" +
                $"Size: {totalBounds.size}\n\n" +
                "Buat PathGrid Manager terlebih dahulu!", 
                "OK");
        }
    }
}
