using UnityEngine;
using UnityEngine.UI;

public class KeyUI : MonoBehaviour
{
    [Header("Setup UI")]
    [Tooltip("Masukkan UI Image Icon Kunci (Satu gambar saja cukup)")]
    public Image keyImage;

    [Header("Warna & Overlay")]
    public Color normalColor = Color.white;
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Gelap & Transparan

    [Header("Referensi Player")]
    public PlayerMovement player;

    private GameObject xOverlay;

    void Start()
    {
        // 1. Cari Player otomatis jika kosong
        if (player == null)
            player = FindObjectOfType<PlayerMovement>();

        // 2. Setup Image utama
        if (keyImage != null)
        {
            keyImage.preserveAspect = true;
            keyImage.material = null; // Pastikan tidak gelap karena lighting

            // 3. Buat "X" secara programatik (lewat script)
            CreateXOverlay();
        }

        // 4. Cek Canvas
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogWarning("KeyUI: Disarankan gunakan 'Screen Space - Overlay' pada Canvas.");
        }
    }

    void CreateXOverlay()
    {
        // Membuat GameObject baru sebagai child dari keyImage
        GameObject go = new GameObject("X_Overlay");
        go.transform.SetParent(keyImage.transform);
        go.layer = keyImage.gameObject.layer; // Pastikan layer sama dengan parent
        
        // Reset posisi dan ukuran agar memenuhi icon kunci
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one; // Reset scale

        // Tambah teks "X"
        Text xText = go.AddComponent<Text>();
        xText.text = "X";
        xText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Font standar Unity
        xText.fontSize = 40;
        xText.color = Color.red;
        xText.alignment = TextAnchor.MiddleCenter;
        
        // Agar terlihat meski ukuran container kecil (35x35)
        xText.horizontalOverflow = HorizontalWrapMode.Overflow;
        xText.verticalOverflow = VerticalWrapMode.Overflow;
        xText.raycastTarget = false;
        
        // Simpan referensi
        xOverlay = go;
    }

    void Update()
    {
        if (player != null && keyImage != null)
        {
            bool hasKey = player.HasKey();

            // Jika punya kunci: Icon terang, X hilang
            // Jika belum: Icon gelap, X muncul
            if (hasKey)
            {
                keyImage.color = normalColor;
                if (xOverlay != null) xOverlay.SetActive(false);
            }
            else
            {
                keyImage.color = lockedColor;
                if (xOverlay != null) xOverlay.SetActive(true);
            }
        }
    }
}