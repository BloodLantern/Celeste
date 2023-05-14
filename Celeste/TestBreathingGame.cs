// Decompiled with JetBrains decompiler
// Type: Celeste.TestBreathingGame
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    public class TestBreathingGame : Scene
    {
        private readonly BreathingMinigame game;

        public TestBreathingGame()
        {
            game = new BreathingMinigame();
            Add(game);
        }

        public override void BeforeRender()
        {
            game.BeforeRender();
            base.BeforeRender();
        }

        public override void Render()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
            game.Render();
            Draw.SpriteBatch.End();
        }
    }
}
