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
        private TimerText.TimerModes timerMode;
        private Vector2 justify;
        public Action OnComplete;
        public TimerText.CountModes CountMode;

        public string Text { get; private set; }

        public TimerText(
            SpriteFont font,
            TimerText.TimerModes mode,
            TimerText.CountModes countMode,
            int frames,
            Vector2 justify,
            Action onComplete = null)
            : base(true)
        {
            this.font = font;
            this.timerMode = mode;
            this.CountMode = countMode;
            this.frames = frames;
            this.justify = justify;
            this.OnComplete = onComplete;
            this.UpdateText();
            this.CalculateOrigin();
        }

        private void UpdateText()
        {
            if (this.timerMode != TimerText.TimerModes.SecondsMilliseconds)
                return;
            this.Text = ((float) (this.frames / 60) + (float) (this.frames % 60) * 0.0166666675f).ToString("0.00");
        }

        private void CalculateOrigin() => this.Origin = (this.font.MeasureString(this.Text) * this.justify).Floor();

        public override void Update()
        {
            base.Update();
            if (this.CountMode == TimerText.CountModes.Down)
            {
                if (this.frames <= 0)
                    return;
                --this.frames;
                if (this.frames == 0 && this.OnComplete != null)
                    this.OnComplete();
                this.UpdateText();
                this.CalculateOrigin();
            }
            else
            {
                ++this.frames;
                this.UpdateText();
                this.CalculateOrigin();
            }
        }

        public override void Render() => Draw.SpriteBatch.DrawString(this.font, this.Text, this.RenderPosition, this.Color, this.Rotation, this.Origin, this.Scale, this.Effects, 0.0f);

        public SpriteFont Font
        {
            get => this.font;
            set
            {
                this.font = value;
                this.CalculateOrigin();
            }
        }

        public int Frames
        {
            get => this.frames;
            set
            {
                if (this.frames == value)
                    return;
                this.frames = value;
                this.UpdateText();
                this.CalculateOrigin();
            }
        }

        public Vector2 Justify
        {
            get => this.justify;
            set
            {
                this.justify = value;
                this.CalculateOrigin();
            }
        }

        public float Width => this.font.MeasureString(this.Text).X;

        public float Height => this.font.MeasureString(this.Text).Y;

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
