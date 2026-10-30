using Karantina13.Core;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class ElectricFence : MonoBehaviour, ISwitchable
    {
        private const float KnockbackToDistance = 0.06f;

        [SerializeField] private bool poweredOnStart = true;
        [SerializeField] private bool invertSignal = true;
        [SerializeField] private Collider2D[] blockers;
        [SerializeField] private SpriteFrameAnimator[] sparks;
        [SerializeField] private SpriteRenderer[] offVisuals;
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private string touchHint = "Elektrik! Ben geçemem ama robot yalıtımlı...";

        private PlayerShock shock;
        private bool isPowered;

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
            shock = new PlayerShock(this, knockbackForce * KnockbackToDistance, touchHint);
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
            if (!isPowered || shock == null) return;
            shock.Poll(blockers);
        }
    }
}
