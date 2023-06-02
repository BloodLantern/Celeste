using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class SeekerCollider : Component
    {
        public Action<Seeker> OnCollide;
        public Collider Collider;

        public SeekerCollider(Action<Seeker> onCollide, Collider collider = null)
            : base(false, false)
        {
            this.OnCollide = onCollide;
            this.Collider = (Collider) null;
        }

        public void Check(Seeker seeker)
        {
            if (this.OnCollide == null)
                return;
            Collider collider = this.Entity.Collider;
            if (this.Collider != null)
                this.Entity.Collider = this.Collider;
            if (seeker.CollideCheck(this.Entity))
                this.OnCollide(seeker);
            this.Entity.Collider = collider;
        }
    }
}
