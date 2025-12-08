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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ganti ke gambar hover
        if (targetImage != null && hoverSprite != null)
            targetImage.sprite = hoverSprite;

        // Animasi transisi scale membesar sedikit (smooth)
        StartScaleAnimation(originalScale * hoverScale);
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
        // Animasi saat diklik (mengecil sedikit)
        StartScaleAnimation(originalScale * clickScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Saat dilepas, kembali ke kondisi hover (karena cursor masih di atas button)
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
            // Menggunakan SmoothStep untuk transisi yang lebih natural
            t = t * t * (3f - 2f * t); 
            
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
    }
}
