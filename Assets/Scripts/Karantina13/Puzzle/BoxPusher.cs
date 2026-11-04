using UnityEngine;

namespace Karantina13.Puzzle
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BoxPusher : MonoBehaviour, IBoxPusher
    {
        private Rigidbody2D body;
        private Vector2 lastPosition;

        public Vector2 PushVelocity { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            lastPosition = body.position;
        }

        private void OnEnable()
        {
            if (body != null) lastPosition = body.position;
        }

        private void FixedUpdate()
        {
            Vector2 current = body.position;
            Vector2 moved = (current - lastPosition) / Time.fixedDeltaTime;
            lastPosition = current;

            Vector2 intended = body.bodyType == RigidbodyType2D.Static ? Vector2.zero : body.linearVelocity;
            PushVelocity = intended.sqrMagnitude > moved.sqrMagnitude ? intended : moved;
        }

        private void OnDisable()
        {
            PushVelocity = Vector2.zero;
        }
    }
}
