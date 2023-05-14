// Decompiled with JetBrains decompiler
// Type: Monocle.NumberText
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework.Graphics;
using System;

namespace Monocle
{
    public class NumberText : GraphicsComponent
    {
        private readonly SpriteFont font;
        private int value;
        private readonly string prefix;
        private string drawString;
        private readonly bool centered;
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
                {
                    return;
                }

                int num = this.value;
                this.value = value;
                UpdateString();
                if (OnValueUpdate == null)
                {
                    return;
                }

                OnValueUpdate(num);
            }
        }

        public void UpdateString()
        {
            drawString = prefix + value.ToString();
            if (!centered)
            {
                return;
            }

            Origin = (font.MeasureString(drawString) / 2f).Floor();
        }

        public override void Render()
        {
            Draw.SpriteBatch.DrawString(font, drawString, RenderPosition, Color, Rotation, Origin, Scale, Effects, 0.0f);
        }

        public float Width => font.MeasureString(drawString).X;

        public float Height => font.MeasureString(drawString).Y;
    }
}
