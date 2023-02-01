// Decompiled with JetBrains decompiler
// Type: Monocle.Circle
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Monocle
{
  public class Circle : Collider
  {
    public float Radius;

    public Circle(float radius, float x = 0.0f, float y = 0.0f)
    {
      this.Radius = radius;
      this.Position.X = x;
      this.Position.Y = y;
    }

    public override float Width
    {
      get => this.Radius * 2f;
      set => this.Radius = value / 2f;
    }

    public override float Height
    {
      get => this.Radius * 2f;
      set => this.Radius = value / 2f;
    }

    public override float Left
    {
      get => this.Position.X - this.Radius;
      set => this.Position.X = value + this.Radius;
    }

    public override float Top
    {
      get => this.Position.Y - this.Radius;
      set => this.Position.Y = value + this.Radius;
    }

    public override float Right
    {
      get => this.Position.X + this.Radius;
      set => this.Position.X = value - this.Radius;
    }

    public override float Bottom
    {
      get => this.Position.Y + this.Radius;
      set => this.Position.Y = value - this.Radius;
    }

    public override Collider Clone() => (Collider) new Circle(this.Radius, this.Position.X, this.Position.Y);

    public override void Render(Camera camera, Color color) => Draw.Circle(this.AbsolutePosition, this.Radius, color, 4);

    public override bool Collide(Vector2 point) => Monocle.Collide.CircleToPoint(this.AbsolutePosition, this.Radius, point);

    public override bool Collide(Rectangle rect) => Monocle.Collide.RectToCircle(rect, this.AbsolutePosition, this.Radius);

    public override bool Collide(Vector2 from, Vector2 to) => Monocle.Collide.CircleToLine(this.AbsolutePosition, this.Radius, from, to);

    public override bool Collide(Circle circle) => (double) Vector2.DistanceSquared(this.AbsolutePosition, circle.AbsolutePosition) < ((double) this.Radius + (double) circle.Radius) * ((double) this.Radius + (double) circle.Radius);

    public override bool Collide(Hitbox hitbox) => hitbox.Collide(this);

    public override bool Collide(Grid grid) => grid.Collide(this);

    public override bool Collide(ColliderList list) => list.Collide(this);
  }
}
