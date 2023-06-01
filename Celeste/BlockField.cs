// Decompiled with JetBrains decompiler
// Type: Celeste.BlockField
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class BlockField : Entity
    {
        public BlockField(Vector2 position, int width, int height)
            : base(position)
        {
            this.Collider = (Collider) new Hitbox((float) width, (float) height);
        }

        public BlockField(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }
    }
}
