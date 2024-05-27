using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class LookoutBlocker : Entity
    {
        public LookoutBlocker(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
        }
    }
}
