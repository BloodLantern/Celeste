// Decompiled with JetBrains decompiler
// Type: Celeste.InvisibleBarrier
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  [Tracked(false)]
  public class InvisibleBarrier : Solid
  {
    public InvisibleBarrier(Vector2 position, float width, float height)
      : base(position, width, height, true)
    {
      this.Tag = (int) Tags.TransitionUpdate;
      this.Collidable = false;
      this.Visible = false;
      this.Add((Component) new ClimbBlocker(true));
      this.SurfaceSoundIndex = 33;
    }

    public InvisibleBarrier(EntityData data, Vector2 offset)
      : this(data.Position + offset, (float) data.Width, (float) data.Height)
    {
    }

    public override void Update()
    {
      this.Collidable = true;
      if (this.CollideCheck<Player>())
        this.Collidable = false;
      if (this.Collidable)
        return;
      this.Active = false;
    }
  }
}
