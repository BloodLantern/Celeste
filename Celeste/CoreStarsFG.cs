﻿// Decompiled with JetBrains decompiler
// Type: Celeste.CoreStarsFG
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class CoreStarsFG : Backdrop
    {
        private static readonly Color[] colors = new Color[2]
        {
            Color.White,
            Calc.HexToColor("d8baf8")
        };
        private readonly CoreStarsFG.Particle[] particles = new CoreStarsFG.Particle[50];
        private float fade;

        public CoreStarsFG()
        {
            for (int i = 0; i < particles.Length; ++i)
            {
                Reset(i, Calc.Random.NextFloat());
            }
        }

        private void Reset(int i, float p)
        {
            particles[i].Percent = p;
            particles[i].Position = new Vector2(Calc.Random.Range(0, 320), Calc.Random.Range(0, 180));
            particles[i].Speed = Calc.Random.Range(2f, 5f);
            particles[i].Spin = Calc.Random.Range(0.25f, 18.849556f);
            particles[i].Duration = Calc.Random.Range(1f, 4f);
            particles[i].Direction = Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 1f);
            particles[i].Color = Calc.Random.Next(CoreStarsFG.colors.Length);
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            for (int i = 0; i < particles.Length; ++i)
            {
                if (particles[i].Percent >= 1.0)
                {
                    Reset(i, 0.0f);
                }

                particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
                particles[i].Position += particles[i].Direction * particles[i].Speed * Engine.DeltaTime;
                _ = particles[i].Direction.Rotate(particles[i].Spin * Engine.DeltaTime);
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
            for (int index = 0; index < particles.Length; ++index)
            {
                Vector2 position = new()
                {
                    X = mod(particles[index].Position.X - camera.X, 320f),
                    Y = mod(particles[index].Position.Y - camera.Y, 180f)
                };
                float percent = particles[index].Percent;
                float num = (double)percent >= 0.699999988079071 ? Calc.ClampedMap(percent, 0.7f, 1f, 1f, 0.0f) : Calc.ClampedMap(percent, 0.0f, 0.3f);
                Draw.Rect(position, 1f, 1f, CoreStarsFG.colors[particles[index].Color] * (fade * num));
            }
        }

        private float mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }

        private struct Particle
        {
            public Vector2 Position;
            public float Percent;
            public float Duration;
            public Vector2 Direction;
            public float Speed;
            public float Spin;
            public int Color;
        }
    }
}
