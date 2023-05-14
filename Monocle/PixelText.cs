// Decompiled with JetBrains decompiler
// Type: Monocle.PixelText
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Monocle
{
    public class PixelText : Component
    {
        private readonly List<PixelText.Char> characters = new();
        private PixelFont font;
        private PixelFontSize size;
        private string text;
        private bool dirty;
        public Vector2 Position;
        public Color Color = Color.White;
        public Vector2 Scale = Vector2.One;

        public PixelFont Font
        {
            get => font;
            set
            {
                if (value != font)
                {
                    dirty = true;
                }

                font = value;
            }
        }

        public float Size
        {
            get => size.Size;
            set
            {
                if ((double)value != size.Size)
                {
                    dirty = true;
                }

                size = font.Get(value);
            }
        }

        public string Text
        {
            get => text;
            set
            {
                if (value != text)
                {
                    dirty = true;
                }

                text = value;
            }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public PixelText(PixelFont font, string text, Color color)
            : base(false, true)
        {
            Font = font;
            Text = text;
            Color = color;
            Text = text;
            size = Font.Sizes[0];
            Refresh();
        }

        public void Refresh()
        {
            dirty = false;
            characters.Clear();
            int num1 = 0;
            int num2 = 1;
            Vector2 zero = Vector2.Zero;
            for (int index = 0; index < text.Length; ++index)
            {
                if (text[index] == '\n')
                {
                    zero.X = 0.0f;
                    zero.Y += size.LineHeight;
                    ++num2;
                }
                PixelFontCharacter pixelFontCharacter = size.Get(text[index]);
                if (pixelFontCharacter != null)
                {
                    characters.Add(new PixelText.Char()
                    {
                        Offset = zero + new Vector2(pixelFontCharacter.XOffset, pixelFontCharacter.YOffset),
                        CharData = pixelFontCharacter,
                        Bounds = pixelFontCharacter.Texture.ClipRect
                    });
                    if (zero.X > (double)num1)
                    {
                        num1 = (int)zero.X;
                    }

                    zero.X += pixelFontCharacter.XAdvance;
                }
            }
            Width = num1;
            Height = num2 * size.LineHeight;
        }

        public override void Render()
        {
            if (dirty)
            {
                Refresh();
            }

            for (int index = 0; index < characters.Count; ++index)
            {
                characters[index].CharData.Texture.Draw(Position + characters[index].Offset, Vector2.Zero, Color);
            }
        }

        private struct Char
        {
            public Vector2 Offset;
            public PixelFontCharacter CharData;
            public Rectangle Bounds;
        }
    }
}
