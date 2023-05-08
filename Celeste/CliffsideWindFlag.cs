// Decompiled with JetBrains decompiler
// Type: Celeste.CliffsideWindFlag
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class CliffsideWindFlag : Entity
    {
        private readonly Segment[] segments;
        private float sine;
        private readonly float random;
        private int sign;

        public CliffsideWindFlag(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            MTexture atlasSubtexturesAt = GFX.Game.GetAtlasSubtexturesAt("scenery/cliffside/flag", data.Int("index"));
            segments = new Segment[atlasSubtexturesAt.Width];
            for (int x = 0; x < segments.Length; ++x)
                segments[x] = new Segment()
                {
                    Texture = atlasSubtexturesAt.GetSubtexture(x, 0, 1, atlasSubtexturesAt.Height),
                    Offset = new Vector2(x, 0.0f)
                };

            sine = Calc.Random.NextFloat((float) Math.PI * 2);
            random = Calc.Random.NextFloat();
            Depth = 8999;
            Tag = (int) Tags.TransitionUpdate;
        }

        private float wind => Calc.ClampedMap(Math.Abs((Scene as Level).Wind.X), 0f, 800f);

        public override void Added(Scene scene)
        {
            base.Added(scene);
            sign = 1;
            if (wind != 0)
                sign = Math.Sign((Scene as Level).Wind.X);

            for (int i = 0; i < segments.Length; ++i)
                SetFlagSegmentPosition(i, true);
        }

        public override void Update()
        {
            base.Update();
            if (wind != 0)
                sign = Math.Sign((Scene as Level).Wind.X);

            sine += Engine.DeltaTime * (4 + (wind * 4)) * (0.8f + (random * 0.2f));
            for (int i = 0; i < segments.Length; ++i)
                SetFlagSegmentPosition(i, false);
        }

        private float Sin(float timer)
        {
            return (float) Math.Sin(-timer);
        }

        private void SetFlagSegmentPosition(int i, bool snap)
        {
            Segment segment = segments[i];
            float num = i * sign * (0.2f + wind * 0.8f * (0.8f + random * 0.2f)) * (0.9f + Sin(sine) * 0.1f);
            float target1 = Calc.LerpClamp(Sin(sine * 0.5f - i * 0.1f) * (i / segments.Length) * i * 0.2f, num, (float) Math.Ceiling(wind));
            float target2 = i / segments.Length * Math.Max(0.1f, 1f - wind) * 16;
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
                float num = index / segments.Length * Sin(-index * 0.1f + sine) * 2;
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
