// Decompiled with JetBrains decompiler
// Type: Celeste.Snow
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class Snow : Backdrop
    {
        public static readonly Color[] ForegroundColors = new Color[2]
        {
            Color.White,
            Color.CornflowerBlue
        };
        public static readonly Color[] BackgroundColors = new Color[2]
        {
            new Color(0.2f, 0.2f, 0.2f, 1f),
            new Color(0.1f, 0.2f, 0.5f, 1f)
        };
        public float Alpha = 1f;
        private float visibleFade = 1f;
        private float linearFade = 1f;
        private readonly Color[] colors;
        private readonly Color[] blendedColors;
        private readonly Snow.Particle[] particles = new Snow.Particle[60];

        public Snow(bool foreground)
        {
            colors = foreground ? Snow.ForegroundColors : Snow.BackgroundColors;
            blendedColors = new Color[colors.Length];
            int speedMin = foreground ? 120 : 40;
            int speedMax = foreground ? 300 : 100;
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Init(colors.Length, speedMin, speedMax);
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            visibleFade = Calc.Approach(visibleFade, IsVisible(scene as Level) ? 1f : 0.0f, Engine.DeltaTime * 2f);
            if (FadeX != null)
            {
                linearFade = FadeX.Value((scene as Level).Camera.X + 160f);
            }

            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position.X -= particles[index].Speed * Engine.DeltaTime;
                particles[index].Position.Y += (float)(Math.Sin(particles[index].Sin) * particles[index].Speed * 0.20000000298023224) * Engine.DeltaTime;
                particles[index].Sin += Engine.DeltaTime;
            }
        }

        public override void Render(Scene scene)
        {
            if (Alpha <= 0.0 || visibleFade <= 0.0 || linearFade <= 0.0)
            {
                return;
            }

            for (int index = 0; index < blendedColors.Length; ++index)
            {
                blendedColors[index] = colors[index] * (Alpha * visibleFade * linearFade);
            }

            Camera camera = (scene as Level).Camera;
            for (int index = 0; index < particles.Length; ++index)
            {
                Vector2 position = new(mod(particles[index].Position.X - camera.X, 320f), mod(particles[index].Position.Y - camera.Y, 180f));
                Color blendedColor = blendedColors[particles[index].Color];
                Draw.Pixel.DrawCentered(position, blendedColor);
            }
        }

        private float mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }

        private struct Particle
        {
            public Vector2 Position;
            public int Color;
            public float Speed;
            public float Sin;

            public void Init(int maxColors, float speedMin, float speedMax)
            {
                Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(180f));
                Color = Calc.Random.Next(maxColors);
                Speed = Calc.Random.Range(speedMin, speedMax);
                Sin = Calc.Random.NextFloat(6.28318548f);
            }
        }
    }
}
