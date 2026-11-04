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
        public const string Laser = "Laser";

        public static int PlayerIndex => LayerMask.NameToLayer(Player);
        public static int CompanionIndex => LayerMask.NameToLayer(Companion);
        public static int PushableIndex => LayerMask.NameToLayer(Pushable);
        public static int HazardIndex => LayerMask.NameToLayer(Hazard);
        public static int EnvironmentIndex => LayerMask.NameToLayer(Environment);
        public static int LaserIndex => LayerMask.NameToLayer(Laser);

        public static void ApplyCollisionRules()
        {
            IgnoreIfBothExist(CompanionIndex, HazardIndex);
            IgnoreIfBothExist(CompanionIndex, LaserIndex);
            IgnoreIfBothExist(PushableIndex, LaserIndex);
        }

        private static void IgnoreIfBothExist(int first, int second)
        {
            if (first < 0 || second < 0) return;
            Physics2D.IgnoreLayerCollision(first, second, true);
        }
    }
}
