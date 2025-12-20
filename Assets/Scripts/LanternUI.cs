using UnityEngine;
using UnityEngine.UI;

public class LanternUI : MonoBehaviour
{
    [Header("Setup Gambar UI")]
    [Tooltip("Masukkan UI Image 'Lentera Kosong/Gelap' (Background)")]
    public Image emptyLanternImage;

    [Tooltip("Masukkan UI Image 'Lentera Terisi/Menyala' (Akan dipotong script)")]
    public Image filledLanternImage;

    [Header("Referensi Player")]
    public LanternController playerLantern;

    void Start()
    {
        // 1. Cari Player otomatis jika kosong
        if (playerLantern == null)
        {
            playerLantern = FindObjectOfType<LanternController>();
        }

        // 2. Setup Gambar Isi (Foreground)
        if (filledLanternImage != null)
        {
            filledLanternImage.type = Image.Type.Filled;
            filledLanternImage.fillMethod = Image.FillMethod.Vertical;
            filledLanternImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            filledLanternImage.preserveAspect = true;

            // FIX LIGHTING: Reset material ke default UI (Unlit/Selalu Terang)
            filledLanternImage.material = null; 
        }

        // 3. Setup Gambar Kosong (Background)
        if (emptyLanternImage != null)
        {
            emptyLanternImage.preserveAspect = true;
            
            // FIX LIGHTING: Reset material ke default UI
            emptyLanternImage.material = null;
        }
        
        // 4. Cek Canvas Render Mode (Opsional tapi membantu)
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning("LanternUI: Disarankan ubah Render Mode Canvas ke 'Screen Space - Overlay' agar UI tidak tertutup kegelapan World.");
        }
    }

    void Update()
    {
        if (playerLantern != null && filledLanternImage != null)
        {
            // Ambil data minyak (0 sampai 100)
            float current = playerLantern.GetCurrentOil();
            float max = playerLantern.maxOil;

            // Hitung rasio (0.0 sampai 1.0)
            float fillRatio = current / max;

            // Update gambar
            filledLanternImage.fillAmount = fillRatio;

            // DEBUGGING: Cek di Console apakah angkanya turun?
            // Hapus baris ini nanti kalau sudah fix
            // Debug.Log($"UI Update: Minyak {current}/{max} = {fillRatio * 100}%");
        }
    }
}
