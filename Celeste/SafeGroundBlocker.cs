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
            CheckWith = checkWith;
        }

        public bool Check(Player player)
        {
            if (!Blocking)
            {
                return false;
            }

            Collider collider = Entity.Collider;
            if (CheckWith != null)
            {
                Entity.Collider = CheckWith;
            }

            int num = player.CollideCheck(Entity) ? 1 : 0;
            Entity.Collider = collider;
            return num != 0;
        }

        public override void DebugRender(Camera camera)
        {
            Collider collider = Entity.Collider;
            if (CheckWith != null)
            {
                Entity.Collider = CheckWith;
            }

            Entity.Collider.Render(camera, Color.Aqua);
            Entity.Collider = collider;
        }
    }
}
