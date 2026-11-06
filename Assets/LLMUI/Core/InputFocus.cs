using UnityEngine;

namespace Karantina13.Core
{
    public static class InputFocus
    {
        public static bool IsTyping { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            IsTyping = false;
        }
    }
}
