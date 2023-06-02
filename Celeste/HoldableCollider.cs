using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class HoldableCollider : Component
    {
        private Collider collider;
        public Action<Holdable> OnCollide;

        public HoldableCollider(Action<Holdable> onCollide, Collider collider = null)
            : base(false, false)
        {
            this.collider = collider;
            this.OnCollide = onCollide;
        }

        public bool Check(Holdable holdable)
        {
            Collider collider = this.Entity.Collider;
            if (this.collider != null)
                this.Entity.Collider = this.collider;
            int num = holdable.Entity.CollideCheck(this.Entity) ? 1 : 0;
            this.Entity.Collider = collider;
            return num != 0;
        }
    }
}
