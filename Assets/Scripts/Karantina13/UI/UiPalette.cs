using UnityEngine;

namespace Karantina13.UI
{
    public static class UiPalette
    {
        public static readonly Color Accent = Hex(0xF2B544);
        public static readonly Color Muted = Hex(0x8A979C);
        public static readonly Color Body = Hex(0xC9D3D6);
        public static readonly Color Bright = Hex(0xE4ECEE);
        public static readonly Color Line = Hex(0x2A3236);
        public static readonly Color InputLine = Hex(0x3A4448);
        public static readonly Color Dim = Hex(0x3A4448);
        public static readonly Color Well = Hex(0x0B0E0F);
        public static readonly Color Backdrop = Hex(0x0B0D0E);
        public static readonly Color Abyss = Hex(0x070809);
        public static readonly Color GridLine = Hex(0x14191B);
        public static readonly Color PanelFill = new Color(0.055f, 0.071f, 0.078f, 0.96f);
        public static readonly Color KeyFill = Hex(0x1A2023);
        public static readonly Color Green = Hex(0x6FCF97);
        public static readonly Color Error = Hex(0xFF6B5B);
        public static readonly Color ErrorText = Hex(0xFF8A7D);
        public static readonly Color ErrorFill = Hex(0x1C1516);
        public static readonly Color Ink = Hex(0x14110A);
        public static readonly Color Placeholder = Hex(0x7D898E);

        public const string AccentHex = "#F2B544";

        public static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

        public static Color Hex(uint rgb) => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }
}
