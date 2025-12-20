using UnityEngine;

public class KeyItem : MonoBehaviour
{
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
