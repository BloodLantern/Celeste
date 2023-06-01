// Decompiled with JetBrains decompiler
// Type: Celeste.DropWipe
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class DropWipe : ScreenWipe
    {
        private const int columns = 10;
        private float[] meetings;
        private Color color;

        public DropWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
            this.color = ScreenWipe.WipeColor;
            this.meetings = new float[10];
            for (int index = 0; index < 10; ++index)
                this.meetings[index] = (float) (0.05000000074505806 + (double) Calc.Random.NextFloat() * 0.89999997615814209);
        }

        public override void Render(Scene scene)
        {
            float num1 = this.WipeIn ? 1f - this.Percent : this.Percent;
            float num2 = 192f;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState) null, (RasterizerState) null, (Effect) null, Engine.ScreenMatrix);
            if ((double) num1 >= 0.99500000476837158)
            {
                Draw.Rect(-10f, -10f, (float) (Engine.Width + 20), (float) (Engine.Height + 20), this.color);
            }
            else
            {
                for (int index = 0; index < 10; ++index)
                {
                    float num3 = (float) index / 10f;
                    float num4 = (float) ((this.WipeIn ? 1.0 - (double) num3 : (double) num3) * 0.30000001192092896);
                    if ((double) num1 > (double) num4)
                    {
                        float num5 = Ease.CubeIn(Math.Min(1f, (float) (((double) num1 - (double) num4) / 0.699999988079071)));
                        float num6 = 1080f * this.meetings[index] * num5;
                        float num7 = (float) (1080.0 * (1.0 - (double) this.meetings[index])) * num5;
                        Draw.Rect((float) ((double) index * (double) num2 - 1.0), -10f, num2 + 2f, num6 + 10f, this.color);
                        Draw.Rect((float) ((double) index * (double) num2 - 1.0), 1080f - num7, num2 + 2f, num7 + 10f, this.color);
                    }
                }
            }
            Draw.SpriteBatch.End();
        }
    }
}
