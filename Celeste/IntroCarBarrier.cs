using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class IntroCarBarrier : Entity
    {
        public IntroCarBarrier(Vector2 position, int depth, Color color)
        {
            this.Position = position;
            this.Depth = depth;
            Monocle.Image image = new Monocle.Image(GFX.Game["scenery/car/barrier"]);
            image.Origin = new Vector2(0.0f, image.Height);
            image.Color = color;
            this.Add((Component) image);
        }
    }
}
