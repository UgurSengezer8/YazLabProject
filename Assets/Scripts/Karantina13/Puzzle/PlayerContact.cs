using Karantina13.Core;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public static class PlayerContact
    {
        public static bool IsPlayer(Collider2D other)
        {
            if (other == null) return false;
            GameObject owner = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            int playerLayer = GameLayers.PlayerIndex;
            if (playerLayer >= 0) return owner.layer == playerLayer;
            return owner.GetComponent<KarantinaCharacterController>() != null && owner.GetComponent<BoxPusher>() == null;
        }
    }
}
