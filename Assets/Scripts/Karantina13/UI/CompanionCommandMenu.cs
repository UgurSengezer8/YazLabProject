using Karantina13.Companion;
using Karantina13.Core;
using Karantina13.Puzzle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Karantina13.UI
{
    public sealed class CompanionCommandMenu : MonoBehaviour
    {
        private static readonly string[] OptionLabels = { "1   Takip et", "2   Bekle", "3   Kutuyu it", "4   Buraya git" };
        private static readonly Color PanelColor = new Color(0.02f, 0.05f, 0.08f, 0.88f);
        private static readonly Color AccentColor = new Color(0.4f, 0.94f, 1f);
        private static readonly Color ButtonColor = new Color(0.1f, 0.16f, 0.22f, 1f);
        private static readonly Color ButtonHoverColor = new Color(0.16f, 0.42f, 0.5f, 1f);

        [SerializeField] private MonoBehaviour receiverSource;
        [SerializeField] private Transform player;

        private readonly CompanionCommandResolver resolver = new CompanionCommandResolver();
        private IHintDisplay hints;
        private GameObject panel;
        private Font font;
        private bool isPickingTarget;
        private int pickStartFrame;
        private bool warnedBadSource;

        public bool IsOpen => panel != null && panel.activeSelf;

        public void Configure(Transform player, MonoBehaviour receiverSource)
        {
            this.player = player;
            this.receiverSource = receiverSource;
        }

        private void Awake()
        {
            HintDisplay display = GetComponent<HintDisplay>();
            if (display == null) display = FindFirstObjectByType<HintDisplay>();
            hints = display;
            if (player == null) player = FindPlayer();
            EnsureEventSystem();
            BuildPanel();
            panel.SetActive(false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (isPickingTarget)
            {
                UpdateTargetPick(keyboard);
                return;
            }

            if (keyboard == null) return;
            if (keyboard.qKey.wasPressedThisFrame)
            {
                SetOpen(!IsOpen);
                return;
            }
            if (!IsOpen) return;

            if (keyboard.escapeKey.wasPressedThisFrame) SetOpen(false);
            else if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) Select(0);
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) Select(1);
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) Select(2);
            else if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame) Select(3);
        }

        private void SetOpen(bool open)
        {
            panel.SetActive(open);
            if (!open && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void Select(int index)
        {
            SetOpen(false);

            ICompanionCommandReceiver receiver = FindReceiver();
            if (receiver == null)
            {
                ShowHint("Robot komut alıcısı bağlı değil.", 3f);
                return;
            }

            switch (index)
            {
                case 0:
                    receiver.Execute(CompanionCommand.Follow(player));
                    ShowHint("Robot: Takip ediyorum.", 2f);
                    break;
                case 1:
                    receiver.Execute(CompanionCommand.Wait());
                    ShowHint("Robot: Burada bekliyorum.", 2f);
                    break;
                case 2:
                    SendPushCommand(receiver);
                    break;
                case 3:
                    isPickingTarget = true;
                    pickStartFrame = Time.frameCount;
                    ShowHint("Hedefi fareyle seç (Esc: iptal)", 10f);
                    break;
            }
        }

        private void SendPushCommand(ICompanionCommandReceiver receiver)
        {
            Vector2 from = receiver.Transform != null ? (Vector2)receiver.Transform.position : Vector2.zero;
            if (!resolver.TryFindNearestBox(from, out IPushable box) || !resolver.TryFindGoal(box.Position, out IPushTarget goal))
            {
                ShowHint("Yakında itilecek kutu yok", 2.5f);
                return;
            }

            receiver.Execute(CompanionCommand.PushBox(box, goal));
            ShowHint("Robot: Kutuyu itiyorum.", 2f);
        }

        private void UpdateTargetPick(Keyboard keyboard)
        {
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame))
            {
                isPickingTarget = false;
                ShowHint("Hedef seçimi iptal edildi.", 1.5f);
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null || Time.frameCount == pickStartFrame || !mouse.leftButton.wasPressedThisFrame) return;

            isPickingTarget = false;
            Camera cam = Camera.main;
            ICompanionCommandReceiver receiver = FindReceiver();
            if (cam == null || receiver == null)
            {
                ShowHint("Robot komut alıcısı bağlı değil.", 3f);
                return;
            }

            Vector2 screenPoint = mouse.position.ReadValue();
            Vector2 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, -cam.transform.position.z));
            receiver.Execute(CompanionCommand.MoveTo(worldPoint));
            ShowHint("Robot: Hedefe gidiyorum.", 2f);
        }

        private ICompanionCommandReceiver FindReceiver()
        {
            if (receiverSource != null)
            {
                if (receiverSource is ICompanionCommandReceiver assigned) return assigned;
                if (!warnedBadSource)
                {
                    Debug.LogWarning(name + ": " + receiverSource.name + " does not implement ICompanionCommandReceiver.", this);
                    warnedBadSource = true;
                }
            }

            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is ICompanionCommandReceiver found) return found;
            }
            return null;
        }

        private void ShowHint(string message, float duration)
        {
            if (hints != null) hints.Show(message, duration);
        }

        private static Transform FindPlayer()
        {
            int playerLayer = GameLayers.PlayerIndex;
            foreach (KarantinaCharacterController character in FindObjectsByType<KarantinaCharacterController>(FindObjectsSortMode.None))
            {
                bool onPlayerLayer = playerLayer >= 0 && character.gameObject.layer == playerLayer;
                bool looksLikePlayer = playerLayer < 0 && character.GetComponent<BoxPusher>() == null;
                if (onPlayerLayer || looksLikePlayer) return character.transform;
            }
            Debug.LogWarning("CompanionCommandMenu: player not found.");
            return null;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private void BuildPanel()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject root = new GameObject("CommandCanvas", typeof(RectTransform));
            root.transform.SetParent(transform, false);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            const float width = 380f;
            const float buttonHeight = 58f;
            const float spacing = 10f;
            const float titleHeight = 64f;
            float height = titleHeight + OptionLabels.Length * (buttonHeight + spacing) + 14f;

            panel = new GameObject("CommandPanel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(1f, 0.5f);
            panelRect.anchorMax = new Vector2(1f, 0.5f);
            panelRect.pivot = new Vector2(1f, 0.5f);
            panelRect.anchoredPosition = new Vector2(-40f, 0f);
            panelRect.sizeDelta = new Vector2(width, height);
            panel.AddComponent<Image>().color = PanelColor;

            Text title = CreateText(panel.transform, "ROBOT KOMUTLARI", 26, TextAnchor.MiddleCenter, AccentColor);
            title.fontStyle = FontStyle.Bold;
            PlaceRow(title.rectTransform, 0f, titleHeight, 0f);

            for (int i = 0; i < OptionLabels.Length; i++)
            {
                int index = i;
                float top = titleHeight + i * (buttonHeight + spacing);
                Button button = CreateButton(panel.transform, OptionLabels[i]);
                PlaceRow((RectTransform)button.transform, top, buttonHeight, 18f);
                button.onClick.AddListener(() => Select(index));
            }
        }

        private Button CreateButton(Transform parent, string label)
        {
            GameObject go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = Color.white;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = ButtonHoverColor;
            colors.pressedColor = AccentColor;
            colors.selectedColor = ButtonColor;
            colors.fadeDuration = 0.05f;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            Text text = CreateText(go.transform, label, 24, TextAnchor.MiddleLeft, Color.white);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 0f);
            textRect.offsetMax = new Vector2(-12f, 0f);
            return button;
        }

        private Text CreateText(Transform parent, string content, int size, TextAnchor alignment, Color color)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static void PlaceRow(RectTransform rect, float top, float height, float sideMargin)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(sideMargin, -top - height);
            rect.offsetMax = new Vector2(-sideMargin, -top);
        }
    }
}
