using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator; // 1. Variabel Animator harus ada

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 2. Mengambil komponen Animator yang ada di Player
        animator = GetComponent<Animator>(); 
    }

    void Update()
    {
        // Input keyboard
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 3. INI KUNCINYA: Mengirim data kecepatan ke Parameter "Speed" di Animator
        if (animator != null)
        {
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }

        // Membalik arah hadap (Flip)
        if (movement.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (movement.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void FixedUpdate()
    {
        // Menggerakkan fisik karakter
        // Gunakan 'velocity' jika 'linearVelocity' error (tergantung versi Unity 6)
        rb.linearVelocity = movement.normalized * moveSpeed;
    }
}