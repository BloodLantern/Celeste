using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class NegaBlock : Solid
    {
        public NegaBlock(Vector2 position, float width, float height)
            : base(position, width, height, false)
        {
        }

        public NegaBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, (float) data.Width, (float) data.Height)
        {
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(this.Collider, Color.Red);
        }
    }
}
