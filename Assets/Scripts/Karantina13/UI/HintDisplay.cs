using UnityEngine;
using UnityEngine.UI;

namespace Karantina13.UI
{
    public sealed class HintDisplay : MonoBehaviour, IHintDisplay
    {
        private const float FadeTime = 0.6f;
        private const string LegendText = "WASD: Hareket   Q: Robot komutları";

        private static readonly Color PanelColor = new Color(0.02f, 0.05f, 0.08f, 0.78f);
        private static readonly Color AccentColor = new Color(0.4f, 0.94f, 1f);

        private Font font;
        private GameObject objectivePanel;
        private Text objectiveText;
        private CanvasGroup messageGroup;
        private Text messageText;
        private float messageHideTime;

        private void Awake()
        {
            BuildCanvas();
        }

        public void Show(string message, float duration)
        {
            if (messageText == null) BuildCanvas();
            messageText.text = message;
            messageHideTime = Time.unscaledTime + Mathf.Max(duration, FadeTime);
            messageGroup.alpha = 1f;
        }

        public void SetObjective(string text)
        {
            if (objectiveText == null) BuildCanvas();
            objectiveText.text = text;
            objectivePanel.SetActive(!string.IsNullOrEmpty(text));
        }

        private void Update()
        {
            if (messageGroup == null) return;
            float remaining = messageHideTime - Time.unscaledTime;
            messageGroup.alpha = Mathf.Clamp01(remaining / FadeTime);
        }

        private void BuildCanvas()
        {
            if (messageText != null) return;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject root = new GameObject("HintCanvas", typeof(RectTransform));
            root.transform.SetParent(transform, false);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            objectivePanel = CreatePanel(root.transform, "Objective", new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(760f, 84f));
            Text objectiveTitle = CreateText(objectivePanel.transform, "Title", 20, TextAnchor.UpperLeft, AccentColor);
            objectiveTitle.text = "GÖREV";
            SetInsets(objectiveTitle.rectTransform, 20f, 10f);
            objectiveText = CreateText(objectivePanel.transform, "Text", 28, TextAnchor.LowerLeft, Color.white);
            SetInsets(objectiveText.rectTransform, 20f, 10f);
            objectivePanel.SetActive(false);

            GameObject messagePanel = CreatePanel(root.transform, "Message", new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(1000f, 90f));
            messageGroup = messagePanel.AddComponent<CanvasGroup>();
            messageGroup.alpha = 0f;
            messageGroup.blocksRaycasts = false;
            messageText = CreateText(messagePanel.transform, "Text", 30, TextAnchor.MiddleCenter, Color.white);
            SetInsets(messageText.rectTransform, 24f, 8f);

            GameObject legendPanel = CreatePanel(root.transform, "Legend", new Vector2(0f, 0f), new Vector2(30f, 30f), new Vector2(520f, 48f));
            Text legend = CreateText(legendPanel.transform, "Text", 22, TextAnchor.MiddleLeft, new Color(0.86f, 0.91f, 0.95f));
            legend.text = LegendText;
            SetInsets(legend.rectTransform, 16f, 4f);
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)panel.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image background = panel.AddComponent<Image>();
            background.color = PanelColor;
            background.raycastTarget = false;
            return panel;
        }

        private Text CreateText(Transform parent, string name, int size, TextAnchor alignment, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            go.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.8f);
            return text;
        }

        private static void SetInsets(RectTransform rect, float horizontal, float vertical)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(horizontal, vertical);
            rect.offsetMax = new Vector2(-horizontal, -vertical);
        }
    }
}
