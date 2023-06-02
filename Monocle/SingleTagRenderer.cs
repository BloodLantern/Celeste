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
            this.Tag = tag;
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
            foreach (Entity entity in scene[this.Tag])
            {
                if (entity.Visible)
                    entity.Render();
            }
            if (Engine.Commands.Open)
            {
                foreach (Entity entity in scene[this.Tag])
                    entity.DebugRender(this.Camera);
            }
            Draw.SpriteBatch.End();
        }

        public override void AfterRender(Scene scene)
        {
        }
    }
}
