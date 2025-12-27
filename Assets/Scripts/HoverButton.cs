using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI References")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.1f;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float clickScale = 0.95f;

    // --- [BARU] BAGIAN AUDIO ---
    [Header("Audio Settings")]
    [SerializeField] private AudioClip hoverSound; // Tarik file suara hover ke sini
    [SerializeField] private AudioClip clickSound; // Tarik file suara klik ke sini (opsional)
    [Range(0f, 1f)] [SerializeField] private float soundVolume = 1f;

    private AudioSource audioSource;
    // ---------------------------

    private Vector3 originalScale;
    private Coroutine currentCoroutine;

    void Start()
    {
        // Otomatis ambil component Image jika belum di-assign
        if (targetImage == null) 
            targetImage = GetComponent<Image>();
        
        originalScale = transform.localScale;

        // Pastikan sprite awal sesuai
        if (targetImage != null && normalSprite != null)
            targetImage.sprite = normalSprite;

        // --- [BARU] SETUP AUDIO SOURCE OTOMATIS ---
        // Cek apakah tombol sudah punya AudioSource? Kalau belum, kita buatkan.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false; // Supaya tidak bunyi pas game mulai
        // ------------------------------------------
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ganti ke gambar hover
        if (targetImage != null && hoverSprite != null)
            targetImage.sprite = hoverSprite;

        // Animasi transisi scale
        StartScaleAnimation(originalScale * hoverScale);

        // --- [BARU] MAINKAN SUARA HOVER ---
        if (hoverSound != null)
        {
            // PlayOneShot memungkinkan suara tumpang tindih (tidak saling memotong)
            audioSource.PlayOneShot(hoverSound, soundVolume);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Kembali ke gambar normal
        if (targetImage != null && normalSprite != null)
            targetImage.sprite = normalSprite;

        // Animasi scale kembali ke awal
        StartScaleAnimation(originalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Animasi saat diklik
        StartScaleAnimation(originalScale * clickScale);

        // --- [BARU] MAINKAN SUARA KLIK ---
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, soundVolume);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Saat dilepas, kembali ke kondisi hover
        StartScaleAnimation(originalScale * hoverScale);
    }

    private void StartScaleAnimation(Vector3 target)
    {
        if (currentCoroutine != null) 
            StopCoroutine(currentCoroutine);
        
        currentCoroutine = StartCoroutine(AnimateScale(target));
    }

    private IEnumerator AnimateScale(Vector3 target)
    {
        float timer = 0f;
        Vector3 start = transform.localScale;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            t = t * t * (3f - 2f * t); 
            
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
    }
}