using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class RespawnTargetTrigger : Entity
    {
        public Vector2 Target;

        public RespawnTargetTrigger(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.Collider = (Collider) new Hitbox((float) data.Width, (float) data.Height);
            this.Target = data.Nodes[0] + offset;
            this.Visible = this.Active = false;
        }
    }
}
