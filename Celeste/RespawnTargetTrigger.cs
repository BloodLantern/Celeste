// Decompiled with JetBrains decompiler
// Type: Celeste.RespawnTargetTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class RespawnTargetTrigger : Entity
    {
        public Vector2 Target;

        public RespawnTargetTrigger(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.Collider = (Collider) new Hitbox((float) data.Width, (float) data.Height);
            this.Target = data.Nodes[0] + offset;
            this.Visible = this.Active = false;
        }
    }
}
