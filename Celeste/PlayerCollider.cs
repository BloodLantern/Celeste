using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class PlayerCollider : Component
    {
        public Action<Player> OnCollide;
        public Collider Collider;
        public Collider FeatherCollider;

        public PlayerCollider(Action<Player> onCollide, Collider collider = null, Collider featherCollider = null)
            : base(false, false)
        {
            OnCollide = onCollide;
            Collider = collider;
            FeatherCollider = featherCollider;
        }

        public bool Check(Player player)
        {
            Collider collider1 = Collider;
            if (FeatherCollider != null && player.StateMachine.State == 19)
                collider1 = FeatherCollider;
            if (collider1 == null)
            {
                if (!player.CollideCheck(Entity))
                    return false;
                OnCollide(player);
                return true;
            }
            Collider collider2 = Entity.Collider;
            Entity.Collider = collider1;
            int num = player.CollideCheck(Entity) ? 1 : 0;
            Entity.Collider = collider2;
            if (num == 0)
                return false;
            OnCollide(player);
            return true;
        }

        public override void DebugRender(Camera camera)
        {
            if (Collider == null)
                return;
            Collider collider = Entity.Collider;
            Entity.Collider = Collider;
            Collider.Render(camera, Color.HotPink);
            Entity.Collider = collider;
        }
    }
}
