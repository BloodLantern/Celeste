using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class WindWipe : ScreenWipe
    {
        private int t;
        private int columns;
        private int rows;
        private VertexPositionColor[] vertexBuffer;

        public WindWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
            t = 40;
            columns = 1920 / t + 1;
            rows = 1080 / t + 1;
            vertexBuffer = new VertexPositionColor[columns * rows * 6];
            for (int index = 0; index < vertexBuffer.Length; ++index)
                vertexBuffer[index].Color = ScreenWipe.WipeColor;
        }

        public override void Render(Scene scene)
        {
            float num1 = columns * rows;
            int index1 = 0;
            for (int index2 = 0; index2 < columns; ++index2)
            {
                for (int index3 = 0; index3 < rows; ++index3)
                {
                    int num2 = WipeIn ? columns - index2 - 1 : index2;
                    float val1_1 = (float) (((index3 + num2 % 2) % 2 * (rows + index3 / 2) + (index3 + num2 % 2 + 1) % 2 * (index3 / 2) + num2 * rows) / (double) num1 * 0.5);
                    float val1_2 = val1_1 + 300f / num1;
                    float num3 = (float) ((Math.Max(val1_1, Math.Min(val1_2, WipeIn ? 1f - Percent : Percent)) - (double) val1_1) / (val1_2 - (double) val1_1));
                    float x1 = (index2 - 0.5f) * t;
                    float y1 = (float) ((index3 - 0.5) * t - t * 0.5 * num3);
                    float x2 = x1 + t;
                    float y2 = y1 + t * num3;
                    vertexBuffer[index1].Position = new Vector3(x1, y1, 0.0f);
                    vertexBuffer[index1 + 1].Position = new Vector3(x2, y1, 0.0f);
                    vertexBuffer[index1 + 2].Position = new Vector3(x1, y2, 0.0f);
                    vertexBuffer[index1 + 3].Position = new Vector3(x2, y1, 0.0f);
                    vertexBuffer[index1 + 4].Position = new Vector3(x2, y2, 0.0f);
                    vertexBuffer[index1 + 5].Position = new Vector3(x1, y2, 0.0f);
                    index1 += 6;
                }
            }
            ScreenWipe.DrawPrimitives(vertexBuffer);
        }
    }
}
