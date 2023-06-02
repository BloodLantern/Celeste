using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class SpawnFacingTrigger : Entity
    {
        public Facings Facing;

        public SpawnFacingTrigger(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.Collider = (Collider) new Hitbox((float) data.Width, (float) data.Height);
            this.Facing = data.Enum<Facings>("facing");
            this.Visible = this.Active = false;
        }
    }
}
