// Decompiled with JetBrains decompiler
// Type: Monocle.Text
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
    public class Text : GraphicsComponent
    {
        private SpriteFont font;
        private string text;
        private Text.HorizontalAlign horizontalOrigin;
        private Text.VerticalAlign verticalOrigin;
        private Vector2 size;

        public Text(
            SpriteFont font,
            string text,
            Vector2 position,
            Color color,
            Text.HorizontalAlign horizontalAlign = Text.HorizontalAlign.Center,
            Text.VerticalAlign verticalAlign = Text.VerticalAlign.Center)
            : base(false)
        {
            this.font = font;
            this.text = text;
            Position = position;
            Color = color;
            horizontalOrigin = horizontalAlign;
            verticalOrigin = verticalAlign;
            UpdateSize();
        }

        public Text(
            SpriteFont font,
            string text,
            Vector2 position,
            Text.HorizontalAlign horizontalAlign = Text.HorizontalAlign.Center,
            Text.VerticalAlign verticalAlign = Text.VerticalAlign.Center)
            : this(font, text, position, Color.White, horizontalAlign, verticalAlign)
        {
        }

        public SpriteFont Font
        {
            get => font;
            set
            {
                font = value;
                UpdateSize();
            }
        }

        public string DrawText
        {
            get => text;
            set
            {
                text = value;
                UpdateSize();
            }
        }

        public Text.HorizontalAlign HorizontalOrigin
        {
            get => horizontalOrigin;
            set
            {
                horizontalOrigin = value;
                UpdateCentering();
            }
        }

        public Text.VerticalAlign VerticalOrigin
        {
            get => verticalOrigin;
            set
            {
                verticalOrigin = value;
                UpdateCentering();
            }
        }

        public float Width => size.X;

        public float Height => size.Y;

        private void UpdateSize()
        {
            size = font.MeasureString(text);
            UpdateCentering();
        }

        private void UpdateCentering()
        {
            Origin.X = horizontalOrigin == Text.HorizontalAlign.Left ? 0.0f : horizontalOrigin == Text.HorizontalAlign.Center ? size.X / 2f : size.X;

            Origin.Y = verticalOrigin == Text.VerticalAlign.Top ? 0.0f : verticalOrigin == Text.VerticalAlign.Center ? size.Y / 2f : size.Y;

            Origin = Origin.Floor();
        }

        public override void Render()
        {
            Draw.SpriteBatch.DrawString(font, text, RenderPosition, Color, Rotation, Origin, Scale, Effects, 0.0f);
        }

        public enum HorizontalAlign
        {
            Left,
            Center,
            Right,
        }

        public enum VerticalAlign
        {
            Top,
            Center,
            Bottom,
        }
    }
}
