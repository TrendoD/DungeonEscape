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

    private Transform playerTarget;
    private int currentPointIndex;
    private Rigidbody2D rb;
    private Vector2 movement;
    private float waitTimer;
    
    // --- TAMBAHAN BARU: DETEKSI MACET ---
    private float stuckTimer; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Mencari Player otomatis
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        PickRandomWaypoint();
    }

    void Update()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer < chaseDistance)
        {
            // --- KEJAR PLAYER ---
            MoveTo(playerTarget.position, chaseSpeed);
            stuckTimer = 0; // Reset timer macet kalau lagi ngejar
        }
        else
        {
            // --- PATROLI ---
            Patrol();
        }

        // Animasi Flip
        if (movement.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (movement.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Transform targetPoint = waypoints[currentPointIndex];
        MoveTo(targetPoint.position, patrolSpeed);

        // 1. Cek apakah sudah sampai tujuan?
        if (Vector2.Distance(transform.position, targetPoint.position) < stopDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > 1f) // Diam 1 detik di titik itu
            {
                PickRandomWaypoint();
            }
        }

        // 2. CEK APAKAH NABRAK TEMBOK? (Stuck Detection)
        // Jika monster "berusaha jalan" TAPI "kecepatannya hampir 0"
        // (Gunakan 'velocity' atau 'linearVelocity' tergantung versi Unity)
        if (rb.linearVelocity.magnitude < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            
            // Kalau macet lebih dari 0.5 detik di tembok
            if (stuckTimer > 0.5f)
            {
                // Ganti tujuan patroli ke titik lain!
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
            waitTimer = 0; // Reset timer tunggu
        }
    }

    void MoveTo(Vector3 target, float speed)
    {
        Vector3 direction = target - transform.position;
        direction.Normalize();
        movement = direction;
        
        // Unity 6 pakai linearVelocity
        rb.linearVelocity = movement * speed;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
}