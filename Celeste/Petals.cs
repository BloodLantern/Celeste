// Decompiled with JetBrains decompiler
// Type: Celeste.Petals
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class Petals : Backdrop
    {
        private static readonly Color[] colors = new Color[1]
        {
            Calc.HexToColor("ff3aa3")
        };
        private readonly Petals.Particle[] particles = new Petals.Particle[40];
        private float fade;

        public Petals()
        {
            for (int i = 0; i < particles.Length; ++i)
            {
                Reset(i);
            }
        }

        private void Reset(int i)
        {
            particles[i].Position = new Vector2(Calc.Random.Range(0, 352), Calc.Random.Range(0, 212));
            particles[i].Speed = Calc.Random.Range(6f, 16f);
            particles[i].Spin = Calc.Random.Range(8f, 12f) * 0.2f;
            particles[i].Color = Calc.Random.Next(Petals.colors.Length);
            particles[i].RotationCounter = Calc.Random.NextAngle();
            particles[i].MaxRotate = Calc.Random.Range(0.3f, 0.6f) * 1.57079637f;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position.Y += particles[index].Speed * Engine.DeltaTime;
                particles[index].RotationCounter += particles[index].Spin * Engine.DeltaTime;
            }
            fade = Calc.Approach(fade, Visible ? 1f : 0.0f, Engine.DeltaTime);
        }

        public override void Render(Scene level)
        {
            if (fade <= 0.0)
            {
                return;
            }

            Camera camera = (level as Level).Camera;
            MTexture mtexture = GFX.Game["particles/petal"];
            for (int index = 0; index < particles.Length; ++index)
            {
                Vector2 vector2 = new()
                {
                    X = Mod(particles[index].Position.X - camera.X, 352f) - 16f,
                    Y = Mod(particles[index].Position.Y - camera.Y, 212f) - 16f
                };
                float angleRadians = (float)(1.5707963705062866 + (Math.Sin(particles[index].RotationCounter * (double)particles[index].MaxRotate) * 1.0));
                Vector2 position = vector2 + Calc.AngleToVector(angleRadians, 4f);
                mtexture.DrawCentered(position, Petals.colors[particles[index].Color] * fade, 1f, angleRadians - 0.8f);
            }
        }

        private float Mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }

        private struct Particle
        {
            public Vector2 Position;
            public float Speed;
            public float Spin;
            public float MaxRotate;
            public int Color;
            public float RotationCounter;
        }
    }
}
