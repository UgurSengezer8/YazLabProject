using Karantina13.UI;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class WallLever : MonoBehaviour
    {
        [SerializeField] private SlidingDoor[] doorsToLock;
        [SerializeField] private Behaviour[] retiredParts;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Color pulledColor = new Color(0.55f, 1f, 0.6f);
        [SerializeField] private string pulledMessage = "Kol çekildi. Kapı artık açık kalacak.";

        private bool pulled;

        public bool IsPulled => pulled;

        public event System.Action Pulled;

        public void Configure(SlidingDoor[] doorsToLock, Behaviour[] retiredParts, SpriteRenderer visual, string pulledMessage)
        {
            this.doorsToLock = doorsToLock;
            this.retiredParts = retiredParts;
            this.visual = visual;
            this.pulledMessage = pulledMessage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (pulled || !PlayerContact.IsPlayer(other)) return;
            Pull();
        }

        public void Pull()
        {
            if (pulled) return;
            pulled = true;

            if (doorsToLock != null)
            {
                foreach (SlidingDoor door in doorsToLock)
                {
                    if (door != null) door.LockOpen();
                }
            }
            if (retiredParts != null)
            {
                foreach (Behaviour part in retiredParts)
                {
                    if (part != null) part.enabled = false;
                }
            }
            if (visual != null)
            {
                visual.flipX = !visual.flipX;
                visual.color = pulledColor;
            }

            IHintDisplay hints = FindFirstObjectByType<HintDisplay>();
            if (hints != null && !string.IsNullOrEmpty(pulledMessage)) hints.Show(pulledMessage, 4f);
            Pulled?.Invoke();
        }
    }
}
