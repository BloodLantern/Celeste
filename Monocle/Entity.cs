using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Monocle
{
    public class Entity : IEnumerable<Component>, IEnumerable
    {
        public bool Active = true;
        public bool Visible = true;
        public bool Collidable = true;
        public Vector2 Position;
        private int tag;
        private Collider collider;
        internal int depth;
        internal double actualDepth;

        public Scene Scene { get; private set; }

        public ComponentList Components { get; private set; }

        public Entity(Vector2 position)
        {
            Position = position;
            Components = new ComponentList(this);
        }

        public Entity()
            : this(Vector2.Zero)
        {
        }

        public virtual void SceneBegin(Scene scene)
        {
        }

        public virtual void SceneEnd(Scene scene)
        {
            if (Components == null)
                return;
            foreach (Component component in Components)
                component.SceneEnd(scene);
        }

        public virtual void Awake(Scene scene)
        {
            if (Components == null)
                return;
            foreach (Component component in Components)
                component.EntityAwake();
        }

        public virtual void Added(Scene scene)
        {
            Scene = scene;
            if (Components != null)
            {
                foreach (Component component in Components)
                    component.EntityAdded(scene);
            }
            Scene.SetActualDepth(this);
        }

        public virtual void Removed(Scene scene)
        {
            if (Components != null)
            {
                foreach (Component component in Components)
                    component.EntityRemoved(scene);
            }
            Scene = null;
        }

        public virtual void Update() => Components.Update();

        public virtual void Render() => Components.Render();

        public virtual void DebugRender(Camera camera)
        {
            Collider?.Render(camera, Collidable ? Color.Red : Color.DarkRed);
            Components.DebugRender(camera);
        }

        public virtual void HandleGraphicsReset() => Components.HandleGraphicsReset();

        public virtual void HandleGraphicsCreate() => Components.HandleGraphicsCreate();

        public void RemoveSelf()
        {
            if (Scene == null)
                return;
            Scene.Entities.Remove(this);
        }

        public int Depth
        {
            get => depth;
            set
            {
                if (depth == value)
                    return;
                depth = value;
                if (Scene == null)
                    return;
                Scene.SetActualDepth(this);
            }
        }

        public float X
        {
            get => Position.X;
            set => Position.X = value;
        }

        public float Y
        {
            get => Position.Y;
            set => Position.Y = value;
        }

        public Collider Collider
        {
            get => collider;
            set
            {
                if (value == collider)
                    return;
                collider?.Removed();
                collider = value;
                if (collider == null)
                    return;
                collider.Added(this);
            }
        }

        public float Width => Collider == null ? 0.0f : Collider.Width;

        public float Height => Collider == null ? 0.0f : Collider.Height;

        public float Left
        {
            get => Collider == null ? X : Position.X + Collider.Left;
            set
            {
                if (Collider == null)
                    Position.X = value;
                else
                    Position.X = value - Collider.Left;
            }
        }

        public float Right
        {
            get => Collider == null ? Position.X : Position.X + Collider.Right;
            set
            {
                if (Collider == null)
                    Position.X = value;
                else
                    Position.X = value - Collider.Right;
            }
        }

        public float Top
        {
            get => Collider == null ? Position.Y : Position.Y + Collider.Top;
            set
            {
                if (Collider == null)
                    Position.Y = value;
                else
                    Position.Y = value - Collider.Top;
            }
        }

        public float Bottom
        {
            get => Collider == null ? Position.Y : Position.Y + Collider.Bottom;
            set
            {
                if (Collider == null)
                    Position.Y = value;
                else
                    Position.Y = value - Collider.Bottom;
            }
        }

        public float CenterX
        {
            get => Collider == null ? Position.X : Position.X + Collider.CenterX;
            set
            {
                if (Collider == null)
                    Position.X = value;
                else
                    Position.X = value - Collider.CenterX;
            }
        }

        public float CenterY
        {
            get => Collider == null ? Position.Y : Position.Y + Collider.CenterY;
            set
            {
                if (Collider == null)
                    Position.Y = value;
                else
                    Position.Y = value - Collider.CenterY;
            }
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

        public Vector2 TopRight
        {
            get => new(Right, Top);
            set
            {
                Right = value.X;
                Top = value.Y;
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

        public Vector2 BottomRight
        {
            get => new(Right, Bottom);
            set
            {
                Right = value.X;
                Bottom = value.Y;
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

        public Vector2 CenterLeft
        {
            get => new(Left, CenterY);
            set
            {
                Left = value.X;
                CenterY = value.Y;
            }
        }

        public Vector2 CenterRight
        {
            get => new(Right, CenterY);
            set
            {
                Right = value.X;
                CenterY = value.Y;
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

        public Vector2 BottomCenter
        {
            get => new(CenterX, Bottom);
            set
            {
                CenterX = value.X;
                Bottom = value.Y;
            }
        }

        public int Tag
        {
            get => tag;
            set
            {
                if (tag == value)
                    return;
                if (Scene != null)
                {
                    for (int index = 0; index < BitTag.TotalTags; ++index)
                    {
                        int num = 1 << index;
                        bool flag = (value & num) != 0;
                        if ((Tag & num) != 0 != flag)
                        {
                            if (flag)
                                Scene.TagLists[index].Add(this);
                            else
                                Scene.TagLists[index].Remove(this);
                        }
                    }
                }
                tag = value;
            }
        }

        public bool TagFullCheck(int tag) => (this.tag & tag) == tag;

        public bool TagCheck(int tag) => (this.tag & tag) != 0;

        public void AddTag(int tag) => Tag |= tag;

        public void RemoveTag(int tag) => Tag &= ~tag;

        public bool CollideCheck(Entity other) => Collide.Check(this, other);

        public bool CollideCheck(Entity other, Vector2 at) => Collide.Check(this, other, at);

        public bool CollideCheck(BitTag tag) => Collide.Check(this, Scene[tag]);

        public bool CollideCheck(BitTag tag, Vector2 at) => Collide.Check(this, Scene[tag], at);

        public bool CollideCheck<T>() where T : Entity => Collide.Check(this, Scene.Tracker.Entities[typeof(T)]);

        public bool CollideCheck<T>(Vector2 at) where T : Entity => Collide.Check(this, Scene.Tracker.Entities[typeof(T)], at);

        public bool CollideCheck<T, Exclude>()
            where T : Entity
            where Exclude : Entity
        {
            List<Entity> entity = Scene.Tracker.Entities[typeof (Exclude)];
            foreach (Entity b in Scene.Tracker.Entities[typeof (T)])
            {
                if (!entity.Contains(b) && Collide.Check(this, b))
                    return true;
            }
            return false;
        }

        public bool CollideCheck<T, Exclude>(Vector2 at)
            where T : Entity
            where Exclude : Entity
        {
            Vector2 position = Position;
            Position = at;
            int num = CollideCheck<T, Exclude>() ? 1 : 0;
            Position = position;
            return num != 0;
        }

        public bool CollideCheck<T, Exclude1, Exclude2>()
            where T : Entity
            where Exclude1 : Entity
            where Exclude2 : Entity
        {
            List<Entity> entity1 = Scene.Tracker.Entities[typeof (Exclude1)];
            List<Entity> entity2 = Scene.Tracker.Entities[typeof (Exclude2)];
            foreach (Entity b in Scene.Tracker.Entities[typeof (T)])
            {
                if (!entity1.Contains(b) && !entity2.Contains(b) && Collide.Check(this, b))
                    return true;
            }
            return false;
        }

        public bool CollideCheck<T, Exclude1, Exclude2>(Vector2 at)
            where T : Entity
            where Exclude1 : Entity
            where Exclude2 : Entity
        {
            Vector2 position = Position;
            Position = at;
            int num = CollideCheck<T, Exclude1, Exclude2>() ? 1 : 0;
            Position = position;
            return num != 0;
        }

        public bool CollideCheckByComponent<T>() where T : Component
        {
            foreach (Component component in Scene.Tracker.Components[typeof (T)])
            {
                if (Collide.Check(this, component.Entity))
                    return true;
            }
            return false;
        }

        public bool CollideCheckByComponent<T>(Vector2 at) where T : Component
        {
            Vector2 position = Position;
            Position = at;
            int num = CollideCheckByComponent<T>() ? 1 : 0;
            Position = position;
            return num != 0;
        }

        public bool CollideCheckOutside(Entity other, Vector2 at) => !Collide.Check(this, other) && Collide.Check(this, other, at);

        public bool CollideCheckOutside(BitTag tag, Vector2 at)
        {
            foreach (Entity b in Scene[tag])
            {
                if (!Collide.Check(this, b) && Collide.Check(this, b, at))
                    return true;
            }
            return false;
        }

        public bool CollideCheckOutside<T>(Vector2 at) where T : Entity
        {
            foreach (Entity b in Scene.Tracker.Entities[typeof (T)])
            {
                if (!Collide.Check(this, b) && Collide.Check(this, b, at))
                    return true;
            }
            return false;
        }

        public bool CollideCheckOutsideByComponent<T>(Vector2 at) where T : Component
        {
            foreach (Component component in Scene.Tracker.Components[typeof (T)])
            {
                if (!Collide.Check(this, component.Entity) && Collide.Check(this, component.Entity, at))
                    return true;
            }
            return false;
        }

        public Entity CollideFirst(BitTag tag) => Collide.First(this, Scene[tag]);

        public Entity CollideFirst(BitTag tag, Vector2 at) => Collide.First(this, Scene[tag], at);

        public T CollideFirst<T>() where T : Entity => Collide.First(this, Scene.Tracker.Entities[typeof(T)]) as T;

        public T CollideFirst<T>(Vector2 at) where T : Entity => Collide.First(this, Scene.Tracker.Entities[typeof(T)], at) as T;

        public T CollideFirstByComponent<T>() where T : Component
        {
            foreach (Component component in Scene.Tracker.Components[typeof (T)])
            {
                if (Collide.Check(this, component.Entity))
                    return component as T;
            }
            return default (T);
        }

        public T CollideFirstByComponent<T>(Vector2 at) where T : Component
        {
            foreach (Component component in Scene.Tracker.Components[typeof (T)])
            {
                if (Collide.Check(this, component.Entity, at))
                    return component as T;
            }
            return default (T);
        }

        public Entity CollideFirstOutside(BitTag tag, Vector2 at)
        {
            foreach (Entity b in Scene[tag])
            {
                if (!Collide.Check(this, b) && Collide.Check(this, b, at))
                    return b;
            }
            return null;
        }

        public T CollideFirstOutside<T>(Vector2 at) where T : Entity
        {
            foreach (Entity b in Scene.Tracker.Entities[typeof (T)])
            {
                if (!Collide.Check(this, b) && Collide.Check(this, b, at))
                    return b as T;
            }
            return default (T);
        }

        public T CollideFirstOutsideByComponent<T>(Vector2 at) where T : Component
        {
            foreach (Component component in Scene.Tracker.Components[typeof (T)])
            {
                if (!Collide.Check(this, component.Entity) && Collide.Check(this, component.Entity, at))
                    return component as T;
            }
            return default (T);
        }

        public List<Entity> CollideAll(BitTag tag) => Collide.All(this, Scene[tag]);

        public List<Entity> CollideAll(BitTag tag, Vector2 at) => Collide.All(this, Scene[tag], at);

        public List<Entity> CollideAll<T>() where T : Entity => Collide.All(this, Scene.Tracker.Entities[typeof(T)]);

        public List<Entity> CollideAll<T>(Vector2 at) where T : Entity => Collide.All(this, Scene.Tracker.Entities[typeof(T)], at);

        public List<Entity> CollideAll<T>(Vector2 at, List<Entity> into) where T : Entity
        {
            into.Clear();
            return Collide.All(this, Scene.Tracker.Entities[typeof(T)], into, at);
        }

        public List<T> CollideAllByComponent<T>() where T : Component
        {
            List<T> objList = new();
            foreach (Component component in Scene.Tracker.Components[typeof (T)])
            {
                if (Collide.Check(this, component.Entity))
                    objList.Add(component as T);
            }
            return objList;
        }

        public List<T> CollideAllByComponent<T>(Vector2 at) where T : Component
        {
            Vector2 position = Position;
            Position = at;
            List<T> objList = CollideAllByComponent<T>();
            Position = position;
            return objList;
        }

        public bool CollideDo(BitTag tag, Action<Entity> action)
        {
            bool flag = false;
            foreach (Entity other in Scene[tag])
            {
                if (CollideCheck(other))
                {
                    action(other);
                    flag = true;
                }
            }
            return flag;
        }

        public bool CollideDo(BitTag tag, Action<Entity> action, Vector2 at)
        {
            bool flag = false;
            Vector2 position = Position;
            Position = at;
            foreach (Entity other in Scene[tag])
            {
                if (CollideCheck(other))
                {
                    action(other);
                    flag = true;
                }
            }
            Position = position;
            return flag;
        }

        public bool CollideDo<T>(Action<T> action) where T : Entity
        {
            bool flag = false;
            foreach (Entity other in Scene.Tracker.Entities[typeof (T)])
            {
                if (CollideCheck(other))
                {
                    action(other as T);
                    flag = true;
                }
            }
            return flag;
        }

        public bool CollideDo<T>(Action<T> action, Vector2 at) where T : Entity
        {
            bool flag = false;
            Vector2 position = Position;
            Position = at;
            foreach (Entity other in Scene.Tracker.Entities[typeof (T)])
            {
                if (CollideCheck(other))
                {
                    action(other as T);
                    flag = true;
                }
            }
            Position = position;
            return flag;
        }

        public bool CollideDoByComponent<T>(Action<T> action) where T : Component
        {
            bool flag = false;
            foreach (Component component in Scene.Tracker.Components[typeof (T)])
            {
                if (CollideCheck(component.Entity))
                {
                    action(component as T);
                    flag = true;
                }
            }
            return flag;
        }

        public bool CollideDoByComponent<T>(Action<T> action, Vector2 at) where T : Component
        {
            bool flag = false;
            Vector2 position = Position;
            Position = at;
            foreach (Component component in Scene.Tracker.Components[typeof (T)])
            {
                if (CollideCheck(component.Entity))
                {
                    action(component as T);
                    flag = true;
                }
            }
            Position = position;
            return flag;
        }

        public bool CollidePoint(Vector2 point) => Collide.CheckPoint(this, point);

        public bool CollidePoint(Vector2 point, Vector2 at) => Collide.CheckPoint(this, point, at);

        public bool CollideLine(Vector2 from, Vector2 to) => Collide.CheckLine(this, from, to);

        public bool CollideLine(Vector2 from, Vector2 to, Vector2 at) => Collide.CheckLine(this, from, to, at);

        public bool CollideRect(Rectangle rect) => Collide.CheckRect(this, rect);

        public bool CollideRect(Rectangle rect, Vector2 at) => Collide.CheckRect(this, rect, at);

        public void Add(Component component) => Components.Add(component);

        public void Remove(Component component) => Components.Remove(component);

        public void Add(params Component[] components) => Components.Add(components);

        public void Remove(params Component[] components) => Components.Remove(components);

        public T Get<T>() where T : Component => Components.Get<T>();

        public IEnumerator<Component> GetEnumerator() => Components.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Entity Closest(params Entity[] entities)
        {
            Entity entity = entities[0];
            float num1 = Vector2.DistanceSquared(Position, entity.Position);
            for (int index = 1; index < entities.Length; ++index)
            {
                float num2 = Vector2.DistanceSquared(Position, entities[index].Position);
                if ((double) num2 < (double) num1)
                {
                    entity = entities[index];
                    num1 = num2;
                }
            }
            return entity;
        }

        public Entity Closest(BitTag tag)
        {
            List<Entity> entityList = Scene[tag];
            Entity entity = null;
            if (entityList.Count >= 1)
            {
                entity = entityList[0];
                float num1 = Vector2.DistanceSquared(Position, entity.Position);
                for (int index = 1; index < entityList.Count; ++index)
                {
                    float num2 = Vector2.DistanceSquared(Position, entityList[index].Position);
                    if ((double) num2 < (double) num1)
                    {
                        entity = entityList[index];
                        num1 = num2;
                    }
                }
            }
            return entity;
        }

        public T SceneAs<T>() where T : Scene => Scene as T;
    }
}
