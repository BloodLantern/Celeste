// Decompiled with JetBrains decompiler
// Type: Celeste.ForegroundDebris
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class ForegroundDebris : Entity
    {
        private Vector2 start;
        private readonly float parallax;

        public ForegroundDebris(Vector2 position)
            : base(position)
        {
            start = Position;
            Depth = -999900;
            string key = "scenery/fgdebris/" + Calc.Random.Choose<string>("rock_a", "rock_b");
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(key);
            atlasSubtextures.Reverse();
            foreach (MTexture texture in atlasSubtextures)
            {
                Monocle.Image img = new(texture);
                _ = img.CenterOrigin();
                Add(img);
                SineWave sine = new(0.4f);
                _ = sine.Randomize();
                sine.OnUpdate = f => img.Y = sine.Value * 2f;
                Add(sine);
            }
            parallax = 0.05f + Calc.Random.NextFloat(0.08f);
        }

        public ForegroundDebris(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override void Render()
        {
            Vector2 vector2 = SceneAs<Level>().Camera.Position + (new Vector2(320f, 180f) / 2f) - start;
            Vector2 position = Position;
            Position -= vector2 * parallax;
            base.Render();
            Position = position;
        }
    }
}
