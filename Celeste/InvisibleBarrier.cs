using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class InvisibleBarrier : Solid
    {
        public InvisibleBarrier(Vector2 position, float width, float height)
            : base(position, width, height, true)
        {
            Tag = (int) Tags.TransitionUpdate;
            Collidable = false;
            Visible = false;
            Add(new ClimbBlocker(true));
            SurfaceSoundIndex = 33;
        }

        public InvisibleBarrier(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        public override void Update()
        {
            Collidable = true;
            if (CollideCheck<Player>())
                Collidable = false;
            if (Collidable)
                return;
            Active = false;
        }
    }
}
