using System.Collections.Generic;
using Karantina13.Core;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class SlidingDoor : MonoBehaviour, ISwitchable
    {
        [SerializeField] private Collider2D blocker;
        [SerializeField] private SpriteFrameAnimator animator;
        [SerializeField] private bool latchOpen = true;

        private readonly List<Collider2D> occupants = new List<Collider2D>();
        private bool isOpen;
        private bool closePending;

        public bool IsOpen => isOpen;
        public bool IsLatched => latchOpen;

        public event System.Action<bool> OpenChanged;

        public void Configure(Collider2D blocker, SpriteFrameAnimator animator)
        {
            this.blocker = blocker;
            this.animator = animator;
        }

        public void SetLatch(bool latch)
        {
            latchOpen = latch;
        }

        private void Awake()
        {
            if (animator != null) animator.ShowFrame(0);
            if (blocker != null) blocker.enabled = true;
        }

        private void FixedUpdate()
        {
            if (!closePending || IsDoorwayOccupied()) return;
            closePending = false;
            ApplyClose();
        }

        public void SetSwitched(bool on)
        {
            if (on) Open();
            else Close();
        }

        public void Open()
        {
            closePending = false;
            if (isOpen) return;
            isOpen = true;
            if (animator != null) animator.PlayForward();
            if (blocker != null) blocker.enabled = false;
            OpenChanged?.Invoke(true);
        }

        public void LockOpen()
        {
            latchOpen = true;
            Open();
        }

        public void Close()
        {
            if (!isOpen || latchOpen) return;
            if (IsDoorwayOccupied())
            {
                closePending = true;
                return;
            }
            ApplyClose();
        }

        private void ApplyClose()
        {
            if (!isOpen || latchOpen) return;
            isOpen = false;
            if (animator != null) animator.PlayBackward();
            if (blocker != null) blocker.enabled = true;
            OpenChanged?.Invoke(false);
        }

        private bool IsDoorwayOccupied()
        {
            BoxCollider2D box = blocker as BoxCollider2D;
            if (box == null) return false;

            Vector2 center = box.transform.TransformPoint(box.offset);
            Vector3 scale = box.transform.lossyScale;
            Vector2 size = new Vector2(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y));

            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;
            filter.SetLayerMask(LayerMask.GetMask(GameLayers.Player, GameLayers.Companion, GameLayers.Pushable));
            int count = Physics2D.OverlapBox(center, size, 0f, filter, occupants);
            for (int i = 0; i < count; i++)
            {
                if (occupants[i] != null && occupants[i] != blocker) return true;
            }
            return false;
        }
    }
}
