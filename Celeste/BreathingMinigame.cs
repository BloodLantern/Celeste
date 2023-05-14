// Decompiled with JetBrains decompiler
// Type: Celeste.BreathingMinigame
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class BreathingMinigame : Entity
    {
        private const float StablizeDuration = 30f;
        private const float StablizeLossRate = 0.5f;
        private const float StablizeIncreaseDelay = 0.2f;
        private const float StablizeLossPenalty = 0.5f;
        private const float Acceleration = 280f;
        private const float Gravity = 280f;
        private const float Maxspeed = 200f;
        private const float Bounds = 450f;
        private const float BGFadeStart = 0.65f;
        private const float FeatherSpriteOffset = -128f;
        private const float FadeBoxInMargin = 300f;
        private const float TargetSineAmplitude = 300f;
        private const float TargetSineFreq = 0.25f;
        private const float TargetBoundsAtStart = 160f;
        private const float TargetBoundsAtEnd = 100f;
        public const float MaxRumble = 0.5f;
        private const float PercentBeforeStartLosing = 0.4f;
        private const float LoseDuration = 5f;
        public bool Completed;
        public bool Pausing;
        private readonly bool winnable;
        private float boxAlpha;
        private float featherAlpha;
        private float bgAlpha;
        private float feather;
        private float speed;
        private float stablizedTimer;
        private float currentTargetBounds = TargetBoundsAtStart;
        private float currentTargetCenter;
        private float speedMultiplier = 1f;
        private float insideTargetTimer;
        private bool boxEnabled;
        private float trailSpeed;
        private bool losing;
        private float losingTimer;
        private readonly Sprite featherSprite;
        private Image featherSlice;
        private Image featherHalfLeft;
        private Image featherHalfRight;
        private SineWave sine;
        private readonly SineWave featherWave;
        private BreathingRumbler rumbler;
        private string text;
        private float textAlpha;
        private VirtualRenderTarget featherBuffer;
        private VirtualRenderTarget smokeBuffer;
        private VirtualRenderTarget tempBuffer;
        private float timer;
        private readonly Particle[] particles;
        private readonly MTexture particleTexture = OVR.Atlas["snow"].GetSubtexture(1, 1, 254, 254);
        private float particleSpeed;
        private float particleAlpha;

        private Vector2 screenCenter => new Vector2(1920f, 1080f) / 2f;

        public BreathingMinigame(bool winnable = true, BreathingRumbler rumbler = null)
        {
            this.rumbler = rumbler;
            this.winnable = winnable;
            Tag = (int)Tags.HUD;
            Depth = 100;
            Add(featherSprite = GFX.GuiSpriteBank.Create(nameof(feather)));
            featherSprite.Position = screenCenter + (Vector2.UnitY * (feather - 128f));
            Add(new Coroutine(Routine()));
            Add(featherWave = new SineWave(TargetSineFreq));
            Add(new BeforeRenderHook(new Action(BeforeRender)));
            particles = new Particle[50];
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Reset();
            }

            particleSpeed = 120f;
        }

        public IEnumerator Routine()
        {
            insideTargetTimer = 1f;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime)
            {
                yield return null;
                if (p > 1f)
                {
                    p = 1f;
                }

                bgAlpha = p * BGFadeStart;
            }
            if (winnable)
            {
                yield return ShowText(1);
                yield return FadeGameIn();
                yield return ShowText(2);
                yield return ShowText(3);
                yield return ShowText(4);
                yield return ShowText(5);
            }
            else
            {
                yield return FadeGameIn();
            }

            Add(new Coroutine(FadeBoxIn()));
            float activeBounds = Bounds;
            while (stablizedTimer < StablizeDuration)
            {
                float t = stablizedTimer / StablizeDuration;
                bool flag = Input.Jump.Check || Input.Dash.Check || Input.Aim.Value.Y < 0f;
                if (winnable)
                {
                    Audio.SetMusicParam("calm", t);
                    Audio.SetMusicParam("gondola_idle", t);
                }
                else
                {
                    Level scene = Scene as Level;
                    if (!losing)
                    {
                        float num = t / PercentBeforeStartLosing;
                        _ = scene.Session.Audio.Music.Layer(1, num);
                        _ = scene.Session.Audio.Music.Layer(3, 1f - num);
                        scene.Session.Audio.Apply();
                    }
                    else
                    {
                        _ = scene.Session.Audio.Music.Layer(1, 1f - losingTimer);
                        _ = scene.Session.Audio.Music.Layer(3, losingTimer);
                        scene.Session.Audio.Apply();
                    }
                }
                if (!winnable && losing)
                {
                    if (Calc.BetweenInterval(losingTimer * 10f, StablizeLossRate))
                    {
                        flag = !flag;
                    }

                    activeBounds = Bounds - (Ease.CubeIn(losingTimer) * 200f);
                }
                if (flag)
                {
                    if (feather > -activeBounds)
                    {
                        speed -= Gravity * Engine.DeltaTime;
                    }

                    particleSpeed -= 2800f * Engine.DeltaTime;
                }
                else
                {
                    if (feather < activeBounds)
                    {
                        speed += Acceleration * Engine.DeltaTime;
                    }

                    particleSpeed += 2800f * Engine.DeltaTime;
                }
                speed = Calc.Clamp(speed, -Maxspeed, Maxspeed);
                if (feather > activeBounds && speedMultiplier == 0f && speed > 0f)
                {
                    speed = 0f;
                }

                if (feather < activeBounds && speedMultiplier == 0f && speed < 0f)
                {
                    speed = 0f;
                }

                particleSpeed = Calc.Clamp(particleSpeed, -1600f, 120f);
                speedMultiplier = Calc.Approach(speedMultiplier, (feather < -activeBounds && speed < 0f) || (feather > activeBounds && speed > 0f) ? 0f : 1f, Engine.DeltaTime * 4f);
                currentTargetBounds = Calc.Approach(currentTargetBounds, TargetBoundsAtStart + (-60f * t), Engine.DeltaTime * 16f);
                feather += speed * speedMultiplier * Engine.DeltaTime;
                if (boxEnabled)
                {
                    currentTargetCenter = -sine.Value * TargetSineAmplitude * MathHelper.Lerp(1f, 0f, Ease.CubeIn(t));
                    float num1 = currentTargetCenter - currentTargetBounds;
                    float num2 = currentTargetCenter + currentTargetBounds;
                    if (feather > num1 && feather < num2)
                    {
                        insideTargetTimer += Engine.DeltaTime;
                        if (insideTargetTimer > StablizeIncreaseDelay)
                        {
                            stablizedTimer += Engine.DeltaTime;
                        }

                        if (rumbler != null)
                        {
                            rumbler.Strength = 0.3f * (1f - t);
                        }
                    }
                    else
                    {
                        if (insideTargetTimer > StablizeIncreaseDelay)
                        {
                            stablizedTimer = Math.Max(0f, stablizedTimer - StablizeLossPenalty);
                        }

                        if (stablizedTimer > 0f)
                        {
                            stablizedTimer -= StablizeLossRate * Engine.DeltaTime;
                        }

                        insideTargetTimer = 0f;
                        if (rumbler != null)
                        {
                            rumbler.Strength = 0.5f * (1f - t);
                        }
                    }
                }
                else if (rumbler != null)
                {
                    rumbler.Strength = 0.2f;
                }

                float target = BGFadeStart + (Math.Min(1f, t / 0.8f) * 0.35f);
                bgAlpha = Calc.Approach(bgAlpha, target, Engine.DeltaTime);
                featherSprite.Position = screenCenter + (Vector2.UnitY * (feather - 128f));
                featherSprite.Play(insideTargetTimer > 0f || !boxEnabled ? "hover" : "flutter");
                particleAlpha = Calc.Approach(particleAlpha, 1f, Engine.DeltaTime);
                if (!winnable && stablizedTimer > 12f)
                {
                    losing = true;
                }

                if (losing)
                {
                    losingTimer += Engine.DeltaTime / LoseDuration;
                    if (losingTimer > 1f)
                    {
                        break;
                    }
                }
                yield return null;
            }
            if (!winnable)
            {
                Pausing = true;
                while (Pausing)
                {
                    if (rumbler != null)
                    {
                        rumbler.Strength = Calc.Approach(rumbler.Strength, 1f, 2f * Engine.DeltaTime);
                    }

                    featherSprite.Position += (screenCenter - featherSprite.Position) * (1f - (float)Math.Pow(0.01f, Engine.DeltaTime));
                    boxAlpha -= Engine.DeltaTime * 10f;
                    particleAlpha = boxAlpha;
                    yield return null;
                }
                losing = false;
                losingTimer = 0f;
                yield return PopFeather();
            }
            else
            {
                bgAlpha = 1f;
                if (rumbler != null)
                {
                    rumbler.RemoveSelf();
                    rumbler = null;
                }
                while (boxAlpha > 0f)
                {
                    yield return null;
                    boxAlpha -= Engine.DeltaTime;
                    particleAlpha = boxAlpha;
                }
                particleAlpha = 0f;
                yield return 2f;
                for (; featherAlpha > 0f; featherAlpha -= Engine.DeltaTime)
                {
                    yield return null;
                }

                yield return 1f;
            }
            Completed = true;
            for (; bgAlpha > 0f; bgAlpha -= Engine.DeltaTime * (winnable ? 1f : 10f))
            {
                yield return null;
            }

            RemoveSelf();
        }

        private IEnumerator ShowText(int num)
        {
            yield return FadeTextTo(0f);
            text = Dialog.Clean("CH4_GONDOLA_FEATHER_" + num);
            yield return 0.1f;
            yield return FadeTextTo(1f);
            while (!Input.MenuConfirm.Pressed)
            {
                yield return null;
            }

            yield return FadeTextTo(0f);
        }

        private IEnumerator FadeGameIn()
        {
            while (featherAlpha < 1f)
            {
                featherAlpha += Engine.DeltaTime;
                yield return null;
            }
            featherAlpha = 1f;
        }

        private IEnumerator FadeBoxIn()
        {
            _ = this;
            yield return winnable ? LoseDuration : 2f;
            while (Math.Abs(feather) > 300f)
            {
                yield return null;
            }

            boxEnabled = true;
            Add(sine = new SineWave(0.12f));
            while (boxAlpha < 1f)
            {
                boxAlpha += Engine.DeltaTime;
                yield return null;
            }
            boxAlpha = 1f;
        }

        private IEnumerator FadeTextTo(float v)
        {
            if (textAlpha != v)
            {
                float from = textAlpha;
                for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
                {
                    yield return null;
                    textAlpha = from + ((v - from) * p);
                }
                textAlpha = v;
            }
        }

        private IEnumerator PopFeather()
        {
            _ = this;
            _ = Audio.Play("event:/game/06_reflection/badeline_feather_slice");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            if (rumbler != null)
            {
                rumbler.RemoveSelf();
                rumbler = null;
            }
            featherSprite.Rotation = 0f;
            featherSprite.Play("hover");
            _ = featherSprite.CenterOrigin();
            featherSprite.Y += featherSprite.Height / 2f;
            yield return 0.25f;
            featherSlice = new Image(GFX.Gui["feather/slice"]);
            _ = featherSlice.CenterOrigin();
            featherSlice.Position = featherSprite.Position;
            featherSlice.Rotation = Calc.Angle(new Vector2(96f, 165f), new Vector2(140f, 112f));
            float p;
            for (p = 0f; p < 1f; p += Engine.DeltaTime * 8f)
            {
                featherSlice.Scale.X = (0.25f + (Calc.YoYo(p) * 0.75f)) * 8f;
                featherSlice.Scale.Y = (0.5f + ((1f - Calc.YoYo(p)) * 0.5f)) * 8f;
                featherSlice.Position = featherSprite.Position + Vector2.Lerp(new Vector2(128f, sbyte.MinValue), new Vector2(sbyte.MinValue, 128f), p);
                yield return null;
            }
            featherSlice.Visible = false;
            (Scene as Level).Shake();
            (Scene as Level).Flash(Color.White);
            featherSprite.Visible = false;
            featherHalfLeft = new Image(GFX.Gui["feather/feather_half0"]);
            _ = featherHalfLeft.CenterOrigin();
            featherHalfRight = new Image(GFX.Gui["feather/feather_half1"]);
            _ = featherHalfRight.CenterOrigin();
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime)
            {
                featherHalfLeft.Position = featherSprite.Position + Vector2.Lerp(Vector2.Zero, new Vector2(sbyte.MinValue, -32f), p);
                featherHalfRight.Position = featherSprite.Position + Vector2.Lerp(Vector2.Zero, new Vector2(128f, 32f), p);
                featherAlpha = 1f - p;
                yield return null;
            }
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
            trailSpeed = Calc.Approach(trailSpeed, speed, Engine.DeltaTime * Maxspeed * 8f);
            if (featherWave != null)
            {
                featherSprite.Rotation = (featherWave.Value * 0.25f) + 0.1f;
            }

            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position.Y += particles[index].Speed * particleSpeed * Engine.DeltaTime;
                if (particleSpeed > -400f)
                {
                    particles[index].Position.X += (particleSpeed + 400f) * (float)Math.Sin(particles[index].Sin) * 0.1f * Engine.DeltaTime;
                }

                particles[index].Sin += Engine.DeltaTime;
                if (particles[index].Position.Y is < sbyte.MinValue or > 1208f)
                {
                    particles[index].Reset();
                    particles[index].Position.Y = particleSpeed >= 0f ? sbyte.MinValue : 1208f;
                }
            }
            base.Update();
        }

        public void BeforeRender()
        {
            if (featherBuffer == null)
            {
                int width = Math.Min(1920, Engine.ViewWidth);
                int height = Math.Min(1080, Engine.ViewHeight);
                featherBuffer = VirtualContent.CreateRenderTarget("breathing-minigame-a", width, height);
                smokeBuffer = VirtualContent.CreateRenderTarget("breathing-minigame-b", width / 2, height / 2);
                tempBuffer = VirtualContent.CreateRenderTarget("breathing-minigame-c", width / 2, height / 2);
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(featherBuffer.Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Matrix scale = Matrix.CreateScale(featherBuffer.Width / 1920f);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, scale);
            if (losing)
            {
                featherSprite.Position += new Vector2(Calc.Random.Range(-1, 1), Calc.Random.Range(-1, 1)).SafeNormalize() * losingTimer * 10f;
                featherSprite.Rotation += Calc.Random.Range(-1, 1) * losingTimer * 0.1f;
            }
            featherSprite.Color = Color.White * featherAlpha;
            if (featherSprite.Visible)
            {
                featherSprite.Render();
            }

            if (featherSlice != null && featherSlice.Visible)
            {
                featherSlice.Render();
            }

            if (featherHalfLeft != null && featherHalfLeft.Visible)
            {
                featherHalfLeft.Color = Color.White * featherAlpha;
                featherHalfRight.Color = Color.White * featherAlpha;
                featherHalfLeft.Render();
                featherHalfRight.Render();
            }
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(smokeBuffer.Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            MagicGlow.Render(featherBuffer.Target, timer, -trailSpeed / Maxspeed * 2f, Matrix.CreateScale(0.5f));
            _ = GaussianBlur.Blur(smokeBuffer.Target, tempBuffer, smokeBuffer);
        }

        public override void Render()
        {
            Color color = insideTargetTimer > StablizeIncreaseDelay ? Color.White : Color.White * 0.6f;
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * bgAlpha);
            if (Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene))
            {
                return;
            }

            MTexture mtexture1 = GFX.Gui["feather/border"];
            MTexture mtexture2 = GFX.Gui["feather/box"];
            Vector2 scale1 = new(((mtexture1.Width * 2f) - 32f) / mtexture2.Width, ((currentTargetBounds * 2f) - 32f) / mtexture2.Height);
            mtexture2.DrawCentered(screenCenter + new Vector2(0f, currentTargetCenter), Color.White * boxAlpha * 0.25f, scale1);
            mtexture1.Draw(screenCenter + new Vector2(-mtexture1.Width, currentTargetCenter - currentTargetBounds), Vector2.Zero, color * boxAlpha, Vector2.One);
            mtexture1.Draw(screenCenter + new Vector2(mtexture1.Width, currentTargetCenter + currentTargetBounds), Vector2.Zero, color * boxAlpha, new Vector2(-1f, -1f));
            if (featherBuffer != null && !featherBuffer.IsDisposed)
            {
                float scale2 = 1920f / featherBuffer.Width;
                Draw.SpriteBatch.Draw(smokeBuffer.Target, Vector2.Zero, new Rectangle?(smokeBuffer.Bounds), Color.White * 0.3f, 0.0f, Vector2.Zero, scale2 * 2f, SpriteEffects.None, 0.0f);
                Draw.SpriteBatch.Draw(featherBuffer.Target, Vector2.Zero, new Rectangle?(featherBuffer.Bounds), Color.White, 0.0f, Vector2.Zero, scale2, SpriteEffects.None, 0.0f);
            }
            Vector2 vector2_1 = new(1f, 1f);
            if (particleSpeed < 0f)
            {
                vector2_1 = new Vector2(Math.Min(1f, 1f / (-particleSpeed * 0.004f)), Math.Max(1f, 1f * -particleSpeed * 0.004f));
            }

            for (int index = 0; index < particles.Length; ++index)
            {
                particleTexture.DrawCentered(particles[index].Position, Color.White * (0.5f * particleAlpha), particles[index].Scale * vector2_1);
            }

            if (!string.IsNullOrEmpty(text) && textAlpha > 0f)
            {
                ActiveFont.Draw(text, new Vector2(960f, 920f), new Vector2(0.5f, 0.5f), Vector2.One, Color.White * textAlpha);
            }

            if (string.IsNullOrEmpty(text) || textAlpha < 1f)
            {
                return;
            }

            Vector2 vector2_2 = ActiveFont.Measure(text);
            Vector2 position = new Vector2(((1920f + vector2_2.X) / 2f) + 40f, 920f + (vector2_2.Y / 2f) - 16f) + new Vector2(0f, timer % 1f < 0.25f ? 6f : 0f);
            GFX.Gui["textboxbutton"].DrawCentered(position);
        }

        public override void Removed(Scene scene)
        {
            Dispose();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Dispose();
            base.SceneEnd(scene);
        }

        private void Dispose()
        {
            if (featherBuffer == null || featherBuffer.IsDisposed)
            {
                return;
            }

            featherBuffer.Dispose();
            featherBuffer = null;
            smokeBuffer.Dispose();
            smokeBuffer = null;
            tempBuffer.Dispose();
            tempBuffer = null;
        }

        private struct Particle
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public float Sin;

            public void Reset()
            {
                float num = Calc.Random.NextFloat();
                float val = num * num * num * num;
                Position = new Vector2(Calc.Random.NextFloat() * 1920f, Calc.Random.NextFloat() * 1080f);
                Scale = Calc.Map(val, 0f, 1f, 0.05f, 0.8f);
                Speed = Scale * Calc.Random.Range(2f, 8f);
                Sin = Calc.Random.NextFloat((float)Math.PI * 2f);
            }
        }
    }
}
