using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class RotateSpinner : Entity
    {
        private const float RotationTime = 1.8f;
        public bool Moving = true;
        private Vector2 center;
        private float rotationPercent;
        private float length;
        private bool fallOutOfScreen;

        public float Angle => MathHelper.Lerp(4.712389f, -1.57079637f, this.Easer(this.rotationPercent));

        public bool Clockwise { get; private set; }

        public RotateSpinner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.Depth = -50;
            this.center = data.Nodes[0] + offset;
            this.Clockwise = data.Bool("clockwise");
            this.Collider = (Collider) new Monocle.Circle(6f);
            this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
            this.Add((Component) new StaticMover()
            {
                SolidChecker = (Func<Solid, bool>) (s => s.CollidePoint(this.center)),
                JumpThruChecker = (Func<JumpThru, bool>) (jt => jt.CollidePoint(this.center)),
                OnMove = (Action<Vector2>) (v =>
                {
                    this.center += v;
                    this.Position = this.Position + v;
                }),
                OnDestroy = (Action) (() => this.fallOutOfScreen = true)
            });
            this.rotationPercent = this.EaserInverse(Calc.Percent(Calc.WrapAngle(Calc.Angle(this.center, this.Position)), -1.57079637f, 4.712389f));
            this.length = (this.Position - this.center).Length();
            this.Position = this.center + Calc.AngleToVector(this.Angle, this.length);
        }

        private float Easer(float v) => v;

        private float EaserInverse(float v) => v;

        public override void Update()
        {
            base.Update();
            if (this.Moving)
            {
                if (this.Clockwise)
                {
                    this.rotationPercent -= Engine.DeltaTime / 1.8f;
                    ++this.rotationPercent;
                }
                else
                    this.rotationPercent += Engine.DeltaTime / 1.8f;
                this.rotationPercent %= 1f;
                this.Position = this.center + Calc.AngleToVector(this.Angle, this.length);
            }
            if (!this.fallOutOfScreen)
                return;
            this.center.Y += 160f * Engine.DeltaTime;
            if ((double) this.Y <= (double) ((this.Scene as Level).Bounds.Bottom + 32))
                return;
            this.RemoveSelf();
        }

        public virtual void OnPlayer(Player player)
        {
            if (player.Die((player.Position - this.Position).SafeNormalize()) == null)
                return;
            this.Moving = false;
        }
    }
}
