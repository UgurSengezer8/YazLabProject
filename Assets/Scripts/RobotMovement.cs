using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Components;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class RobotMovement : MonoBehaviour
{
    public enum PushSide
    {
        None,
        Left,
        Right,
        Top,
        Bottom
    }
    [System.Serializable]
    public struct PushTask
    { 
        public Vector2 robotStartingPosition;
        public Vector2 robotEndingPosition;
        public float pushDistance;
        public Vector2 boxEndPosition;

        public Vector2 pushDirection;
        public PushSide pushSide;
    }
    private Coroutine activeTaskCoroutine;

    public float boxPushPointDistance;

    public Queue<PushTask> TaskQueue = new Queue<PushTask>();
    public Transform point;
    private bool isProcessingQueue = false;
    [SerializeField] private float pushSpeed = 2.5f;
    

    public NavMeshSurface surfaceRobot;
    public NavMeshSurface surface;
    private Camera _mainCamera;
    private NavMeshAgent agent;
    private Transform currentDestination;
    private Vector2[] corners;
    private Vector2[] corners2;
    private PlayerInputs playerInputs;

    Transform box;
    public Transform player;
    [Header("Seçilen Kutu")]
    public GameObject selectedBox; // Tıkladığında kutu burada tutulacak

    [Header("Ayarlar")]
    public LayerMask boxLayer; // Işının sadece kutulara çarpması için

    private BoxColorChange hoveredBox; // Anlık olarak farenin altında olan kutu

    public int currentMenuMission = 5;
    private EventBinding<Mission> missionEventBinding;
    private void OnEnable()
    {
        missionEventBinding = new EventBinding<Mission>(SelectMission);
        EventBus<Mission>.Subscribe(missionEventBinding);
    }
    private void OnDisable()
    {
        // Obje kapandığında abonelikten çıkıyoruz.
        EventBus<Mission>.UnSubscribe(missionEventBinding);
    }
    private void Awake()
    {
        playerInputs = new PlayerInputs();
        playerInputs.Player.Enable();
        playerInputs.Player.Click.performed +=  LeftMouseClicked;
        playerInputs.Player.RightClick.performed += RightMouseClicked;

        _mainCamera = Camera.main;
    }
    private void SelectMission(Mission mission)
    {
        currentMenuMission = mission.id;
    }
    void Start()
    {
        currentMenuMission = 5;
        currentDestination = point;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;/*
        corners = CalculateBoxPathDynamic();
        corners2 = ProcessingPoints(corners);
        BuildTaskQueueFromCorners(corners2, boxPushPointDistance);*/
        agent.isStopped = false;

    }
    private void SelectBox()
    {
        if (currentMenuMission ==1)
        {
            TaskQueue.Clear();

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            // 2. Farenin olduğu noktaya ışın at
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, boxLayer);

            if (hit.collider != null)
            {
                BoxColorChange box = hit.collider.GetComponent<BoxColorChange>();

                // Eğer yeni bir kutunun üzerine geldiysek
                if (box != null && box != hoveredBox)
                {
                    if (hoveredBox != null) hoveredBox.HoverOff(); // Öncekinin rengini sıfırla

                    hoveredBox = box;
                    hoveredBox.HoverOn(); // Yenisini yeşil yap
                }
            }
            else
            {
                // Farenin altında kutu yoksa durumu sıfırla
                if (hoveredBox != null)
                {
                    hoveredBox.HoverOff();
                    hoveredBox = null;
                }
            }
        }
        
    }
    private void Update()
    {
        SelectBox();
        if (currentMenuMission ==2)
        {
            if (box!=null)
            {
            ProcessQueue();

            }
            if (hoveredBox!=null)
            {
                hoveredBox.HoverOff();

            }
    
        }

        if (Vector2.Distance(player.position,transform.position)>2 && currentMenuMission == 0)
        {
            if (hoveredBox != null)
            {
                hoveredBox.HoverOff();

            }
            agent.isStopped = false;
            Debug.Log("çalıştıııııııııııııııııııııııııııııııııı");
            agent.stoppingDistance = 2;
            agent.SetDestination(player.position);

        }
    }
    public void RightMouseClicked(InputAction.CallbackContext context)
    {
        

        
    }
    public void LeftMouseClicked(InputAction.CallbackContext context)
    {
        Debug.Log("currrrrrrrrrrrrrrrrrr"+ currentMenuMission);
        agent.stoppingDistance = 0;
        if (!context.performed) return;
        if (hoveredBox != null)
        {

            selectedBox = hoveredBox.gameObject;
            Debug.Log($"Seçilen GameObject: {selectedBox.name}");
            box = selectedBox.transform;
        }
        if (currentMenuMission ==2 &&box!=null)
        {
            if (hoveredBox != null)
            {
                hoveredBox.HoverOff();

            }
            if (!context.performed) return;
            var rayhit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
            if (!rayhit.collider)
            {
                return;
            }
            Debug.Log("tıklandı");
            TaskQueue.Clear();

            Vector2 worldHitPoint = rayhit.point;
            if (!IsPointOnNavMesh(rayhit.point, surface, 0.2f))
            {
                return;
            }
            point.position = worldHitPoint;
            StopCurrentTask();

            agent.updateRotation = false;
            agent.updateUpAxis = false;
            corners = CalculateBoxPathDynamic();
            corners2 = ProcessingPoints(corners);
            if (corners2.Length != 0)
            {
                BuildTaskQueueFromCorners(corners2, boxPushPointDistance);

            }
            Debug.Log(worldHitPoint);
        }
        if (currentMenuMission == 3)
        {
            if (hoveredBox != null)
            {
                hoveredBox.HoverOff();

            }
            agent.isStopped = false;
            var rayhit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
            if (!rayhit.collider)
            {

                return;
            }
            Debug.Log("tıklandı");

            Vector2 worldHitPoint = rayhit.point;
            if (!IsPointOnNavMesh(rayhit.point, surfaceRobot, 0.2f))
            {
                return;
            }
            TaskQueue.Clear();

            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.SetDestination(worldHitPoint);
        }

    }
    public Vector2[] CalculateBoxPathDynamic()
    {
        NavMeshPath path = new NavMeshPath();
        Vector2[] corn;
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = surface.agentTypeID,
            areaMask = NavMesh.AllAreas
        };
        if (NavMesh.CalculatePath(box.position, point.position, filter, path))
        {
            corn = new Vector2[path.corners.Length];
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                for (int i = 0; i < path.corners.Length; i++)
                {
                    corn[i] = new Vector2(path.corners[i].x, path.corners[i].y);
                }
                return corn;
            }
            else
            {
                Debug.Log("Kutu için kesintisiz bir yol bulunamadı! (Engeller yolu tamamen kapatıyor olabilir)");
            }
        }
        return new Vector2[0];
    }
    private void OnDrawGizmos()
    {
        if (corners == null || corners.Length == 0) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector2(box.position.x+1,box.position.y), 0.2f);
        Gizmos.DrawWireSphere(new Vector2(box.position.x - 1, box.position.y), 0.2f);
        Gizmos.DrawWireSphere(new Vector2(box.position.x , box.position.y+1), 0.2f);
        Gizmos.DrawWireSphere(new Vector2(box.position.x , box.position.y-1), 0.2f);


        Gizmos.color = Color.green;

        for (int i = 0; i < corners.Length; i++)
        {

            Gizmos.DrawWireSphere(corners[i], 0.2f);

            if (i < corners.Length - 1)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
        Gizmos.color = Color.red;

        for (int i = 0; i < corners2.Length; i++)
        {

            Gizmos.DrawWireSphere(corners2[i], 0.15f);

            if (i < corners2.Length - 1)
            {
                Gizmos.DrawLine(corners2[i], corners2[i + 1]);
            }
        }
    }

    private Vector2[] ProcessingPoints(Vector2[] corners)
    {
        if (corners==null)
        {
            return new Vector2[0];
        }
        Vector2 currentPoint;
        Vector2 nextPoint;
        Queue<Vector2> positionQueue = new Queue<Vector2>();
        for (int i = 0; i < corners.Length-1; i++)
        {

            currentPoint = corners[i];
            nextPoint = corners[i+1];
            Vector2 fark = (currentPoint - nextPoint).normalized;
            if (Mathf.Abs(fark.x)>0.999f|| Mathf.Abs(fark.y) > 0.999f)
            {
                Debug.Log(i+"ile"+(i+1)+"aradaki açı dik");  //dik olduğunu bulduk 
                positionQueue.Enqueue(currentPoint);
                Debug.Log(i +"ye eklendi" +currentPoint);  //dik olduğunu bulduk 

            }
            else
            {
                positionQueue.Enqueue(currentPoint);

                Debug.Log(i +"ye eklendi" +currentPoint);  //dik olduğunu bulduk 


                Vector2 newNode = new Vector2(currentPoint.x, nextPoint.y);
                if (IsPointOnNavMesh(newNode,surface,0.2f))
                {
                    positionQueue.Enqueue(newNode);

                }
                else
                {
                    newNode = new Vector2(nextPoint.x, currentPoint.y);
                    positionQueue.Enqueue(newNode);

                }

                Debug.Log(i + "ye eklendi" + newNode);  //dik olduğunu bulduk 

                Debug.Log(i+"ile"+(i+1)+"aradaki açı eğik");
                //burda köşe oluşturmalıyız

            }
        }
        positionQueue.Enqueue(corners[corners.Length-1]);
        Vector2[] newCorners = new Vector2[positionQueue.Count];
        Debug.Log("count "+ positionQueue.Count);
        int c = positionQueue.Count;

        for (int i = 0; i < c; i++)
        {
            newCorners[i] = positionQueue.Dequeue();
        }
        return newCorners;
    }
    public void ProcessQueue()
    {
        if (!isProcessingQueue && TaskQueue.Count > 0)
        {
            activeTaskCoroutine =  StartCoroutine(ExecuteTasksRoutine());
        }
    }
    private Vector2 GetSideDirection(PushSide side)
    {
        switch (side)
        {
            case PushSide.Left: return Vector2.left*1.2f;     // (-1.2, 0)
            case PushSide.Right: return Vector2.right * 1.2f;   // (1.2, 0)
            case PushSide.Top: return Vector2.up * 1.2f;        // (0, 1.2)
            case PushSide.Bottom: return Vector2.down * 1.2f;   // (0, -1.2)
            default: return Vector2.zero;
        }
    }
    private IEnumerator ExecuteTasksRoutine()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        isProcessingQueue = true;
        PushSide lastPushSide =PushSide.None;

        float cornerDistance = 1;

        while (TaskQueue.Count > 0)
        {
            
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            // 1. Kuyruğun en başındaki görevi al
            PushTask currentTask = TaskQueue.Dequeue();
            
            if (lastPushSide==PushSide.None)
            {
                //ilk giderken
                Debug.Log("nonemuş");
                
                Vector3 kutuyaGoreKonum = (gameObject.transform.position -box.transform.position).normalized;
                Debug.Log("kutuyaGöre= "+kutuyaGoreKonum);
                //Debug.Log(kutuyaGoreKonum);
                if (Mathf.Abs(kutuyaGoreKonum.x) > Mathf.Abs(kutuyaGoreKonum.y))
                {

                    //yatayda bi yerde
                    if (kutuyaGoreKonum.x>0)
                    {
                        lastPushSide = PushSide.Right;
                    }
                    if (kutuyaGoreKonum.x < 0)
                    {
                        lastPushSide = PushSide.Left;
                    }
                }
                if (Mathf.Abs(kutuyaGoreKonum.y) > Mathf.Abs(kutuyaGoreKonum.x))
                {

                    if (kutuyaGoreKonum.y > 0)
                    {
                        lastPushSide = PushSide.Top;
                    }
                    if (kutuyaGoreKonum.y < 0)
                    {
                        lastPushSide = PushSide.Bottom;
                    }
                    //dikeyde bi yerde
                }
                Debug.Log("nondan gelen" + lastPushSide);
            }
            Vector2 lastPushSideVector = GetSideDirection(lastPushSide);
            Vector2 currentPushSideVector = GetSideDirection(currentTask.pushSide);
            bool flag = false;
            Debug.Log("last = " +lastPushSideVector);
            Debug.Log("current = " + currentPushSideVector);
            if (currentTask.pushSide == lastPushSide)
            {
                Debug.Log("yön aynı");
            }
            else
            {
                Vector2 cornerDir = new Vector2(box.position.x, box.position.y) + currentPushSideVector + lastPushSideVector;
                Vector2 cornerD = Vector2.zero;
                agent.isStopped = false;
                

                if (currentPushSideVector + lastPushSideVector == Vector2.zero)
                {
                    Debug.Log("yönler zıt");
                    flag = true;
                    //zıt yönlerde itmemizi istiyor
                    if (lastPushSide==PushSide.Left)
                    {
                        //rigt a gitcek
                        Debug.Log("a");

                        cornerD = new Vector2(box.position.x, box.position.y) + GetSideDirection(PushSide.Left) + GetSideDirection(PushSide.Bottom);
                        lastPushSide = PushSide.Bottom;
                    }
                    else if (lastPushSide == PushSide.Right)
                    {
                        //left e gitcek
                        Debug.Log("sağdayız zıt");
                         cornerD = new Vector2(box.position.x, box.position.y) + GetSideDirection(PushSide.Right) + GetSideDirection(PushSide.Bottom);
                        Debug.Log(cornerD);
                        lastPushSide = PushSide.Bottom;


                    }
                    else if (lastPushSide == PushSide.Top)
                    {
                        Debug.Log("b");

                        //bottom a gitcek
                        cornerD = new Vector2(box.position.x, box.position.y) + GetSideDirection(PushSide.Left) + GetSideDirection(PushSide.Top);
                        lastPushSide = PushSide.Left;


                    }
                    else if (lastPushSide == PushSide.Bottom)
                    {
                        Debug.Log("c");

                        cornerD = new Vector2(box.position.x, box.position.y) + GetSideDirection(PushSide.Left) + GetSideDirection(PushSide.Bottom);
                        lastPushSide = PushSide.Right;

                    }
                    while (agent.pathPending || agent.remainingDistance > 0.1f)
                    {
                        yield return null; // Bir sonraki kareye kadar bekle
                    }
                }
                if (flag ==true)
                {
                    agent.SetDestination(cornerD);

                    while (agent.pathPending || agent.remainingDistance > 0.1f)
                    {

                        yield return null; 
                    }
                    flag = false;

                }
                lastPushSideVector = GetSideDirection(lastPushSide);
                currentPushSideVector = GetSideDirection(currentTask.pushSide);
                cornerDir = new Vector2(box.position.x, box.position.y) + currentPushSideVector + lastPushSideVector;

                agent.SetDestination(cornerDir);
                
                while (agent.pathPending || agent.remainingDistance > 0.1f)
                {
                    yield return null; 
                }
            }
            
            //köşeye vardı itme noktasına git burdan sonra
            agent.isStopped = false;
            agent.SetDestination(currentTask.robotStartingPosition);

            while (agent.pathPending || agent.remainingDistance > 0.1f)
            {
                yield return null; 
            }
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            while (Vector2.Distance(new Vector2(box.position.x, box.position.y), currentTask.boxEndPosition) > 0.1f)
            {
                
                transform.position += new Vector3 (currentTask.pushDirection.x, currentTask.pushDirection.y ,0) * pushSpeed * Time.deltaTime;

               

                yield return null;
            }
            box.position = currentTask.boxEndPosition;
            Debug.Log("kutu vardı");
            
            lastPushSide = currentTask.pushSide;
        }
        lastPushSide = PushSide.None;
        activeTaskCoroutine = null;
        isProcessingQueue = false;
        Debug.Log("Kuyruktaki tüm itme görevleri başarıyla tamamlandı.");
    }
    public void BuildTaskQueueFromCorners(Vector2[] corners , float pushDistance)
    {
        Vector2 pushPoint = Vector2.zero;
        Vector2 TargetPushPoint = Vector2.zero;
        PushSide pushSide;
        TaskQueue.Clear();
        for (int i = 0; i < corners2.Length - 1; i++)
        {
            if (Mathf.Abs(corners2[i].x - corners2[i + 1].x) > Mathf.Abs(corners2[i].y - corners2[i + 1].y))
            {
                if (corners2[i].x > corners2[i + 1].x)
                {
                    //sola gitmeli

                    Debug.Log("sola gitmeli");
                    pushSide = PushSide.Right;
                    pushPoint = new Vector2(corners2[i].x + pushDistance, corners2[i].y);
                    TargetPushPoint = new Vector2(corners2[i + 1].x + pushDistance, corners2[i + 1].y);
                }
                else
                {
                    pushSide = PushSide.Left;

                    Debug.Log("Sağa gitmeli");
                    pushPoint = new Vector2(corners2[i].x - pushDistance, corners2[i].y);
                    TargetPushPoint = new Vector2(corners2[i + 1].x - pushDistance, corners2[i + 1].y);
                    
                    //sağa gitmeli
                }
            }
            else
            {
                //yukarı yada aşağı
                if (corners2[i].y > corners2[i + 1].y)
                {
                    pushSide = PushSide.Top;

                    Debug.Log("Aşağı gitmeli");
                    pushPoint = new Vector2(corners2[i].x, corners2[i].y + pushDistance);
                    TargetPushPoint = new Vector2(corners2[i + 1].x, corners2[i + 1].y + pushDistance);


                    //aşağı gitmeli
                }
                else
                {
                    pushSide = PushSide.Bottom;

                    Debug.Log("yukarı gitmeli");
                    pushPoint = new Vector2(corners2[i].x, corners2[i].y - pushDistance);
                    TargetPushPoint = new Vector2(corners2[i + 1].x, corners2[i + 1].y - pushDistance);


                    //yukarı gitmeli
                }
            }
            TaskQueue.Enqueue(new PushTask
            {
                robotStartingPosition = pushPoint,
                robotEndingPosition = TargetPushPoint,
                boxEndPosition = corners2[i + 1],
                pushDirection = (corners2[i + 1] - corners2[i]).normalized,
                pushSide = pushSide,
            });
        }
    }
    public bool IsPathValid(Vector3 startPosition, Vector3 targetPosition,NavMeshSurface _surface)
    {
        NavMeshPath path = new NavMeshPath();

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = _surface.agentTypeID,
            areaMask = NavMesh.AllAreas
        };

        bool hasPath = NavMesh.CalculatePath(startPosition, targetPosition, filter, path);

        if (hasPath && path.status == NavMeshPathStatus.PathComplete)
        {
            return true; 
        }

        return false; 
    }
    private bool IsPointOnNavMesh(Vector2 point, NavMeshSurface surface, float maxDistance = 0.2f)
    {
        NavMeshHit hit;

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = surface.agentTypeID, 
            areaMask = NavMesh.AllAreas
        };

        Vector3 checkPosition = new Vector3(point.x, point.y, transform.position.z);

        
        return NavMesh.SamplePosition(checkPosition, out hit, maxDistance, filter);
    }
    public void StopCurrentTask()
    {
        
        if (activeTaskCoroutine != null)
        {
            StopCoroutine(activeTaskCoroutine);
            activeTaskCoroutine = null;
        }

        
        TaskQueue.Clear();
        isProcessingQueue = false;

        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }
}
