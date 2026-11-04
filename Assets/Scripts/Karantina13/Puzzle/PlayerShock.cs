using System.Collections;
using System.Collections.Generic;
using Karantina13.Core;
using Karantina13.UI;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class PlayerShock
    {
        private const float FlashDuration = 0.2f;
        private const float ShockCooldown = 0.5f;
        private const float HintDuration = 3f;
        private static readonly Color FlashColor = new Color(1f, 0.3f, 0.3f);

        private readonly MonoBehaviour owner;
        private readonly float pushDistance;
        private readonly string hint;
        private readonly ContactPoint2D[] contacts = new ContactPoint2D[8];
        private float nextShockTime;
        private IHintDisplay hints;

        public PlayerShock(MonoBehaviour owner, float pushDistance, string hint)
        {
            this.owner = owner;
            this.pushDistance = pushDistance;
            this.hint = hint;
        }

        public void Poll(IReadOnlyList<Collider2D> colliders)
        {
            if (colliders == null || Time.time < nextShockTime) return;

            for (int c = 0; c < colliders.Count; c++)
            {
                Collider2D source = colliders[c];
                if (source == null || !source.enabled || !source.gameObject.activeInHierarchy) continue;

                int count = source.GetContacts(contacts);
                for (int i = 0; i < count; i++)
                {
                    Collider2D other = contacts[i].collider == source ? contacts[i].otherCollider : contacts[i].collider;
                    if (other == null || other.attachedRigidbody == null || !IsPlayer(other)) continue;
                    Shock(other.attachedRigidbody, source);
                    return;
                }
            }
        }

        private void Shock(Rigidbody2D playerBody, Collider2D source)
        {
            nextShockTime = Time.time + ShockCooldown;

            Vector2 playerPosition = playerBody.position;
            Vector2 away = playerPosition - source.ClosestPoint(playerPosition);
            if (away.sqrMagnitude < 0.0001f) away = playerPosition - (Vector2)source.bounds.center;
            if (away.sqrMagnitude < 0.0001f) away = Vector2.left;

            playerBody.linearVelocity = Vector2.zero;
            playerBody.position = playerPosition + away.normalized * pushDistance;

            SpriteRenderer playerRenderer = playerBody.GetComponentInChildren<SpriteRenderer>();
            if (playerRenderer != null && owner != null && owner.isActiveAndEnabled) owner.StartCoroutine(Flash(playerRenderer));

            if (hints == null) hints = Object.FindFirstObjectByType<HintDisplay>();
            if (hints != null && !string.IsNullOrEmpty(hint)) hints.Show(hint, HintDuration);
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
            GameObject body = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            int playerLayer = GameLayers.PlayerIndex;
            if (playerLayer >= 0) return body.layer == playerLayer;
            return body.GetComponent<KarantinaCharacterController>() != null && body.GetComponent<BoxPusher>() == null;
        }
    }
}
