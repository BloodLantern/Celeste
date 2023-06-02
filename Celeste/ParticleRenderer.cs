using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class ParticleRenderer : Monocle.Renderer
    {
        public List<ParticleSystem> Systems;

        public ParticleRenderer(params ParticleSystem[] system)
        {
            this.Systems = new List<ParticleSystem>();
            this.Systems.AddRange((IEnumerable<ParticleSystem>) system);
        }

        public override void Update(Scene scene)
        {
            foreach (Entity system in this.Systems)
                system.Update();
            base.Update(scene);
        }

        public override void Render(Scene scene)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, (Effect) null, Engine.ScreenMatrix);
            foreach (Entity system in this.Systems)
                system.Render();
            Draw.SpriteBatch.End();
        }
    }
}
