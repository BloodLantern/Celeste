using Monocle;

namespace Celeste
{
    public class SpaceController : Entity
    {
        private Level level;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            if (entity.Top > level.Camera.Bottom + 12.0)
            {
                entity.Bottom = level.Camera.Top - 4f;
            }
            else
            {
                if (entity.Bottom >= level.Camera.Top - 4.0)
                    return;
                entity.Top = level.Camera.Bottom + 12f;
            }
        }
    }
}
