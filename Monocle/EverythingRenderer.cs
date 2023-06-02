using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
    public class EverythingRenderer : Renderer
    {
        public BlendState BlendState;
        public SamplerState SamplerState;
        public Effect Effect;
        public Camera Camera;

        public EverythingRenderer()
        {
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
            scene.Entities.Render();
            if (Engine.Commands.Open)
                scene.Entities.DebugRender(this.Camera);
            Draw.SpriteBatch.End();
        }

        public override void AfterRender(Scene scene)
        {
        }
    }
}
