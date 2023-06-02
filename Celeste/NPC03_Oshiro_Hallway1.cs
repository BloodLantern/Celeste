using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC03_Oshiro_Hallway1 : NPC
    {
        private bool talked;

        public NPC03_Oshiro_Hallway1(Vector2 position)
            : base(position)
        {
            this.Add((Component) (this.Sprite = (Sprite) new OshiroSprite(-1)));
            this.Add((Component) (this.Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64)));
            this.MoveAnim = "move";
            this.IdleAnim = "idle";
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!this.Session.GetFlag("oshiro_resort_talked_2"))
                return;
            this.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            Player entity = this.Scene.Tracker.GetEntity<Player>();
            if (this.talked || entity == null || (double) entity.X <= (double) this.X - 60.0)
                return;
            this.Scene.Add((Entity) new CS03_OshiroHallway1(entity, (NPC) this));
            this.talked = true;
        }
    }
}
