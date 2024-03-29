﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class BridgeFixed : Solid
    {
        public BridgeFixed(EntityData data, Vector2 offset)
            : base(data.Position + offset, (float) data.Width, 8f, true)
        {
            MTexture texture = GFX.Game["scenery/bridge_fixed"];
            for (int x = 0; (double) x < (double) this.Width; x += texture.Width)
            {
                Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
                if ((double) (x + rectangle.Width) > (double) this.Width)
                    rectangle.Width = (int) this.Width - x;
                Monocle.Image image = new Monocle.Image(texture);
                image.Position = new Vector2((float) x, -8f);
                this.Add((Component) image);
            }
        }
    }
}
