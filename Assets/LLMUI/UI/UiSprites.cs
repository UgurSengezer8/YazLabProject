using System;
using UnityEngine;

namespace Karantina13.UI
{
    public static class UiSprites
    {
        private static Sprite rings;
        private static Sprite disc;
        private static Sprite ring;
        private static Sprite largeRings;
        private static Sprite gridCell;

        public static Sprite Rings => rings != null ? rings : rings = Build(128, RingsAlpha);
        public static Sprite Disc => disc != null ? disc : disc = Build(64, (d, half) => Smooth(half - 1f - d));
        public static Sprite Ring => ring != null ? ring : ring = Build(128, (d, half) => Band(d, half - 3f, 1.5f));
        public static Sprite GridCell => gridCell != null ? gridCell : gridCell = BuildGridCell(72);
        public static Sprite LargeRings => largeRings != null ? largeRings : largeRings = Build(512, LargeRingsAlpha);

        private static float RingsAlpha(float d, float half)
        {
            float s = half / 60f;
            float a = Smooth(5f * s - d);
            a = Mathf.Max(a, Band(d, 16f * s, 1.2f * s));
            a = Mathf.Max(a, 0.75f * Band(d, 28f * s, 1.2f * s));
            a = Mathf.Max(a, 0.55f * Band(d, 40f * s, 1.2f * s));
            a = Mathf.Max(a, 0.35f * Band(d, 52f * s, 1.2f * s));
            return a;
        }

        private static float LargeRingsAlpha(float d, float half)
        {
            float s = half / 60f;
            float a = Smooth(5f * s - d);
            for (int i = 0; i < 5; i++) a = Mathf.Max(a, Band(d, (16f + i * 10f) * s, 0.9f));
            return a;
        }

        private static float Band(float d, float radius, float halfWidth) => Smooth(halfWidth - Mathf.Abs(d - radius));

        private static float Smooth(float x) => Mathf.Clamp01(x + 0.5f);

        private static Sprite BuildGridCell(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point;
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    byte a = (byte)(x == 0 || y == 0 ? 255 : 0);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite Build(int size, Func<float, float, float> alpha)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            float half = size * 0.5f;
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half));
                    byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha(d, half)) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
