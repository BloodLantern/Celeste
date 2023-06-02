using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public class TestBreathingGame : Scene
    {
        private BreathingMinigame game;

        public TestBreathingGame()
        {
            this.game = new BreathingMinigame();
            this.Add((Entity) this.game);
        }

        public override void BeforeRender()
        {
            this.game.BeforeRender();
            base.BeforeRender();
        }

        public override void Render()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, (DepthStencilState) null, (RasterizerState) null, (Effect) null, Engine.ScreenMatrix);
            this.game.Render();
            Draw.SpriteBatch.End();
        }
    }
}
