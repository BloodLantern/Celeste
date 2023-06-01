// Decompiled with JetBrains decompiler
// Type: Celeste.Planets
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class Planets : Backdrop
    {
        private Planets.Planet[] planets;
        public const int MapWidth = 640;
        public const int MapHeight = 360;

        public Planets(int count, string size)
        {
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("bgs/10/" + size);
            this.planets = new Planets.Planet[count];
            for (int index = 0; index < this.planets.Length; ++index)
            {
                this.planets[index].Texture = Calc.Random.Choose<MTexture>(atlasSubtextures);
                this.planets[index].Position = new Vector2()
                {
                    X = Calc.Random.NextFloat(640f),
                    Y = Calc.Random.NextFloat(360f)
                };
            }
        }

        public override void Render(Scene scene)
        {
            Vector2 position1 = (scene as Level).Camera.Position;
            Color color = this.Color * this.FadeAlphaMultiplier;
            for (int index = 0; index < this.planets.Length; ++index)
            {
                Vector2 position2 = new Vector2()
                {
                    X = this.Mod(this.planets[index].Position.X - position1.X * this.Scroll.X, 640f) - 32f,
                    Y = this.Mod(this.planets[index].Position.Y - position1.Y * this.Scroll.Y, 360f) - 32f
                };
                this.planets[index].Texture.DrawCentered(position2, color);
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
