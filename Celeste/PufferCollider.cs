using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class PufferCollider : Component
    {
        public Action<Puffer> OnCollide;
        public Collider Collider;

        public PufferCollider(Action<Puffer> onCollide, Collider collider = null)
            : base(false, false)
        {
            OnCollide = onCollide;
            Collider = null;
        }

        public void Check(Puffer puffer)
        {
            if (OnCollide == null)
                return;
            Collider collider = Entity.Collider;
            if (Collider != null)
                Entity.Collider = Collider;
            if (puffer.CollideCheck(Entity))
                OnCollide(puffer);
            Entity.Collider = collider;
        }
    }
}
