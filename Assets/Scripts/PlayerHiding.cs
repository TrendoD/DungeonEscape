using UnityEngine;

/// <summary>
/// Komponen untuk menangani state bersembunyi player.
/// Pasang script ini di GameObject Player.
/// </summary>
public class PlayerHiding : MonoBehaviour
{
    [Header("Hide Settings")]
    [Tooltip("Pengurangan cahaya saat bersembunyi (0.5 = 50%)")]
    [Range(0.1f, 1f)]
    public float lightReductionMultiplier = 0.5f;

    [Header("Audio (Optional)")]
    [Tooltip("Suara saat masuk persembunyian")]
    public AudioClip enterHideSound;
    
    [Tooltip("Suara saat keluar persembunyian")]
    public AudioClip exitHideSound;

    // Public Properties
    public bool IsHiding { get; private set; }
    public HidingSpot CurrentHidingSpot { get; private set; }

    // Component References
    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;
    private LanternController lanternController;
    private AudioSource audioSource;
    
    // State
    private Vector3 originalPosition;
    private Vector3 tilemapHidePosition; // Posisi hide dari tilemap

    void Start()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        lanternController = GetComponentInChildren<LanternController>();
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Validate components
        if (spriteRenderer == null)
            Debug.LogWarning("PlayerHiding: Tidak menemukan SpriteRenderer!");
        
        if (playerMovement == null)
            Debug.LogWarning("PlayerHiding: Tidak menemukan PlayerMovement!");
            
        if (lanternController == null)
            Debug.LogWarning("PlayerHiding: Tidak menemukan LanternController di children!");
    }

    /// <summary>
    /// Masuk ke mode bersembunyi (dari HidingSpot GameObject)
    /// </summary>
    public void EnterHiding(HidingSpot spot, Vector3 hidePosition)
    {
        if (IsHiding) return;

        IsHiding = true;
        CurrentHidingSpot = spot;
        originalPosition = transform.position;
        tilemapHidePosition = Vector3.zero;

        PerformEnterHiding(hidePosition);
    }

    /// <summary>
    /// Masuk ke mode bersembunyi (dari Tilemap)
    /// </summary>
    public void EnterHidingFromTilemap(Vector3 hidePosition)
    {
        if (IsHiding) return;

        IsHiding = true;
        CurrentHidingSpot = null;
        originalPosition = transform.position;
        tilemapHidePosition = hidePosition;

        PerformEnterHiding(hidePosition);
    }

    /// <summary>
    /// Internal method untuk perform hiding actions
    /// </summary>
    private void PerformEnterHiding(Vector3 hidePosition)
    {
        // 1. Pindahkan player ke posisi sembunyi
        transform.position = hidePosition;

        // 2. Sembunyikan sprite player
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // 3. Disable movement
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }

        // 4. Kurangi cahaya lantern
        if (lanternController != null)
        {
            lanternController.SetHidingMode(true, lightReductionMultiplier);
        }

        // 5. Play sound
        if (enterHideSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(enterHideSound);
        }

        Debug.Log("Player masuk persembunyian!");
    }

    /// <summary>
    /// Keluar dari mode bersembunyi
    /// </summary>
    public void ExitHiding()
    {
        if (!IsHiding) return;

        IsHiding = false;
        
        // 1. Kembalikan posisi player ke posisi sebelum masuk box
        transform.position = originalPosition;

        CurrentHidingSpot = null;
        tilemapHidePosition = Vector3.zero;

        // 2. Tampilkan sprite player
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // 3. Enable movement
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }

        // 4. Kembalikan cahaya lantern
        if (lanternController != null)
        {
            lanternController.SetHidingMode(false, 1f);
        }

        // 5. Play sound
        if (exitHideSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(exitHideSound);
        }

        Debug.Log("Player keluar dari persembunyian!");
    }
}
