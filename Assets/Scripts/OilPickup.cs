using UnityEngine;
using UnityEngine.Rendering.Universal; // Wajib untuk Light2D

public class OilPickup : MonoBehaviour
{
    [Header("Pengaturan Item")]
    [Tooltip("Jumlah minyak yang ditambahkan (100 = Full Refill)")]
    public float oilAmount = 100f;

    [Header("Visual Efek")]
    [Tooltip("Kecepatan naik turun")]
    public float floatSpeed = 2f;
    [Tooltip("Jarak naik turun")]
    public float floatHeight = 0.15f;

    [Header("Efek Cahaya (Wajib pasang Light 2D)")]
    public Light2D itemLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float pulseSpeed = 3f;

    [Header("Audio (Opsional)")]
    [Tooltip("Masukkan AudioClip suara ambil barang disini")]
    public AudioClip pickupSound;

    private Vector3 startPos;

    void Start()
    {
        // Simpan posisi awal agar naiknya relatif dari sini
        startPos = transform.position;

        // Cari Light2D otomatis jika belum di-assign
        if (itemLight == null)
        {
            itemLight = GetComponent<Light2D>();
            if (itemLight == null) itemLight = GetComponentInChildren<Light2D>();
        }
    }

    void Update()
    {
        // 1. Animasi Floating (Naik Turun)
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 2. Animasi Pulsing Light (Terang Redup)
        if (itemLight != null)
        {
            // Buat nilai 0 sampai 1 berulang-ulang
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            itemLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Cek apakah yang menabrak memiliki tag "Player"
        if (other.CompareTag("Player"))
        {
            // 2. Cari script LanternController di player (atau anak objeknya)
            LanternController lantern = other.GetComponentInChildren<LanternController>();

            if (lantern != null)
            {
                // 3. Isi minyak
                lantern.AddOil(oilAmount);

                // 4. Mainkan suara jika ada (menggunakan PlayClipAtPoint agar suara tidak putus saat object hancur)
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // 5. Hancurkan object Jerigen ini
                Destroy(gameObject);
            }
        }
    }
}
