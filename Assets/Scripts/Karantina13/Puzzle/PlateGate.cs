using Karantina13.Core;
using UnityEngine;

namespace Karantina13.Puzzle
{
    public sealed class PlateGate : MonoBehaviour
    {
        [SerializeField] private PressurePlate[] inputs;
        [SerializeField] private MonoBehaviour[] outputs;

        private bool isOn;
        private bool initialized;

        public bool IsOn => isOn;

        public void Configure(PressurePlate[] inputs, MonoBehaviour[] outputs)
        {
            this.inputs = inputs;
            this.outputs = outputs;
        }

        private void FixedUpdate()
        {
            bool on = AllPressed();
            if (initialized && on == isOn) return;
            initialized = true;
            isOn = on;
            if (outputs == null) return;
            foreach (MonoBehaviour output in outputs)
            {
                ISwitchable switchable = output as ISwitchable;
                if (switchable != null) switchable.SetSwitched(isOn);
            }
        }

        private bool AllPressed()
        {
            if (inputs == null || inputs.Length == 0) return false;
            foreach (PressurePlate plate in inputs)
            {
                if (plate == null || !plate.IsPressed) return false;
            }
            return true;
        }
    }
}
