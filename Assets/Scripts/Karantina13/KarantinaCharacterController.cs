using UnityEngine;
using UnityEngine.InputSystem;

namespace Karantina13
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
    public sealed class KarantinaCharacterController : MonoBehaviour
    {
        public enum ControlScheme { WASD, ArrowKeys }
        public ControlScheme controls;
        [Min(0.1f)] public float moveSpeed = 4f;
        public string characterLabel = "PLAYER";

        public Vector2 MoveInput { get; private set; }
        public string Facing { get; private set; } = "Down";
        public bool IsMoving => MoveInput.sqrMagnitude > 0.001f;
        private Rigidbody2D body;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Vector2 spawnPosition;
        private string currentState;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            spawnPosition = body.position;
        }

        private void Update()
        {
            MoveInput = ReadMovement(Keyboard.current, controls);
            if (IsMoving)
            {
                if (Mathf.Abs(MoveInput.x) > Mathf.Abs(MoveInput.y))
                    Facing = MoveInput.x > 0 ? "Right" : "Left";
                else
                    Facing = MoveInput.y > 0 ? "Up" : "Down";
            }
            string state = (IsMoving ? "Move_" : "Idle_") + Facing;
            if (state != currentState)
            {
                animator.Play(state, 0, 0f);
                currentState = state;
            }
        }

        public static Vector2 ReadMovement(Keyboard keyboard, ControlScheme scheme)
        {
            if (keyboard == null) return Vector2.zero;
            bool arrows = scheme == ControlScheme.ArrowKeys;
            float x = ((arrows ? keyboard.rightArrowKey : keyboard.dKey).isPressed ? 1 : 0)
                    - ((arrows ? keyboard.leftArrowKey : keyboard.aKey).isPressed ? 1 : 0);
            float y = ((arrows ? keyboard.upArrowKey : keyboard.wKey).isPressed ? 1 : 0)
                    - ((arrows ? keyboard.downArrowKey : keyboard.sKey).isPressed ? 1 : 0);
            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }

        private void FixedUpdate() => body.linearVelocity = MoveInput * moveSpeed;
        private void LateUpdate() => spriteRenderer.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 20f);

        private void OnDisable()
        {
            MoveInput = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                MoveInput = Vector2.zero;
                if (body != null) body.linearVelocity = Vector2.zero;
            }
        }

        public void ResetToSpawn()
        {
            body.linearVelocity = Vector2.zero;
            body.position = spawnPosition;
            transform.position = spawnPosition;
            MoveInput = Vector2.zero;
            Facing = "Down";
            currentState = null;
        }
    }
}
