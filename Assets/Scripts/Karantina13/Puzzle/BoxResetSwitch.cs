using System.Collections.Generic;
using Karantina13.UI;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class BoxResetSwitch : MonoBehaviour
    {
        private const float Cooldown = 1.5f;

        [SerializeField] private Rigidbody2D box;
        [SerializeField] private Vector2 homePosition;
        [SerializeField] private Vector2 homeClearance = new Vector2(1.6f, 1.2f);
        [SerializeField] private string resetMessage = "Kutu başa döndü.";
        [SerializeField] private string blockedMessage = "Kutunun düşeceği yer dolu.";

        private readonly List<Collider2D> found = new List<Collider2D>();
        private float lastUse = -10f;

        public void Configure(Rigidbody2D box, Vector2 homePosition)
        {
            this.box = box;
            this.homePosition = homePosition;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!PlayerContact.IsPlayer(other) || Time.time - lastUse < Cooldown) return;
            lastUse = Time.time;
            TryReset();
        }

        public bool TryReset()
        {
            if (box == null) return false;
            if (Vector2.Distance(box.position, homePosition) < 0.1f) return false;

            bool blocked = IsHomeBlocked();
            if (!blocked)
            {
                box.position = homePosition;
                box.transform.position = homePosition;
            }

            IHintDisplay hints = FindFirstObjectByType<HintDisplay>();
            if (hints != null) hints.Show(blocked ? blockedMessage : resetMessage, 3f);
            return !blocked;
        }

        private bool IsHomeBlocked()
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(box.gameObject.layer));
            int count = Physics2D.OverlapBox(homePosition, homeClearance, 0f, filter, found);
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = found[i];
                if (hit != null && hit.attachedRigidbody != box) return true;
            }
            return false;
        }
    }
}
