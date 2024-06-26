﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class IntroPavement : Solid
    {
        private int columns;

        public IntroPavement(Vector2 position, int width)
            : base(position, width, 8f, true)
        {
            columns = width / 8;
            Depth = -10;
            SurfaceSoundIndex = 1;
            SurfaceSoundPriority = 10;
        }

        public override void Awake(Scene scene)
        {
            for (int index = 0; index < columns; ++index)
            {
                int num = index >= columns - 2 ? (index != columns - 2 ? 3 : 2) : Calc.Random.Next(0, 2);
                Image image = new Image(GFX.Game["scenery/car/pavement"].GetSubtexture(num * 8, 0, 8, 8));
                image.Position = new Vector2(index * 8, 0.0f);
                Add(image);
            }
        }
    }
}
