using UnityEngine;

namespace Karantina13.Core
{
    public static class GameLayers
    {
        public const string Player = "Player";
        public const string Companion = "Companion";
        public const string Pushable = "Pushable";
        public const string Hazard = "Hazard";
        public const string Environment = "Environment";

        public static int PlayerIndex => LayerMask.NameToLayer(Player);
        public static int CompanionIndex => LayerMask.NameToLayer(Companion);
        public static int PushableIndex => LayerMask.NameToLayer(Pushable);
        public static int HazardIndex => LayerMask.NameToLayer(Hazard);
        public static int EnvironmentIndex => LayerMask.NameToLayer(Environment);

        public static void ApplyCollisionRules()
        {
            int companion = CompanionIndex;
            int hazard = HazardIndex;
            if (companion < 0 || hazard < 0) return;
            Physics2D.IgnoreLayerCollision(companion, hazard, true);
        }
    }
}
