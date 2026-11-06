using NavMeshPlus.Components;
using UnityEngine;

public class BarrierControl : MonoBehaviour
{
    private bool open = false;
    private EventBinding<BoxTargetStateEvent> boxTargetEventBinding;

    [Header("Target Settings")]
    public string targetID;

    [Header("Barrier Objects")]
    // Inspector'dan BarrierDown ve BarriersUp objelerini sürükle
    [SerializeField] private GameObject barrierDown;
    [SerializeField] private GameObject barriersUp;

    [Header("NavMesh Settings")]
    public NavMeshSurface surface;
    public NavMeshSurface surfaceRobot;
    private int count = 0;

    private Collider2D doorCollider;
    // private Animator animator; // Eğer animasyon da oynatacaksan burayı açabilirsin

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        // animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        boxTargetEventBinding = new EventBinding<BoxTargetStateEvent>(DoorOpening);
        EventBus<BoxTargetStateEvent>.Subscribe(boxTargetEventBinding);
    }

    private void OnDisable()
    {
        EventBus<BoxTargetStateEvent>.UnSubscribe(boxTargetEventBinding);
    }

    private void DoorOpening(BoxTargetStateEvent e)
    {
        if (targetID != e.targetID) return;

        if (e.open)
        {
            count++;
            if (e.count<=count )
            {
                open = true;
                OpenDoor();
                Debug.Log("Bariyerler açıldı (Aşağıda)");
            }
            
        }
        else
        {
            count--;
            if (count<e.count)
            {
                open = false;
                CloseDoor();
                Debug.Log("Bariyerler kapandı (Yukarıda)");
            }
            
        }
    }

    public void OpenDoor()
    {
        if (barrierDown != null) barrierDown.SetActive(true);
        if (barriersUp != null) barriersUp.SetActive(false);


        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        RebuildNavMesh();
    }

    public void CloseDoor()
    {
        if (barrierDown != null) barrierDown.SetActive(false);
        if (barriersUp != null) barriersUp.SetActive(true);


        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }

        RebuildNavMesh();
    }

    private void RebuildNavMesh()
    {
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
        if (surfaceRobot != null)
        {
            surfaceRobot.BuildNavMesh();
        }
    }
}
