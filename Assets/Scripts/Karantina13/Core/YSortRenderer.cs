using UnityEngine;

namespace Karantina13.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class YSortRenderer : MonoBehaviour
    {
        [SerializeField] private int baseOrder = 100;
        [SerializeField] private bool isStatic;

        private SpriteRenderer spriteRenderer;

        public void Configure(int baseOrder, bool isStatic)
        {
            this.baseOrder = baseOrder;
            this.isStatic = isStatic;
            UpdateOrder();
        }

        private void Start()
        {
            UpdateOrder();
            if (isStatic) enabled = false;
        }

        private void LateUpdate() => UpdateOrder();

        private void UpdateOrder()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = baseOrder - Mathf.RoundToInt(transform.position.y * 20f);
        }
    }
}
