using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class Lamp : Entity
    {
        private Monocle.Image sprite;

        public Lamp(Vector2 position, bool broken)
        {
            this.Position = position;
            this.Depth = 5;
            this.Add((Component) (this.sprite = new Monocle.Image(GFX.Game["scenery/lamp"].GetSubtexture(broken ? 16 : 0, 0, 16, 80))));
            this.sprite.Origin = new Vector2(this.sprite.Width / 2f, this.sprite.Height);
            if (broken)
                return;
            this.Add((Component) new BloomPoint(new Vector2(0.0f, -66f), 1f, 16f));
        }
    }
}
