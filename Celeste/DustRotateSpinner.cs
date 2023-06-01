// Decompiled with JetBrains decompiler
// Type: Celeste.DustRotateSpinner
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
            this.Add((Component) (this.dusty = new DustGraphic(true)));
        }

        public override void Update()
        {
            base.Update();
            if (!this.Moving)
                return;
            DustGraphic dusty1 = this.dusty;
            DustGraphic dusty2 = this.dusty;
            double angle = (double) this.Angle;
            double num = 1.5707963705062866 * (this.Clockwise ? 1.0 : -1.0);
            Vector2 vector;
            Vector2 vector2_1 = vector = Calc.AngleToVector((float) (angle + num), 1f);
            dusty2.EyeTargetDirection = vector;
            Vector2 vector2_2 = vector2_1;
            dusty1.EyeDirection = vector2_2;
            if (!this.Scene.OnInterval(0.02f))
                return;
            this.SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, this.Position, Vector2.One * 4f);
        }

        public override void OnPlayer(Player player)
        {
            base.OnPlayer(player);
            this.dusty.OnHitPlayer();
        }
    }
}
