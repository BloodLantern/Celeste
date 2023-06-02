using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class PufferCollider : Component
    {
        public Action<Puffer> OnCollide;
        public Collider Collider;

        public PufferCollider(Action<Puffer> onCollide, Collider collider = null)
            : base(false, false)
        {
            this.OnCollide = onCollide;
            this.Collider = (Collider) null;
        }

        public void Check(Puffer puffer)
        {
            if (this.OnCollide == null)
                return;
            Collider collider = this.Entity.Collider;
            if (this.Collider != null)
                this.Entity.Collider = this.Collider;
            if (puffer.CollideCheck(this.Entity))
                this.OnCollide(puffer);
            this.Entity.Collider = collider;
        }
    }
}
