using System.Linq.Expressions;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }
    private Rigidbody2D rb;
    private EventBinding<PlayerMoveEvent> MoveDirectionBinding;
    private Vector2 currentDirection;
    public int moveSpeed;
    public bool isWalking;
    private IState currentState;
    public LookDirection currentLookDirection;
    public bool isTouchingBox;
    
    private void OnEnable()
    {
        MoveDirectionBinding = new EventBinding<PlayerMoveEvent>(HandleMoveEvent);
        EventBus<PlayerMoveEvent>.Subscribe(MoveDirectionBinding);
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        currentState = new IdleState();
        currentState.EnterState(this);
    }
    public void ChangeState(IState newState)
    {
        currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }
    void HandleMoveEvent(PlayerMoveEvent e)
    {
        currentDirection = e.direction;
        isWalking = e.isWalking;
        currentLookDirection = GetDirection(currentDirection);
    }
    private void Update()
    {
        currentState.UpdateState(this);
        isTouchingBox = IsTouchingBox(currentLookDirection);
        //Debug.Log(isTouchingBox);
    }
    void FixedUpdate()
    {
        rb.linearVelocity = currentDirection*moveSpeed;
    }
    private void OnDisable()
    {
        EventBus<PlayerMoveEvent>.UnSubscribe(MoveDirectionBinding);

    }
    public enum LookDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
    public LookDirection GetDirection(Vector2 direction )
    {
        if (direction.sqrMagnitude < 0.01f)
            return LookDirection.None;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x > 0 ? LookDirection.Right : LookDirection.Left;
        }
        else
        {
            return direction.y > 0 ? LookDirection.Up : LookDirection.Down;
        }
    }
    [Header("Pushing Settings")]
    [SerializeField] private LayerMask pushableLayer; 
    [SerializeField] private float rayDistance = 0.6f;

    
    public bool IsTouchingBox( LookDirection direction)
    {
        if (isWalking==false) return false;

        Vector2 rayDir = direction switch
        {
            LookDirection.Up => Vector2.up,
            LookDirection.Down => Vector2.down,
            LookDirection.Left => Vector2.left,
            LookDirection.Right => Vector2.right,
            _ => Vector2.zero
        };

        if (rayDir == Vector2.zero) return false;

        RaycastHit2D hit = Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y + +0.5f, transform.position.z), rayDir, rayDistance, pushableLayer);

        return hit.collider != null;
    }
    private void OnDrawGizmosSelected()
    {
        Vector2 rayDir = currentLookDirection switch
        {
            LookDirection.Up => Vector2.up,
            LookDirection.Down => Vector2.down,
            LookDirection.Left => Vector2.left,
            LookDirection.Right => Vector2.right,
            _ => Vector2.down
        };

        RaycastHit2D hit = Physics2D.Raycast(new Vector3(transform.position.x,transform.position.y+0.5f,transform.position.z), rayDir, rayDistance, pushableLayer);

        if (hit.collider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y + +0.5f, transform.position.z), hit.point);

            Gizmos.DrawWireSphere(hit.point, 0.05f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y + +0.5f, transform.position.z), rayDir * rayDistance);
        }
    }
}
