// Decompiled with JetBrains decompiler
// Type: Celeste.Slider
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
  public class Slider : Entity
  {
    private const float MaxSpeed = 80f;
    private const float Accel = 400f;
    private Vector2 dir;
    private Vector2 surface;
    private bool foundSurfaceAfterCorner;
    private bool gotOutOfWall;
    private float speed;
    private bool moving;

    public Slider(Vector2 position, bool clockwise, Slider.Surfaces surface)
      : base(position)
    {
      this.Collider = (Collider) new Monocle.Circle(10f);
      this.Add((Component) new StaticMover());
      switch (surface)
      {
        case Slider.Surfaces.Ceiling:
          this.dir = -Vector2.UnitX;
          this.surface = -Vector2.UnitY;
          break;
        case Slider.Surfaces.LeftWall:
          this.dir = -Vector2.UnitY;
          this.surface = -Vector2.UnitX;
          break;
        case Slider.Surfaces.RightWall:
          this.dir = Vector2.UnitY;
          this.surface = Vector2.UnitX;
          break;
        default:
          this.dir = Vector2.UnitX;
          this.surface = Vector2.UnitY;
          break;
      }
      if (!clockwise)
        this.dir *= -1f;
      this.moving = true;
      this.foundSurfaceAfterCorner = this.gotOutOfWall = true;
      this.speed = 80f;
      this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
    }

    public Slider(EntityData e, Vector2 offset)
      : this(e.Position + offset, e.Bool("clockwise", true), e.Enum<Slider.Surfaces>(nameof (surface)))
    {
    }

    public override void Awake(Scene scene)
    {
      base.Awake(scene);
      int num = 0;
      while (!this.Scene.CollideCheck<Solid>(this.Position))
      {
        this.Position = this.Position + this.surface;
        if (num >= 100)
          throw new Exception("Couldn't find surface");
      }
    }

    private void OnPlayer(Player Player)
    {
      Player.Die((Player.Center - this.Center).SafeNormalize(-Vector2.UnitY));
      this.moving = false;
    }

    public override void Update()
    {
      base.Update();
      if (!this.moving)
        return;
      this.speed = Calc.Approach(this.speed, 80f, 400f * Engine.DeltaTime);
      this.Position = this.Position + this.dir * this.speed * Engine.DeltaTime;
      if (!this.OnSurfaceCheck())
      {
        if (!this.foundSurfaceAfterCorner)
          return;
        this.Position = this.Position.Round();
        int num = 0;
        while (!this.OnSurfaceCheck())
        {
          this.Position = this.Position - this.dir;
          ++num;
          if (num >= 100)
            throw new Exception("Couldn't get back onto corner!");
        }
        this.foundSurfaceAfterCorner = false;
        Vector2 dir = this.dir;
        this.dir = this.surface;
        this.surface = -dir;
      }
      else
      {
        this.foundSurfaceAfterCorner = true;
        if (this.InWallCheck())
        {
          if (!this.gotOutOfWall)
            return;
          this.Position = this.Position.Round();
          int num = 0;
          while (this.InWallCheck())
          {
            this.Position = this.Position - this.dir;
            ++num;
            if (num >= 100)
              throw new Exception("Couldn't get out of wall!");
          }
          this.Position = this.Position + (this.dir - this.surface);
          this.gotOutOfWall = false;
          Vector2 surface = this.surface;
          this.surface = this.dir;
          this.dir = -surface;
        }
        else
          this.gotOutOfWall = true;
      }
    }

    private bool OnSurfaceCheck() => this.Scene.CollideCheck<Solid>(this.Position.Round() + this.surface);

    private bool InWallCheck() => this.Scene.CollideCheck<Solid>(this.Position.Round() - this.surface);

    public override void Render() => Draw.Circle(this.Position, 12f, Color.Red, 8);

    public enum Surfaces
    {
      Floor,
      Ceiling,
      LeftWall,
      RightWall,
    }
  }
}
