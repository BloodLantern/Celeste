// Decompiled with JetBrains decompiler
// Type: Monocle.Component
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace Monocle
{
    public class Component
    {
        public bool Active;
        public bool Visible;

        public Entity Entity { get; private set; }

        public Component(bool active, bool visible)
        {
            Active = active;
            Visible = visible;
        }

        public virtual void Added(Entity entity)
        {
            Entity = entity;
            if (Scene == null)
            {
                return;
            }

            Scene.Tracker.ComponentAdded(this);
        }

        public virtual void Removed(Entity entity)
        {
            Scene?.Tracker.ComponentRemoved(this);
            Entity = null;
        }

        public virtual void EntityAdded(Scene scene)
        {
            scene.Tracker.ComponentAdded(this);
        }

        public virtual void EntityRemoved(Scene scene)
        {
            scene.Tracker.ComponentRemoved(this);
        }

        public virtual void SceneEnd(Scene scene)
        {
        }

        public virtual void EntityAwake()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Render()
        {
        }

        public virtual void DebugRender(Camera camera)
        {
        }

        public virtual void HandleGraphicsReset()
        {
        }

        public virtual void HandleGraphicsCreate()
        {
        }

        public void RemoveSelf()
        {
            if (Entity == null)
            {
                return;
            }

            Entity.Remove(this);
        }

        public T SceneAs<T>() where T : Scene
        {
            return Scene as T;
        }

        public T EntityAs<T>() where T : Entity
        {
            return Entity as T;
        }

        public Scene Scene => Entity?.Scene;
    }
}
