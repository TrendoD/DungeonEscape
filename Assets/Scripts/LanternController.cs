using UnityEngine;
using UnityEngine.Rendering.Universal; // Namespace untuk Light2D

public class LanternController : MonoBehaviour
{
    [Header("Komponen")]
    [Tooltip("Masukkan komponen Light 2D di sini (harus tipe Point Light)")]
    public Light2D lanternLight;
    
    [Tooltip("Tarik Global Light (penerang scene saat dev) ke sini agar otomatis mati saat mulai")]
    public Light2D globalLight;

    [Header("Pengaturan Minyak")]
    public float maxOil = 100f;
    [Tooltip("Jumlah minyak berkurang per detik. GDD: 1% tiap 3 detik = ~0.33")]
    public float depletionRate = 0.33f; 
    
    [Header("Visual Cahaya")]
    public float maxOuterRadius = 6f; // Radius saat minyak 100%
    public float minOuterRadius = 1.5f; // Radius saat minyak hampir habis
    
    // Private variables
    private float currentOil;

    void Start()
    {
        currentOil = maxOil;

        // Matikan Global Light jika di-assign
        if (globalLight != null)
        {
            globalLight.enabled = false;
        }
        
        // Coba cari Light2D di anak objek jika belum di-assign
        if (lanternLight == null)
        {
            lanternLight = GetComponentInChildren<Light2D>();
        }

        if (lanternLight == null)
        {
            Debug.LogError("LanternController: Tidak menemukan komponen Light2D! Pastikan ada Light 2D (Point) di anak objek Player.");
        }
    }

    void Update()
    {
        if (currentOil > 0)
        {
            // Kurangi minyak seiring waktu
            currentOil -= depletionRate * Time.deltaTime;
            
            // Batasi agar tidak minus
            if (currentOil < 0) currentOil = 0;

            // Update tampilan cahaya
            UpdateLightVisuals();

            // Cek kondisi mati
            if (currentOil <= 0)
            {
                OnOilDepleted();
            }
        }
    }

    void UpdateLightVisuals()
    {
        if (lanternLight != null)
        {
            // Hitung persentase sisa minyak (0.0 sampai 1.0)
            float oilPercentage = currentOil / maxOil;
            
            // Ubah radius cahaya berdasarkan persentase minyak
            // Mathf.Lerp akan transisi halus dari minRadius ke maxRadius
            lanternLight.pointLightOuterRadius = Mathf.Lerp(minOuterRadius, maxOuterRadius, oilPercentage);
            
            // Opsional: Kecilkan inner radius juga agar konsisten
            lanternLight.pointLightInnerRadius = lanternLight.pointLightOuterRadius * 0.1f;
        }
    }

    void OnOilDepleted()
    {
        Debug.Log("GAME OVER: Minyak Lentera Habis! (Gelap Total)");
        // TODO: Panggil fungsi Game Over di script GameManager nanti
    }

    // Fungsi untuk menambah minyak (dipakai saat ambil item Jerigen)
    public void AddOil(float amount)
    {
        currentOil += amount;
        if (currentOil > maxOil) currentOil = maxOil;
        
        UpdateLightVisuals();
        Debug.Log($"Minyak diisi! Sekarang: {currentOil}");
    }
    
    // Getter untuk UI
    public float GetCurrentOil()
    {
        return currentOil;
    }
}
