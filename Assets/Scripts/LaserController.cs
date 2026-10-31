using UnityEngine;

public class LaserController : MonoBehaviour
{
    public Transform laserPoint;
    public GameObject laserFire;
    [Header("Laser Settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask hitLayers; // Lazerin çarpacağı katmanlar
    [SerializeField] private LineRenderer lineRenderer;

    private void Update()
    {
        ShootLaser();
    }

    private void ShootLaser()
    {
        Vector2 origin = laserPoint.position;
        Vector2 direction = Vector2.up; // Yukarı yönü

        // 2D Raycast atışı yapıyoruz
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, hitLayers);

        Vector2 endPoint;

        if (hit.collider != null)
        {
            // Lazer bir şeye çarptıysa bitiş noktası çarpışma noktasıdır
            endPoint = hit.point;

            // --- İSTEDİĞİN BİLGİLER ---
            Vector3 hitObjectPosition = hit.transform.position; // Değdiği nesnenin merkez konumu
            Vector2 exactHitPoint = hit.point;                 // Lazerin nesneye tam değdiği nokta
            float laserLength = hit.distance;                  // Değdiği nesneye olan uzunluk/mesafe

            // Konsolda değerleri görmek için:
            Debug.Log($"Değdiği Obje: {hit.collider.name}");
            Debug.Log($"Objenin Konumu: {hitObjectPosition}");
            Debug.Log($"Lazer Uzunluğu: {laserLength} birim");
            laserFire.transform.position = new Vector3(laserPoint.position.x, laserPoint.transform.position.y+ laserLength/2, laserPoint.position.z);
            laserFire.transform.localScale = new Vector3(laserFire.transform.localScale.x, laserLength, laserFire.transform.localScale.z);
        }
        else
        {
            // Hiçbir şeye değmiyorsa lazer maksimum mesafeye kadar uzansın
            endPoint = origin + (direction * maxDistance);
        }

        // --- OYUN EKRANINDA ÇİZME (Line Renderer) ---
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, origin);   // 0. indeks: Lazerin başladığı yer
            lineRenderer.SetPosition(1, endPoint); // 1. indeks: Lazerin bittiği yer
        }

        // --- SCENE PENCERESİNDE QUICK DEBUG ÇİZİMİ ---
        Debug.DrawLine(origin, endPoint, Color.red);
    }

}
