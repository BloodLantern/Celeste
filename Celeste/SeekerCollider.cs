using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class SeekerCollider : Component
    {
        public Action<Seeker> OnCollide;
        public Collider Collider;

        public SeekerCollider(Action<Seeker> onCollide, Collider collider = null)
            : base(false, false)
        {
            OnCollide = onCollide;
            Collider = null;
        }

        public void Check(Seeker seeker)
        {
            if (OnCollide == null)
                return;
            Collider collider = Entity.Collider;
            if (Collider != null)
                Entity.Collider = Collider;
            if (seeker.CollideCheck(Entity))
                OnCollide(seeker);
            Entity.Collider = collider;
        }
    }
}
