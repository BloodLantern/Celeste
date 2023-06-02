using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Monocle
{
    public class PixelText : Component
    {
        private List<PixelText.Char> characters = new List<PixelText.Char>();
        private PixelFont font;
        private PixelFontSize size;
        private string text;
        private bool dirty;
        public Vector2 Position;
        public Color Color = Color.White;
        public Vector2 Scale = Vector2.One;

        public PixelFont Font
        {
            get => this.font;
            set
            {
                if (value != this.font)
                    this.dirty = true;
                this.font = value;
            }
        }

        public float Size
        {
            get => this.size.Size;
            set
            {
                if ((double) value != (double) this.size.Size)
                    this.dirty = true;
                this.size = this.font.Get(value);
            }
        }

        public string Text
        {
            get => this.text;
            set
            {
                if (value != this.text)
                    this.dirty = true;
                this.text = value;
            }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public PixelText(PixelFont font, string text, Color color)
            : base(false, true)
        {
            this.Font = font;
            this.Text = text;
            this.Color = color;
            this.Text = text;
            this.size = this.Font.Sizes[0];
            this.Refresh();
        }

        public void Refresh()
        {
            this.dirty = false;
            this.characters.Clear();
            int num1 = 0;
            int num2 = 1;
            Vector2 zero = Vector2.Zero;
            for (int index = 0; index < this.text.Length; ++index)
            {
                if (this.text[index] == '\n')
                {
                    zero.X = 0.0f;
                    zero.Y += (float) this.size.LineHeight;
                    ++num2;
                }
                PixelFontCharacter pixelFontCharacter = this.size.Get((int) this.text[index]);
                if (pixelFontCharacter != null)
                {
                    this.characters.Add(new PixelText.Char()
                    {
                        Offset = zero + new Vector2((float) pixelFontCharacter.XOffset, (float) pixelFontCharacter.YOffset),
                        CharData = pixelFontCharacter,
                        Bounds = pixelFontCharacter.Texture.ClipRect
                    });
                    if ((double) zero.X > (double) num1)
                        num1 = (int) zero.X;
                    zero.X += (float) pixelFontCharacter.XAdvance;
                }
            }
            this.Width = num1;
            this.Height = num2 * this.size.LineHeight;
        }

        public override void Render()
        {
            if (this.dirty)
                this.Refresh();
            for (int index = 0; index < this.characters.Count; ++index)
                this.characters[index].CharData.Texture.Draw(this.Position + this.characters[index].Offset, Vector2.Zero, this.Color);
        }

        private struct Char
        {
            public Vector2 Offset;
            public PixelFontCharacter CharData;
            public Rectangle Bounds;
        }
    }
}
