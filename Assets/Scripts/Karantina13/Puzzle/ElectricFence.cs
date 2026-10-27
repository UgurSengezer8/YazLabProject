using System.Collections;
using Karantina13.Core;
using Karantina13.UI;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class ElectricFence : MonoBehaviour, ISwitchable
    {
        private const float FlashDuration = 0.2f;
        private const float ShockCooldown = 0.5f;
        private const float KnockbackToDistance = 0.06f;
        private static readonly Color FlashColor = new Color(1f, 0.3f, 0.3f);

        [SerializeField] private bool poweredOnStart = true;
        [SerializeField] private bool invertSignal = true;
        [SerializeField] private Collider2D[] blockers;
        [SerializeField] private SpriteFrameAnimator[] sparks;
        [SerializeField] private SpriteRenderer[] offVisuals;
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private string touchHint = "Elektrik! Ben geçemem ama robot yalıtımlı...";

        private readonly ContactPoint2D[] contacts = new ContactPoint2D[8];
        private bool isPowered;
        private float nextShockTime;
        private IHintDisplay hints;

        public bool IsPowered => isPowered;

        public event System.Action<bool> PowerChanged;

        public void Configure(Collider2D[] blockers, SpriteFrameAnimator[] sparks, SpriteRenderer[] offVisuals)
        {
            this.blockers = blockers;
            this.sparks = sparks;
            this.offVisuals = offVisuals;
        }

        private void Awake()
        {
            GameLayers.ApplyCollisionRules();
            isPowered = poweredOnStart;
            ApplyVisuals();
        }

        public void SetSwitched(bool on)
        {
            SetPowered(invertSignal ? !on : on);
        }

        private void SetPowered(bool powered)
        {
            bool changed = powered != isPowered;
            isPowered = powered;
            ApplyVisuals();
            if (changed) PowerChanged?.Invoke(isPowered);
        }

        private void ApplyVisuals()
        {
            if (blockers != null)
            {
                foreach (Collider2D blocker in blockers)
                    if (blocker != null) blocker.enabled = isPowered;
            }

            if (sparks != null)
            {
                foreach (SpriteFrameAnimator spark in sparks)
                    if (spark != null) spark.gameObject.SetActive(isPowered);
            }

            if (offVisuals != null)
            {
                foreach (SpriteRenderer offVisual in offVisuals)
                    if (offVisual != null) offVisual.enabled = !isPowered;
            }
        }

        private void FixedUpdate()
        {
            if (!isPowered || blockers == null || Time.time < nextShockTime) return;

            foreach (Collider2D blocker in blockers)
            {
                if (blocker == null || !blocker.enabled || !blocker.gameObject.activeInHierarchy) continue;

                int count = blocker.GetContacts(contacts);
                for (int i = 0; i < count; i++)
                {
                    Collider2D other = contacts[i].collider == blocker ? contacts[i].otherCollider : contacts[i].collider;
                    if (other == null || other.attachedRigidbody == null || !IsPlayer(other)) continue;
                    Shock(other.attachedRigidbody, blocker);
                    return;
                }
            }
        }

        private void Shock(Rigidbody2D playerBody, Collider2D blocker)
        {
            nextShockTime = Time.time + ShockCooldown;

            Vector2 playerPosition = playerBody.position;
            Vector2 away = playerPosition - blocker.ClosestPoint(playerPosition);
            if (away.sqrMagnitude < 0.0001f) away = playerPosition - (Vector2)blocker.bounds.center;
            if (away.sqrMagnitude < 0.0001f) away = Vector2.left;

            playerBody.linearVelocity = Vector2.zero;
            playerBody.position = playerPosition + away.normalized * knockbackForce * KnockbackToDistance;

            SpriteRenderer playerRenderer = playerBody.GetComponentInChildren<SpriteRenderer>();
            if (playerRenderer != null) StartCoroutine(Flash(playerRenderer));

            if (hints == null) hints = FindFirstObjectByType<HintDisplay>();
            if (hints != null && !string.IsNullOrEmpty(touchHint)) hints.Show(touchHint, 3f);
        }

        private static IEnumerator Flash(SpriteRenderer target)
        {
            Color original = target.color;
            target.color = FlashColor;
            yield return new WaitForSeconds(FlashDuration);
            if (target != null) target.color = original;
        }

        private static bool IsPlayer(Collider2D other)
        {
            GameObject owner = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            int playerLayer = GameLayers.PlayerIndex;
            if (playerLayer >= 0) return owner.layer == playerLayer;
            return owner.GetComponent<KarantinaCharacterController>() != null && owner.GetComponent<BoxPusher>() == null;
        }
    }
}
