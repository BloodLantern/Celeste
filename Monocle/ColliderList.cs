using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Monocle
{
    public class ColliderList : Collider
    {
        public Collider[] colliders { get; private set; }

        public ColliderList(params Collider[] colliders) => this.colliders = colliders;

        public void Add(params Collider[] toAdd)
        {
            Collider[] colliderArray = new Collider[colliders.Length + toAdd.Length];
            for (int index = 0; index < colliders.Length; ++index)
                colliderArray[index] = colliders[index];
            for (int index = 0; index < toAdd.Length; ++index)
            {
                colliderArray[index + colliders.Length] = toAdd[index];
                toAdd[index].Added(Entity);
            }
            colliders = colliderArray;
        }

        public void Remove(params Collider[] toRemove)
        {
            Collider[] colliderArray = new Collider[colliders.Length - toRemove.Length];
            int index = 0;
            foreach (Collider collider in colliders)
            {
                if (!toRemove.Contains(collider))
                {
                    colliderArray[index] = collider;
                    ++index;
                }
            }
            colliders = colliderArray;
        }

        internal override void Added(Entity entity)
        {
            base.Added(entity);
            foreach (Collider collider in colliders)
                collider.Added(entity);
        }

        internal override void Removed()
        {
            base.Removed();
            foreach (Collider collider in colliders)
                collider.Removed();
        }

        public override float Width
        {
            get => Right - Left;
            set => throw new NotImplementedException();
        }

        public override float Height
        {
            get => Bottom - Top;
            set => throw new NotImplementedException();
        }

        public override float Left
        {
            get
            {
                float left = colliders[0].Left;
                for (int index = 1; index < colliders.Length; ++index)
                {
                    if (colliders[index].Left < (double) left)
                        left = colliders[index].Left;
                }
                return left;
            }
            set
            {
                float num = value - Left;
                foreach (Collider collider in colliders)
                    Position.X += num;
            }
        }

        public override float Right
        {
            get
            {
                float right = colliders[0].Right;
                for (int index = 1; index < colliders.Length; ++index)
                {
                    if (colliders[index].Right > (double) right)
                        right = colliders[index].Right;
                }
                return right;
            }
            set
            {
                float num = value - Right;
                foreach (Collider collider in colliders)
                    Position.X += num;
            }
        }

        public override float Top
        {
            get
            {
                float top = colliders[0].Top;
                for (int index = 1; index < colliders.Length; ++index)
                {
                    if (colliders[index].Top < (double) top)
                        top = colliders[index].Top;
                }
                return top;
            }
            set
            {
                float num = value - Top;
                foreach (Collider collider in colliders)
                    Position.Y += num;
            }
        }

        public override float Bottom
        {
            get
            {
                float bottom = colliders[0].Bottom;
                for (int index = 1; index < colliders.Length; ++index)
                {
                    if (colliders[index].Bottom > (double) bottom)
                        bottom = colliders[index].Bottom;
                }
                return bottom;
            }
            set
            {
                float num = value - Bottom;
                foreach (Collider collider in colliders)
                    Position.Y += num;
            }
        }

        public override Collider Clone()
        {
            Collider[] colliderArray = new Collider[colliders.Length];
            for (int index = 0; index < colliders.Length; ++index)
                colliderArray[index] = colliders[index].Clone();
            return new ColliderList(colliderArray);
        }

        public override void Render(Camera camera, Color color)
        {
            foreach (Collider collider in colliders)
                collider.Render(camera, color);
        }

        public override bool Collide(Vector2 point)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.Collide(point))
                    return true;
            }
            return false;
        }

        public override bool Collide(Rectangle rect)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.Collide(rect))
                    return true;
            }
            return false;
        }

        public override bool Collide(Vector2 from, Vector2 to)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.Collide(from, to))
                    return true;
            }
            return false;
        }

        public override bool Collide(Hitbox hitbox)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.Collide(hitbox))
                    return true;
            }
            return false;
        }

        public override bool Collide(Grid grid)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.Collide(grid))
                    return true;
            }
            return false;
        }

        public override bool Collide(Circle circle)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.Collide(circle))
                    return true;
            }
            return false;
        }

        public override bool Collide(ColliderList list)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.Collide(list))
                    return true;
            }
            return false;
        }
    }
}
