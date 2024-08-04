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

        public override Collider Clone() => new Circle(Radius, Position.X, Position.Y);

        public override void Render(Camera camera, Color color) => Draw.Circle(AbsolutePosition, Radius, color, 4);

        public override bool Collide(Vector2 point) => Monocle.Collide.CircleToPoint(AbsolutePosition, Radius, point);

        public override bool Collide(Rectangle rect) => Monocle.Collide.RectToCircle(rect, AbsolutePosition, Radius);

        public override bool Collide(Vector2 from, Vector2 to) => Monocle.Collide.CircleToLine(AbsolutePosition, Radius, from, to);

        public override bool Collide(Circle circle) => Vector2.DistanceSquared(AbsolutePosition, circle.AbsolutePosition) < (Radius + (double) circle.Radius) * (Radius + (double) circle.Radius);

        public override bool Collide(Hitbox hitbox) => hitbox.Collide(this);

        public override bool Collide(Grid grid) => grid.Collide(this);

        public override bool Collide(ColliderList list) => list.Collide(this);
    }
}
