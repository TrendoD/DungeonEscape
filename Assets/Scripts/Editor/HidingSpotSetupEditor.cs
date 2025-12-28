using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor Script untuk setup Hiding Spot dengan mudah.
/// Cara pakai:
/// 1. Pilih GameObject kotak di Hierarchy
/// 2. Klik Tools > Dungeon Escape > Setup Hiding Spot
/// </summary>
public class HidingSpotSetupEditor : Editor
{
    [MenuItem("Tools/Dungeon Escape/Setup Hiding Spot")]
    static void SetupHidingSpot()
    {
        // Cek apakah ada object yang dipilih
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog(
                "Hiding Spot Setup",
                "Silakan pilih GameObject kotak di Hierarchy terlebih dahulu!",
                "OK"
            );
            return;
        }

        // Konfirmasi setup
        bool confirm = EditorUtility.DisplayDialog(
            "Setup Hiding Spot",
            $"Akan menambahkan komponen HidingSpot ke '{selected.name}'.\n\n" +
            "Komponen yang akan ditambahkan:\n" +
            "• HidingSpot script\n" +
            "• BoxCollider2D (jika belum ada)\n\n" +
            "Lanjutkan?",
            "Ya, Setup!",
            "Batal"
        );

        if (!confirm) return;

        // Start recording undo
        Undo.RegisterCompleteObjectUndo(selected, "Setup Hiding Spot");

        // 1. Tambah atau setup BoxCollider2D
        BoxCollider2D collider = selected.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(selected);
        }
        collider.isTrigger = true;

        // Perbesar sedikit trigger area
        SpriteRenderer sr = selected.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            collider.size = sr.bounds.size * 1.5f;
        }
        else
        {
            collider.size = new Vector2(1.5f, 1.5f);
        }

        // 2. Tambah HidingSpot script
        HidingSpot hidingSpot = selected.GetComponent<HidingSpot>();
        if (hidingSpot == null)
        {
            hidingSpot = Undo.AddComponent<HidingSpot>(selected);
        }

        // 3. Setup default values
        hidingSpot.interactKey = KeyCode.Return;
        hidingSpot.hideOffset = Vector2.zero;

        // Mark dirty untuk save
        EditorUtility.SetDirty(selected);

        // Success message
        EditorUtility.DisplayDialog(
            "Setup Selesai!",
            $"Hiding Spot berhasil di-setup pada '{selected.name}'!\n\n" +
            "Tips:\n" +
            "• Pastikan Player memiliki komponen 'PlayerHiding'\n" +
            "• Sesuaikan ukuran collider trigger jika perlu\n" +
            "• Ubah Hide Offset jika posisi sembunyi perlu disesuaikan",
            "OK"
        );

        Debug.Log($"[HidingSpotSetupEditor] Setup berhasil untuk: {selected.name}");
    }

    [MenuItem("Tools/Dungeon Escape/Setup Player for Hiding")]
    static void SetupPlayerForHiding()
    {
        // Cari player di scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            EditorUtility.DisplayDialog(
                "Player Setup",
                "Tidak menemukan GameObject dengan tag 'Player'!\n" +
                "Pastikan player sudah di-tag dengan benar.",
                "OK"
            );
            return;
        }

        // Konfirmasi
        bool confirm = EditorUtility.DisplayDialog(
            "Setup Player for Hiding",
            $"Akan menambahkan komponen PlayerHiding ke '{player.name}'.\n\n" +
            "Lanjutkan?",
            "Ya, Setup!",
            "Batal"
        );

        if (!confirm) return;

        // Add PlayerHiding component
        Undo.RegisterCompleteObjectUndo(player, "Add PlayerHiding");
        
        PlayerHiding playerHiding = player.GetComponent<PlayerHiding>();
        if (playerHiding == null)
        {
            playerHiding = Undo.AddComponent<PlayerHiding>(player);
        }

        // Setup default
        playerHiding.lightReductionMultiplier = 0.5f;

        EditorUtility.SetDirty(player);

        EditorUtility.DisplayDialog(
            "Setup Selesai!",
            $"PlayerHiding berhasil ditambahkan ke '{player.name}'!\n\n" +
            "Sekarang player bisa bersembunyi di HidingSpot.",
            "OK"
        );

        Debug.Log($"[HidingSpotSetupEditor] PlayerHiding ditambahkan ke: {player.name}");
    }

    [MenuItem("Tools/Dungeon Escape/Setup Tilemap Hiding Manager")]
    static void SetupTilemapHidingManager()
    {
        // Cek apakah ada Tilemap yang dipilih
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog(
                "Tilemap Hiding Setup",
                "Silakan pilih GameObject Tilemap di Hierarchy terlebih dahulu!\n\n" +
                "Tilemap harus berisi tile kotak/box yang akan jadi tempat sembunyi.",
                "OK"
            );
            return;
        }

        // Cek apakah ada Tilemap component
        UnityEngine.Tilemaps.Tilemap tilemap = selected.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        if (tilemap == null)
        {
            bool createNew = EditorUtility.DisplayDialog(
                "Tilemap Not Found",
                $"'{selected.name}' tidak memiliki komponen Tilemap.\n\n" +
                "Apakah Anda ingin menambahkan TilemapHidingManager sebagai komponen terpisah?",
                "Ya, Lanjutkan",
                "Batal"
            );
            
            if (!createNew) return;
        }

        // Konfirmasi setup
        bool confirm = EditorUtility.DisplayDialog(
            "Setup Tilemap Hiding Manager",
            $"Akan menambahkan komponen ke '{selected.name}':\n\n" +
            "• TilemapHidingManager (untuk hiding)\n" +
            "• TilemapCollider2D (agar kotak solid)\n" +
            "• CompositeCollider2D (optimasi)\n\n" +
            "Lanjutkan?",
            "Ya, Setup!",
            "Batal"
        );

        if (!confirm) return;

        // Add component
        Undo.RegisterCompleteObjectUndo(selected, "Setup Tilemap Hiding Manager");
        
        // 1. Add TilemapHidingManager
        TilemapHidingManager manager = selected.GetComponent<TilemapHidingManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<TilemapHidingManager>(selected);
        }

        // Auto-assign tilemap jika ada
        if (tilemap != null)
        {
            manager.hidingTilemap = tilemap;
        }

        // Setup defaults
        manager.interactKey = KeyCode.Return;
        manager.detectionRange = 0.8f;

        // 2. Add TilemapCollider2D untuk membuat kotak solid (tidak bisa ditembus)
        UnityEngine.Tilemaps.TilemapCollider2D tilemapCollider = selected.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
        if (tilemapCollider == null)
        {
            tilemapCollider = Undo.AddComponent<UnityEngine.Tilemaps.TilemapCollider2D>(selected);
        }
        // Set to use composite for better performance
        tilemapCollider.usedByComposite = true;

        // 3. Add Rigidbody2D (required for CompositeCollider2D)
        Rigidbody2D rb = selected.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = Undo.AddComponent<Rigidbody2D>(selected);
        }
        rb.bodyType = RigidbodyType2D.Static; // Static agar tidak jatuh

        // 4. Add CompositeCollider2D untuk optimasi
        CompositeCollider2D compositeCollider = selected.GetComponent<CompositeCollider2D>();
        if (compositeCollider == null)
        {
            compositeCollider = Undo.AddComponent<CompositeCollider2D>(selected);
        }

        EditorUtility.SetDirty(selected);

        EditorUtility.DisplayDialog(
            "Setup Selesai!",
            $"Tilemap Hiding berhasil di-setup pada '{selected.name}'!\n\n" +
            "Komponen yang ditambahkan:\n" +
            "✓ TilemapHidingManager\n" +
            "✓ TilemapCollider2D (kotak solid)\n" +
            "✓ CompositeCollider2D\n" +
            "✓ Rigidbody2D (Static)\n\n" +
            "Kotak sekarang tidak bisa ditembus!",
            "OK"
        );

        Debug.Log($"[HidingSpotSetupEditor] TilemapHidingManager + Colliders ditambahkan ke: {selected.name}");
    }

    // Custom Inspector untuk HidingSpot
    [CustomEditor(typeof(HidingSpot))]
    public class HidingSpotInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Tips Penggunaan:\n" +
                "• Pastikan collider trigger cukup besar untuk player masuk\n" +
                "• Sesuaikan Hide Offset jika player perlu sembunyi di posisi tertentu\n" +
                "• Tekan Enter untuk masuk/keluar persembunyian",
                MessageType.Info
            );

            if (GUILayout.Button("Test Collider Size"))
            {
                HidingSpot spot = (HidingSpot)target;
                BoxCollider2D col = spot.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    Debug.Log($"Collider size: {col.size}, isTrigger: {col.isTrigger}");
                }
            }
        }
    }
}
