using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    // --- AUDIO SETTINGS (DIUBAH) ---
    [Header("Audio Settings")]
    public AudioClip stepSound;       // Masukkan file suara langkah (4 detik) di sini
    
    private AudioSource audioSource;
    // -----------------------------

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;

    // Inventory State
    private bool hasKey = false;

    // Hiding State - untuk disable movement saat bersembunyi
    private bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // --- SETUP AUDIO ---
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; 
        audioSource.volume = 0.5f; 
        
        // PENTING: Karena filenya panjang, kita set Loop jadi true
        // Agar kalau jalan lebih dari 4 detik, suaranya nyambung terus
        audioSource.loop = true; 
    }

    // --- Inventory Methods ---
    public void PickupKey()
    {
        hasKey = true;
        Debug.Log("Player picked up the Key!");
    }

    public bool HasKey()
    {
        return hasKey;
    }

    // --- Hiding Methods ---
    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    void Update()
    {
        // Jika tidak bisa bergerak (sedang bersembunyi), skip semua input
        if (!canMove)
        {
            movement = Vector2.zero;
            HandleFootsteps();
            return;
        }

        // 1. Input keyboard
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. Animasi
        if (animator != null)
        {
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }

        // 3. Flip Character
        if (movement.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (movement.x < 0) transform.localScale = new Vector3(-1, 1, 1);

        // 4. --- LOGIKA AUDIO BARU (Play & Stop) ---
        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        // Cek apakah ada input gerak?
        if (movement.sqrMagnitude > 0.1f)
        {
            // KONDISI: Karakter Jalan, tapi suara BELUM bunyi
            if (!audioSource.isPlaying)
            {
                audioSource.clip = stepSound;
                audioSource.Play(); // Mulai putar lagu
            }
        }
        else
        {
            // KONDISI: Karakter Diam, tapi suara MASIH bunyi
            if (audioSource.isPlaying)
            {
                audioSource.Stop(); // Paksa berhenti detik itu juga!
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * moveSpeed;
    }
}