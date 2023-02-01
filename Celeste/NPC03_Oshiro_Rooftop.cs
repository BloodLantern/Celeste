// Decompiled with JetBrains decompiler
// Type: Celeste.NPC03_Oshiro_Rooftop
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  public class NPC03_Oshiro_Rooftop : NPC
  {
    public NPC03_Oshiro_Rooftop(Vector2 position)
      : base(position)
    {
      this.Add((Component) (this.Sprite = (Sprite) new OshiroSprite(1)));
      (this.Sprite as OshiroSprite).AllowTurnInvisible = false;
      this.MoveAnim = "move";
      this.IdleAnim = "idle";
      this.Add((Component) (this.Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64)));
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      if (this.Session.GetFlag("oshiro_resort_roof"))
      {
        this.RemoveSelf();
      }
      else
      {
        this.Visible = false;
        this.Scene.Add((Entity) new CS03_OshiroRooftop((NPC) this));
      }
    }
  }
}
