using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BridgeFixed : Solid
    {
        public BridgeFixed(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, 8f, true)
        {
            MTexture texture = GFX.Game["scenery/bridge_fixed"];
            for (int x = 0; x < (double) Width; x += texture.Width)
            {
                Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                if (x + rectangle.Width > (double) Width)
                    rectangle.Width = (int) Width - x;
                Image image = new Image(texture);
                image.Position = new Vector2(x, -8f);
                Add(image);
            }
        }
    }
}
