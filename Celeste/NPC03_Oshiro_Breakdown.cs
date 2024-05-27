using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class NPC03_Oshiro_Breakdown : NPC
    {
        private bool talked;

        public NPC03_Oshiro_Breakdown(Vector2 position)
            : base(position)
        {
            Add(Sprite = new OshiroSprite(1));
            Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
            MoveAnim = "move";
            IdleAnim = "idle";
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Session.GetFlag("oshiro_breakdown"))
                return;
            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (talked || entity == null)
                return;
            double x1 = entity.X;
            Rectangle bounds = Level.Bounds;
            double num1 = bounds.Left + 370;
            if (x1 <= num1 && entity.OnSafeGround)
            {
                double y1 = entity.Y;
                bounds = Level.Bounds;
                double y2 = bounds.Center.Y;
                if (y1 < y2)
                    goto label_4;
            }
            double x2 = entity.X;
            bounds = Level.Bounds;
            double num2 = bounds.Left + 320;
            if (x2 > num2)
                return;
label_4:
            Scene.Add(new CS03_OshiroBreakdown(entity, this));
            talked = true;
        }
    }
}
