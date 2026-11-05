using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Karantina13.UI.UiFactory;
using static Karantina13.UI.UiPalette;

namespace Karantina13.UI.Menu
{
    public sealed class MainMenuScreen : MonoBehaviour
    {
        private enum MenuView { Menu, Controls, Settings, Opening, Loading }

        private sealed class MenuEntry
        {
            public bool Enabled;
            public Action Run;
            public Image Frame;
            public Image Fill;
            public Text Text;
            public Text Marker;
        }

        private const string VolumeKey = "karantina13.volume";
        private const float ButtonHeight = 84f;
        private const float ButtonGap = 12f;

        private static readonly (string key, string action)[] ControlRows =
        {
            ("W A S D", "Hareket"),
            ("Q", "Robot komut menüsü"),
            ("1 – 4", "Komut seç"),
            ("SPACE", "Dış sesle konuş"),
            ("ENTER", "Mesajı gönder"),
            ("ESC", "Paneli kapat · geri")
        };

        [SerializeField] private string firstLevelScene = "Level01";

        private readonly MenuEntry[] entries = new MenuEntry[5];
        private readonly Text[] settingValues = new Text[2];
        private readonly Image[] settingFrames = new Image[2];
        private RectTransform canvas;
        private GameObject controlsDialog;
        private GameObject settingsDialog;
        private Image alertDot;
        private OpeningSequence opening;
        private MenuView view = MenuView.Menu;
        private int selected;
        private int settingSelected;
        private int volumeStep;

        private void Awake()
        {
            volumeStep = Mathf.Clamp(PlayerPrefs.GetInt(VolumeKey, 10), 0, 10);
            AudioListener.volume = volumeStep / 10f;

            EnsureEventSystem();
            canvas = CreateOverlayCanvas(transform, "MainMenuCanvas", 40);
            BuildBackground();
            BuildHeader();
            BuildTitle();
            BuildMenu();
            BuildTransmission();
            BuildFooter();
            controlsDialog = BuildControlsDialog();
            settingsDialog = BuildSettingsDialog();
            controlsDialog.SetActive(false);
            settingsDialog.SetActive(false);

            opening = gameObject.AddComponent<OpeningSequence>();
            opening.Finished += LoadFirstLevel;
            Select(0);
        }

        private void OnDestroy()
        {
            if (opening != null) opening.Finished -= LoadFirstLevel;
        }

        private void Update()
        {
            float blink = 0.5f + 0.5f * Mathf.Cos(Time.unscaledTime * 4.5f);
            alertDot.color = WithAlpha(Error, Mathf.Lerp(0.25f, 1f, blink));

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            switch (view)
            {
                case MenuView.Menu:
                    UpdateMenu(keyboard);
                    break;
                case MenuView.Controls:
                    if (Back(keyboard) || Confirm(keyboard)) CloseDialogs();
                    break;
                case MenuView.Settings:
                    UpdateSettings(keyboard);
                    break;
            }
        }

        private void UpdateMenu(Keyboard keyboard)
        {
            if (Up(keyboard)) Step(-1);
            else if (Down(keyboard)) Step(1);
            else if (Confirm(keyboard)) Activate(selected);
        }

        private void UpdateSettings(Keyboard keyboard)
        {
            if (Back(keyboard))
            {
                CloseDialogs();
                return;
            }
            if (Up(keyboard) || Down(keyboard)) SelectSetting(1 - settingSelected);
            else if (Left(keyboard)) ChangeSetting(-1);
            else if (Right(keyboard) || Confirm(keyboard)) ChangeSetting(1);
        }

        private void Step(int direction)
        {
            int next = selected;
            for (int i = 0; i < entries.Length; i++)
            {
                next = (next + direction + entries.Length) % entries.Length;
                if (entries[next].Enabled) break;
            }
            Select(next);
        }

        private void Select(int index)
        {
            if (!entries[index].Enabled) return;
            selected = index;
            for (int i = 0; i < entries.Length; i++)
            {
                MenuEntry entry = entries[i];
                bool active = i == selected;
                entry.Frame.color = active ? Accent : Line;
                entry.Fill.color = active ? Accent : Backdrop;
                entry.Text.color = active ? Ink : entry.Enabled ? Body : Placeholder;
                entry.Marker.enabled = active;
            }
        }

        private void Activate(int index)
        {
            if (view != MenuView.Menu || !entries[index].Enabled) return;
            Select(index);
            entries[index].Run();
        }

        private void StartNewGame()
        {
            view = MenuView.Opening;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            opening.Begin(canvas);
        }

        private void LoadFirstLevel()
        {
            view = MenuView.Loading;
            if (Application.CanStreamedLevelBeLoaded(firstLevelScene)) SceneManager.LoadScene(firstLevelScene);
            else Debug.LogWarning(name + ": " + firstLevelScene + " Build Settings içinde yok.", this);
        }

        private void OpenControls()
        {
            view = MenuView.Controls;
            controlsDialog.SetActive(true);
        }

        private void OpenSettings()
        {
            view = MenuView.Settings;
            settingsDialog.SetActive(true);
            SelectSetting(0);
            RefreshSettings();
        }

        private void CloseDialogs()
        {
            controlsDialog.SetActive(false);
            settingsDialog.SetActive(false);
            view = MenuView.Menu;
        }

        private static void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void SelectSetting(int index)
        {
            settingSelected = index;
            for (int i = 0; i < settingFrames.Length; i++) settingFrames[i].color = i == index ? Accent : Line;
        }

        private void ChangeSetting(int direction)
        {
            if (settingSelected == 0) Screen.fullScreen = !Screen.fullScreen;
            else
            {
                volumeStep = Mathf.Clamp(volumeStep + direction, 0, 10);
                AudioListener.volume = volumeStep / 10f;
                PlayerPrefs.SetInt(VolumeKey, volumeStep);
                PlayerPrefs.Save();
            }
            RefreshSettings();
        }

        private void RefreshSettings()
        {
            settingValues[0].text = Screen.fullScreen ? "‹  AÇIK  ›" : "‹  KAPALI  ›";
            settingValues[1].text = "‹  " + new string('█', volumeStep) + new string('·', 10 - volumeStep) + "  ›";
        }

        private static bool Up(Keyboard k) => k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame;
        private static bool Down(Keyboard k) => k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame;
        private static bool Left(Keyboard k) => k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame;
        private static bool Right(Keyboard k) => k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame;
        private static bool Confirm(Keyboard k) => k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame;
        private static bool Back(Keyboard k) => k.escapeKey.wasPressedThisFrame || k.backspaceKey.wasPressedThisFrame;

        private void BuildBackground()
        {
            Image backdrop = CreateImage(canvas, "Backdrop", Backdrop);
            Stretch(backdrop.rectTransform, 0f, 0f, 0f, 0f);

            Image grid = CreateImage(canvas, "Grid", GridLine, UiSprites.GridCell);
            grid.type = Image.Type.Tiled;
            Stretch(grid.rectTransform, 0f, 0f, 0f, 0f);

            Image rings = CreateImage(canvas, "Rings", WithAlpha(Accent, 0.16f), UiSprites.LargeRings);
            Place(rings.rectTransform, Vector2.one, new Vector2(255f, 30f), new Vector2(1140f, 1140f));
        }

        private void BuildHeader()
        {
            RectTransform bar = CreateRect(canvas, "Header");
            PlaceTop(bar, 0f, 0f, 0f, 66f);
            Image rule = CreateImage(bar, "Rule", Line);
            PlaceBottom(rule.rectTransform, 0f, 2f);

            Text left = CreateText(bar, "Status", UiFonts.Mono, 18, TextAnchor.MiddleLeft, Muted);
            left.text = "TESİS DURUMU: KARANTİNA";
            left.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(left.rectTransform, new Vector2(0f, 0.5f), new Vector2(72f, 0f), new Vector2(600f, 30f));

            Text right = CreateText(bar, "Subject", UiFonts.Mono, 18, TextAnchor.MiddleRight, Muted);
            right.text = "DENEK #13 · GÖZETİM ALTINDA";
            right.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(right.rectTransform, new Vector2(1f, 0.5f), new Vector2(-72f, 0f), new Vector2(600f, 30f));

            alertDot = CreateImage(bar, "Alert", Error, UiSprites.Disc);
            Place(alertDot.rectTransform, new Vector2(1f, 0.5f), new Vector2(-72f - right.preferredWidth - 14f, 0f), new Vector2(12f, 12f));
        }

        private void BuildTitle()
        {
            Text kicker = CreateText(canvas, "Kicker", UiFonts.Mono, 20, TextAnchor.MiddleLeft, Muted);
            kicker.text = "DOSYA 13 / KAPALI TESİS";
            Place(kicker.rectTransform, new Vector2(0f, 1f), new Vector2(144f, -192f), new Vector2(900f, 30f));

            Text title = CreateText(canvas, "Title", UiFonts.Display, 156, TextAnchor.UpperLeft, Bright);
            title.text = "KARANTİNA<color=" + AccentHex + ">-13</color>";
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(140f, -214f), new Vector2(1300f, 190f));

            Text tagline = CreateText(canvas, "Tagline", UiFonts.Mono, 24, TextAnchor.MiddleLeft, Body);
            tagline.text = "Bir robot, bir ses ve kilitli kapılar.";
            Place(tagline.rectTransform, new Vector2(0f, 1f), new Vector2(144f, -404f), new Vector2(900f, 36f));
        }

        private void BuildMenu()
        {
            RectTransform list = CreateRect(canvas, "Menu");
            Place(list, new Vector2(0f, 1f), new Vector2(144f, -476f), new Vector2(570f, 5f * ButtonHeight + 4f * ButtonGap));

            AddEntry(list, 0, "YENİ OYUN", true, StartNewGame, null);
            AddEntry(list, 1, "DEVAM ET", false, null, "kayıt yok");
            AddEntry(list, 2, "KONTROLLER", true, OpenControls, null);
            AddEntry(list, 3, "AYARLAR", true, OpenSettings, null);
            AddEntry(list, 4, "ÇIKIŞ", true, Quit, null);
        }

        private void AddEntry(RectTransform list, int index, string label, bool enabled, Action run, string note)
        {
            Button button = CreateButton(list, label, label, Line, Backdrop, Body, UiFonts.Display, 30, out Image fill, out Text text);
            RectTransform rect = (RectTransform)button.transform;
            PlaceTop(rect, 0f, 0f, index * (ButtonHeight + ButtonGap), ButtonHeight);
            text.alignment = TextAnchor.MiddleLeft;
            text.rectTransform.offsetMin = new Vector2(70f, 0f);
            button.interactable = enabled;

            Text marker = CreateText(fill.transform, "Marker", UiFonts.Display, 34, TextAnchor.MiddleLeft, Ink);
            marker.text = "›";
            Stretch(marker.rectTransform, 28f, 0f, 0f, 4f);

            if (!string.IsNullOrEmpty(note))
            {
                Text noteText = CreateText(fill.transform, "Note", UiFonts.Mono, 18, TextAnchor.MiddleRight, Placeholder);
                noteText.text = note;
                Stretch(noteText.rectTransform, 0f, 0f, 30f, 0f);
            }

            MenuEntry entry = new MenuEntry { Enabled = enabled, Run = run, Frame = button.targetGraphic as Image, Fill = fill, Text = text, Marker = marker };
            entries[index] = entry;
            if (!enabled) return;

            button.onClick.AddListener(() => Activate(index));
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry hover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            hover.callback.AddListener(_ =>
            {
                if (view == MenuView.Menu) Select(index);
            });
            trigger.triggers.Add(hover);
        }

        private void BuildTransmission()
        {
            Image frame = CreateFrame(canvas, "Transmission", Line, PanelFill, out RectTransform inner);
            Place(frame.rectTransform, new Vector2(1f, 0f), new Vector2(-72f, 144f), new Vector2(600f, 132f));

            Image icon = CreateImage(inner, "Icon", Accent, UiSprites.Rings);
            Place(icon.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(51f, 51f));

            Text title = CreateText(inner, "Title", UiFonts.Display, 21, TextAnchor.MiddleLeft, Accent);
            title.text = "KANAL 13 · CIZIRTI";
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(96f, -20f), new Vector2(480f, 30f));

            Text message = CreateText(inner, "Message", UiFonts.Mono, 21, TextAnchor.UpperLeft, Accent);
            message.text = "...beni duyabiliyor musun?\nUyandığında konuşacağız.";
            message.lineSpacing = 1.2f;
            Place(message.rectTransform, new Vector2(0f, 1f), new Vector2(96f, -56f), new Vector2(480f, 66f));
        }

        private void BuildFooter()
        {
            RectTransform bar = CreateRect(canvas, "Footer");
            PlaceBottom(bar, 0f, 78f);
            Image rule = CreateImage(bar, "Rule", Line);
            PlaceTop(rule.rectTransform, 0f, 0f, 0f, 2f);

            CreateKeycap(bar, new Vector2(0f, 0.5f), new Vector2(72f, 0f), "W / S", 84f);
            AddFooterLabel(bar, "seç", 168f);
            CreateKeycap(bar, new Vector2(0f, 0.5f), new Vector2(240f, 0f), "ENTER", 104f);
            AddFooterLabel(bar, "onayla", 356f);

            Text version = CreateText(bar, "Version", UiFonts.Mono, 18, TextAnchor.MiddleRight, Muted);
            version.text = "v" + Application.version + " · YazLab I";
            Place(version.rectTransform, new Vector2(1f, 0.5f), new Vector2(-72f, 0f), new Vector2(500f, 30f));
        }

        private static void AddFooterLabel(RectTransform bar, string label, float x)
        {
            Text text = CreateText(bar, label, UiFonts.Mono, 18, TextAnchor.MiddleLeft, Muted);
            text.text = label;
            Place(text.rectTransform, new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(120f, 30f));
        }

        private RectTransform BuildDialog(string name, string heading, Vector2 size)
        {
            Image shade = CreateImage(canvas, name, WithAlpha(Abyss, 0.78f));
            shade.raycastTarget = true;
            Stretch(shade.rectTransform, 0f, 0f, 0f, 0f);

            Image frame = CreateFrame(shade.transform, "Dialog", Line, PanelFill, out RectTransform inner);
            Place(frame.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, size);

            Text title = CreateText(inner, "Heading", UiFonts.Display, 30, TextAnchor.MiddleLeft, Accent);
            title.text = heading;
            PlaceTop(title.rectTransform, 48f, 48f, 36f, 40f);

            Image rule = CreateImage(inner, "Rule", Line);
            PlaceTop(rule.rectTransform, 48f, 48f, 96f, 2f);

            Text hint = CreateText(inner, "Hint", UiFonts.Mono, 18, TextAnchor.MiddleLeft, Muted);
            PlaceBottom(hint.rectTransform, 30f, 30f);
            hint.rectTransform.offsetMin = new Vector2(48f, hint.rectTransform.offsetMin.y);
            return inner;
        }

        private GameObject BuildControlsDialog()
        {
            RectTransform inner = BuildDialog("Controls", "KONTROLLER", new Vector2(900f, 640f));
            inner.Find("Hint").GetComponent<Text>().text = "Esc / Enter: geri";

            for (int i = 0; i < ControlRows.Length; i++)
            {
                float top = 132f + i * 64f;
                Text key = CreateText(inner, "Key" + i, UiFonts.Display, 24, TextAnchor.MiddleLeft, Bright);
                key.text = ControlRows[i].key;
                PlaceTop(key.rectTransform, 48f, 560f, top, 44f);

                Text action = CreateText(inner, "Action" + i, UiFonts.Mono, 22, TextAnchor.MiddleLeft, Body);
                action.text = ControlRows[i].action;
                PlaceTop(action.rectTransform, 300f, 48f, top, 44f);
            }

            Button back = CreateButton(inner, "Back", "GERİ", Line, PanelFill, Body, UiFonts.Display, 22, out _, out _);
            Place((RectTransform)back.transform, new Vector2(1f, 0f), new Vector2(-48f, 24f), new Vector2(160f, 56f));
            back.onClick.AddListener(CloseDialogs);
            return inner.parent.parent.gameObject;
        }

        private GameObject BuildSettingsDialog()
        {
            RectTransform inner = BuildDialog("Settings", "AYARLAR", new Vector2(900f, 420f));
            inner.Find("Hint").GetComponent<Text>().text = "W/S: seç · A/D: değiştir · Esc: geri";

            string[] labels = { "Tam ekran", "Ses seviyesi" };
            for (int i = 0; i < labels.Length; i++)
            {
                int index = i;
                Image row = CreateFrame(inner, "Row" + i, Line, Backdrop, out RectTransform rowInner);
                row.raycastTarget = true;
                PlaceTop(row.rectTransform, 48f, 48f, 132f + i * 92f, 76f);
                settingFrames[i] = row;

                Text label = CreateText(rowInner, "Label", UiFonts.Mono, 22, TextAnchor.MiddleLeft, Body);
                label.text = labels[i];
                Stretch(label.rectTransform, 24f, 0f, 24f, 0f);

                Text value = CreateText(rowInner, "Value", UiFonts.MonoBold, 22, TextAnchor.MiddleRight, Accent);
                Stretch(value.rectTransform, 24f, 0f, 24f, 0f);
                settingValues[i] = value;

                Button button = row.gameObject.AddComponent<Button>();
                button.targetGraphic = row;
                button.transition = Selectable.Transition.None;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.AddListener(() =>
                {
                    SelectSetting(index);
                    ChangeSetting(1);
                });
            }

            Button back = CreateButton(inner, "Back", "GERİ", Line, PanelFill, Body, UiFonts.Display, 22, out _, out _);
            Place((RectTransform)back.transform, new Vector2(1f, 0f), new Vector2(-48f, 24f), new Vector2(160f, 56f));
            back.onClick.AddListener(CloseDialogs);
            return inner.parent.parent.gameObject;
        }
    }
}
