using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class StardustFG : Backdrop
    {
        private static readonly Color[] colors = new Color[3]
        {
            Calc.HexToColor("4cccef"),
            Calc.HexToColor("f243bd"),
            Calc.HexToColor("42f1dd")
        };
        private Particle[] particles = new Particle[50];
        private float fade;
        private Vector2 scale = Vector2.One;

        public StardustFG()
        {
            for (int i = 0; i < particles.Length; ++i)
                Reset(i, Calc.Random.NextFloat());
        }

        private void Reset(int i, float p)
        {
            particles[i].Percent = p;
            particles[i].Position = new Vector2(Calc.Random.Range(0, 320), Calc.Random.Range(0, 180));
            particles[i].Speed = Calc.Random.Range(4, 14);
            particles[i].Spin = Calc.Random.Range(0.25f, 18.849556f);
            particles[i].Duration = Calc.Random.Range(1f, 4f);
            particles[i].Direction = Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 1f);
            particles[i].Color = Calc.Random.Next(StardustFG.colors.Length);
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Level level = scene as Level;
            int num = level.Wind.Y == 0.0 ? 1 : 0;
            Vector2 vector2 = Vector2.Zero;
            if (num != 0)
            {
                scale.X = Math.Max(1f, Math.Abs(level.Wind.X) / 100f);
                scale.Y = 1f;
                vector2 = new Vector2(level.Wind.X, 0.0f);
            }
            else
            {
                scale.X = 1f;
                scale.Y = Math.Max(1f, Math.Abs(level.Wind.Y) / 40f);
                vector2 = new Vector2(0.0f, level.Wind.Y * 2f);
            }
            for (int i = 0; i < particles.Length; ++i)
            {
                if (particles[i].Percent >= 1.0)
                    Reset(i, 0.0f);
                particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
                particles[i].Position += (particles[i].Direction * particles[i].Speed + vector2) * Engine.DeltaTime;
                particles[i].Direction.Rotate(particles[i].Spin * Engine.DeltaTime);
            }
            fade = Calc.Approach(fade, Visible ? 1f : 0.0f, Engine.DeltaTime);
        }

        public override void Render(Scene level)
        {
            if (fade <= 0.0)
                return;
            Camera camera = (level as Level).Camera;
            for (int index = 0; index < particles.Length; ++index)
            {
                Vector2 position = new Vector2();
                position.X = mod(particles[index].Position.X - camera.X, 320f);
                position.Y = mod(particles[index].Position.Y - camera.Y, 180f);
                float percent = particles[index].Percent;
                float num = (percent >= 0.699999988079071 ? Calc.ClampedMap(percent, 0.7f, 1f, 1f, 0.0f) : Calc.ClampedMap(percent, 0.0f, 0.3f)) * FadeAlphaMultiplier;
                Draw.Rect(position, scale.X, scale.Y, StardustFG.colors[particles[index].Color] * (fade * num));
            }
        }

        private float mod(float x, float m) => (x % m + m) % m;

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
