﻿using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
    public class TagExcludeRenderer : Renderer
    {
        public BlendState BlendState;
        public SamplerState SamplerState;
        public Effect Effect;
        public Camera Camera;
        public int ExcludeTag;

        public TagExcludeRenderer(int excludeTag)
        {
            this.ExcludeTag = excludeTag;
            this.BlendState = BlendState.AlphaBlend;
            this.SamplerState = SamplerState.LinearClamp;
            this.Camera = new Camera();
        }

        public override void BeforeRender(Scene scene)
        {
        }

        public override void Render(Scene scene)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, this.BlendState, this.SamplerState, DepthStencilState.None, RasterizerState.CullNone, this.Effect, this.Camera.Matrix * Engine.ScreenMatrix);
            foreach (Entity entity in scene.Entities)
            {
                if (entity.Visible && (entity.Tag & this.ExcludeTag) == 0)
                    entity.Render();
            }
            if (Engine.Commands.Open)
            {
                foreach (Entity entity in scene.Entities)
                {
                    if ((entity.Tag & this.ExcludeTag) == 0)
                        entity.DebugRender(this.Camera);
                }
            }
            Draw.SpriteBatch.End();
        }

        public override void AfterRender(Scene scene)
        {
        }
    }
}
