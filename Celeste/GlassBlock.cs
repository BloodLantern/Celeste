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
    private bool sinks;
    private float startY;
    private List<GlassBlock.Line> lines = new List<GlassBlock.Line>();
    private Color lineColor = Color.White;

    public GlassBlock(Vector2 position, float width, float height, bool sinks)
      : base(position, width, height, false)
    {
      this.sinks = sinks;
      this.startY = this.Y;
      this.Depth = -10000;
      this.Add((Component) new LightOcclude());
      this.Add((Component) new MirrorSurface());
      this.SurfaceSoundIndex = 32;
    }

    public GlassBlock(EntityData data, Vector2 offset)
      : this(data.Position + offset, (float) data.Width, (float) data.Height, data.Bool(nameof (sinks)))
    {
    }

    public override void Awake(Scene scene)
    {
      base.Awake(scene);
      int tiles1 = (int) this.Width / 8;
      int tiles2 = (int) this.Height / 8;
      this.AddSide(new Vector2(0.0f, 0.0f), new Vector2(0.0f, -1f), tiles1);
      this.AddSide(new Vector2((float) (tiles1 - 1), 0.0f), new Vector2(1f, 0.0f), tiles2);
      this.AddSide(new Vector2((float) (tiles1 - 1), (float) (tiles2 - 1)), new Vector2(0.0f, 1f), tiles1);
      this.AddSide(new Vector2(0.0f, (float) (tiles2 - 1)), new Vector2(-1f, 0.0f), tiles2);
    }

    private float Mod(float x, float m) => (x % m + m) % m;

    private void AddSide(Vector2 start, Vector2 normal, int tiles)
    {
      Vector2 vector2_1 = new Vector2(-normal.Y, normal.X);
      for (int index = 0; index < tiles; ++index)
      {
        if (this.Open(start + vector2_1 * (float) index + normal))
        {
          Vector2 vector2_2 = (start + vector2_1 * (float) index) * 8f + new Vector2(4f) - vector2_1 * 4f + normal * 4f;
          if (!this.Open(start + vector2_1 * (float) (index - 1)))
            vector2_2 -= vector2_1;
          while (index < tiles && this.Open(start + vector2_1 * (float) index + normal))
            ++index;
          Vector2 vector2_3 = (start + vector2_1 * (float) index) * 8f + new Vector2(4f) - vector2_1 * 4f + normal * 4f;
          if (!this.Open(start + vector2_1 * (float) index))
            vector2_3 += vector2_1;
          this.lines.Add(new GlassBlock.Line(vector2_2 + normal, vector2_3 + normal));
        }
      }
    }

    private bool Open(Vector2 tile)
    {
      Vector2 point = new Vector2((float) ((double) this.X + (double) tile.X * 8.0 + 4.0), (float) ((double) this.Y + (double) tile.Y * 8.0 + 4.0));
      return !this.Scene.CollideCheck<SolidTiles>(point) && !this.Scene.CollideCheck<GlassBlock>(point);
    }

    public override void Render()
    {
      foreach (GlassBlock.Line line in this.lines)
        Draw.Line(this.Position + line.A, this.Position + line.B, this.lineColor);
    }

    private struct Line
    {
      public Vector2 A;
      public Vector2 B;

      public Line(Vector2 a, Vector2 b)
      {
        this.A = a;
        this.B = b;
      }
    }
  }
}
