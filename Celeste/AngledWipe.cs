using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class AngledWipe : ScreenWipe
    {
        private const int rows = 6;
        private const float angleSize = 64f;
        private VertexPositionColor[] vertexBuffer = new VertexPositionColor[36];

        public AngledWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
            for (int index = 0; index < vertexBuffer.Length; ++index)
                vertexBuffer[index].Color = ScreenWipe.WipeColor;
        }

        public override void Render(Scene scene)
        {
            float num1 = 183.333328f;
            float num2 = -64f;
            float num3 = 1984f;
            for (int index1 = 0; index1 < 6; ++index1)
            {
                int index2 = index1 * 6;
                float x = num2;
                float y = (float) (index1 * (double) num1 - 10.0);
                float num4 = 0.0f;
                float num5 = index1 / 6f;
                float num6 = (float) ((WipeIn ? 1.0 - num5 : num5) * 0.30000001192092896);
                if (Percent > (double) num6)
                    num4 = Math.Min(1f, (float) ((Percent - (double) num6) / 0.699999988079071));
                if (WipeIn)
                    num4 = 1f - num4;
                float num7 = num3 * num4;
                vertexBuffer[index2].Position = new Vector3(x, y, 0.0f);
                vertexBuffer[index2 + 1].Position = new Vector3(x + num7, y, 0.0f);
                vertexBuffer[index2 + 2].Position = new Vector3(x, y + num1, 0.0f);
                vertexBuffer[index2 + 3].Position = new Vector3(x + num7, y, 0.0f);
                vertexBuffer[index2 + 4].Position = new Vector3((float) (x + (double) num7 + 64.0), y + num1, 0.0f);
                vertexBuffer[index2 + 5].Position = new Vector3(x, y + num1, 0.0f);
            }
            if (WipeIn)
            {
                for (int index = 0; index < vertexBuffer.Length; ++index)
                {
                    vertexBuffer[index].Position.X = 1920f - vertexBuffer[index].Position.X;
                    vertexBuffer[index].Position.Y = 1080f - vertexBuffer[index].Position.Y;
                }
            }
            ScreenWipe.DrawPrimitives(vertexBuffer);
        }
    }
}
