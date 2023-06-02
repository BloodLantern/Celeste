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
