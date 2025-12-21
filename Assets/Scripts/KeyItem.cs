using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Visual Efek")]
    [Tooltip("Kecepatan naik turun")]
    public float floatSpeed = 2f;
    [Tooltip("Jarak naik turun")]
    public float floatHeight = 0.15f;

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
                
                // Play sound effect here if available
                // AudioSource.PlayClipAtPoint(pickupSound, transform.position);

                // Destroy the key object
                Destroy(gameObject);
            }
        }
    }
}
