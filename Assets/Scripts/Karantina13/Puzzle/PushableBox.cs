using System.Collections.Generic;
using UnityEngine;

namespace Karantina13.Puzzle
{
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PushableBox : MonoBehaviour, IPushable, IPlateWeight
    {
        private const float Skin = 0.02f;
        private const float RestDelay = 0.2f;
        private const float MinPushSpeed = 0.1f;
        private const float TouchMargin = 0.1f;

        [SerializeField] private float maxPushSpeed = 3f;

        private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>();
        private readonly List<Collider2D> touching = new List<Collider2D>();
        private Rigidbody2D body;
        private BoxCollider2D boxCollider;
        private ContactFilter2D castFilter;
        private ContactFilter2D touchFilter;
        private float lastMoveTime = -10f;

        public Vector2 Position => body != null ? body.position : (Vector2)transform.position;
        public bool IsResting => Time.time - lastMoveTime > RestDelay;
        public Collider2D Collider => boxCollider != null ? boxCollider : GetComponent<BoxCollider2D>();

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            castFilter = new ContactFilter2D();
            castFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            castFilter.useTriggers = false;

            touchFilter = new ContactFilter2D();
            touchFilter.useTriggers = false;
        }

        public bool TryPush(Vector2 direction, float speed)
        {
            if (body == null || !isActiveAndEnabled) return false;

            Vector2 axis = SnapToAxis(direction);
            if (axis == Vector2.zero || speed <= 0f) return false;

            float distance = Mathf.Min(speed, maxPushSpeed) * Time.fixedDeltaTime;
            if (IsBlocked(axis, distance + Skin)) return false;

            body.MovePosition(body.position + axis * distance);
            lastMoveTime = Time.time;
            return true;
        }

        private void FixedUpdate()
        {
            Bounds bounds = boxCollider.bounds;
            Vector2 center = bounds.center;
            Vector2 size = (Vector2)bounds.size + Vector2.one * (TouchMargin * 2f);
            int count = Physics2D.OverlapBox(center, size, 0f, touchFilter, touching);

            Vector2 bestAxis = Vector2.zero;
            float bestSpeed = MinPushSpeed;
            for (int i = 0; i < count; i++)
            {
                Collider2D other = touching[i];
                if (other == null || other == boxCollider || other.attachedRigidbody == body) continue;

                IBoxPusher pusher = other.GetComponentInParent<IBoxPusher>();
                if (pusher == null) continue;

                Vector2 axis = SnapToAxis(center - (Vector2)other.bounds.center);
                float pushSpeed = Vector2.Dot(pusher.PushVelocity, axis);
                if (pushSpeed <= bestSpeed) continue;

                bestAxis = axis;
                bestSpeed = pushSpeed;
            }

            if (bestAxis != Vector2.zero) TryPush(bestAxis, bestSpeed);
        }

        private bool IsBlocked(Vector2 axis, float distance)
        {
            int count = body.Cast(axis, castFilter, hits, distance);
            for (int i = 0; i < count; i++)
            {
                Collider2D hitCollider = hits[i].collider;
                if (hitCollider == null || hitCollider.isTrigger) continue;
                if (hitCollider.GetComponentInParent<IBoxPusher>() != null) continue;
                return true;
            }
            return false;
        }

        private static Vector2 SnapToAxis(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return Vector2.zero;
            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)) return new Vector2(Mathf.Sign(direction.x), 0f);
            return new Vector2(0f, Mathf.Sign(direction.y));
        }
    }
}
