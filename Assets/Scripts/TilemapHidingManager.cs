using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Manager untuk mendeteksi hiding spot dari Tilemap.
/// Pasang script ini di GameObject dengan Tilemap yang berisi kotak-kotak persembunyian.
/// </summary>
public class TilemapHidingManager : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [Tooltip("Tilemap yang berisi tile kotak/box untuk sembunyi")]
    public Tilemap hidingTilemap;
    
    [Tooltip("Daftar tile yang bisa jadi tempat sembunyi (kotak, peti, dsb)")]
    public TileBase[] hidingTiles;

    [Header("Interaction Settings")]
    [Tooltip("Tombol untuk masuk/keluar persembunyian")]
    public KeyCode interactKey = KeyCode.Return; // Enter key
    
    [Tooltip("Radius deteksi untuk player masuk box. Semakin besar, semakin jauh player bisa enter.")]
    [Range(0.3f, 3f)]
    public float detectionRange = 0.8f;

    // References
    private Transform playerTransform;
    private PlayerHiding playerHiding;
    private Vector3Int currentNearbyTile;
    private bool isNearHidingSpot = false;
    private float closestDistance = float.MaxValue;

    void Start()
    {
        // Cari player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHiding = player.GetComponent<PlayerHiding>();
            
            if (playerHiding == null)
            {
                Debug.LogWarning("TilemapHidingManager: Player tidak memiliki komponen PlayerHiding!");
            }
        }
        else
        {
            Debug.LogError("TilemapHidingManager: Tidak menemukan Player dengan tag 'Player'!");
        }

        // Coba auto-find tilemap jika belum di-assign
        if (hidingTilemap == null)
        {
            hidingTilemap = GetComponent<Tilemap>();
        }
    }

    void Update()
    {
        if (playerTransform == null || hidingTilemap == null || playerHiding == null) return;

        // Cek tile terdekat
        CheckNearbyHidingTile();

        // Handle input
        if (isNearHidingSpot && Input.GetKeyDown(interactKey))
        {
            if (!playerHiding.IsHiding)
            {
                EnterHiding();
            }
            else
            {
                ExitHiding();
            }
        }
    }

    void CheckNearbyHidingTile()
    {
        if (playerHiding.IsHiding)
        {
            // Jika sedang bersembunyi, tetap nearby
            isNearHidingSpot = true;
            return;
        }

        // Konversi posisi player ke cell position
        Vector3Int playerCell = hidingTilemap.WorldToCell(playerTransform.position);
        
        // Reset state
        isNearHidingSpot = false;
        closestDistance = float.MaxValue;
        Vector3Int closestTile = Vector3Int.zero;
        
        // Cek tile di sekitar player (3x3 area) dan cari yang TERDEKAT
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int checkCell = playerCell + new Vector3Int(x, y, 0);
                TileBase tile = hidingTilemap.GetTile(checkCell);
                
                if (tile != null && IsHidingTile(tile))
                {
                    // Cek jarak ke center tile
                    Vector3 tileCenter = hidingTilemap.GetCellCenterWorld(checkCell);
                    float distance = Vector2.Distance(playerTransform.position, tileCenter);
                    
                    // Simpan tile terdekat dalam range
                    if (distance <= detectionRange && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTile = checkCell;
                        isNearHidingSpot = true;
                    }
                }
            }
        }
        
        // Set tile terdekat sebagai current
        if (isNearHidingSpot)
        {
            currentNearbyTile = closestTile;
        }
    }

    bool IsHidingTile(TileBase tile)
    {
        // Cek apakah tile ada di list hiding tiles
        if (hidingTiles == null || hidingTiles.Length == 0)
        {
            // Jika tidak ada list, semua tile di tilemap ini dianggap hiding spot
            return true;
        }

        foreach (TileBase hidingTile in hidingTiles)
        {
            if (tile == hidingTile)
            {
                return true;
            }
        }
        return false;
    }

    void EnterHiding()
    {
        if (playerHiding == null) return;

        // Dapatkan posisi center tile sebagai posisi sembunyi
        Vector3 hidePosition = hidingTilemap.GetCellCenterWorld(currentNearbyTile);
        
        // Gunakan HidingSpot virtual (null) karena ini dari tilemap
        playerHiding.EnterHidingFromTilemap(hidePosition);
        
        Debug.Log($"Player bersembunyi di tile: {currentNearbyTile}");
    }

    void ExitHiding()
    {
        if (playerHiding == null) return;
        
        playerHiding.ExitHiding();
        
        Debug.Log("Player keluar dari persembunyian tilemap");
    }


    // Visualisasi di Scene view
    void OnDrawGizmosSelected()
    {
        if (hidingTilemap == null) return;

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        
        // Gambar area deteksi di semua hiding tiles
        BoundsInt bounds = hidingTilemap.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = hidingTilemap.GetTile(cell);
                
                if (tile != null && IsHidingTile(tile))
                {
                    Vector3 center = hidingTilemap.GetCellCenterWorld(cell);
                    Gizmos.DrawWireSphere(center, detectionRange);
                }
            }
        }
    }
}
