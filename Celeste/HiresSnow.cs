// Decompiled with JetBrains decompiler
// Type: Celeste.HiresSnow
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class HiresSnow : Monocle.Renderer
    {
        public float Alpha = 1f;
        public float ParticleAlpha = 1f;
        public Vector2 Direction = new(-1f, 0.0f);
        public ScreenWipe AttachAlphaTo;
        private readonly HiresSnow.Particle[] particles;
        private readonly MTexture overlay;
        private readonly MTexture snow;
        private float timer;
        private readonly float overlayAlpha;

        public HiresSnow(float overlayAlpha = 0.45f)
        {
            this.overlayAlpha = overlayAlpha;
            overlay = OVR.Atlas[nameof(overlay)];
            snow = OVR.Atlas[nameof(snow)].GetSubtexture(1, 1, 254, 254);
            particles = new HiresSnow.Particle[50];
            Reset();
        }

        public void Reset()
        {
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Reset(Direction);
                particles[index].Position.X = Calc.Random.NextFloat(Engine.Width);
                particles[index].Position.Y = Calc.Random.NextFloat(Engine.Height);
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (AttachAlphaTo != null)
            {
                Alpha = AttachAlphaTo.Percent;
            }

            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position += Direction * particles[index].Speed * Engine.DeltaTime;
                particles[index].Position.Y += (float)(Math.Sin(particles[index].Sin) * 100.0) * Engine.DeltaTime;
                particles[index].Sin += Engine.DeltaTime;
                if (particles[index].Position.X < (double)sbyte.MinValue || particles[index].Position.X > (double)(Engine.Width + 128) || particles[index].Position.Y < (double)sbyte.MinValue || particles[index].Position.Y > (double)(Engine.Height + 128))
                {
                    particles[index].Reset(Direction);
                }
            }
            timer += Engine.DeltaTime;
        }

        public override void Render(Scene scene)
        {
            float x = Calc.Clamp(Direction.Length(), 0.0f, 20f);
            float num1 = 0.0f;
            Vector2 vector2 = Vector2.One;
            bool flag = (double)x > 1.0;
            if (flag)
            {
                num1 = Direction.Angle();
                vector2 = new Vector2(x, (float)(0.20000000298023224 + ((1.0 - ((double)x / 20.0)) * 0.800000011920929)));
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, null, null, null, Engine.ScreenMatrix);
            float num2 = Alpha * ParticleAlpha;
            for (int index = 0; index < particles.Length; ++index)
            {
                Color color = particles[index].Color;
                float rotation = particles[index].Rotation;
                if ((double)num2 < 1.0)
                {
                    color *= num2;
                }

                snow.DrawCentered(particles[index].Position, color, vector2 * particles[index].Scale, flag ? num1 : rotation);
            }
            Draw.SpriteBatch.Draw(overlay.Texture.Texture, Vector2.Zero, new Rectangle?(new Rectangle(-(int)(timer * 32f % overlay.Width), -(int)(timer * 20f % overlay.Height), 1920, 1080)), Color.White * (Alpha * overlayAlpha));
            Draw.SpriteBatch.End();
        }

        private struct Particle
        {
            public float Scale;
            public Vector2 Position;
            public float Speed;
            public float Sin;
            public float Rotation;
            public Color Color;

            public void Reset(Vector2 direction)
            {
                float num = Calc.Random.NextFloat();
                float val = num * (num * num * num);
                Scale = Calc.Map(val, 0.0f, 1f, 0.05f, 0.8f);
                Speed = Scale * Calc.Random.Range(2500, 5000);
                if (direction.X < 0.0)
                {
                    Position = new Vector2(Engine.Width + 128, Calc.Random.NextFloat(Engine.Height));
                }
                else if (direction.X > 0.0)
                {
                    Position = new Vector2(sbyte.MinValue, Calc.Random.NextFloat(Engine.Height));
                }
                else if (direction.Y > 0.0)
                {
                    Position = new Vector2(Calc.Random.NextFloat(Engine.Width), sbyte.MinValue);
                }
                else if (direction.Y < 0.0)
                {
                    Position = new Vector2(Calc.Random.NextFloat(Engine.Width), Engine.Height + 128);
                }

                Sin = Calc.Random.NextFloat(6.28318548f);
                Rotation = Calc.Random.NextFloat(6.28318548f);
                Color = Color.Lerp(Color.White, Color.Transparent, val * 0.8f);
            }
        }
    }
}
