using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Setting Kecepatan")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.5f;

    [Header("Setting Jarak")]
    public float chaseDistance = 4f;
    public float stopDistance = 0.5f;

    [Header("Titik Patroli")]
    public Transform[] waypoints;

    // --- AUDIO SETTINGS (BARU) ---
    [Header("Audio Settings")]
    public AudioClip footstepSound;    // Masukkan suara langkah monster
    public float stepInterval = 0.5f;  // Jarak waktu antar langkah (detik)
    public AudioClip chaseSound;       // Masukkan suara saat mengejar (Looping)
    
    // Kita butuh 2 AudioSource: Satu untuk langkah, satu untuk suara kejar
    private AudioSource stepSource;
    private AudioSource chaseSource;
    private float stepTimer;
    private bool isChasingState = false; // Penanda apakah sedang mode kejar
    // -----------------------------

    private Transform playerTarget;
    private int currentPointIndex;
    private Rigidbody2D rb;
    private Vector2 movement;
    private float waitTimer;
    
    // Deteksi Macet
    private float stuckTimer; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        PickRandomWaypoint();

        // --- SETUP AUDIO (OTOMATIS) ---
        // 1. Setup AudioSource untuk Langkah Kaki
        stepSource = gameObject.AddComponent<AudioSource>();
        stepSource.playOnAwake = false;
        stepSource.spatialBlend = 1f; // 3D Sound (Makin jauh makin pelan)
        stepSource.rolloffMode = AudioRolloffMode.Linear;
        stepSource.maxDistance = 15f; // Jarak terdengar
        stepSource.volume = 0.6f;

        // 2. Setup AudioSource untuk Suara Kejar (Looping)
        chaseSource = gameObject.AddComponent<AudioSource>();
        chaseSource.clip = chaseSound;
        chaseSource.loop = true;      // Agar suaranya nyambung terus saat ngejar
        chaseSource.playOnAwake = false;
        chaseSource.spatialBlend = 1f; // 3D Sound
        chaseSource.rolloffMode = AudioRolloffMode.Linear;
        chaseSource.maxDistance = 12f; // Jarak terdengar (lebih jauh dikit)
        chaseSource.volume = 0.8f;
    }

    void Update()
    {
        // 1. Handle Suara Langkah Kaki (Setiap bergerak)
        HandleFootsteps();

        if (playerTarget == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer < chaseDistance)
        {
            // --- KONDISI: LAGI MENGEJAR ---
            MoveTo(playerTarget.position, chaseSpeed);
            stuckTimer = 0;

            // Logika Suara Chase: Nyalakan jika belum nyala
            if (!isChasingState)
            {
                isChasingState = true;
                if (chaseSound != null) chaseSource.Play(); // Mulai suara kejar
            }
        }
        else
        {
            // --- KONDISI: LAGI PATROLI ---
            Patrol();

            // Logika Suara Chase: Matikan jika monster berhenti mengejar
            if (isChasingState)
            {
                isChasingState = false;
                if (chaseSound != null) chaseSource.Stop(); // Hentikan suara kejar
            }
        }

        // Animasi Flip
        if (movement.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (movement.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void HandleFootsteps()
    {
        // Cek apakah monster sedang bergerak (Velocity > 0.1)
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            
            if (stepTimer <= 0)
            {
                if (footstepSound != null) stepSource.PlayOneShot(footstepSound);
                    stepTimer = stepInterval; // Langkah normal pas patroli
            }
        }
        else
        {
            stepTimer = 0; // Reset biar pas jalan langsung bunyi
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Transform targetPoint = waypoints[currentPointIndex];
        MoveTo(targetPoint.position, patrolSpeed);

        if (Vector2.Distance(transform.position, targetPoint.position) < stopDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > 1f) 
            {
                PickRandomWaypoint();
            }
        }

        // Stuck Detection
        if (rb.linearVelocity.magnitude < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 0.5f)
            {
                PickRandomWaypoint();
                stuckTimer = 0;
            }
        }
        else
        {
            stuckTimer = 0;
        }
    }

    void PickRandomWaypoint()
    {
        if (waypoints.Length > 0)
        {
            currentPointIndex = Random.Range(0, waypoints.Length);
            waitTimer = 0; 
        }
    }

    void MoveTo(Vector3 target, float speed)
    {
        Vector3 direction = target - transform.position;
        direction.Normalize();
        movement = direction;
        
        rb.linearVelocity = movement * speed;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}