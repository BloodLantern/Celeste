using Microsoft.Xna.Framework;
using System;

namespace Monocle
{
    public abstract class Collider
    {
        public Vector2 Position;

        public Entity Entity { get; private set; }

        internal virtual void Added(Entity entity) => this.Entity = entity;

        internal virtual void Removed() => this.Entity = (Entity) null;

        public bool Collide(Entity entity) => this.Collide(entity.Collider);

        public bool Collide(Collider collider)
        {
            switch (collider)
            {
                case Hitbox _:
                    return this.Collide(collider as Hitbox);
                case Grid _:
                    return this.Collide(collider as Grid);
                case ColliderList _:
                    return this.Collide(collider as ColliderList);
                case Circle _:
                    return this.Collide(collider as Circle);
                default:
                    throw new Exception("Collisions against the collider type are not implemented!");
            }
        }

        public abstract bool Collide(Vector2 point);

        public abstract bool Collide(Rectangle rect);

        public abstract bool Collide(Vector2 from, Vector2 to);

        public abstract bool Collide(Hitbox hitbox);

        public abstract bool Collide(Grid grid);

        public abstract bool Collide(Circle circle);

        public abstract bool Collide(ColliderList list);

        public abstract Collider Clone();

        public abstract void Render(Camera camera, Color color);

        public abstract float Width { get; set; }

        public abstract float Height { get; set; }

        public abstract float Top { get; set; }

        public abstract float Bottom { get; set; }

        public abstract float Left { get; set; }

        public abstract float Right { get; set; }

        public void CenterOrigin()
        {
            this.Position.X = (float) (-(double) this.Width / 2.0);
            this.Position.Y = (float) (-(double) this.Height / 2.0);
        }

        public float CenterX
        {
            get => this.Left + this.Width / 2f;
            set => this.Left = value - this.Width / 2f;
        }

        public float CenterY
        {
            get => this.Top + this.Height / 2f;
            set => this.Top = value - this.Height / 2f;
        }

        public Vector2 TopLeft
        {
            get => new Vector2(this.Left, this.Top);
            set
            {
                this.Left = value.X;
                this.Top = value.Y;
            }
        }

        public Vector2 TopCenter
        {
            get => new Vector2(this.CenterX, this.Top);
            set
            {
                this.CenterX = value.X;
                this.Top = value.Y;
            }
        }

        public Vector2 TopRight
        {
            get => new Vector2(this.Right, this.Top);
            set
            {
                this.Right = value.X;
                this.Top = value.Y;
            }
        }

        public Vector2 CenterLeft
        {
            get => new Vector2(this.Left, this.CenterY);
            set
            {
                this.Left = value.X;
                this.CenterY = value.Y;
            }
        }

        public Vector2 Center
        {
            get => new Vector2(this.CenterX, this.CenterY);
            set
            {
                this.CenterX = value.X;
                this.CenterY = value.Y;
            }
        }

        public Vector2 Size => new Vector2(this.Width, this.Height);

        public Vector2 HalfSize => this.Size * 0.5f;

        public Vector2 CenterRight
        {
            get => new Vector2(this.Right, this.CenterY);
            set
            {
                this.Right = value.X;
                this.CenterY = value.Y;
            }
        }

        public Vector2 BottomLeft
        {
            get => new Vector2(this.Left, this.Bottom);
            set
            {
                this.Left = value.X;
                this.Bottom = value.Y;
            }
        }

        public Vector2 BottomCenter
        {
            get => new Vector2(this.CenterX, this.Bottom);
            set
            {
                this.CenterX = value.X;
                this.Bottom = value.Y;
            }
        }

        public Vector2 BottomRight
        {
            get => new Vector2(this.Right, this.Bottom);
            set
            {
                this.Right = value.X;
                this.Bottom = value.Y;
            }
        }

        public void Render(Camera camera) => this.Render(camera, Color.Red);

        public Vector2 AbsolutePosition => this.Entity != null ? this.Entity.Position + this.Position : this.Position;

        public float AbsoluteX => this.Entity != null ? this.Entity.Position.X + this.Position.X : this.Position.X;

        public float AbsoluteY => this.Entity != null ? this.Entity.Position.Y + this.Position.Y : this.Position.Y;

        public float AbsoluteTop => this.Entity != null ? this.Top + this.Entity.Position.Y : this.Top;

        public float AbsoluteBottom => this.Entity != null ? this.Bottom + this.Entity.Position.Y : this.Bottom;

        public float AbsoluteLeft => this.Entity != null ? this.Left + this.Entity.Position.X : this.Left;

        public float AbsoluteRight => this.Entity != null ? this.Right + this.Entity.Position.X : this.Right;

        public Rectangle Bounds => new Rectangle((int) this.AbsoluteLeft, (int) this.AbsoluteTop, (int) this.Width, (int) this.Height);
    }
}
