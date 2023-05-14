// Decompiled with JetBrains decompiler
// Type: Celeste.RainFG
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class RainFG : Backdrop
    {
        public float Alpha = 1f;
        private float visibleFade = 1f;
        private float linearFade = 1f;
        private readonly RainFG.Particle[] particles = new RainFG.Particle[240];

        public RainFG()
        {
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Init();
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            bool flag = IsVisible(scene as Level);
            (scene as Level).Raining = flag;
            visibleFade = Calc.Approach(visibleFade, flag ? 1f : 0.0f, Engine.DeltaTime * (flag ? 10f : 0.25f));
            if (FadeX != null)
            {
                linearFade = FadeX.Value((scene as Level).Camera.X + 160f);
            }

            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position += particles[index].Speed * Engine.DeltaTime;
            }
        }

        public override void Render(Scene scene)
        {
            if (Alpha <= 0.0 || visibleFade <= 0.0 || linearFade <= 0.0)
            {
                return;
            }

            Color color = Calc.HexToColor("161933") * 0.5f * Alpha * linearFade * visibleFade;
            Camera camera = (scene as Level).Camera;
            for (int index = 0; index < particles.Length; ++index)
            {
                Vector2 position = new(mod((float)(particles[index].Position.X - (double)camera.X - 32.0), 384f), mod((float)(particles[index].Position.Y - (double)camera.Y - 32.0), 244f));
                Draw.Pixel.DrawCentered(position, color, particles[index].Scale, particles[index].Rotation);
            }
        }

        private float mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }

        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Speed;
            public float Rotation;
            public Vector2 Scale;

            public void Init()
            {
                Position = new Vector2(Calc.Random.NextFloat(384f) - 32f, Calc.Random.NextFloat(244f) - 32f);
                Rotation = 1.57079637f + Calc.Random.Range(-0.05f, 0.05f);
                Speed = Calc.AngleToVector(Rotation, Calc.Random.Range(200f, 600f));
                Scale = new Vector2((float)(4.0 + (((double)Speed.Length() - 200.0) / 400.0 * 12.0)), 1f);
            }
        }
    }
}
