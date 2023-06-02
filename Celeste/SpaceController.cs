using Monocle;

namespace Celeste
{
    public class SpaceController : Entity
    {
        private Level level;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.level = this.SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            Player entity = this.Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            if ((double) entity.Top > (double) this.level.Camera.Bottom + 12.0)
            {
                entity.Bottom = this.level.Camera.Top - 4f;
            }
            else
            {
                if ((double) entity.Bottom >= (double) this.level.Camera.Top - 4.0)
                    return;
                entity.Top = this.level.Camera.Bottom + 12f;
            }
        }
    }
}
