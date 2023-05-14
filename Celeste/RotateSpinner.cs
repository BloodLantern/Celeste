// Decompiled with JetBrains decompiler
// Type: Celeste.RotateSpinner
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
        private readonly float length;
        private bool fallOutOfScreen;

        public float Angle => MathHelper.Lerp(4.712389f, -1.57079637f, Easer(rotationPercent));

        public bool Clockwise { get; private set; }

        public RotateSpinner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = -50;
            center = data.Nodes[0] + offset;
            Clockwise = data.Bool("clockwise");
            Collider = new Monocle.Circle(6f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(new StaticMover()
            {
                SolidChecker = s => s.CollidePoint(center),
                JumpThruChecker = jt => jt.CollidePoint(center),
                OnMove = v =>
                {
                    center += v;
                    Position += v;
                },
                OnDestroy = () => fallOutOfScreen = true
            });
            rotationPercent = EaserInverse(Calc.Percent(Calc.WrapAngle(Calc.Angle(center, Position)), -1.57079637f, 4.712389f));
            length = (Position - center).Length();
            Position = center + Calc.AngleToVector(Angle, length);
        }

        private float Easer(float v)
        {
            return v;
        }

        private float EaserInverse(float v)
        {
            return v;
        }

        public override void Update()
        {
            base.Update();
            if (Moving)
            {
                if (Clockwise)
                {
                    rotationPercent -= Engine.DeltaTime / 1.8f;
                    ++rotationPercent;
                }
                else
                {
                    rotationPercent += Engine.DeltaTime / 1.8f;
                }

                rotationPercent %= 1f;
                Position = center + Calc.AngleToVector(Angle, length);
            }
            if (!fallOutOfScreen)
            {
                return;
            }

            center.Y += 160f * Engine.DeltaTime;
            if ((double)Y <= (Scene as Level).Bounds.Bottom + 32)
            {
                return;
            }

            RemoveSelf();
        }

        public virtual void OnPlayer(Player player)
        {
            if (player.Die((player.Position - Position).SafeNormalize()) == null)
            {
                return;
            }

            Moving = false;
        }
    }
}
