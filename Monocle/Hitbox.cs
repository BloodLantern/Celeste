using Microsoft.Xna.Framework;

namespace Monocle
{
    public class Hitbox : Collider
    {
        private float width;
        private float height;

        public Hitbox(float width, float height, float x = 0.0f, float y = 0.0f)
        {
            this.width = width;
            this.height = height;
            Position.X = x;
            Position.Y = y;
        }

        public override float Width
        {
            get => width;
            set => width = value;
        }

        public override float Height
        {
            get => height;
            set => height = value;
        }

        public override float Left
        {
            get => Position.X;
            set => Position.X = value;
        }

        public override float Top
        {
            get => Position.Y;
            set => Position.Y = value;
        }

        public override float Right
        {
            get => Position.X + Width;
            set => Position.X = value - Width;
        }

        public override float Bottom
        {
            get => Position.Y + Height;
            set => Position.Y = value - Height;
        }

        public bool Intersects(Hitbox hitbox) => AbsoluteLeft < (double) hitbox.AbsoluteRight && AbsoluteRight > (double) hitbox.AbsoluteLeft && AbsoluteBottom > (double) hitbox.AbsoluteTop && AbsoluteTop < (double) hitbox.AbsoluteBottom;

        public bool Intersects(float x, float y, float width, float height) => AbsoluteRight > (double) x && AbsoluteBottom > (double) y && AbsoluteLeft < x + (double) width && AbsoluteTop < y + (double) height;

        public override Collider Clone() => new Hitbox(width, height, Position.X, Position.Y);

        public override void Render(Camera camera, Color color) => Draw.HollowRect(AbsoluteX, AbsoluteY, Width, Height, color);

        public void SetFromRectangle(Rectangle rect)
        {
            Position = new Vector2(rect.X, rect.Y);
            Width = rect.Width;
            Height = rect.Height;
        }

        public void Set(float x, float y, float w, float h)
        {
            Position = new Vector2(x, y);
            Width = w;
            Height = h;
        }

        public void GetTopEdge(out Vector2 from, out Vector2 to)
        {
            from.X = AbsoluteLeft;
            to.X = AbsoluteRight;
            from.Y = to.Y = AbsoluteTop;
        }

        public void GetBottomEdge(out Vector2 from, out Vector2 to)
        {
            from.X = AbsoluteLeft;
            to.X = AbsoluteRight;
            from.Y = to.Y = AbsoluteBottom;
        }

        public void GetLeftEdge(out Vector2 from, out Vector2 to)
        {
            from.Y = AbsoluteTop;
            to.Y = AbsoluteBottom;
            from.X = to.X = AbsoluteLeft;
        }

        public void GetRightEdge(out Vector2 from, out Vector2 to)
        {
            from.Y = AbsoluteTop;
            to.Y = AbsoluteBottom;
            from.X = to.X = AbsoluteRight;
        }

        public override bool Collide(Vector2 point) => Monocle.Collide.RectToPoint(AbsoluteLeft, AbsoluteTop, Width, Height, point);

        public override bool Collide(Rectangle rect) => AbsoluteRight > (double) rect.Left && AbsoluteBottom > (double) rect.Top && AbsoluteLeft < (double) rect.Right && AbsoluteTop < (double) rect.Bottom;

        public override bool Collide(Vector2 from, Vector2 to) => Monocle.Collide.RectToLine(AbsoluteLeft, AbsoluteTop, Width, Height, from, to);

        public override bool Collide(Hitbox hitbox) => Intersects(hitbox);

        public override bool Collide(Grid grid) => grid.Collide(Bounds);

        public override bool Collide(Circle circle) => Monocle.Collide.RectToCircle(AbsoluteLeft, AbsoluteTop, Width, Height, circle.AbsolutePosition, circle.Radius);

        public override bool Collide(ColliderList list) => list.Collide(this);
    }
}
