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
        Debug.Log(currentLookDirection);
    }
    private void Update()
    {
        currentState.UpdateState(this);
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
}
