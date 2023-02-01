// Decompiled with JetBrains decompiler
// Type: Celeste.NPC04_Theo
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  public class NPC04_Theo : NPC
  {
    private bool started;

    public NPC04_Theo(Vector2 position)
      : base(position)
    {
      this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("theo")));
      this.IdleAnim = "idle";
      this.MoveAnim = "walk";
      this.Visible = false;
      this.Maxspeed = 48f;
      this.SetupTheoSpriteSounds();
    }

    public override void Update()
    {
      base.Update();
      if (this.started)
        return;
      Gondola first = this.Scene.Entities.FindFirst<Gondola>();
      Player entity = this.Scene.Tracker.GetEntity<Player>();
      if (first == null || entity == null || (double) entity.X <= (double) first.Left - 16.0)
        return;
      this.started = true;
      this.Scene.Add((Entity) new CS04_Gondola((NPC) this, first, entity));
    }
  }
}
