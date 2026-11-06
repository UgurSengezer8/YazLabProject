using Unity.VisualScripting;
using UnityEngine;

public class PlateControler : MonoBehaviour
{
    public bool open { get; private set; }
    [Header("Target Settings")]
    public string targetID;
    [Header("Visual Settings (Plate Rengi)")]
    [SerializeField] private SpriteRenderer plateRenderer;
    [SerializeField] private Color defaultColor = Color.white; 
    [SerializeField] private Color activeColor = Color.green;
    public int count;
    private void Awake()
    {
        if (plateRenderer == null)
        {
            plateRenderer = GetComponent<SpriteRenderer>();
        }
        UpdateColor();
    }
    private void UpdateColor()
    {
        if (plateRenderer != null)
        {
            plateRenderer.color = open ? activeColor : defaultColor;
        }
    }
    public void SetOpen(bool open)
    {
        if (open == this.open) return; 

        this.open = open;
        Debug.Log($"TargetZone {targetID} durumu değişti: {open}");
        EventBus<BoxTargetStateEvent>.Publish(new BoxTargetStateEvent() { open=this.open ,targetID =this.targetID,count=count});
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("box"))
        {
            SetOpen(true);
            UpdateColor();

        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("box"))
        {
            SetOpen(false);
            UpdateColor();
        }
    }
}
