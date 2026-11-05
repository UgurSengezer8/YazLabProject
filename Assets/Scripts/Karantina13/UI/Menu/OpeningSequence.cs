using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Karantina13.UI.UiFactory;
using static Karantina13.UI.UiPalette;

namespace Karantina13.UI.Menu
{
    public sealed class OpeningSequence : MonoBehaviour
    {
        private readonly struct Beat
        {
            public readonly string Lead;
            public readonly string Line;

            public Beat(string lead, string line)
            {
                Lead = lead;
                Line = line;
            }
        }

        private const float CharactersPerSecond = 42f;
        private const float PulsePeriod = 2.2f;

        private static readonly Beat[] Beats =
        {
            new Beat("Uyandın demek.", "Kapılar kilitli, tel elektrikli, ve yanında sadece o küçük robot var."),
            new Beat("Robot seni dinler.", "Q ile ona komut verebilirsin. Benimle konuşmak istersen Space'e bas."),
            new Beat("Bir şey daha.", "Bakalım bu tesiste ne kadar dayanacaksın.")
        };

        private readonly Image[] pulses = new Image[2];
        private GameObject root;
        private Text leadText;
        private Text lineText;
        private int beatIndex;
        private float typed;
        private int startFrame;
        private bool running;

        public event Action Finished;

        public void Begin(RectTransform canvas)
        {
            if (root == null) Build(canvas);
            root.SetActive(true);
            root.transform.SetAsLastSibling();
            beatIndex = 0;
            typed = 0f;
            startFrame = Time.frameCount;
            running = true;
            ShowBeat();
        }

        private void Update()
        {
            if (!running) return;
            Beat beat = Beats[beatIndex];
            if (typed < beat.Line.Length)
            {
                typed = Mathf.Min(beat.Line.Length, typed + CharactersPerSecond * Time.unscaledDeltaTime);
                lineText.text = beat.Line.Substring(0, Mathf.FloorToInt(typed));
            }
            else
            {
                bool caret = Mathf.Repeat(Time.unscaledTime, 1f) < 0.5f;
                lineText.text = beat.Line + (caret ? " ▌" : "  ");
            }

            AnimatePulse(Time.unscaledTime);
            if (Time.frameCount != startFrame) ReadInput();
        }

        private void ReadInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Finish();
                return;
            }

            bool advance = keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) advance = true;
            if (!advance) return;

            if (typed < Beats[beatIndex].Line.Length)
            {
                typed = Beats[beatIndex].Line.Length;
                return;
            }

            beatIndex++;
            if (beatIndex >= Beats.Length) Finish();
            else ShowBeat();
        }

        private void ShowBeat()
        {
            typed = 0f;
            leadText.text = Beats[beatIndex].Lead;
            lineText.text = string.Empty;
        }

        private void Finish()
        {
            if (!running) return;
            running = false;
            Finished?.Invoke();
        }

        private void AnimatePulse(float now)
        {
            for (int i = 0; i < pulses.Length; i++)
            {
                float phase = Mathf.Repeat(now / PulsePeriod + i * 0.5f, 1f);
                pulses[i].rectTransform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.6f, phase);
                pulses[i].color = WithAlpha(Accent, Mathf.Lerp(0.7f, 0f, phase));
            }
        }

        private void Build(RectTransform canvas)
        {
            Image backdrop = CreateImage(canvas, "Opening", Abyss);
            backdrop.raycastTarget = true;
            Stretch(backdrop.rectTransform, 0f, 0f, 0f, 0f);
            root = backdrop.gameObject;
            RectTransform area = backdrop.rectTransform;

            RectTransform icon = CreateRect(area, "Icon");
            Place(icon, new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(180f, 180f));
            for (int i = 0; i < pulses.Length; i++)
            {
                pulses[i] = CreateImage(icon, "Pulse" + i, Accent, UiSprites.Ring);
                Stretch(pulses[i].rectTransform, 0f, 0f, 0f, 0f);
            }
            Image grille = CreateImage(icon, "Grille", Accent, UiSprites.Rings);
            Place(grille.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(126f, 126f));

            Text label = CreateText(area, "Channel", UiFonts.Mono, 20, TextAnchor.MiddleCenter, Muted);
            label.text = "KANAL 13 · BAĞLANTI KURULDU";
            Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 46f), new Vector2(1140f, 30f));

            leadText = CreateText(area, "Lead", UiFonts.Mono, 33, TextAnchor.MiddleCenter, Muted);
            Place(leadText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(1140f, 50f));

            lineText = CreateText(area, "Line", UiFonts.Mono, 33, TextAnchor.UpperCenter, Accent);
            lineText.lineSpacing = 1.3f;
            lineText.supportRichText = false;
            Place(lineText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(1140f, 140f));

            RectTransform keys = CreateRect(area, "Keys");
            Place(keys, new Vector2(0.5f, 0f), new Vector2(0f, 64f), new Vector2(460f, 42f));
            CreateKeycap(keys, new Vector2(0f, 0.5f), Vector2.zero);
            Text advance = CreateText(keys, "Advance", UiFonts.Mono, 20, TextAnchor.MiddleLeft, Muted);
            advance.text = "devam";
            Place(advance.rectTransform, new Vector2(0f, 0.5f), new Vector2(118f, 0f), new Vector2(110f, 30f));
            CreateKeycap(keys, new Vector2(0f, 0.5f), new Vector2(256f, 0f), "ESC", 74f);
            Text skip = CreateText(keys, "Skip", UiFonts.Mono, 20, TextAnchor.MiddleLeft, Muted);
            skip.text = "atla";
            Place(skip.rectTransform, new Vector2(0f, 0.5f), new Vector2(344f, 0f), new Vector2(110f, 30f));
        }
    }
}
