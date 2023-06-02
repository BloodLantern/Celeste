using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class Wire : Entity
    {
        public Color Color = Calc.HexToColor("595866");
        public SimpleCurve Curve;
        private float sineX;
        private float sineY;

        public Wire(Vector2 from, Vector2 to, bool above)
        {
            this.Curve = new SimpleCurve(from, to, Vector2.Zero);
            this.Depth = above ? -8500 : 2000;
            Random random = new Random((int) Math.Min(from.X, to.X));
            this.sineX = random.NextFloat(4f);
            this.sineY = random.NextFloat(4f);
        }

        public Wire(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Nodes[0] + offset, data.Bool("above"))
        {
        }

        public override void Render()
        {
            Level level = this.SceneAs<Level>();
            this.Curve.Control = (this.Curve.Begin + this.Curve.End) / 2f + new Vector2(0.0f, 24f) + new Vector2((float) Math.Sin((double) this.sineX + (double) level.WindSineTimer * 2.0), (float) Math.Sin((double) this.sineY + (double) level.WindSineTimer * 2.7999999523162842)) * 8f * level.VisualWind;
            Vector2 start = this.Curve.Begin;
            for (int index = 1; index <= 16; ++index)
            {
                Vector2 point = this.Curve.GetPoint((float) index / 16f);
                Draw.Line(start, point, this.Color);
                start = point;
            }
        }
    }
}
