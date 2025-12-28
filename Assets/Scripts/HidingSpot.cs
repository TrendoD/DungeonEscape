using UnityEngine;

/// <summary>
/// Komponen untuk tempat persembunyian (kotak/box).
/// Pasang script ini di GameObject kotak yang bisa jadi tempat sembunyi.
/// Pastikan GameObject memiliki Collider2D dengan isTrigger = true.
/// </summary>
public class HidingSpot : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Tombol untuk masuk/keluar persembunyian")]
    public KeyCode interactKey = KeyCode.Return; // Enter key

    [Header("Visual Settings")]
    [Tooltip("Offset posisi player saat bersembunyi (relatif terhadap kotak)")]
    public Vector2 hideOffset = Vector2.zero;

    // State
    private bool isPlayerNearby = false;
    private PlayerHiding playerHiding;
    private bool isOccupied = false;

    void Start()
    {
        // Setup collider sebagai trigger jika belum
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"HidingSpot: Collider di {gameObject.name} bukan trigger! Mengubah ke trigger.");
            col.isTrigger = true;
        }
    }

    void Update()
    {
        // Cek input hanya jika player dekat atau sedang bersembunyi di sini
        if (isPlayerNearby && playerHiding != null)
        {
            if (Input.GetKeyDown(interactKey))
            {
                if (!playerHiding.IsHiding)
                {
                    // Masuk persembunyian
                    EnterHiding();
                }
                else if (playerHiding.CurrentHidingSpot == this)
                {
                    // Keluar persembunyian (hanya jika bersembunyi di spot ini)
                    ExitHiding();
                }
            }
        }
    }

    void EnterHiding()
    {
        if (playerHiding != null && !isOccupied)
        {
            Vector3 hidePosition = transform.position + (Vector3)hideOffset;
            playerHiding.EnterHiding(this, hidePosition);
            isOccupied = true;
        }
    }

    void ExitHiding()
    {
        if (playerHiding != null && isOccupied)
        {
            playerHiding.ExitHiding();
            isOccupied = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerHiding = other.GetComponent<PlayerHiding>();
            
            if (playerHiding == null)
            {
                Debug.LogWarning("HidingSpot: Player tidak memiliki komponen PlayerHiding!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            playerHiding = null;
        }
    }


    // Public getter untuk cek status
    public bool IsOccupied => isOccupied;
}
