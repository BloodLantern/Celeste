// Decompiled with JetBrains decompiler
// Type: Monocle.SingleTagRenderer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
    public class SingleTagRenderer : Renderer
    {
        public BitTag Tag;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public Effect Effect;
        public Camera Camera;

        public SingleTagRenderer(BitTag tag)
        {
            Tag = tag;
            BlendState = BlendState.AlphaBlend;
            SamplerState = SamplerState.LinearClamp;
            Camera = new Camera();
        }

        public override void BeforeRender(Scene scene)
        {
        }

        public override void Render(Scene scene)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState, DepthStencilState.None, RasterizerState.CullNone, Effect, Camera.Matrix * Engine.ScreenMatrix);
            foreach (Entity entity in scene[Tag])
            {
                if (entity.Visible)
                {
                    entity.Render();
                }
            }
            if (Engine.Commands.Open)
            {
                foreach (Entity entity in scene[Tag])
                {
                    entity.DebugRender(Camera);
                }
            }
            Draw.SpriteBatch.End();
        }

        public override void AfterRender(Scene scene)
        {
        }
    }
}
