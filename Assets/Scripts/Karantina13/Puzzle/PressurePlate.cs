using System.Collections.Generic;
using Karantina13.Core;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class PressurePlate : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(1.4f, 1.4f);
        [SerializeField] private float centerTolerance = 0.6f;
        [SerializeField] private bool stayPressed = true;
        [SerializeField] private MonoBehaviour[] targets;
        [SerializeField] private SpriteRenderer plateRenderer;
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color pressedColor = new Color(0.55f, 1f, 0.6f);

        private readonly List<Collider2D> found = new List<Collider2D>();
        private bool isPressed;

        public bool IsPressed => isPressed;
        public bool IsOccupied => isPressed;
        public Vector2 Position => transform.position;

        public event System.Action<bool> PressedChanged;

        public void Configure(MonoBehaviour[] targets, SpriteRenderer plateRenderer)
        {
            this.targets = targets;
            this.plateRenderer = plateRenderer;
        }

        private void Awake()
        {
            if (targets == null) return;
            foreach (MonoBehaviour target in targets)
            {
                if (target != null && !(target is ISwitchable))
                    Debug.LogWarning(name + ": " + target.name + " is not ISwitchable and will be ignored.", this);
            }
        }

        private void Start()
        {
            isPressed = HasWeight();
            ApplyTint();
            NotifyTargets();
        }

        private void FixedUpdate()
        {
            if (stayPressed && isPressed) return;

            bool pressed = HasWeight();
            if (pressed == isPressed) return;

            isPressed = pressed;
            ApplyTint();
            PressedChanged?.Invoke(isPressed);
            NotifyTargets();
        }

        private bool HasWeight()
        {
            Vector2 center = Position;
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;
            int count = Physics2D.OverlapBox(center, size, 0f, filter, found);
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = found[i];
                IPlateWeight weight = hit.GetComponentInParent<IPlateWeight>();
                if (weight != null && Vector2.Distance(weight.Position, center) <= centerTolerance) return true;
            }
            return false;
        }

        private void ApplyTint()
        {
            if (plateRenderer != null) plateRenderer.color = isPressed ? pressedColor : idleColor;
        }

        private void NotifyTargets()
        {
            if (targets == null) return;
            foreach (MonoBehaviour target in targets)
            {
                ISwitchable switchable = target as ISwitchable;
                if (switchable != null) switchable.SetSwitched(isPressed);
            }
        }
    }
}
