using UnityEngine;

namespace Karantina13.UI
{
    public static class UiFonts
    {
        private const string Root = "Karantina13/Fonts/";

        private static Font fallback;
        private static Font mono;
        private static Font monoBold;
        private static Font monoItalic;
        private static Font display;

        public static Font Mono => mono != null ? mono : mono = Load("IBMPlexMono-Regular", Fallback);
        public static Font MonoBold => monoBold != null ? monoBold : monoBold = Load("IBMPlexMono-SemiBold", Mono);
        public static Font MonoItalic => monoItalic != null ? monoItalic : monoItalic = Load("IBMPlexMono-Italic", Mono);
        public static Font Display => display != null ? display : display = Load("Oswald-SemiBold", MonoBold);

        private static Font Fallback => fallback != null ? fallback : fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        private static Font Load(string fileName, Font otherwise)
        {
            Font font = Resources.Load<Font>(Root + fileName);
            return font != null ? font : otherwise;
        }
    }
}
