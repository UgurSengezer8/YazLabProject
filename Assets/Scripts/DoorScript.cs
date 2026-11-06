using System.Threading;
using NavMeshPlus.Components;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    private bool open = false;
    private EventBinding<BoxTargetStateEvent> boxTargetEventBinding;
    [Header("Target Settings")]

    public string targetID;
    [Header("Animation State Names")]
    [SerializeField] private string openStateName = "DoorOpen";   // Animator penceresindeki State adı
    [SerializeField] private string closeStateName = "DoorClose";

    private Animator animator;
    private Collider2D doorCollider;
    public NavMeshSurface surface;
    public NavMeshSurface surfaceRobot;


    public GameObject child;
    private int count;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        doorCollider = GetComponent<Collider2D>();
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
        if (targetID!=e.targetID)
        {
            return;

        }
        if (e.open == open)
        {
            return;
        }
        if (e.open == true)
        {
            count++;
            //kapıyı aç
            
            if (e.count <= count && open ==false)
            {
                open = true;
                OpenDoor();

                Debug.Log("kapı açıldı");
            }
        }
        else
        {
            count--;

            if (count < e.count && open == true)
            {
                open = false;
                CloseDoor();
                Debug.Log("kapı kapandı");
            }
            

        }
    }
    public void OpenDoor()
    {
        animator.Play(openStateName, 0, 0f);
        child.SetActive(false);
        if (surface!=null)
        {
            surface.BuildNavMesh();

        }
        if (surfaceRobot != null)
        {
            surfaceRobot.BuildNavMesh();

        }
        // 2. Fizik engelini kaldır
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }

    public void CloseDoor()
    {
        animator.Play(closeStateName, 0, 0f);
        child.SetActive(true);
        if (surface != null)
        {
            surface.BuildNavMesh();

        }
        if (surfaceRobot != null)
        {
            surfaceRobot.BuildNavMesh();

        }
        if (doorCollider != null)
        {
            doorCollider.enabled = true;
        }
    }
}
