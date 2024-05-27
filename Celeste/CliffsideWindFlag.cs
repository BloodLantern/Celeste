using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class CliffsideWindFlag : Entity
    {
        private Segment[] segments;
        private float sine;
        private float random;
        private int sign;

        public CliffsideWindFlag(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            MTexture atlasSubtexturesAt = GFX.Game.GetAtlasSubtexturesAt("scenery/cliffside/flag", data.Int("index"));
            segments = new Segment[atlasSubtexturesAt.Width];
            for (int x = 0; x < segments.Length; ++x)
                segments[x] = new Segment
                {
                    Texture = atlasSubtexturesAt.GetSubtexture(x, 0, 1, atlasSubtexturesAt.Height),
                    Offset = new Vector2(x, 0.0f)
                };
            sine = Calc.Random.NextFloat(6.28318548f);
            random = Calc.Random.NextFloat();
            Depth = 8999;
            Tag = (int) Tags.TransitionUpdate;
        }

        private float wind => Calc.ClampedMap(Math.Abs((Scene as Level).Wind.X), 0.0f, 800f);

        public override void Added(Scene scene)
        {
            base.Added(scene);
            sign = 1;
            if (wind != 0.0)
                sign = Math.Sign((Scene as Level).Wind.X);
            for (int i = 0; i < segments.Length; ++i)
                SetFlagSegmentPosition(i, true);
        }

        public override void Update()
        {
            base.Update();
            if (wind != 0.0)
                sign = Math.Sign((Scene as Level).Wind.X);
            sine += (float) (Engine.DeltaTime * (4.0 + wind * 4.0) * (0.800000011920929 + random * 0.20000000298023224));
            for (int i = 0; i < segments.Length; ++i)
                SetFlagSegmentPosition(i, false);
        }

        private float Sin(float timer) => (float) Math.Sin(-(double) timer);

        private void SetFlagSegmentPosition(int i, bool snap)
        {
            Segment segment = segments[i];
            float num = (float) (i * sign * (0.20000000298023224 + wind * 0.800000011920929 * (0.800000011920929 + random * 0.20000000298023224)) * (0.89999997615814209 + Sin(sine) * 0.10000000149011612));
            float target1 = Calc.LerpClamp((float) (Sin((float) (sine * 0.5 - i * 0.10000000149011612)) * (i / (double) segments.Length) * i * 0.20000000298023224), num, (float) Math.Ceiling(wind));
            float target2 = (float) (i / (double) segments.Length * Math.Max(0.1f, 1f - wind) * 16.0);
            if (!snap)
            {
                segment.Offset.X = Calc.Approach(segment.Offset.X, target1, Engine.DeltaTime * 40f);
                segment.Offset.Y = Calc.Approach(segment.Offset.Y, target2, Engine.DeltaTime * 40f);
            }
            else
            {
                segment.Offset.X = target1;
                segment.Offset.Y = target2;
            }
        }

        public override void Render()
        {
            base.Render();
            for (int index = 0; index < segments.Length; ++index)
            {
                Segment segment = segments[index];
                float num = (float) (index / (double) segments.Length * Sin(-index * 0.1f + sine) * 2.0);
                segment.Texture.Draw(Position + segment.Offset + Vector2.UnitY * num);
            }
        }

        private class Segment
        {
            public MTexture Texture;
            public Vector2 Offset;
        }
    }
}
