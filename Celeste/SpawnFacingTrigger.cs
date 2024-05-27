using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class SpawnFacingTrigger : Entity
    {
        public Facings Facing;

        public SpawnFacingTrigger(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Facing = data.Enum<Facings>("facing");
            Visible = Active = false;
        }
    }
}
