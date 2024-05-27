using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked()]
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
                return false;
            Collider collider = Entity.Collider;
            if (CheckWith != null)
                Entity.Collider = CheckWith;
            int num = player.CollideCheck(Entity) ? 1 : 0;
            Entity.Collider = collider;
            return num != 0;
        }

        public override void DebugRender(Camera camera)
        {
            Collider collider = Entity.Collider;
            if (CheckWith != null)
                Entity.Collider = CheckWith;
            Entity.Collider.Render(camera, Color.Aqua);
            Entity.Collider = collider;
        }
    }
}
