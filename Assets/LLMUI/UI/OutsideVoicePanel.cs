using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Karantina13.Core;
using Karantina13.Dialogue;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Karantina13.UI.UiFactory;
using static Karantina13.UI.UiPalette;

namespace Karantina13.UI
{
    public sealed class OutsideVoicePanel : MonoBehaviour
    {
        private enum LinkState { Idle, Thinking, Offline }

        private const int MaxLength = 200;
        private const int MaxHistory = 24;
        private const int PreviewLength = 48;
        private const float ToastDuration = 5f;
        private const float PulsePeriod = 1.6f;
        private const float PanelMaxWidth = 1800f;
        private const float PanelHeight = 540f;
        private const float ScreenMargin = 60f;
        private const float BottomMargin = 48f;

        [SerializeField] private MonoBehaviour channelSource;
        [SerializeField, Min(1f)] private float responseTimeout = 25f;

        private readonly Queue<GameObject> rows = new Queue<GameObject>();
        private readonly ConcurrentQueue<string> incoming = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> failures = new ConcurrentQueue<string>();

        private IOutsideVoiceChannel channel;
        private LinkState state = LinkState.Idle;
        private bool isOpen;
        private bool warnedBadSource;
        private int unread;
        private int dotPhase;
        private float nextDotTime;
        private float waitStartTime;
        private float toastHideTime;
        private float nextResolveTime;
        private float lastCanvasWidth = -1f;
        private string pendingMessage;

        private RectTransform canvasRect;
        private RectTransform panelRect;
        private GameObject panel;
        private GameObject toast;
        private Image chip;
        private Image chipIcon;
        private Text chipLabel;
        private GameObject badge;
        private Text badgeText;
        private Text toastPreview;
        private readonly Image[] pulses = new Image[2];
        private Image portraitBorder;
        private Image portraitRings;
        private Text nameText;
        private Image statusDot;
        private Text statusText;
        private VoiceWaveform waveform;
        private readonly Image[] signalBars = new Image[5];
        private Text signalStatus;
        private RectTransform logContent;
        private GameObject thinkingRow;
        private Text thinkingText;
        private GameObject errorRow;
        private InputField inputField;
        private Text placeholderText;
        private CanvasGroup inputGroup;
        private Image sendFill;
        private Text sendLabel;
        private Button sendButton;
        private Text hintText;
        private Text counterText;

        public bool IsOpen => isOpen;

        public void Configure(MonoBehaviour source)
        {
            channelSource = source;
        }

        private void Awake()
        {
            EnsureEventSystem();
            BuildCanvas();
            panel.SetActive(false);
            toast.SetActive(false);
            ApplyState(LinkState.Idle);
            RefreshChip();
        }

        private void Start()
        {
            ResolveChannel();
        }

        private void OnDestroy()
        {
            if (channel != null)
            {
                channel.MessageReceived -= HandleMessage;
                channel.RequestFailed -= HandleFailure;
            }
            if (isOpen) InputFocus.IsTyping = false;
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            if (channel == null && now >= nextResolveTime)
            {
                nextResolveTime = now + 2f;
                ResolveChannel();
            }

            DrainChannel();
            UpdateKeys();
            UpdateTimers(now);
        }

        private void LateUpdate()
        {
            float width = canvasRect.rect.width;
            if (Mathf.Approximately(width, lastCanvasWidth)) return;
            lastCanvasWidth = width;
            panelRect.sizeDelta = new Vector2(Mathf.Min(width - ScreenMargin * 2f, PanelMaxWidth), PanelHeight);
        }

        private void UpdateKeys()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (!isOpen)
            {
                if (keyboard.spaceKey.wasPressedThisFrame) SetOpen(true);
                return;
            }
            if (keyboard.escapeKey.wasPressedThisFrame) SetOpen(false);
        }

        private void UpdateTimers(float now)
        {
            if (state == LinkState.Thinking)
            {
                if (now - waitStartTime > responseTimeout) GoOffline();
                else if (now >= nextDotTime)
                {
                    dotPhase = (dotPhase + 1) % 3;
                    nextDotTime = now + 0.4f;
                    thinkingText.text = "Düşünüyor" + new string('.', dotPhase + 1);
                }
            }

            if (toast.activeSelf)
            {
                AnimatePulse(now);
                if (now >= toastHideTime)
                {
                    toast.SetActive(false);
                    RefreshChip();
                }
            }
            else if (!isOpen && unread > 0)
            {
                float blink = 0.5f + 0.5f * Mathf.Cos(now * 4.5f);
                chipIcon.color = WithAlpha(Accent, Mathf.Lerp(0.35f, 1f, blink));
            }
        }

        private void SetOpen(bool open)
        {
            if (isOpen == open) return;
            isOpen = open;
            InputFocus.IsTyping = open;
            panel.SetActive(open);
            if (open)
            {
                unread = 0;
                toast.SetActive(false);
                StartCoroutine(FocusInputNextFrame());
            }
            else
            {
                inputField.DeactivateInputField();
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            }
            RefreshChip();
        }

        private IEnumerator FocusInputNextFrame()
        {
            yield return null;
            if (!isOpen || !inputField.interactable) yield break;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
        }

        private void OnEndEdit(string _)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) Submit();
        }

        private void Submit()
        {
            if (state == LinkState.Thinking) return;
            string message = inputField.text.Trim();
            if (message.Length == 0) return;
            inputField.text = string.Empty;
            AddLine(false, message);
            pendingMessage = message;
            Dispatch(message);
        }

        private void Retry()
        {
            if (string.IsNullOrEmpty(pendingMessage))
            {
                ApplyState(LinkState.Idle);
                return;
            }
            Dispatch(pendingMessage);
        }

        private void Dispatch(string message)
        {
            ResolveChannel();
            if (channel == null)
            {
                GoOffline();
                return;
            }

            waitStartTime = Time.unscaledTime;
            ApplyState(LinkState.Thinking);
            try
            {
                channel.Send(message);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(name + ": " + exception.Message, this);
                GoOffline();
            }
        }

        private void GoOffline()
        {
            ApplyState(LinkState.Offline);
            if (isOpen) StartCoroutine(FocusInputNextFrame());
        }

        private void HandleMessage(string message) => incoming.Enqueue(message ?? string.Empty);

        private void HandleFailure(string reason) => failures.Enqueue(reason ?? string.Empty);

        private void DrainChannel()
        {
            while (failures.TryDequeue(out string reason))
            {
                if (!string.IsNullOrEmpty(reason)) Debug.LogWarning(name + ": " + reason, this);
                if (state == LinkState.Thinking) GoOffline();
            }
            while (incoming.TryDequeue(out string message)) ReceiveVoice(message);
        }

        private void ReceiveVoice(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                if (state == LinkState.Thinking) GoOffline();
                return;
            }

            pendingMessage = null;
            AddLine(true, message.Trim());
            ApplyState(LinkState.Idle);
            if (isOpen)
            {
                StartCoroutine(FocusInputNextFrame());
                return;
            }

            unread++;
            toastPreview.text = Preview(message);
            toastHideTime = Time.unscaledTime + ToastDuration;
            toast.SetActive(true);
            RefreshChip();
        }

        private void ResolveChannel()
        {
            if (channel != null) return;
            IOutsideVoiceChannel found = null;
            if (channelSource != null)
            {
                found = channelSource as IOutsideVoiceChannel;
                if (found == null && !warnedBadSource)
                {
                    Debug.LogWarning(name + ": " + channelSource.name + " does not implement IOutsideVoiceChannel.", this);
                    warnedBadSource = true;
                }
            }
            if (found == null)
            {
                foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                {
                    if (behaviour is IOutsideVoiceChannel candidate)
                    {
                        found = candidate;
                        break;
                    }
                }
            }
            if (found == null) return;

            channel = found;
            channel.MessageReceived += HandleMessage;
            channel.RequestFailed += HandleFailure;
        }

        private void AddLine(bool fromVoice, string text)
        {
            GameObject row = CreateRow(fromVoice ? "SES" : "SEN", fromVoice ? Accent : Muted, text, fromVoice ? Accent : Body, UiFonts.Mono, out _);
            row.transform.SetSiblingIndex(thinkingRow.transform.GetSiblingIndex());
            rows.Enqueue(row);
            if (rows.Count > MaxHistory) Destroy(rows.Dequeue());
        }

        private void ApplyState(LinkState next)
        {
            state = next;
            bool offline = next == LinkState.Offline;
            bool thinking = next == LinkState.Thinking;

            statusDot.color = offline ? Error : Green;
            statusText.text = offline ? "Kanal 13 · sinyal yok" : thinking ? "Kanal 13 · dinliyor" : "Kanal 13 · bağlı";
            statusText.color = offline ? ErrorText : Muted;
            nameText.color = offline ? Muted : Accent;
            portraitRings.color = offline ? Dim : Accent;
            portraitBorder.color = thinking ? Accent : Line;

            waveform.SetMode(offline ? VoiceWaveform.Mode.Flat : thinking ? VoiceWaveform.Mode.Active : VoiceWaveform.Mode.Calm, offline ? ErrorText : Accent);
            for (int i = 0; i < signalBars.Length; i++) signalBars[i].color = !offline && i < 4 ? Accent : Line;
            signalStatus.text = offline ? "Hat koptu · yeniden aranıyor" : thinking ? "Ses kaydediliyor..." : "Kaynak: bilinmiyor · şifreli hat";
            signalStatus.color = offline ? ErrorText : Muted;

            thinkingRow.SetActive(thinking);
            if (thinking)
            {
                dotPhase = 2;
                nextDotTime = 0f;
            }
            errorRow.SetActive(offline);

            inputField.interactable = !thinking;
            sendButton.interactable = !thinking;
            inputGroup.alpha = thinking ? 0.5f : 1f;
            sendFill.color = thinking ? Dim : Accent;
            sendLabel.color = thinking ? Body : Ink;
            placeholderText.text = thinking ? "Ses cevap veriyor..." : "Dış sese bir şey yaz...";
            hintText.text = thinking ? "Cevap bekleniyor · oyun devam ediyor" : "Enter: gönder · Esc: kapat";
        }

        private void RefreshChip()
        {
            chip.gameObject.SetActive(!isOpen && !toast.activeSelf);
            bool waiting = unread > 0;
            chip.color = waiting ? Accent : Line;
            chipIcon.color = waiting ? Accent : Muted;
            chipLabel.text = waiting ? "Dış ses seni bekliyor" : "Dış sesle konuş";
            chipLabel.color = waiting ? Accent : Muted;
            badge.SetActive(waiting);
            badgeText.text = unread > 9 ? "9+" : unread.ToString();
        }

        private void AnimatePulse(float now)
        {
            for (int i = 0; i < pulses.Length; i++)
            {
                float phase = Mathf.Repeat(now / PulsePeriod + i * 0.5f, 1f);
                pulses[i].rectTransform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.5f, phase);
                pulses[i].color = WithAlpha(Accent, Mathf.Lerp(0.9f, 0f, phase));
            }
        }

        private static string Preview(string message)
        {
            string single = message.Replace('\n', ' ').Replace('\r', ' ').Trim();
            return single.Length > PreviewLength ? single.Substring(0, PreviewLength - 3).TrimEnd() + "..." : single;
        }

        private void BuildCanvas()
        {
            canvasRect = CreateOverlayCanvas(transform, "OutsideVoiceCanvas", 55);
            BuildChip(canvasRect);
            BuildToast(canvasRect);
            BuildPanel(canvasRect);
        }

        private void BuildChip(Transform parent)
        {
            chip = CreateFrame(parent, "VoiceChip", Line, PanelFill, out RectTransform inner);
            Place(chip.rectTransform, Vector2.zero, new Vector2(30f, 92f), new Vector2(440f, 72f));

            chipIcon = CreateImage(inner, "Icon", Muted, UiSprites.Rings);
            Place(chipIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(42f, 42f));

            Image badgeImage = CreateImage(chipIcon.transform, "Badge", Accent, UiSprites.Disc);
            Place(badgeImage.rectTransform, Vector2.one, new Vector2(14f, 12f), new Vector2(30f, 30f));
            badge = badgeImage.gameObject;
            badgeText = CreateText(badge.transform, "Count", UiFonts.Display, 18, TextAnchor.MiddleCenter, Ink);
            Stretch(badgeText.rectTransform, 0f, 0f, 0f, 0f);

            CreateKeycap(inner, new Vector2(0f, 0.5f), new Vector2(76f, 0f));

            chipLabel = CreateText(inner, "Label", UiFonts.Mono, 20, TextAnchor.MiddleLeft, Muted);
            chipLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(chipLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(190f, 0f), new Vector2(240f, 40f));
        }

        private void BuildToast(Transform parent)
        {
            Image frame = CreateFrame(parent, "VoiceToast", Accent, PanelFill, out RectTransform inner);
            Place(frame.rectTransform, Vector2.zero, new Vector2(30f, 92f), new Vector2(960f, 126f));
            toast = frame.gameObject;

            RectTransform icon = CreateRect(inner, "Icon");
            Place(icon, new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(80f, 80f));
            for (int i = 0; i < pulses.Length; i++)
            {
                pulses[i] = CreateImage(icon, "Pulse" + i, Accent, UiSprites.Ring);
                Stretch(pulses[i].rectTransform, 0f, 0f, 0f, 0f);
            }
            Image rings = CreateImage(icon, "Grille", Accent, UiSprites.Rings);
            Place(rings.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(58f, 58f));

            Text title = CreateText(inner, "Title", UiFonts.Display, 24, TextAnchor.MiddleLeft, Accent);
            title.text = "DIŞ SES";
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(126f, -20f), new Vector2(110f, 34f));

            Text subtitle = CreateText(inner, "Subtitle", UiFonts.Mono, 18, TextAnchor.MiddleLeft, Muted);
            subtitle.text = "Kanal 13 · yeni mesaj";
            subtitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(240f, -22f), new Vector2(300f, 30f));

            toastPreview = CreateText(inner, "Preview", UiFonts.Mono, 22, TextAnchor.MiddleLeft, Bright);
            toastPreview.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(toastPreview.rectTransform, new Vector2(0f, 1f), new Vector2(126f, -62f), new Vector2(660f, 36f));

            CreateKeycap(inner, new Vector2(1f, 0.5f), new Vector2(-30f, 14f));
            Text reply = CreateText(inner, "Reply", UiFonts.Mono, 17, TextAnchor.MiddleCenter, Muted);
            reply.text = "cevapla";
            Place(reply.rectTransform, new Vector2(1f, 0.5f), new Vector2(-30f, -26f), new Vector2(104f, 24f));
        }

        private void BuildPanel(Transform parent)
        {
            Image frame = CreateFrame(parent, "VoicePanel", Line, PanelFill, out RectTransform inner);
            panel = frame.gameObject;
            panelRect = frame.rectTransform;
            Place(panelRect, new Vector2(0.5f, 0f), new Vector2(0f, BottomMargin), new Vector2(PanelMaxWidth, PanelHeight));
            frame.raycastTarget = true;

            RectTransform content = CreateRect(inner, "Content");
            Stretch(content, 36f, 30f, 36f, 30f);

            BuildSpeaker(content);
            BuildSignal(content);
            BuildDialogue(content);
        }

        private void BuildSpeaker(RectTransform content)
        {
            RectTransform column = CreateRect(content, "Speaker");
            column.anchorMin = Vector2.zero;
            column.anchorMax = new Vector2(0f, 1f);
            column.pivot = new Vector2(0f, 1f);
            column.offsetMin = Vector2.zero;
            column.offsetMax = new Vector2(252f, 0f);

            portraitBorder = CreateFrame(column, "Portrait", Line, Well, out RectTransform portrait);
            Place(portraitBorder.rectTransform, new Vector2(0f, 1f), Vector2.zero, new Vector2(252f, 252f));
            portraitRings = CreateImage(portrait, "Grille", Accent, UiSprites.Rings);
            Place(portraitRings.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(168f, 168f));

            nameText = CreateText(column, "Name", UiFonts.Display, 33, TextAnchor.MiddleLeft, Accent);
            nameText.text = "DIŞ SES";
            Place(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, -270f), new Vector2(252f, 46f));

            statusDot = CreateImage(column, "Dot", Green, UiSprites.Disc);
            Place(statusDot.rectTransform, new Vector2(0f, 1f), new Vector2(0f, -340f), new Vector2(12f, 12f));
            statusText = CreateText(column, "Status", UiFonts.Mono, 18, TextAnchor.MiddleLeft, Muted);
            statusText.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(statusText.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -331f), new Vector2(228f, 30f));
        }

        private void BuildSignal(RectTransform content)
        {
            RectTransform column = CreateRect(content, "Signal");
            column.anchorMin = new Vector2(1f, 0f);
            column.anchorMax = Vector2.one;
            column.pivot = Vector2.one;
            column.offsetMin = new Vector2(-330f, 0f);
            column.offsetMax = Vector2.zero;

            Image divider = CreateImage(column, "Divider", Line);
            RectTransform dividerRect = divider.rectTransform;
            dividerRect.anchorMin = Vector2.zero;
            dividerRect.anchorMax = new Vector2(0f, 1f);
            dividerRect.pivot = new Vector2(0f, 0.5f);
            dividerRect.offsetMin = Vector2.zero;
            dividerRect.offsetMax = new Vector2(2f, 0f);

            Text title = CreateText(column, "Title", UiFonts.Display, 21, TextAnchor.MiddleLeft, Muted);
            title.text = "SİNYAL";
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(36f, -18f), new Vector2(260f, 30f));

            Image waveFrame = CreateFrame(column, "Wave", Line, Well, out RectTransform waveArea);
            Place(waveFrame.rectTransform, new Vector2(0f, 1f), new Vector2(36f, -66f), new Vector2(294f, 144f));
            waveform = waveArea.gameObject.AddComponent<VoiceWaveform>();
            waveform.Build(waveArea, 34, 4f);

            Text frequencyLabel = CreateText(column, "FrequencyLabel", UiFonts.Mono, 18, TextAnchor.MiddleLeft, Muted);
            frequencyLabel.text = "FREKANS";
            Place(frequencyLabel.rectTransform, new Vector2(0f, 1f), new Vector2(36f, -234f), new Vector2(160f, 26f));

            Text frequency = CreateText(column, "Frequency", UiFonts.Display, 33, TextAnchor.MiddleLeft, Bright);
            frequency.text = "13.40 MHz";
            frequency.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(frequency.rectTransform, new Vector2(0f, 1f), new Vector2(36f, -260f), new Vector2(190f, 46f));

            for (int i = 0; i < signalBars.Length; i++)
            {
                Image bar = CreateImage(column, "Bar" + i, Accent);
                RectTransform rect = bar.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(262f + i * 12f, -302f);
                rect.sizeDelta = new Vector2(7f, 9f + i * 6f);
                signalBars[i] = bar;
            }

            signalStatus = CreateText(column, "SignalStatus", UiFonts.Mono, 18, TextAnchor.UpperLeft, Muted);
            Place(signalStatus.rectTransform, new Vector2(0f, 1f), new Vector2(36f, -326f), new Vector2(294f, 54f));
        }

        private void BuildDialogue(RectTransform content)
        {
            RectTransform column = CreateRect(content, "Dialogue");
            column.anchorMin = Vector2.zero;
            column.anchorMax = Vector2.one;
            column.offsetMin = new Vector2(288f, 0f);
            column.offsetMax = new Vector2(-366f, 0f);

            Text header = CreateText(column, "Header", UiFonts.Display, 21, TextAnchor.MiddleLeft, Muted);
            header.text = "İNTERKOM";
            Place(header.rectTransform, new Vector2(0f, 1f), new Vector2(0f, -12f), new Vector2(300f, 32f));

            Button close = CreateButton(column, "Close", "×", Line, PanelFill, Muted, UiFonts.Mono, 30, out _, out _);
            Place((RectTransform)close.transform, Vector2.one, Vector2.zero, new Vector2(56f, 56f));
            close.onClick.AddListener(() => SetOpen(false));

            Image rule = CreateImage(column, "Rule", Line);
            RectTransform ruleRect = rule.rectTransform;
            ruleRect.anchorMin = new Vector2(0f, 1f);
            ruleRect.anchorMax = Vector2.one;
            ruleRect.pivot = new Vector2(0.5f, 1f);
            ruleRect.offsetMin = new Vector2(0f, -70f);
            ruleRect.offsetMax = new Vector2(0f, -68f);

            hintText = CreateText(column, "Hint", UiFonts.Mono, 18, TextAnchor.LowerLeft, Muted);
            PlaceBottom(hintText.rectTransform, 0f, 26f);
            counterText = CreateText(column, "Counter", UiFonts.Mono, 18, TextAnchor.LowerRight, Muted);
            counterText.text = "0 / " + MaxLength;
            PlaceBottom(counterText.rectTransform, 0f, 26f);

            BuildInputRow(column);
            BuildLog(column);
        }

        private void BuildInputRow(RectTransform column)
        {
            RectTransform row = CreateRect(column, "InputRow");
            PlaceBottom(row, 44f, 72f);
            inputGroup = row.gameObject.AddComponent<CanvasGroup>();

            Text label = CreateText(row, "Label", UiFonts.MonoBold, 22, TextAnchor.MiddleLeft, Muted);
            label.text = "SEN >";
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(label.rectTransform, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(80f, 40f));

            Image inputFrame = CreateFrame(row, "Input", InputLine, Well, out RectTransform inputArea);
            inputFrame.raycastTarget = true;
            RectTransform inputRect = inputFrame.rectTransform;
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = Vector2.one;
            inputRect.offsetMin = new Vector2(94f, 0f);
            inputRect.offsetMax = new Vector2(-210f, 0f);
            inputArea.gameObject.AddComponent<RectMask2D>();

            Text inputText = CreateText(inputArea, "Text", UiFonts.Mono, 22, TextAnchor.MiddleLeft, Bright);
            inputText.supportRichText = false;
            Stretch(inputText.rectTransform, 20f, 0f, 20f, 0f);
            placeholderText = CreateText(inputArea, "Placeholder", UiFonts.MonoItalic, 22, TextAnchor.MiddleLeft, Placeholder);
            Stretch(placeholderText.rectTransform, 20f, 0f, 20f, 0f);

            inputField = inputFrame.gameObject.AddComponent<InputField>();
            inputField.targetGraphic = inputFrame;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            inputField.characterLimit = MaxLength;
            inputField.lineType = InputField.LineType.SingleLine;
            inputField.customCaretColor = true;
            inputField.caretColor = Accent;
            inputField.caretWidth = 2;
            inputField.selectionColor = WithAlpha(Accent, 0.35f);
            inputField.navigation = new Navigation { mode = Navigation.Mode.None };
            inputField.transition = Selectable.Transition.None;
            inputField.onEndEdit.AddListener(OnEndEdit);
            inputField.onValueChanged.AddListener(value => counterText.text = value.Length + " / " + MaxLength);

            sendButton = CreateButton(row, "Send", "GÖNDER", Accent, Accent, Ink, UiFonts.Display, 23, out sendFill, out sendLabel);
            Place((RectTransform)sendButton.transform, new Vector2(1f, 0.5f), Vector2.zero, new Vector2(196f, 72f));
            sendButton.onClick.AddListener(Submit);
        }

        private void BuildLog(RectTransform column)
        {
            RectTransform area = CreateRect(column, "Log");
            area.anchorMin = Vector2.zero;
            area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(0f, 136f);
            area.offsetMax = new Vector2(0f, -88f);
            area.gameObject.AddComponent<RectMask2D>();

            logContent = CreateRect(area, "Lines");
            logContent.anchorMin = Vector2.zero;
            logContent.anchorMax = new Vector2(1f, 0f);
            logContent.pivot = new Vector2(0.5f, 0f);
            logContent.offsetMin = Vector2.zero;
            logContent.offsetMax = Vector2.zero;
            VerticalLayoutGroup layout = logContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 15f;
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = logContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            thinkingRow = CreateRow("SES", Accent, "Düşünüyor...", Accent, UiFonts.MonoItalic, out thinkingText);
            BuildErrorRow();
        }

        private void BuildErrorRow()
        {
            Image frame = CreateFrame(logContent, "Offline", Error, ErrorFill, out RectTransform inner);
            LayoutElement element = frame.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 104f;
            element.preferredHeight = 104f;
            errorRow = frame.gameObject;

            Text title = CreateText(inner, "Title", UiFonts.MonoBold, 21, TextAnchor.MiddleLeft, ErrorText);
            title.text = "Sinyal kesildi";
            PlaceTop(title.rectTransform, 22f, 244f, 16f, 34f);

            Text detail = CreateText(inner, "Detail", UiFonts.Mono, 18, TextAnchor.UpperLeft, Body);
            detail.text = "Ses şu an cevap veremiyor. Oyuna devam edebilirsin, mesajın kaybolmadı.";
            PlaceTop(detail.rectTransform, 22f, 244f, 50f, 52f);

            Button retry = CreateButton(inner, "Retry", "TEKRAR DENE", ErrorText, ErrorFill, ErrorText, UiFonts.Display, 20, out _, out _);
            Place((RectTransform)retry.transform, new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(206f, 60f));
            retry.onClick.AddListener(Retry);
        }

        private GameObject CreateRow(string speaker, Color speakerColor, string message, Color messageColor, Font messageFont, out Text messageText)
        {
            RectTransform row = CreateRect(logContent, speaker + "Line");
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 21f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Text label = CreateText(row, "Speaker", UiFonts.MonoBold, 22, TextAnchor.UpperLeft, speakerColor);
            label.text = speaker;
            LayoutElement labelElement = label.gameObject.AddComponent<LayoutElement>();
            labelElement.minWidth = 66f;
            labelElement.preferredWidth = 66f;
            labelElement.flexibleWidth = 0f;

            messageText = CreateText(row, "Message", messageFont, 22, TextAnchor.UpperLeft, messageColor);
            messageText.supportRichText = false;
            messageText.lineSpacing = 1.15f;
            messageText.text = message;
            LayoutElement messageElement = messageText.gameObject.AddComponent<LayoutElement>();
            messageElement.minWidth = 0f;
            messageElement.flexibleWidth = 1f;
            return row.gameObject;
        }
    }
}
