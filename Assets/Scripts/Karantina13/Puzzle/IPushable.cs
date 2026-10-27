using UnityEngine;

namespace Karantina13.Puzzle
{
    public interface IPushable
    {
        Vector2 Position { get; }
        bool IsResting { get; }
        bool TryPush(Vector2 direction, float speed);
        Collider2D Collider { get; }
    }
}
