﻿// Decompiled with JetBrains decompiler
// Type: Celeste.RotatingPlatform
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class RotatingPlatform : JumpThru
    {
        private const float RotateSpeed = 1.04719758f;
        private Vector2 center;
        private readonly bool clockwise;
        private readonly float length;
        private float currentAngle;

        public RotatingPlatform(Vector2 position, int width, Vector2 center, bool clockwise)
            : base(position, width, false)
        {
            Collider.Position.X = -width / 2;
            Collider.Position.Y = (float)(-(double)Height / 2.0);
            this.center = center;
            this.clockwise = clockwise;
            length = (position - center).Length();
            currentAngle = (position - center).Angle();
            SurfaceSoundIndex = 5;
            Add(new LightOcclude(0.2f));
        }

        public override void Update()
        {
            base.Update();
            if (clockwise)
            {
                currentAngle -= 1.04719758f * Engine.DeltaTime;
            }
            else
            {
                currentAngle += 1.04719758f * Engine.DeltaTime;
            }

            currentAngle = Calc.WrapAngle(currentAngle);
            MoveTo(center + Calc.AngleToVector(currentAngle, length));
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Collider, Color.White);
        }
    }
}
