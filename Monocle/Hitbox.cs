// Decompiled with JetBrains decompiler
// Type: Monocle.Hitbox
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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

        public bool Intersects(Hitbox hitbox)
        {
            return (double)AbsoluteLeft < (double)hitbox.AbsoluteRight && (double)AbsoluteRight > (double)hitbox.AbsoluteLeft && (double)AbsoluteBottom > (double)hitbox.AbsoluteTop && (double)AbsoluteTop < (double)hitbox.AbsoluteBottom;
        }

        public bool Intersects(float x, float y, float width, float height)
        {
            return (double)AbsoluteRight > (double)x && (double)AbsoluteBottom > (double)y && (double)AbsoluteLeft < (double)x + (double)width && (double)AbsoluteTop < (double)y + (double)height;
        }

        public override Collider Clone()
        {
            return new Hitbox(width, height, Position.X, Position.Y);
        }

        public override void Render(Camera camera, Color color)
        {
            Draw.HollowRect(AbsoluteX, AbsoluteY, Width, Height, color);
        }

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

        public override bool Collide(Vector2 point)
        {
            return Monocle.Collide.RectToPoint(AbsoluteLeft, AbsoluteTop, Width, Height, point);
        }

        public override bool Collide(Rectangle rect)
        {
            return (double)AbsoluteRight > rect.Left && (double)AbsoluteBottom > rect.Top && (double)AbsoluteLeft < rect.Right && (double)AbsoluteTop < rect.Bottom;
        }

        public override bool Collide(Vector2 from, Vector2 to)
        {
            return Monocle.Collide.RectToLine(AbsoluteLeft, AbsoluteTop, Width, Height, from, to);
        }

        public override bool Collide(Hitbox hitbox)
        {
            return Intersects(hitbox);
        }

        public override bool Collide(Grid grid)
        {
            return grid.Collide(Bounds);
        }

        public override bool Collide(Circle circle)
        {
            return Monocle.Collide.RectToCircle(AbsoluteLeft, AbsoluteTop, Width, Height, circle.AbsolutePosition, circle.Radius);
        }

        public override bool Collide(ColliderList list)
        {
            return list.Collide(this);
        }
    }
}
