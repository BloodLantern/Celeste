using Microsoft.Xna.Framework;

namespace Monocle
{
    public class SpecEntity<T> : Entity where T : Scene
    {
        public T SpecScene { get; private set; }

        public SpecEntity(Vector2 position)
            : base(position)
        {
        }

        public SpecEntity()
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!(this.Scene is T))
                return;
            this.SpecScene = this.Scene as T;
        }

        public override void Removed(Scene scene)
        {
            this.SpecScene = default (T);
            base.Removed(scene);
        }
    }
}
