using UnityEngine;
using System.Collections.Generic;

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

    // --- PATHFINDING SETTINGS ---
    [Header("Pathfinding Settings")]
    public float pathUpdateInterval = 0.3f; // Seberapa sering update path (detik)
    private float pathUpdateTimer;
    private List<Vector3> currentPath;
    private int currentPathIndex;
    
    [Header("Debug Pathfinding")]
    public bool showPathGizmos = true;
    public Color pathColor = Color.yellow;

    // --- AUDIO SETTINGS ---
    [Header("Audio Settings")]
    public AudioClip footstepSound;
    public float stepInterval = 0.5f;
    public AudioClip chaseSound;
    
    private AudioSource stepSource;
    private AudioSource chaseSource;
    private float stepTimer;
    private bool isChasingState = false;
    
    // Base volumes (sebelum dikalikan SFX Volume)
    private const float BASE_STEP_VOLUME = 0.6f;
    private const float BASE_CHASE_VOLUME = 0.8f;

    private Transform playerTarget;
    private int currentPointIndex;
    private Rigidbody2D rb;
    private Vector2 movement;
    private float waitTimer;
    
    // Deteksi Macet (untuk patrol waypoint saja)
    private float stuckTimer;
    private Vector3 lastPosition;
    private float stuckCheckTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        PickRandomWaypoint();

        // --- SETUP AUDIO (OTOMATIS) ---
        stepSource = gameObject.AddComponent<AudioSource>();
        stepSource.playOnAwake = false;
        stepSource.spatialBlend = 1f;
        stepSource.rolloffMode = AudioRolloffMode.Linear;
        stepSource.maxDistance = 15f;

        chaseSource = gameObject.AddComponent<AudioSource>();
        chaseSource.clip = chaseSound;
        chaseSource.loop = true;
        chaseSource.playOnAwake = false;
        chaseSource.spatialBlend = 1f;
        chaseSource.rolloffMode = AudioRolloffMode.Linear;
        chaseSource.maxDistance = 12f;
        
        // Subscribe ke SFX volume changes
        AudioManager.OnSFXVolumeChanged += UpdateAudioVolumes;
        UpdateAudioVolumes(AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : 1f);
        
        lastPosition = transform.position;
    }

    void OnDestroy()
    {
        AudioManager.OnSFXVolumeChanged -= UpdateAudioVolumes;
    }

    private void UpdateAudioVolumes(float sfxVolume)
    {
        if (stepSource != null)
            stepSource.volume = BASE_STEP_VOLUME * sfxVolume;
        if (chaseSource != null)
            chaseSource.volume = BASE_CHASE_VOLUME * sfxVolume;
    }

    void Update()
    {
        HandleFootsteps();

        if (playerTarget == null) return;

        // Cek apakah player sedang bersembunyi
        PlayerHiding playerHiding = playerTarget.GetComponent<PlayerHiding>();
        if (playerHiding != null && playerHiding.IsHiding)
        {
            if (isChasingState)
            {
                isChasingState = false;
                if (chaseSound != null) chaseSource.Stop();
            }
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer < chaseDistance)
        {
            // --- KONDISI: MENGEJAR PLAYER ---
            ChasePlayer();

            if (!isChasingState)
            {
                isChasingState = true;
                if (chaseSound != null) chaseSource.Play();
            }
        }
        else
        {
            // --- KONDISI: PATROLI ---
            Patrol();

            if (isChasingState)
            {
                isChasingState = false;
                if (chaseSound != null) chaseSource.Stop();
            }
        }

        // Animasi Flip
        if (movement.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (movement.x < 0) transform.localScale = new Vector3(-1, 1, 1);
        
        // Stuck check - jika tidak bergerak sama sekali untuk waktu lama
        HandleStuckCheck();
    }

    void HandleFootsteps()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            
            if (stepTimer <= 0)
            {
                if (footstepSound != null) stepSource.PlayOneShot(footstepSound);
                    stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0;
        }
    }

    /// <summary>
    /// Mengejar player menggunakan A* pathfinding
    /// Prioritas: Ikuti path dengan offset dulu, baru recalculate jika perlu
    /// </summary>
    void ChasePlayer()
    {
        // Jika sudah punya path dan masih valid, prioritaskan ikuti path ini
        if (currentPath != null && currentPath.Count > 0 && currentPathIndex < currentPath.Count)
        {
            // Cek apakah masih progress di path ini
            float distanceToCurrentWaypoint = Vector2.Distance(transform.position, currentPath[currentPathIndex]);
            
            // Hanya recalculate jika sudah lama atau path habis
            pathUpdateTimer -= Time.deltaTime;
            if (pathUpdateTimer <= 0)
            {
                // Recalculate dengan interval lebih lama saat chase
                UpdatePathToTarget(playerTarget.position);
                pathUpdateTimer = pathUpdateInterval * 2f; // Double interval saat chase
            }
        }
        else
        {
            // Tidak ada path, buat baru
            UpdatePathToTarget(playerTarget.position);
            pathUpdateTimer = pathUpdateInterval * 2f;
        }
        
        FollowPath(chaseSpeed);
    }

    /// <summary>
    /// Patrol ke waypoint menggunakan A* pathfinding
    /// </summary>
    void Patrol()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("Enemy has no waypoints assigned!");
            return;
        }

        Transform targetPoint = waypoints[currentPointIndex];
        
        // Cek apakah sudah sampai di waypoint DULU sebelum update path
        float distanceToWaypoint = Vector2.Distance(transform.position, targetPoint.position);
        if (distanceToWaypoint < stopDistance * 1.5f)
        {
            // Sudah sampai atau sangat dekat dengan waypoint
            waitTimer += Time.deltaTime;
            if (waitTimer > 0.5f) // Kurangi waktu tunggu
            {
                // Langsung pilih waypoint baru
                PickRandomWaypoint();
                // Langsung update path ke waypoint baru
                targetPoint = waypoints[currentPointIndex];
                UpdatePathToTarget(targetPoint.position);
                return;
            }
        }
        
        // Update path jika belum ada atau sudah sampai
        if (currentPath == null || currentPath.Count == 0)
        {
            UpdatePathToTarget(targetPoint.position);
        }
        
        FollowPath(patrolSpeed);
    }
    
    /// <summary>
    /// Update path ke target menggunakan Pathfinding
    /// </summary>
    void UpdatePathToTarget(Vector3 targetPos)
    {
        if (Pathfinding.Instance != null)
        {
            currentPath = Pathfinding.Instance.FindPath(transform.position, targetPos);
            currentPathIndex = 0;
            
            if (currentPath != null && currentPath.Count > 0)
            {
                // Skip waypoint pertama jika terlalu dekat (posisi saat ini)
                if (currentPath.Count > 1)
                {
                    if (Vector2.Distance(transform.position, currentPath[0]) < stopDistance)
                    {
                        currentPathIndex = 1;
                    }
                }
            }
            else
            {
                // Path not found - gunakan direct movement sebagai fallback
                currentPath = new List<Vector3> { targetPos };
                currentPathIndex = 0;
            }
        }
        else
        {
            // Fallback: direct movement jika pathfinding belum ready
            currentPath = new List<Vector3> { targetPos };
            currentPathIndex = 0;
        }
    }
    
    /// <summary>
    /// Follow path yang sudah dihitung
    /// </summary>
    void FollowPath(float speed)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        if (currentPathIndex >= currentPath.Count)
        {
            // Sudah sampai di akhir path
            rb.linearVelocity = Vector2.zero;
            currentPath = null;
            return;
        }
        
        Vector3 targetWaypoint = currentPath[currentPathIndex];
        float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint);
        
        if (distanceToWaypoint < stopDistance)
        {
            // Sudah sampai di waypoint ini, lanjut ke berikutnya
            currentPathIndex++;
            
            if (currentPathIndex >= currentPath.Count)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            
            targetWaypoint = currentPath[currentPathIndex];
        }
        
        // Hitung arah ke waypoint
        Vector2 desiredDirection = ((Vector2)targetWaypoint - (Vector2)transform.position).normalized;
        
        // Apply wall sliding behavior
        Vector2 finalVelocity = ApplyWallSliding(desiredDirection, speed);
        
        movement = finalVelocity.normalized;
        rb.linearVelocity = finalVelocity;
    }
    
    /// <summary>
    /// Apply wall sliding - jika arah terhalang, coba geser sepanjang tembok
    /// </summary>
    Vector2 ApplyWallSliding(Vector2 desiredDirection, float speed)
    {
        float rayDistance = 0.6f; // Jarak deteksi wall
        LayerMask wallMask = LayerMask.GetMask("WALL");
        
        // Raycast ke arah yang diinginkan
        RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredDirection, rayDistance, wallMask);
        
        if (hit.collider == null)
        {
            // Tidak ada halangan, gerak normal
            return desiredDirection * speed;
        }
        
        // Ada halangan! Hitung sliding direction
        Vector2 wallNormal = hit.normal;
        
        // Project desired direction onto wall plane (sliding)
        Vector2 slideDirection = desiredDirection - Vector2.Dot(desiredDirection, wallNormal) * wallNormal;
        slideDirection.Normalize();
        
        // Cek apakah slide direction juga terhalang
        RaycastHit2D slideHit = Physics2D.Raycast(transform.position, slideDirection, rayDistance, wallMask);
        
        if (slideHit.collider == null && slideDirection.magnitude > 0.1f)
        {
            // Bisa slide
            return slideDirection * speed * 0.8f; // Sedikit lebih lambat saat sliding
        }
        
        // Coba arah sebaliknya
        Vector2 reverseSlide = -slideDirection;
        RaycastHit2D reverseHit = Physics2D.Raycast(transform.position, reverseSlide, rayDistance, wallMask);
        
        if (reverseHit.collider == null && reverseSlide.magnitude > 0.1f)
        {
            return reverseSlide * speed * 0.8f;
        }
        
        // Tidak bisa slide, return velocity sangat kecil (akan trigger stuck detection)
        return desiredDirection * speed * 0.1f;
    }
    
    /// <summary>
    /// Deteksi jika benar-benar stuck dan coba local avoidance
    /// </summary>
    void HandleStuckCheck()
    {
        stuckCheckTimer += Time.deltaTime;
        
        if (stuckCheckTimer >= 0.3f) // Check lebih sering
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            
            if (distanceMoved < 0.05f && rb.linearVelocity.magnitude > 0.1f)
            {
                // Kita mencoba bergerak tapi tidak berpindah = stuck
                stuckTimer += 0.3f;
                
                if (stuckTimer >= 0.5f) // React lebih cepat
                {
                    // Coba perpendicular dodge - gerak ke samping untuk menghindari pojok
                    TryPerpendicularDodge();
                    stuckTimer = 0;
                }
            }
            else
            {
                stuckTimer = 0;
            }
            
            lastPosition = transform.position;
            stuckCheckTimer = 0;
        }
    }
    
    /// <summary>
    /// Coba bergerak tegak lurus dari arah saat ini untuk menghindari stuck
    /// </summary>
    void TryPerpendicularDodge()
    {
        if (currentPath == null || currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            // Tidak ada path, recalculate
            if (isChasingState && playerTarget != null)
            {
                currentPath = null;
            }
            else if (waypoints.Length > 0)
            {
                PickRandomWaypoint();
            }
            return;
        }
        
        Vector3 targetWaypoint = currentPath[currentPathIndex];
        Vector2 toTarget = (targetWaypoint - transform.position).normalized;
        
        // Hitung arah perpendicular (tegak lurus)
        Vector2 perpendicular1 = new Vector2(-toTarget.y, toTarget.x); // Rotate 90 derajat
        Vector2 perpendicular2 = new Vector2(toTarget.y, -toTarget.x); // Rotate -90 derajat
        
        float dodgeDistance = 0.8f;
        
        // Cek arah mana yang tidak ada obstacle
        bool canDodge1 = CanMoveTo(perpendicular1, dodgeDistance);
        bool canDodge2 = CanMoveTo(perpendicular2, dodgeDistance);
        
        if (canDodge1 && !canDodge2)
        {
            // Dodge ke arah 1
            ApplyDodgeMovement(perpendicular1, dodgeDistance);
        }
        else if (canDodge2 && !canDodge1)
        {
            // Dodge ke arah 2
            ApplyDodgeMovement(perpendicular2, dodgeDistance);
        }
        else if (canDodge1 && canDodge2)
        {
            // Keduanya bisa, pilih random
            Vector2 chosen = Random.value > 0.5f ? perpendicular1 : perpendicular2;
            ApplyDodgeMovement(chosen, dodgeDistance);
        }
        else
        {
            // Tidak bisa dodge, recalculate path
            Debug.Log("Cannot dodge, recalculating path...");
            currentPath = null;
            
            if (!isChasingState && waypoints.Length > 0)
            {
                PickRandomWaypoint();
            }
        }
    }
    
    /// <summary>
    /// Cek apakah bisa bergerak ke arah tertentu
    /// </summary>
    bool CanMoveTo(Vector2 direction, float distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, LayerMask.GetMask("WALL"));
        return hit.collider == null;
    }
    
    /// <summary>
    /// Apply dodge movement ke arah tertentu
    /// </summary>
    void ApplyDodgeMovement(Vector2 direction, float distance)
    {
        // Gerak sedikit ke arah perpendicular untuk menghindari corner
        Vector3 dodgePosition = transform.position + (Vector3)(direction * distance * 0.5f);
        
        // Sisipkan dodge waypoint ke path
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            currentPath.Insert(currentPathIndex, dodgePosition);
        }
    }

    void PickRandomWaypoint()
    {
        if (waypoints.Length > 0)
        {
            currentPointIndex = Random.Range(0, waypoints.Length);
            waitTimer = 0;
            currentPath = null; // Force recalculate
        }
    }

    void OnDrawGizmos()
    {
        // Path visualization - selalu tampil tanpa perlu select enemy
        if (showPathGizmos && currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = pathColor;
            
            // Draw line from enemy to first waypoint
            if (currentPathIndex < currentPath.Count)
            {
                Gizmos.DrawLine(transform.position, currentPath[currentPathIndex]);
            }
            
            // Draw path
            for (int i = currentPathIndex; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                Gizmos.DrawSphere(currentPath[i], 0.1f);
            }
            
            // Draw end point
            if (currentPath.Count > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(currentPath[currentPath.Count - 1], 0.15f);
            }
        }
        else
        {
            // No path - tampilkan indikator merah di posisi enemy
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Chase distance - hanya tampil saat select
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }

    // ==================== GAME OVER TRIGGER ====================

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHiding playerHiding = collision.gameObject.GetComponent<PlayerHiding>();
            if (playerHiding != null && playerHiding.IsHiding)
            {
                return;
            }

            Debug.Log("GAME OVER: Player tertangkap oleh monster!");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver("Tertangkap Monster");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHiding playerHiding = other.GetComponent<PlayerHiding>();
            if (playerHiding != null && playerHiding.IsHiding)
            {
                return;
            }

            Debug.Log("GAME OVER: Player tertangkap oleh monster!");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver("Tertangkap Monster");
            }
        }
    }
}