using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Monocle
{
    public class TimerText : GraphicsComponent
    {
        private const float DELTA_TIME = 0.0166666675f;
        private SpriteFont font;
        private int frames;
        private TimerModes timerMode;
        private Vector2 justify;
        public Action OnComplete;
        public CountModes CountMode;

        public string Text { get; private set; }

        public TimerText(
            SpriteFont font,
            TimerModes mode,
            CountModes countMode,
            int frames,
            Vector2 justify,
            Action onComplete = null)
            : base(true)
        {
            this.font = font;
            timerMode = mode;
            CountMode = countMode;
            this.frames = frames;
            this.justify = justify;
            OnComplete = onComplete;
            UpdateText();
            CalculateOrigin();
        }

        private void UpdateText()
        {
            if (timerMode != TimerModes.SecondsMilliseconds)
                return;
            Text = (frames / 60 + frames % 60 * 0.0166666675f).ToString("0.00");
        }

        private void CalculateOrigin() => Origin = (font.MeasureString(Text) * justify).Floor();

        public override void Update()
        {
            base.Update();
            if (CountMode == CountModes.Down)
            {
                if (frames <= 0)
                    return;
                --frames;
                if (frames == 0 && OnComplete != null)
                    OnComplete();
                UpdateText();
                CalculateOrigin();
            }
            else
            {
                ++frames;
                UpdateText();
                CalculateOrigin();
            }
        }

        public override void Render() => Draw.SpriteBatch.DrawString(font, Text, RenderPosition, Color, Rotation, Origin, Scale, Effects, 0.0f);

        public SpriteFont Font
        {
            get => font;
            set
            {
                font = value;
                CalculateOrigin();
            }
        }

        public int Frames
        {
            get => frames;
            set
            {
                if (frames == value)
                    return;
                frames = value;
                UpdateText();
                CalculateOrigin();
            }
        }

        public Vector2 Justify
        {
            get => justify;
            set
            {
                justify = value;
                CalculateOrigin();
            }
        }

        public float Width => font.MeasureString(Text).X;

        public float Height => font.MeasureString(Text).Y;

        public enum CountModes
        {
            Down,
            Up,
        }

        public enum TimerModes
        {
            SecondsMilliseconds,
        }
    }
}
