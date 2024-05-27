using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class Planets : Backdrop
    {
        private Planet[] planets;
        public const int MapWidth = 640;
        public const int MapHeight = 360;

        public Planets(int count, string size)
        {
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("bgs/10/" + size);
            planets = new Planet[count];
            for (int index = 0; index < planets.Length; ++index)
            {
                planets[index].Texture = Calc.Random.Choose(atlasSubtextures);
                planets[index].Position = new Vector2
                {
                    X = Calc.Random.NextFloat(640f),
                    Y = Calc.Random.NextFloat(360f)
                };
            }
        }

        public override void Render(Scene scene)
        {
            Vector2 position1 = (scene as Level).Camera.Position;
            Color color = Color * FadeAlphaMultiplier;
            for (int index = 0; index < planets.Length; ++index)
            {
                Vector2 position2 = new Vector2
                {
                    X = Mod(planets[index].Position.X - position1.X * Scroll.X, 640f) - 32f,
                    Y = Mod(planets[index].Position.Y - position1.Y * Scroll.Y, 360f) - 32f
                };
                planets[index].Texture.DrawCentered(position2, color);
            }
        }

        private float Mod(float x, float m) => (x % m + m) % m;

        private struct Planet
        {
            public MTexture Texture;
            public Vector2 Position;
        }
    }
}
