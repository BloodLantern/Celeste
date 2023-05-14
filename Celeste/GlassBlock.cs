// Decompiled with JetBrains decompiler
// Type: Celeste.GlassBlock
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
    public class GlassBlock : Solid
    {
        private readonly bool sinks;
        private readonly float startY;
        private readonly List<GlassBlock.Line> lines = new();
        private Color lineColor = Color.White;

        public GlassBlock(Vector2 position, float width, float height, bool sinks)
            : base(position, width, height, false)
        {
            this.sinks = sinks;
            startY = Y;
            Depth = -10000;
            Add(new LightOcclude());
            Add(new MirrorSurface());
            SurfaceSoundIndex = 32;
        }

        public GlassBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Bool(nameof(sinks)))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            int tiles1 = (int)Width / 8;
            int tiles2 = (int)Height / 8;
            AddSide(new Vector2(0.0f, 0.0f), new Vector2(0.0f, -1f), tiles1);
            AddSide(new Vector2(tiles1 - 1, 0.0f), new Vector2(1f, 0.0f), tiles2);
            AddSide(new Vector2(tiles1 - 1, tiles2 - 1), new Vector2(0.0f, 1f), tiles1);
            AddSide(new Vector2(0.0f, tiles2 - 1), new Vector2(-1f, 0.0f), tiles2);
        }

        private float Mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }

        private void AddSide(Vector2 start, Vector2 normal, int tiles)
        {
            Vector2 vector2_1 = new(-normal.Y, normal.X);
            for (int index = 0; index < tiles; ++index)
            {
                if (Open(start + (vector2_1 * index) + normal))
                {
                    Vector2 vector2_2 = ((start + (vector2_1 * index)) * 8f) + new Vector2(4f) - (vector2_1 * 4f) + (normal * 4f);
                    if (!Open(start + (vector2_1 * (index - 1))))
                    {
                        vector2_2 -= vector2_1;
                    }

                    while (index < tiles && Open(start + (vector2_1 * index) + normal))
                    {
                        ++index;
                    }

                    Vector2 vector2_3 = ((start + (vector2_1 * index)) * 8f) + new Vector2(4f) - (vector2_1 * 4f) + (normal * 4f);
                    if (!Open(start + (vector2_1 * index)))
                    {
                        vector2_3 += vector2_1;
                    }

                    lines.Add(new GlassBlock.Line(vector2_2 + normal, vector2_3 + normal));
                }
            }
        }

        private bool Open(Vector2 tile)
        {
            Vector2 point = new((float)((double)X + (tile.X * 8.0) + 4.0), (float)((double)Y + (tile.Y * 8.0) + 4.0));
            return !Scene.CollideCheck<SolidTiles>(point) && !Scene.CollideCheck<GlassBlock>(point);
        }

        public override void Render()
        {
            foreach (GlassBlock.Line line in lines)
            {
                Draw.Line(Position + line.A, Position + line.B, lineColor);
            }
        }

        private struct Line
        {
            public Vector2 A;
            public Vector2 B;

            public Line(Vector2 a, Vector2 b)
            {
                A = a;
                B = b;
            }
        }
    }
}
