using Microsoft.Xna.Framework.Graphics;
using System;

namespace Monocle
{
    public class NumberText : GraphicsComponent
    {
        private SpriteFont font;
        private int value;
        private string prefix;
        private string drawString;
        private bool centered;
        public Action<int> OnValueUpdate;

        public NumberText(SpriteFont font, string prefix, int value, bool centered = false)
            : base(false)
        {
            this.font = font;
            this.prefix = prefix;
            this.value = value;
            this.centered = centered;
            UpdateString();
        }

        public int Value
        {
            get => value;
            set
            {
                if (this.value == value)
                    return;
                int num = this.value;
                this.value = value;
                UpdateString();
                if (OnValueUpdate == null)
                    return;
                OnValueUpdate(num);
            }
        }

        public void UpdateString()
        {
            drawString = prefix + value;
            if (!centered)
                return;
            Origin = (font.MeasureString(drawString) / 2f).Floor();
        }

        public override void Render() => Draw.SpriteBatch.DrawString(font, drawString, RenderPosition, Color, Rotation, Origin, Scale, Effects, 0.0f);

        public float Width => font.MeasureString(drawString).X;

        public float Height => font.MeasureString(drawString).Y;
    }
}
