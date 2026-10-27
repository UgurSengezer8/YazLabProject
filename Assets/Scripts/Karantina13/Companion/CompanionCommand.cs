using Karantina13.Puzzle;
using UnityEngine;

namespace Karantina13.Companion
{
    public readonly struct CompanionCommand
    {
        private CompanionCommand(CompanionCommandType type, Transform followTarget, Vector2 destination, IPushable box, PressurePlate goalPlate)
        {
            Type = type;
            FollowTarget = followTarget;
            Destination = destination;
            Box = box;
            GoalPlate = goalPlate;
        }

        public CompanionCommandType Type { get; }
        public Transform FollowTarget { get; }
        public Vector2 Destination { get; }
        public IPushable Box { get; }
        public PressurePlate GoalPlate { get; }

        public static CompanionCommand Follow(Transform target)
        {
            return new CompanionCommand(CompanionCommandType.Follow, target, Vector2.zero, null, null);
        }

        public static CompanionCommand Wait()
        {
            return new CompanionCommand(CompanionCommandType.Wait, null, Vector2.zero, null, null);
        }

        public static CompanionCommand MoveTo(Vector2 destination)
        {
            return new CompanionCommand(CompanionCommandType.MoveTo, null, destination, null, null);
        }

        public static CompanionCommand PushBox(IPushable box, PressurePlate goal)
        {
            Vector2 destination = goal != null ? goal.Position : Vector2.zero;
            return new CompanionCommand(CompanionCommandType.PushNearestBox, null, destination, box, goal);
        }
    }
}
