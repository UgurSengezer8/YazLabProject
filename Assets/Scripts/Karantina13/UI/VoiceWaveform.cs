using UnityEngine;
using UnityEngine.UI;

namespace Karantina13.UI
{
    public sealed class VoiceWaveform : MonoBehaviour
    {
        public enum Mode { Calm, Active, Flat }

        private RectTransform area;
        private RectTransform[] bars;
        private Image[] images;
        private float seed;
        private Mode mode = Mode.Calm;

        public void Build(RectTransform target, int count, float barWidth)
        {
            area = target;
            bars = new RectTransform[count];
            images = new Image[count];
            seed = Random.value * 100f;
            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject("Bar" + i, typeof(RectTransform));
                go.transform.SetParent(area, false);
                Image image = go.AddComponent<Image>();
                image.raycastTarget = false;
                RectTransform rect = (RectTransform)go.transform;
                float x = (i + 0.5f) / count;
                rect.anchorMin = new Vector2(x, 0.5f);
                rect.anchorMax = new Vector2(x, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(barWidth, 2f);
                bars[i] = rect;
                images[i] = image;
            }
        }

        public void SetMode(Mode value, Color color)
        {
            mode = value;
            if (images == null) return;
            for (int i = 0; i < images.Length; i++)
            {
                bool gap = value == Mode.Flat && i % 2 == 1;
                images[i].color = gap ? Color.clear : color;
            }
        }

        private void Update()
        {
            if (bars == null) return;
            float maxHeight = Mathf.Max(4f, area.rect.height - 20f);
            float t = Time.unscaledTime;
            for (int i = 0; i < bars.Length; i++)
            {
                float height;
                switch (mode)
                {
                    case Mode.Flat:
                        height = 2f;
                        break;
                    case Mode.Active:
                        height = Mathf.Lerp(0.15f, 1f, Mathf.PerlinNoise(seed + i * 0.35f, t * 3.2f)) * maxHeight;
                        break;
                    default:
                        height = Mathf.Lerp(0.06f, 0.4f, Mathf.PerlinNoise(seed + i * 0.25f, t * 0.9f)) * maxHeight;
                        break;
                }
                bars[i].sizeDelta = new Vector2(bars[i].sizeDelta.x, Mathf.Max(2f, height));
            }
        }
    }
}
