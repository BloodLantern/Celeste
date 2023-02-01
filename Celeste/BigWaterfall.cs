// Decompiled with JetBrains decompiler
// Type: Celeste.BigWaterfall
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
  public class BigWaterfall : Entity
  {
    private BigWaterfall.Layers layer;
    private float width;
    private float height;
    private float parallax;
    private List<float> lines = new List<float>();
    private Color surfaceColor;
    private Color fillColor;
    private float sine;
    private SoundSource loopingSfx;
    private float fade;

    private Vector2 RenderPosition => this.RenderPositionAtCamera((this.Scene as Level).Camera.Position + new Vector2(160f, 90f));

    public BigWaterfall(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Tag = (int) Tags.TransitionUpdate;
      this.layer = data.Enum<BigWaterfall.Layers>(nameof (layer), BigWaterfall.Layers.BG);
      this.width = (float) data.Width;
      this.height = (float) data.Height;
      if (this.layer == BigWaterfall.Layers.FG)
      {
        this.Depth = -49900;
        this.parallax = (float) (0.10000000149011612 + (double) Calc.Random.NextFloat() * 0.20000000298023224);
        this.surfaceColor = Water.SurfaceColor;
        this.fillColor = Water.FillColor;
        this.Add((Component) new DisplacementRenderHook(new Action(this.RenderDisplacement)));
        this.lines.Add(3f);
        this.lines.Add(this.width - 4f);
        this.Add((Component) (this.loopingSfx = new SoundSource()));
        this.loopingSfx.Play("event:/env/local/waterfall_big_main");
      }
      else
      {
        this.Depth = 10010;
        this.parallax = (float) -(0.699999988079071 + (double) Calc.Random.NextFloat() * 0.20000000298023224);
        this.surfaceColor = Calc.HexToColor("89dbf0") * 0.5f;
        this.fillColor = Calc.HexToColor("29a7ea") * 0.3f;
        this.lines.Add(6f);
        this.lines.Add(this.width - 7f);
      }
      this.fade = 1f;
      this.Add((Component) new TransitionListener()
      {
        OnIn = (Action<float>) (f => this.fade = f),
        OnOut = (Action<float>) (f => this.fade = 1f - f)
      });
      if ((double) this.width <= 16.0)
        return;
      int num = Calc.Random.Next((int) ((double) this.width / 16.0));
      for (int index = 0; index < num; ++index)
        this.lines.Add(8f + Calc.Random.NextFloat(this.width - 16f));
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      if (!(this.Scene as Level).Transitioning)
        return;
      this.fade = 0.0f;
    }

    public Vector2 RenderPositionAtCamera(Vector2 camera)
    {
      Vector2 vector2 = this.Position + new Vector2(this.width, this.height) / 2f - camera;
      Vector2 zero = Vector2.Zero;
      if (this.layer == BigWaterfall.Layers.BG)
        zero -= vector2 * 0.6f;
      else if (this.layer == BigWaterfall.Layers.FG)
        zero += vector2 * 0.2f;
      return this.Position + zero;
    }

    public void RenderDisplacement() => Draw.Rect(this.RenderPosition.X, this.Y, this.width, this.height, new Color(0.5f, 0.5f, 1f, 1f));

    public override void Update()
    {
      this.sine += Engine.DeltaTime;
      if (this.loopingSfx != null)
        this.loopingSfx.Position = new Vector2(this.RenderPosition.X - this.X, Calc.Clamp((this.Scene as Level).Camera.Position.Y + 90f, this.Y, this.height) - this.Y);
      base.Update();
    }

    public override void Render()
    {
      float x = this.RenderPosition.X;
      Color color1 = this.fillColor * this.fade;
      Color color2 = this.surfaceColor * this.fade;
      Draw.Rect(x, this.Y, this.width, this.height, color1);
      if (this.layer == BigWaterfall.Layers.FG)
      {
        Draw.Rect(x - 1f, this.Y, 3f, this.height, color2);
        Draw.Rect((float) ((double) x + (double) this.width - 2.0), this.Y, 3f, this.height, color2);
        foreach (float line in this.lines)
          Draw.Rect(x + line, this.Y, 1f, this.height, color2);
      }
      else
      {
        Vector2 position = (this.Scene as Level).Camera.Position;
        int height = 3;
        double num1 = (double) Math.Max(this.Y, (float) Math.Floor((double) position.Y / (double) height) * (float) height);
        float num2 = Math.Min(this.Y + this.height, position.Y + 180f);
        for (float y = (float) num1; (double) y < (double) num2; y += (float) height)
        {
          int num3 = (int) (Math.Sin((double) y / 6.0 - (double) this.sine * 8.0) * 2.0);
          Draw.Rect(x, y, (float) (4 + num3), (float) height, color2);
          Draw.Rect((float) ((double) x + (double) this.width - 4.0) + (float) num3, y, (float) (4 - num3), (float) height, color2);
          foreach (float line in this.lines)
            Draw.Rect(x + (float) num3 + line, y, 1f, (float) height, color2);
        }
      }
    }

    private enum Layers
    {
      FG,
      BG,
    }
  }
}
