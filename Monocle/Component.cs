namespace Monocle
{
    public class Component
    {
        public bool Active;
        public bool Visible;

        public Entity Entity { get; private set; }

        public Component(bool active, bool visible)
        {
            this.Active = active;
            this.Visible = visible;
        }

        public virtual void Added(Entity entity)
        {
            this.Entity = entity;
            if (this.Scene == null)
                return;
            this.Scene.Tracker.ComponentAdded(this);
        }

        public virtual void Removed(Entity entity)
        {
            if (this.Scene != null)
                this.Scene.Tracker.ComponentRemoved(this);
            this.Entity = (Entity) null;
        }

        public virtual void EntityAdded(Scene scene) => scene.Tracker.ComponentAdded(this);

        public virtual void EntityRemoved(Scene scene) => scene.Tracker.ComponentRemoved(this);

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
            if (this.Entity == null)
                return;
            this.Entity.Remove(this);
        }

        public T SceneAs<T>() where T : Scene => this.Scene as T;

        public T EntityAs<T>() where T : Entity => this.Entity as T;

        public Scene Scene => this.Entity == null ? (Scene) null : this.Entity.Scene;
    }
}
