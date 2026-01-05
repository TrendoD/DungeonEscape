using UnityEngine;
using UnityEngine.SceneManagement; // Wajib untuk pindah level
using System.Collections; // Wajib untuk IEnumerator (Coroutine)

public class DoorExit : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Tulis nama Scene (Map) selanjutnya persis dengan nama file-nya")]
    public string nextLevelName;

    [Header("Audio Settings")]
    public AudioClip openSound;   // Suara pintu terbuka
    public AudioClip lockedSound; // Suara pintu terkunci (saat tidak punya kunci)
    
    private AudioSource audioSource;
    private bool isPlayerNearby = false;
    private PlayerMovement playerInRange;
    
    // Base volume untuk door sounds
    private const float BASE_DOOR_VOLUME = 1f;

    void Start()
    {
        // Pasang AudioSource otomatis jika lupa menambahkannya di Inspector
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false; // Supaya tidak bunyi sendiri pas game mulai
        
        // Subscribe ke SFX volume changes
        AudioManager.OnSFXVolumeChanged += UpdateVolume;
        UpdateVolume(AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : 1f);
    }

    void OnDestroy()
    {
        AudioManager.OnSFXVolumeChanged -= UpdateVolume;
    }

    private void UpdateVolume(float sfxVolume)
    {
        if (audioSource != null)
            audioSource.volume = BASE_DOOR_VOLUME * sfxVolume;
    }

    private void Update()
    {
        // Cek input tombol ENTER hanya jika pemain dekat
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.Return))
        {
            TryOpenDoor();
        }
    }

    private void TryOpenDoor()
    {
        // Cek apakah player membawa kunci?
        if (playerInRange != null && playerInRange.HasKey())
        {
            // --- JIKA PUNYA KUNCI ---
            Debug.Log("Door Unlocked! Opening...");
            
            // Panggil Coroutine untuk menunggu suara selesai baru pindah scene
            StartCoroutine(PlaySoundAndLoadLevel());
        }
        else
        {
            // --- JIKA TIDAK PUNYA KUNCI ---
            Debug.Log("Door Locked! You need a key.");
            
            // Mainkan suara terkunci (Ceklek-ceklek)
            if (lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
        }
    }

    // Trik IEnumerator untuk memberi jeda waktu
    IEnumerator PlaySoundAndLoadLevel()
    {
        // 1. Mainkan suara buka pintu
        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // 2. Tunggu sebentar (misal 1 detik) agar suara terdengar
        // Kamu bisa ubah angkanya sesuai panjang durasi suara kamu (misal 1.5f)
        yield return new WaitForSeconds(1f); 

        // 3. Baru pindah scene setelah menunggu
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.LogWarning("Next level name belum diisi di Inspector!");
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