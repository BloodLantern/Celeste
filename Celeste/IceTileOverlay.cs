using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class IceTileOverlay : Entity
    {
        private List<MTexture> surfaces;
        private float alpha;

        public IceTileOverlay()
        {
            Depth = -10010;
            Tag = (int) Tags.Global;
            Visible = false;
            surfaces = GFX.Game.GetAtlasSubtextures("scenery/iceSurface");
        }

        public override void Update()
        {
            base.Update();
            alpha = Calc.Approach(alpha, (Scene as Level).CoreMode == Session.CoreModes.Cold ? 1f : 0.0f, Engine.DeltaTime * 4f);
            Visible = alpha > 0.0;
        }

        public override void Render()
        {
            Level scene = Scene as Level;
            Camera camera = scene.Camera;
            Color color = Color.White * alpha;
            int num1 = (int) (Math.Floor((camera.Left - (double) scene.SolidTiles.X) / 8.0) - 1.0);
            int num2 = (int) (Math.Floor((camera.Top - (double) scene.SolidTiles.Y) / 8.0) - 1.0);
            int num3 = (int) (Math.Ceiling((camera.Right - (double) scene.SolidTiles.X) / 8.0) + 1.0);
            int num4 = (int) (Math.Ceiling((camera.Bottom - (double) scene.SolidTiles.Y) / 8.0) + 1.0);
            for (int x = num1; x < num3; ++x)
            {
                for (int y = num2; y < num4; ++y)
                {
                    if (scene.SolidsData.SafeCheck(x, y) != '0' && scene.SolidsData.SafeCheck(x, y - 1) == '0')
                    {
                        Vector2 position = scene.SolidTiles.Position + new Vector2(x, y) * 8f;
                        surfaces[(x * 5 + y * 17) % surfaces.Count].Draw(position, Vector2.Zero, color);
                    }
                }
            }
        }
    }
}
