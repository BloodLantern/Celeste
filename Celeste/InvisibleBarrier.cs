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
            Tag = (int)Tags.TransitionUpdate;
            Collidable = false;
            Visible = false;
            Add(new ClimbBlocker(true));
            SurfaceSoundIndex = 33;
        }

        public InvisibleBarrier(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        public override void Update()
        {
            Collidable = true;
            if (CollideCheck<Player>())
            {
                Collidable = false;
            }

            if (Collidable)
            {
                return;
            }

            Active = false;
        }
    }
}
