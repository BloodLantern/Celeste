using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class HoldableCollider : Component
    {
        private Collider collider;
        public Action<Holdable> OnCollide;

        public HoldableCollider(Action<Holdable> onCollide, Collider collider = null)
            : base(false, false)
        {
            this.collider = collider;
            OnCollide = onCollide;
        }

        public bool Check(Holdable holdable)
        {
            Collider collider = Entity.Collider;
            if (this.collider != null)
                Entity.Collider = this.collider;
            int num = holdable.Entity.CollideCheck(Entity) ? 1 : 0;
            Entity.Collider = collider;
            return num != 0;
        }
    }
}
