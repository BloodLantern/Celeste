using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class ParticleRenderer : Renderer
    {
        public List<ParticleSystem> Systems;

        public ParticleRenderer(params ParticleSystem[] system)
        {
            Systems = new List<ParticleSystem>();
            Systems.AddRange(system);
        }

        public override void Update(Scene scene)
        {
            foreach (Entity system in Systems)
                system.Update();
            base.Update(scene);
        }

        public override void Render(Scene scene)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
            foreach (Entity system in Systems)
                system.Render();
            Draw.SpriteBatch.End();
        }
    }
}
