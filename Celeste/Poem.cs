using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste
{
    public class Poem : Entity
    {
        private const float textScale = 1.5f;
        private static readonly Color[] colors = new Color[4]
        {
            Calc.HexToColor("8cc7fa"),
            Calc.HexToColor("ff668a"),
            Calc.HexToColor("fffc24"),
            Calc.HexToColor("ffffff")
        };
        public float Alpha = 1f;
        public float TextAlpha = 1f;
        public Vector2 Offset;
        public Sprite Heart;
        public float ParticleSpeed = 1f;
        public float Shake;
        private float timer;
        private readonly string text;
        private bool disposed;
        private readonly VirtualRenderTarget poem;
        private readonly VirtualRenderTarget smoke;
        private readonly VirtualRenderTarget temp;
        private readonly Particle[] particles = new Particle[80];

        public Color Color { get; private set; }

        public Poem(string text, int heartIndex, float heartAlpha)
        {
            if (text != null)
                this.text = ActiveFont.FontSize.AutoNewline(text, 1024);
            Color = colors[heartIndex];
            Heart = GFX.GuiSpriteBank.Create("heartgem" + heartIndex);
            Heart.Play("spin");
            Heart.Position = new Vector2(1920f, 1080f) * 0.5f;
            Heart.Color = Color.White * heartAlpha;
            int width = Math.Min(1920, Engine.ViewWidth);
            int height = Math.Min(1080, Engine.ViewHeight);
            poem = VirtualContent.CreateRenderTarget("poem-a", width, height);
            smoke = VirtualContent.CreateRenderTarget("poem-b", width / 2, height / 2);
            temp = VirtualContent.CreateRenderTarget("poem-c", width / 2, height / 2);
            Tag = (int) Tags.HUD | (int) Tags.FrozenUpdate;
            Add(new BeforeRenderHook(new Action(BeforeRender)));
            for (int index = 0; index < particles.Length; ++index)
                particles[index].Reset(Calc.Random.NextFloat());
        }

        public void Dispose()
        {
            if (disposed)
                return;
            poem.Dispose();
            smoke.Dispose();
            temp.Dispose();
            RemoveSelf();
            disposed = true;
        }

        private void DrawPoem(Vector2 offset, Color color)
        {
            MTexture mtexture = GFX.Gui["poemside"];
            float num = ActiveFont.Measure(text).X * 1.5f;
            Vector2 position = new Vector2(960f, 540f) + offset;
            mtexture.DrawCentered(position - Vector2.UnitX * (num / 2 + 64), color);
            ActiveFont.Draw(text, position, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, color);
            mtexture.DrawCentered(position + Vector2.UnitX * (num / 2 + 64), color);
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Percent += Engine.DeltaTime / particles[index].Duration * ParticleSpeed;
                if (particles[index].Percent > 1)
                    particles[index].Reset(0f);
            }
            Heart.Update();
        }

        public void BeforeRender()
        {
            if (disposed)
                return;
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D) poem);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Matrix scale = Matrix.CreateScale(poem.Width / 1920f);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, scale);
            Heart.Position = Offset + new Vector2(1920f, 1080f) * 0.5f;
            Heart.Scale = Vector2.One * (float) (1.0 + Shake * 0.10000000149011612);
            MTexture atla = OVR.Atlas["snow"];
            for (int index = 0; index < particles.Length; ++index)
            {
                Poem.Particle particle = particles[index];
                float num1 = Ease.SineIn(particle.Percent);
                Vector2 position = Heart.Position + particle.Direction * (1f - num1) * 1920f;
                float x = (float) (1.0 + (double) num1 * 2.0);
                float y = (float) (0.25 * (0.25 + (1.0 - (double) num1) * 0.75));
                float num2 = 1f - num1;
                atla.DrawCentered(position, Color * num2, new Vector2(x, y), (-particle.Direction).Angle());
            }
            Sprite heart = Heart;
            heart.Position = heart.Position + new Vector2(Calc.Random.Range(-1f, 1f), Calc.Random.Range(-1f, 1f)) * 16f * Shake;
            Heart.Render();
            if (!string.IsNullOrEmpty(text))
            {
                DrawPoem(Offset + new Vector2(-2f, 0.0f), Color.Black * TextAlpha);
                DrawPoem(Offset + new Vector2(2f, 0.0f), Color.Black * TextAlpha);
                DrawPoem(Offset + new Vector2(0.0f, -2f), Color.Black * TextAlpha);
                DrawPoem(Offset + new Vector2(0.0f, 2f), Color.Black * TextAlpha);
                DrawPoem(Offset + Vector2.Zero, Color * TextAlpha);
            }
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D) smoke);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            MagicGlow.Render((RenderTarget2D)poem, timer, -1f, Matrix.CreateScale(0.5f));
            GaussianBlur.Blur((RenderTarget2D)smoke, temp, smoke);
        }

        public override void Render()
        {
            if (disposed || Scene.Paused)
                return;
            float scale = 1920f / poem.Width;
            Draw.SpriteBatch.Draw((RenderTarget2D)smoke, Vector2.Zero, new Rectangle?(smoke.Bounds), Color.White * 0.3f * Alpha, 0.0f, Vector2.Zero, scale * 2f, SpriteEffects.None, 0.0f);
            Draw.SpriteBatch.Draw((RenderTarget2D)poem, Vector2.Zero, new Rectangle?(poem.Bounds), Color.White * Alpha, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }

        private struct Particle
        {
            public Vector2 Direction;
            public float Percent;
            public float Duration;

            public void Reset(float percent)
            {
                Direction = Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 1f);
                Percent = percent;
                Duration = (float) (0.5 + (double) Calc.Random.NextFloat() * 0.5);
            }
        }
    }
}
