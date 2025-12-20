using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading levels

public class DoorExit : MonoBehaviour
{
    [Tooltip("Name of the next scene (level) to load")]
    public string nextLevelName;

    private bool isPlayerNearby = false;
    private PlayerMovement playerInRange;

    private void Update()
    {
        // Check for interaction input (Enter key) only if player is nearby
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.Return))
        {
            TryOpenDoor();
        }
    }

    private void TryOpenDoor()
    {
        if (playerInRange != null && playerInRange.HasKey())
        {
            Debug.Log("Door Unlocked! Level Complete.");
            // Load the next level
            // If nextLevelName is empty, maybe just reload current or go to main menu
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                Debug.LogWarning("Next level name is not set in the inspector!");
            }
        }
        else
        {
            Debug.Log("The door is locked. You need a Key.");
            // Optional: Play "Locked" sound or show UI message
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerInRange = other.GetComponent<PlayerMovement>();
            Debug.Log("Press ENTER to open Door");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            playerInRange = null;
        }
    }
}
