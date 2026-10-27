using UnityEngine;

namespace Karantina13.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 8f;
        [SerializeField] private bool loop = true;

        private SpriteRenderer spriteRenderer;
        private int currentFrame;
        private int playDirection;
        private float timer;

        public bool IsPlaying => HasFrames && (loop ? isActiveAndEnabled : playDirection != 0);

        private bool HasFrames => frames != null && frames.Length > 0;

        public void Configure(Sprite[] frames, float fps, bool loop)
        {
            this.frames = frames;
            framesPerSecond = fps;
            this.loop = loop;
            ShowFrame(0);
        }

        public void PlayForward()
        {
            if (loop) return;
            playDirection = 1;
            timer = 0f;
        }

        public void PlayBackward()
        {
            if (loop) return;
            playDirection = -1;
            timer = 0f;
        }

        public void ShowFrame(int index)
        {
            if (!HasFrames) return;
            playDirection = 0;
            SetFrame(Mathf.Clamp(index, 0, frames.Length - 1));
        }

        private void Start()
        {
            if (HasFrames) SetFrame(Mathf.Clamp(currentFrame, 0, frames.Length - 1));
        }

        private void Update()
        {
            if (!HasFrames || framesPerSecond <= 0f) return;
            if (!loop && playDirection == 0) return;

            float frameTime = 1f / framesPerSecond;
            timer += Time.deltaTime;
            while (timer >= frameTime)
            {
                timer -= frameTime;
                Advance();
                if (!loop && playDirection == 0) break;
            }
        }

        private void Advance()
        {
            if (loop)
            {
                SetFrame((currentFrame + 1) % frames.Length);
                return;
            }

            int next = currentFrame + playDirection;
            if (next < 0 || next >= frames.Length)
            {
                playDirection = 0;
                return;
            }

            SetFrame(next);
            if (next == 0 || next == frames.Length - 1) playDirection = 0;
        }

        private void SetFrame(int index)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            currentFrame = index;
            spriteRenderer.sprite = frames[index];
        }
    }
}
