using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked()]
    public class WaterInteraction : Component
    {
        public static ParticleType P_Drip;
        public Func<bool> IsDashing;
        public float DrippingTimer;
        public float DrippingOffset;

        public WaterInteraction(Func<bool> isDashing)
            : base(false, false)
        {
            IsDashing = isDashing;
        }

        public override void Update()
        {
            if (DrippingTimer <= 0.0)
                return;
            DrippingTimer -= Engine.DeltaTime;
            if (!Scene.OnInterval(0.1f))
                return;
            float x = Entity.Left - 2f + Calc.Random.NextFloat(Entity.Width + 4f);
            float y = Entity.Top + DrippingOffset + Calc.Random.NextFloat(Entity.Height - DrippingOffset);
            (Scene as Level).ParticlesFG.Emit(WaterInteraction.P_Drip, new Vector2(x, y));
        }
    }
}
