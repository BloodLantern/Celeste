// Decompiled with JetBrains decompiler
// Type: Celeste.AngledWipe
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
        private readonly VertexPositionColor[] vertexBuffer = new VertexPositionColor[36];

        public AngledWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
            for (int index = 0; index < vertexBuffer.Length; ++index)
            {
                vertexBuffer[index].Color = WipeColor;
            }
        }

        public override void Render(Scene scene)
        {
            float num1 = 183.333328f;
            float num2 = -angleSize;
            float num3 = 1984f; // 31 * angleSize ?
            for (int index1 = 0; index1 < rows; ++index1)
            {
                int index2 = index1 * rows;
                float x = num2;
                float y = (index1 * num1) - 10;
                float num4 = 0;
                float num5 = index1 / 6f;
                float num6 = (WipeIn ? 1f - num5 : num5) * 0.3f;
                if (Percent > num6)
                {
                    num4 = Math.Min(1f, (Percent - num6) / 0.7f);
                }

                if (WipeIn)
                {
                    num4 = 1f - num4;
                }

                float num7 = num3 * num4;
                vertexBuffer[index2].Position = new Vector3(x, y, 0);
                vertexBuffer[index2 + 1].Position = new Vector3(x + num7, y, 0);
                vertexBuffer[index2 + 2].Position = new Vector3(x, y + num1, 0);
                vertexBuffer[index2 + 3].Position = new Vector3(x + num7, y, 0);
                vertexBuffer[index2 + 4].Position = new Vector3(x + num7 + angleSize, y + num1, 0);
                vertexBuffer[index2 + 5].Position = new Vector3(x, y + num1, 0);
            }
            if (WipeIn)
            {
                for (int index = 0; index < vertexBuffer.Length; ++index)
                {
                    vertexBuffer[index].Position.X = 1920f - vertexBuffer[index].Position.X;
                    vertexBuffer[index].Position.Y = 1080f - vertexBuffer[index].Position.Y;
                }
            }
            DrawPrimitives(vertexBuffer);
        }
    }
}
