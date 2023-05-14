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
            Radius = radius;
            Position.X = x;
            Position.Y = y;
        }

        public override float Width
        {
            get => Radius * 2f;
            set => Radius = value / 2f;
        }

        public override float Height
        {
            get => Radius * 2f;
            set => Radius = value / 2f;
        }

        public override float Left
        {
            get => Position.X - Radius;
            set => Position.X = value + Radius;
        }

        public override float Top
        {
            get => Position.Y - Radius;
            set => Position.Y = value + Radius;
        }

        public override float Right
        {
            get => Position.X + Radius;
            set => Position.X = value - Radius;
        }

        public override float Bottom
        {
            get => Position.Y + Radius;
            set => Position.Y = value - Radius;
        }

        public override Collider Clone()
        {
            return new Circle(Radius, Position.X, Position.Y);
        }

        public override void Render(Camera camera, Color color)
        {
            Draw.Circle(AbsolutePosition, Radius, color, 4);
        }

        public override bool Collide(Vector2 point)
        {
            return Monocle.Collide.CircleToPoint(AbsolutePosition, Radius, point);
        }

        public override bool Collide(Rectangle rect)
        {
            return Monocle.Collide.RectToCircle(rect, AbsolutePosition, Radius);
        }

        public override bool Collide(Vector2 from, Vector2 to)
        {
            return Monocle.Collide.CircleToLine(AbsolutePosition, Radius, from, to);
        }

        public override bool Collide(Circle circle)
        {
            return (double)Vector2.DistanceSquared(AbsolutePosition, circle.AbsolutePosition) < (Radius + (double)circle.Radius) * (Radius + (double)circle.Radius);
        }

        public override bool Collide(Hitbox hitbox)
        {
            return hitbox.Collide(this);
        }

        public override bool Collide(Grid grid)
        {
            return grid.Collide(this);
        }

        public override bool Collide(ColliderList list)
        {
            return list.Collide(this);
        }
    }
}
