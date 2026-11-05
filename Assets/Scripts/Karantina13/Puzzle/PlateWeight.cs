using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class PlateWeight : MonoBehaviour, IPlateWeight
    {
        private Collider2D feet;

        public Vector2 Position => feet != null && feet.enabled ? (Vector2)feet.bounds.center : (Vector2)transform.position;

        private void Awake()
        {
            foreach (Collider2D candidate in GetComponentsInChildren<Collider2D>())
            {
                if (candidate.isTrigger) continue;
                feet = candidate;
                break;
            }
        }
    }
}
