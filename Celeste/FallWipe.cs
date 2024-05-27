using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class FallWipe : ScreenWipe
    {
        private VertexPositionColor[] vertexBuffer = new VertexPositionColor[9];

        public FallWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
            for (int index = 0; index < vertexBuffer.Length; ++index)
                vertexBuffer[index].Color = ScreenWipe.WipeColor;
        }

        public override void Render(Scene scene)
        {
            float percent = Percent;
            Vector2 vector2_1 = new Vector2(960f, (float) (1080.0 - 2160.0 * percent));
            Vector2 vector2_2 = new Vector2(-10f, (float) (2160.0 * (1.0 - percent)));
            Vector2 vector2_3 = new Vector2(Right, (float) (2160.0 * (1.0 - percent)));
            if (!WipeIn)
            {
                vertexBuffer[0].Position = new Vector3(vector2_1, 0.0f);
                vertexBuffer[1].Position = new Vector3(vector2_2, 0.0f);
                vertexBuffer[2].Position = new Vector3(vector2_3, 0.0f);
                vertexBuffer[3].Position = new Vector3(vector2_2, 0.0f);
                vertexBuffer[4].Position = new Vector3(vector2_3, 0.0f);
                vertexBuffer[5].Position = new Vector3(vector2_2.X, (float) (vector2_2.Y + 1080.0 + 10.0), 0.0f);
                vertexBuffer[6].Position = new Vector3(vector2_3, 0.0f);
                vertexBuffer[8].Position = new Vector3(vector2_3.X, (float) (vector2_3.Y + 1080.0 + 10.0), 0.0f);
                vertexBuffer[7].Position = new Vector3(vector2_2.X, (float) (vector2_2.Y + 1080.0 + 10.0), 0.0f);
            }
            else
            {
                vertexBuffer[0].Position = new Vector3(vector2_2.X, (float) (vector2_1.Y - 1080.0 - 10.0), 0.0f);
                vertexBuffer[1].Position = new Vector3(vector2_3.X, (float) (vector2_1.Y - 1080.0 - 10.0), 0.0f);
                vertexBuffer[2].Position = new Vector3(vector2_1, 0.0f);
                vertexBuffer[3].Position = new Vector3(vector2_2.X, (float) (vector2_1.Y - 1080.0 - 10.0), 0.0f);
                vertexBuffer[4].Position = new Vector3(vector2_1, 0.0f);
                vertexBuffer[5].Position = new Vector3(vector2_2, 0.0f);
                vertexBuffer[6].Position = new Vector3(vector2_3.X, (float) (vector2_1.Y - 1080.0 - 10.0), 0.0f);
                vertexBuffer[7].Position = new Vector3(vector2_3, 0.0f);
                vertexBuffer[8].Position = new Vector3(vector2_1, 0.0f);
            }
            for (int index = 0; index < vertexBuffer.Length; ++index)
                vertexBuffer[index].Position.Y = 1080f - vertexBuffer[index].Position.Y;
            ScreenWipe.DrawPrimitives(vertexBuffer);
        }
    }
}
