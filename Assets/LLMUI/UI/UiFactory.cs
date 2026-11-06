using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Karantina13.UI
{
    public static class UiFactory
    {
        public static RectTransform CreateOverlayCanvas(Transform parent, string name, int sortingOrder)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvas.pixelPerfect = true;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            root.AddComponent<GraphicRaycaster>();
            return (RectTransform)root.transform;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        public static Text CreateText(Transform parent, string name, Font font, int size, TextAnchor alignment, Color color)
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
            return text;
        }

        public static Image CreateFrame(Transform parent, string name, Color border, Color fill, out RectTransform inner)
        {
            Image outer = CreateImage(parent, name, border);
            Image body = CreateImage(outer.transform, "Fill", fill);
            inner = body.rectTransform;
            Stretch(inner, 2f, 2f, 2f, 2f);
            return outer;
        }

        public static Image CreateImage(Transform parent, string name, Color color, Sprite sprite = null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static RectTransform CreateRect(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static Button CreateButton(Transform parent, string name, string label, Color border, Color fill, Color textColor, Font font, int size, out Image fillImage, out Text text)
        {
            Image frame = CreateFrame(parent, name, border, fill, out RectTransform inner);
            frame.raycastTarget = true;
            fillImage = inner.GetComponent<Image>();
            Button button = frame.gameObject.AddComponent<Button>();
            button.targetGraphic = frame;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.fadeDuration = 0.05f;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            text = CreateText(inner, "Label", font, size, TextAnchor.MiddleCenter, textColor);
            text.text = label;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
            return button;
        }

        public static Text CreateKeycap(Transform parent, Vector2 anchor, Vector2 position, string label = "SPACE", float width = 104f)
        {
            Image frame = CreateFrame(parent, "Key", UiPalette.InputLine, UiPalette.KeyFill, out RectTransform inner);
            Place(frame.rectTransform, anchor, position, new Vector2(width, 42f));
            inner.offsetMin = new Vector2(2f, 5f);
            Text text = CreateText(inner, "Label", UiFonts.Display, 19, TextAnchor.MiddleCenter, UiPalette.Bright);
            text.text = label;
            Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
            return text;
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static void PlaceTop(RectTransform rect, float left, float right, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -top - height);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void PlaceBottom(RectTransform rect, float bottom, float height)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(0f, bottom);
            rect.offsetMax = new Vector2(0f, bottom + height);
        }

        public static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}
