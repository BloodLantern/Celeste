// Decompiled with JetBrains decompiler
// Type: Monocle.Collider
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System;

namespace Monocle
{
    public abstract class Collider
    {
        public Vector2 Position;

        public Entity Entity { get; private set; }

        internal virtual void Added(Entity entity)
        {
            Entity = entity;
        }

        internal virtual void Removed()
        {
            Entity = null;
        }

        public bool Collide(Entity entity)
        {
            return Collide(entity.Collider);
        }

        public bool Collide(Collider collider)
        {
            return collider switch
            {
                Hitbox _ => Collide(collider as Hitbox),
                Grid _ => Collide(collider as Grid),
                ColliderList _ => Collide(collider as ColliderList),
                Circle _ => Collide(collider as Circle),
                _ => throw new Exception("Collisions against the collider type are not implemented!"),
            };
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
            Position.X = (float)(-(double)Width / 2.0);
            Position.Y = (float)(-(double)Height / 2.0);
        }

        public float CenterX
        {
            get => Left + (Width / 2f);
            set => Left = value - (Width / 2f);
        }

        public float CenterY
        {
            get => Top + (Height / 2f);
            set => Top = value - (Height / 2f);
        }

        public Vector2 TopLeft
        {
            get => new(Left, Top);
            set
            {
                Left = value.X;
                Top = value.Y;
            }
        }

        public Vector2 TopCenter
        {
            get => new(CenterX, Top);
            set
            {
                CenterX = value.X;
                Top = value.Y;
            }
        }

        public Vector2 TopRight
        {
            get => new(Right, Top);
            set
            {
                Right = value.X;
                Top = value.Y;
            }
        }

        public Vector2 CenterLeft
        {
            get => new(Left, CenterY);
            set
            {
                Left = value.X;
                CenterY = value.Y;
            }
        }

        public Vector2 Center
        {
            get => new(CenterX, CenterY);
            set
            {
                CenterX = value.X;
                CenterY = value.Y;
            }
        }

        public Vector2 Size => new(Width, Height);

        public Vector2 HalfSize => Size * 0.5f;

        public Vector2 CenterRight
        {
            get => new(Right, CenterY);
            set
            {
                Right = value.X;
                CenterY = value.Y;
            }
        }

        public Vector2 BottomLeft
        {
            get => new(Left, Bottom);
            set
            {
                Left = value.X;
                Bottom = value.Y;
            }
        }

        public Vector2 BottomCenter
        {
            get => new(CenterX, Bottom);
            set
            {
                CenterX = value.X;
                Bottom = value.Y;
            }
        }

        public Vector2 BottomRight
        {
            get => new(Right, Bottom);
            set
            {
                Right = value.X;
                Bottom = value.Y;
            }
        }

        public void Render(Camera camera)
        {
            Render(camera, Color.Red);
        }

        public Vector2 AbsolutePosition => Entity != null ? Entity.Position + Position : Position;

        public float AbsoluteX => Entity != null ? Entity.Position.X + Position.X : Position.X;

        public float AbsoluteY => Entity != null ? Entity.Position.Y + Position.Y : Position.Y;

        public float AbsoluteTop => Entity != null ? Top + Entity.Position.Y : Top;

        public float AbsoluteBottom => Entity != null ? Bottom + Entity.Position.Y : Bottom;

        public float AbsoluteLeft => Entity != null ? Left + Entity.Position.X : Left;

        public float AbsoluteRight => Entity != null ? Right + Entity.Position.X : Right;

        public Rectangle Bounds => new((int)AbsoluteLeft, (int)AbsoluteTop, (int)Width, (int)Height);
    }
}
