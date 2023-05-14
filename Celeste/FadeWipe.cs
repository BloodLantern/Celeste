// Decompiled with JetBrains decompiler
// Type: Celeste.FadeWipe
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class FadeWipe : ScreenWipe
    {
        private readonly VertexPositionColor[] vertexBuffer = new VertexPositionColor[6];
        public Action<float> OnUpdate;

        public FadeWipe(Scene scene, bool wipeIn, Action onComplete = null)
            : base(scene, wipeIn, onComplete)
        {
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (OnUpdate == null)
            {
                return;
            }

            OnUpdate(Percent);
        }

        public override void Render(Scene scene)
        {
            Color color = ScreenWipe.WipeColor * (WipeIn ? 1f - Ease.CubeIn(Percent) : Ease.CubeOut(Percent));
            vertexBuffer[0].Color = color;
            vertexBuffer[0].Position = new Vector3(-10f, -10f, 0.0f);
            vertexBuffer[1].Color = color;
            vertexBuffer[1].Position = new Vector3(Right, -10f, 0.0f);
            vertexBuffer[2].Color = color;
            vertexBuffer[2].Position = new Vector3(-10f, Bottom, 0.0f);
            vertexBuffer[3].Color = color;
            vertexBuffer[3].Position = new Vector3(Right, -10f, 0.0f);
            vertexBuffer[4].Color = color;
            vertexBuffer[4].Position = new Vector3(Right, Bottom, 0.0f);
            vertexBuffer[5].Color = color;
            vertexBuffer[5].Position = new Vector3(-10f, Bottom, 0.0f);
            ScreenWipe.DrawPrimitives(vertexBuffer);
        }
    }
}
