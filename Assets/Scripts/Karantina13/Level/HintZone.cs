using Karantina13.Core;
using Karantina13.Puzzle;
using Karantina13.UI;
using UnityEngine;

namespace Karantina13.Level
{
    public sealed class HintZone : MonoBehaviour
    {
        [SerializeField] private string objective;
        [SerializeField] private string message;
        [SerializeField] private bool onlyOnce = true;

        private bool triggered;

        public void Configure(string objective, string message)
        {
            this.objective = objective;
            this.message = message;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (onlyOnce && triggered) return;
            if (!IsPlayer(other)) return;

            HintDisplay hints = FindFirstObjectByType<HintDisplay>();
            if (hints == null)
            {
                Debug.LogWarning(name + ": no HintDisplay in scene.", this);
                return;
            }

            triggered = true;
            if (!string.IsNullOrEmpty(objective)) hints.SetObjective(objective);
            if (!string.IsNullOrEmpty(message)) hints.Show(message, 4f);
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
