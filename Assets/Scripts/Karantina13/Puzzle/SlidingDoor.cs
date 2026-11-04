using Karantina13.Core;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class SlidingDoor : MonoBehaviour, ISwitchable
    {
        [SerializeField] private Collider2D blocker;
        [SerializeField] private SpriteFrameAnimator animator;
        [SerializeField] private bool latchOpen = true;

        private bool isOpen;

        public bool IsOpen => isOpen;

        public event System.Action<bool> OpenChanged;

        public void Configure(Collider2D blocker, SpriteFrameAnimator animator)
        {
            this.blocker = blocker;
            this.animator = animator;
        }

        private void Awake()
        {
            if (animator != null) animator.ShowFrame(0);
            if (blocker != null) blocker.enabled = true;
        }

        public void SetSwitched(bool on)
        {
            if (on) Open();
            else Close();
        }

        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            if (animator != null) animator.PlayForward();
            if (blocker != null) blocker.enabled = false;
            OpenChanged?.Invoke(true);
        }

        public void Close()
        {
            if (!isOpen || latchOpen) return;
            isOpen = false;
            if (animator != null) animator.PlayBackward();
            if (blocker != null) blocker.enabled = true;
            OpenChanged?.Invoke(false);
        }
    }
}
