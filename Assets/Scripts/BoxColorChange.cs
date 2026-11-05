using UnityEngine;

public class BoxColorChange : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color defaultColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.color; // Kutunun normal rengini hafızaya al
    }

    public void HoverOn()
    {
        spriteRenderer.color = Color.green; // Üzerine gelince yeşil yap
    }

    public void HoverOff()
    {
        spriteRenderer.color = defaultColor; // Üzerinden çıkınca eski haline döndür
    }
}
