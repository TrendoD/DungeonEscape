using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Visual Efek")]
    [Tooltip("Kecepatan naik turun")]
    public float floatSpeed = 2f;
    [Tooltip("Jarak naik turun")]
    public float floatHeight = 0.15f;

    // --- [BARU] VARIABLES AUDIO ---
    [Header("Audio Settings")]
    [Tooltip("Tarik file suara kunci (ting/clink) ke sini")]
    public AudioClip pickupSound; 
    [Range(0f, 1f)] public float soundVolume = 0.7f; // Slider volume (0 sampai 1)
    // -----------------------------

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Animasi Floating (Naik Turun)
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger is the Player
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.PickupKey();
                
                // --- [BARU] MAINKAN SUARA ---
                if (pickupSound != null)
                {
                    // Kita pakai PlayClipAtPoint karena object kuncinya langsung di-Destroy.
                    // Fungsi ini membuat "Speaker Sementara" di posisi kunci yang tetap bunyi walaupun kuncinya hilang.
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
                }

                // Destroy the key object
                Destroy(gameObject);
            }
        }
    }
}