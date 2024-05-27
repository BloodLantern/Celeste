using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class SummitCloud : Entity
    {
        private Image image;
        private float diff;

        public SummitCloud(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = -10550;
            diff = Calc.Random.Range(0.1f, 0.2f);
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("scenery/summitclouds/cloud");
            image = new Image(Calc.Random.Choose(atlasSubtextures));
            image.CenterOrigin();
            image.Scale.X = Calc.Random.Choose(-1, 1);
            Add(image);
            SineWave sineWave = new SineWave(Calc.Random.Range(0.05f, 0.15f));
            sineWave.Randomize();
            sineWave.OnUpdate = f => image.Y = f * 8f;
            Add(sineWave);
        }

        private Vector2 RenderPosition => Position + (Position + new Vector2(128f, 64f) / 2f - ((Scene as Level).Camera.Position + new Vector2(160f, 90f))) * (0.1f + diff);

        public override void Render()
        {
            Vector2 position = Position;
            Position = RenderPosition;
            base.Render();
            Position = position;
        }
    }
}
