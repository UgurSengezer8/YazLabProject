using UnityEngine;

public class TiledLaser : MonoBehaviour
{
    [Header("Lazer Ayarları")]
    public Transform firePoint;     // Lazerin çıkış noktası (Genellikle objenin kendisi)
    public float maxLength = 15f;   // Önünde engel yoksa uzayacağı maksimum uzunluk
    public LayerMask obstacleLayer; // Lazerin çarpacağı katmanlar (Duvar, Kutu vb.)

    [Header("Vuruş Efekti")]
    public GameObject impactEffect; // Lazerin vurduğu noktada belirecek obje

    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;

    private void Awake()
    {

        // Eğer Inspector'dan ateş noktası seçilmediyse kendi yerini referans al
        if (firePoint == null) firePoint = transform;
    }

    private void Update()
    {
        UpdateLaser();
    }

    private void UpdateLaser()
    {
        // 1. İleriye (Sağa doğru) ışın fırlat
        // firePoint.right: X ekseninde ileri demektir. 
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.up, maxLength, obstacleLayer);

        float currentLength = maxLength; // Başlangıçta maksimum uzunluğu varsay

        if (hit.collider != null)
        {
            // Lazer bir engele çarptı, uzunluğu aradaki mesafe kadar yap
            currentLength = hit.distance;

            // Vuruş efektini aktif et ve koordinatını çarptığı noktaya (hit.point) al
            if (impactEffect != null)
            {
                impactEffect.SetActive(true);
                impactEffect.transform.position = hit.point;
            }
        }
        else
        {
            // Lazer boşluğa gidiyor
            if (impactEffect != null)
            {
                impactEffect.SetActive(false); // Vuruş efektini gizle
            }
        }

        // 2. Sprite Uzunluğunu Güncelle (Tiled modu için)
        // Sprite'ın Y (kalınlık) değerini koruyarak sadece X (uzunluk) değerini değiştiriyoruz
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, currentLength);

        // 3. Box Collider Uzunluğunu ve Merkezini Güncelle
        boxCollider.size = new Vector2(boxCollider.size.x,currentLength);

        // Pivot noktası solda olduğu için, collider'ın merkezini uzunluğun yarısı kadar ileri kaydırıyoruz
        boxCollider.offset = new Vector2( boxCollider.offset.x,currentLength / 2f);
    }
}
