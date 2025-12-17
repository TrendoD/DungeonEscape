using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Karakter yang mau diikuti
    public float smoothSpeed = 0.125f; // Seberapa halus pergerakan kamera
    public Vector3 offset = new Vector3(0, 0, -10); // Jarak kamera (z harus -10 agar 2D terlihat)

    void LateUpdate()
    {
        if (target != null)
        {
            // Tentukan posisi tujuan
            Vector3 desiredPosition = target.position + offset;
            // Gerakkan kamera perlahan ke tujuan (Lerp)
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        }
    }
}