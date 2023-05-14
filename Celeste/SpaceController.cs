// Decompiled with JetBrains decompiler
// Type: Celeste.SpaceController
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
            {
                return;
            }

            if ((double)entity.Top > (double)level.Camera.Bottom + 12.0)
            {
                entity.Bottom = level.Camera.Top - 4f;
            }
            else
            {
                if ((double)entity.Bottom >= (double)level.Camera.Top - 4.0)
                {
                    return;
                }

                entity.Top = level.Camera.Bottom + 12f;
            }
        }
    }
}
