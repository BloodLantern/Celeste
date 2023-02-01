// Decompiled with JetBrains decompiler
// Type: Celeste.SafeGroundBlocker
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  [Tracked(false)]
  public class SafeGroundBlocker : Component
  {
    public bool Blocking = true;
    public Collider CheckWith;

    public SafeGroundBlocker(Collider checkWith = null)
      : base(false, false)
    {
      this.CheckWith = checkWith;
    }

    public bool Check(Player player)
    {
      if (!this.Blocking)
        return false;
      Collider collider = this.Entity.Collider;
      if (this.CheckWith != null)
        this.Entity.Collider = this.CheckWith;
      int num = player.CollideCheck(this.Entity) ? 1 : 0;
      this.Entity.Collider = collider;
      return num != 0;
    }

    public override void DebugRender(Camera camera)
    {
      Collider collider = this.Entity.Collider;
      if (this.CheckWith != null)
        this.Entity.Collider = this.CheckWith;
      this.Entity.Collider.Render(camera, Color.Aqua);
      this.Entity.Collider = collider;
    }
  }
}
