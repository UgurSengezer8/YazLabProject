using UnityEngine;

namespace Karantina13.Puzzle
{
    public interface IPushTarget
    {
        Vector2 Position { get; }
        bool IsSatisfied { get; }
    }
}
