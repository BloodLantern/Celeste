using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class DustRotateSpinner : RotateSpinner
    {
        private DustGraphic dusty;

        public DustRotateSpinner(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Add(dusty = new DustGraphic(true));
        }

        public override void Update()
        {
            base.Update();
            if (!Moving)
                return;
            DustGraphic dusty1 = dusty;
            DustGraphic dusty2 = dusty;
            double angle = Angle;
            double num = 1.5707963705062866 * (Clockwise ? 1.0 : -1.0);
            Vector2 vector;
            Vector2 vector2_1 = vector = Calc.AngleToVector((float) (angle + num), 1f);
            dusty2.EyeTargetDirection = vector;
            Vector2 vector2_2 = vector2_1;
            dusty1.EyeDirection = vector2_2;
            if (!Scene.OnInterval(0.02f))
                return;
            SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, Position, Vector2.One * 4f);
        }

        public override void OnPlayer(Player player)
        {
            base.OnPlayer(player);
            dusty.OnHitPlayer();
        }
    }
}
