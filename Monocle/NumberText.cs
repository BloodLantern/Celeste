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
            this.UpdateString();
        }

        public int Value
        {
            get => this.value;
            set
            {
                if (this.value == value)
                    return;
                int num = this.value;
                this.value = value;
                this.UpdateString();
                if (this.OnValueUpdate == null)
                    return;
                this.OnValueUpdate(num);
            }
        }

        public void UpdateString()
        {
            this.drawString = this.prefix + this.value.ToString();
            if (!this.centered)
                return;
            this.Origin = (this.font.MeasureString(this.drawString) / 2f).Floor();
        }

        public override void Render() => Draw.SpriteBatch.DrawString(this.font, this.drawString, this.RenderPosition, this.Color, this.Rotation, this.Origin, this.Scale, this.Effects, 0.0f);

        public float Width => this.font.MeasureString(this.drawString).X;

        public float Height => this.font.MeasureString(this.drawString).Y;
    }
}
