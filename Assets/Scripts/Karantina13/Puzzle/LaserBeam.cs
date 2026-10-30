using Karantina13.Core;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class LaserBeam : MonoBehaviour, IPushTarget
    {
        private const float KnockbackToDistance = 0.06f;
        private const float CastThickness = 0.1f;
        private const float MinBlockerLength = 0.05f;
        private const float SegmentTolerance = 0.25f;
        private const float PassedDistance = 0.5f;

        [SerializeField] private Vector2 direction = Vector2.down;
        [SerializeField] private float maxLength = 16f;
        [SerializeField] private float beamWidth = 1.6f;
        [SerializeField] private float segmentLength = 2f;
        [SerializeField] private Vector2 exitDirection = Vector2.right;
        [SerializeField] private BoxCollider2D blocker;
        [SerializeField] private SpriteFrameAnimator[] segments;
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private string touchHint = "Lazer! Robot kutuyu ışının önüne itmeli.";

        private Collider2D[] shockColliders;
        private PlayerShock shock;
        private Transform player;
        private int pushableMask;
        private bool isBlocked;
        private float currentLength;

        public bool IsBlocked => isBlocked;
        public float CurrentLength => currentLength;
        public Vector2 Position => (Vector2)transform.position + Axis * segmentLength * 0.5f;
        public bool IsSatisfied => isBlocked || IsPlayerPast();

        public event System.Action<bool> BlockedChanged;

        private Vector2 Axis => direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.down;
        private bool IsHorizontal => Mathf.Abs(Axis.x) > Mathf.Abs(Axis.y);

        public void Configure(BoxCollider2D blocker, SpriteFrameAnimator[] segments, float segmentLength, float maxLength, Vector2 exitDirection)
        {
            this.blocker = blocker;
            this.segments = segments;
            this.segmentLength = segmentLength;
            this.maxLength = maxLength;
            this.exitDirection = exitDirection;
        }

        private void Awake()
        {
            GameLayers.ApplyCollisionRules();
            pushableMask = LayerMask.GetMask(GameLayers.Pushable);
            shockColliders = new Collider2D[] { blocker };
            shock = new PlayerShock(this, knockbackForce * KnockbackToDistance, touchHint);
            player = FindPlayer();
            currentLength = maxLength;
            ApplyLength();
        }

        private void FixedUpdate()
        {
            UpdateBeam();
            shock.Poll(shockColliders);
        }

        private void UpdateBeam()
        {
            Vector2 castSize = IsHorizontal
                ? new Vector2(CastThickness, beamWidth * 0.8f)
                : new Vector2(beamWidth * 0.8f, CastThickness);

            RaycastHit2D hit = pushableMask != 0
                ? Physics2D.BoxCast(transform.position, castSize, 0f, Axis, maxLength, pushableMask)
                : default;

            bool blocked = hit.collider != null;
            currentLength = blocked ? hit.distance : maxLength;
            ApplyLength();

            if (blocked == isBlocked) return;
            isBlocked = blocked;
            BlockedChanged?.Invoke(isBlocked);
        }

        private void ApplyLength()
        {
            if (blocker != null)
            {
                if (currentLength < MinBlockerLength)
                {
                    blocker.enabled = false;
                }
                else
                {
                    Vector2 size = IsHorizontal ? new Vector2(currentLength, beamWidth) : new Vector2(beamWidth, currentLength);
                    Vector2 offset = Axis * currentLength * 0.5f;
                    if (blocker.size != size) blocker.size = size;
                    if (blocker.offset != offset) blocker.offset = offset;
                    if (!blocker.enabled) blocker.enabled = true;
                }
            }

            if (segments == null) return;
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == null) continue;
                bool visible = (i + 1) * segmentLength <= currentLength + SegmentTolerance;
                if (segments[i].gameObject.activeSelf != visible) segments[i].gameObject.SetActive(visible);
            }
        }

        private bool IsPlayerPast()
        {
            if (player == null) return false;
            Vector2 fromBeam = (Vector2)player.position - (Vector2)transform.position;
            return Vector2.Dot(fromBeam, exitDirection) > PassedDistance;
        }

        private static Transform FindPlayer()
        {
            int playerLayer = GameLayers.PlayerIndex;
            foreach (KarantinaCharacterController character in FindObjectsByType<KarantinaCharacterController>(FindObjectsSortMode.None))
            {
                bool onPlayerLayer = playerLayer >= 0 && character.gameObject.layer == playerLayer;
                bool looksLikePlayer = playerLayer < 0 && character.GetComponent<BoxPusher>() == null;
                if (onPlayerLayer || looksLikePlayer) return character.transform;
            }
            return null;
        }
    }
}
