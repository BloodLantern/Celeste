// Decompiled with JetBrains decompiler
// Type: Celeste.LookoutBlocker
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
  [Tracked(false)]
  public class LookoutBlocker : Entity
  {
    public LookoutBlocker(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Collider = (Collider) new Hitbox((float) data.Width, (float) data.Height);
    }
  }
}
