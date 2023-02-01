// Decompiled with JetBrains decompiler
// Type: Celeste.GameplayRenderer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
