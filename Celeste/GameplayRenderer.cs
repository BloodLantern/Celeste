using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public class GameplayRenderer : Monocle.Renderer
    {
        public Camera Camera;
        private static GameplayRenderer instance;

        public GameplayRenderer()
        {
            GameplayRenderer.instance = this;
            this.Camera = new Camera(320, 180);
        }

        public static void Begin() => Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, (Effect) null, GameplayRenderer.instance.Camera.Matrix);

        public override void Render(Scene scene)
        {
            GameplayRenderer.Begin();
            scene.Entities.RenderExcept((int) Tags.HUD);
            if (Engine.Commands.Open)
                scene.Entities.DebugRender(this.Camera);
            GameplayRenderer.End();
        }

        public static void End() => Draw.SpriteBatch.End();
    }
}
