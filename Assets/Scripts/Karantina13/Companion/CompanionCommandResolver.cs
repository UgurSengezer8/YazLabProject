using Karantina13.Puzzle;
using UnityEngine;

namespace Karantina13.Companion
{
    public sealed class CompanionCommandResolver
    {
        private const float OnPlateDistance = 0.75f;

        public bool TryFindNearestBox(Vector2 from, out IPushable box)
        {
            box = null;
            PushableBox[] boxes = Object.FindObjectsByType<PushableBox>(FindObjectsSortMode.None);
            PressurePlate[] plates = Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);

            float bestDistance = float.MaxValue;
            bool bestIsPlaced = true;
            foreach (PushableBox candidate in boxes)
            {
                if (!candidate.isActiveAndEnabled) continue;

                bool placed = IsOnPressedPlate(candidate.Position, plates);
                float distance = (candidate.Position - from).sqrMagnitude;
                bool better = box == null
                    || (bestIsPlaced && !placed)
                    || (placed == bestIsPlaced && distance < bestDistance);
                if (!better) continue;

                box = candidate;
                bestDistance = distance;
                bestIsPlaced = placed;
            }
            return box != null;
        }

        public bool TryFindGoalPlate(Vector2 boxPosition, out PressurePlate plate)
        {
            plate = null;
            float bestDistance = float.MaxValue;
            foreach (PressurePlate candidate in Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None))
            {
                if (!candidate.isActiveAndEnabled || candidate.IsPressed) continue;

                float distance = (candidate.Position - boxPosition).sqrMagnitude;
                if (distance >= bestDistance) continue;

                plate = candidate;
                bestDistance = distance;
            }
            return plate != null;
        }

        public bool TryFindGoal(Vector2 boxPosition, out IPushTarget goal)
        {
            goal = null;
            float bestDistance = float.MaxValue;
            ConsiderGoals(Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None), boxPosition, ref goal, ref bestDistance);
            ConsiderGoals(Object.FindObjectsByType<LaserBeam>(FindObjectsSortMode.None), boxPosition, ref goal, ref bestDistance);
            return goal != null;
        }

        private static void ConsiderGoals<T>(T[] candidates, Vector2 boxPosition, ref IPushTarget goal, ref float bestDistance) where T : MonoBehaviour, IPushTarget
        {
            foreach (T candidate in candidates)
            {
                if (candidate == null || !candidate.isActiveAndEnabled || candidate.IsSatisfied) continue;

                float distance = (candidate.Position - boxPosition).sqrMagnitude;
                if (distance >= bestDistance) continue;

                goal = candidate;
                bestDistance = distance;
            }
        }

        private static bool IsOnPressedPlate(Vector2 position, PressurePlate[] plates)
        {
            foreach (PressurePlate plate in plates)
            {
                if (plate.IsPressed && Vector2.Distance(plate.Position, position) <= OnPlateDistance) return true;
            }
            return false;
        }
    }
}
