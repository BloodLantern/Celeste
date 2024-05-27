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
        private const float featherSpriteOffset = -128f;
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
        private bool winnable;
        private float boxAlpha;
        private float featherAlpha;
        private float bgAlpha;
        private float feather;
        private float speed;
        private float stablizedTimer;
        private float currentTargetBounds = 160f;
        private float currentTargetCenter;
        private float speedMultiplier = 1f;
        private float insideTargetTimer;
        private bool boxEnabled;
        private float trailSpeed;
        private bool losing;
        private float losingTimer;
        private Sprite featherSprite;
        private Image featherSlice;
        private Image featherHalfLeft;
        private Image featherHalfRight;
        private SineWave sine;
        private SineWave featherWave;
        private BreathingRumbler rumbler;
        private string text;
        private float textAlpha;
        private VirtualRenderTarget featherBuffer;
        private VirtualRenderTarget smokeBuffer;
        private VirtualRenderTarget tempBuffer;
        private float timer;
        private Particle[] particles;
        private MTexture particleTexture = OVR.Atlas["snow"].GetSubtexture(1, 1, 254, 254);
        private float particleSpeed;
        private float particleAlpha;

        private Vector2 screenCenter => new Vector2(1920f, 1080f) / 2f;

        public BreathingMinigame(bool winnable = true, BreathingRumbler rumbler = null)
        {
            this.rumbler = rumbler;
            this.winnable = winnable;
            Tag = (int) Tags.HUD;
            Depth = 100;
            Add(featherSprite = GFX.GuiSpriteBank.Create(nameof (BreathingMinigame.feather)));
            featherSprite.Position = screenCenter + Vector2.UnitY * (feather - 128f);
            Add(new Coroutine(Routine()));
            Add(featherWave = new SineWave(0.25f));
            Add(new BeforeRenderHook(BeforeRender));
            particles = new Particle[50];
            for (int index = 0; index < particles.Length; ++index)
                particles[index].Reset();
            particleSpeed = 120f;
        }

        public IEnumerator Routine()
        {
            BreathingMinigame breathingMinigame = this;
            breathingMinigame.insideTargetTimer = 1f;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime)
            {
                yield return null;
                if (p > 1.0)
                    p = 1f;
                breathingMinigame.bgAlpha = p * 0.65f;
            }
            if (breathingMinigame.winnable)
            {
                yield return breathingMinigame.ShowText(1);
                yield return breathingMinigame.FadeGameIn();
                yield return breathingMinigame.ShowText(2);
                yield return breathingMinigame.ShowText(3);
                yield return breathingMinigame.ShowText(4);
                yield return breathingMinigame.ShowText(5);
            }
            else
                yield return breathingMinigame.FadeGameIn();
            breathingMinigame.Add(new Coroutine(breathingMinigame.FadeBoxIn()));
            float activeBounds = 450f;
            while (breathingMinigame.stablizedTimer < 30.0)
            {
                float t = breathingMinigame.stablizedTimer / 30f;
                bool flag = Input.Jump.Check || Input.Dash.Check || Input.Aim.Value.Y < 0.0;
                if (breathingMinigame.winnable)
                {
                    Audio.SetMusicParam("calm", t);
                    Audio.SetMusicParam("gondola_idle", t);
                }
                else
                {
                    Level scene = breathingMinigame.Scene as Level;
                    if (!breathingMinigame.losing)
                    {
                        float num = t / 0.4f;
                        scene.Session.Audio.Music.Layer(1, num);
                        scene.Session.Audio.Music.Layer(3, 1f - num);
                        scene.Session.Audio.Apply();
                    }
                    else
                    {
                        scene.Session.Audio.Music.Layer(1, 1f - breathingMinigame.losingTimer);
                        scene.Session.Audio.Music.Layer(3, breathingMinigame.losingTimer);
                        scene.Session.Audio.Apply();
                    }
                }
                if (!breathingMinigame.winnable && breathingMinigame.losing)
                {
                    if (Calc.BetweenInterval(breathingMinigame.losingTimer * 10f, 0.5f))
                        flag = !flag;
                    activeBounds = (float) (450.0 - Ease.CubeIn(breathingMinigame.losingTimer) * 200.0);
                }
                if (flag)
                {
                    if (breathingMinigame.feather > -(double) activeBounds)
                        breathingMinigame.speed -= 280f * Engine.DeltaTime;
                    breathingMinigame.particleSpeed -= 2800f * Engine.DeltaTime;
                }
                else
                {
                    if (breathingMinigame.feather < (double) activeBounds)
                        breathingMinigame.speed += 280f * Engine.DeltaTime;
                    breathingMinigame.particleSpeed += 2800f * Engine.DeltaTime;
                }
                breathingMinigame.speed = Calc.Clamp(breathingMinigame.speed, -200f, 200f);
                if (breathingMinigame.feather > (double) activeBounds && breathingMinigame.speedMultiplier == 0.0 && breathingMinigame.speed > 0.0)
                    breathingMinigame.speed = 0.0f;
                if (breathingMinigame.feather < (double) activeBounds && breathingMinigame.speedMultiplier == 0.0 && breathingMinigame.speed < 0.0)
                    breathingMinigame.speed = 0.0f;
                breathingMinigame.particleSpeed = Calc.Clamp(breathingMinigame.particleSpeed, -1600f, 120f);
                breathingMinigame.speedMultiplier = Calc.Approach(breathingMinigame.speedMultiplier, breathingMinigame.feather < -(double) activeBounds && breathingMinigame.speed < 0.0 || breathingMinigame.feather > (double) activeBounds && breathingMinigame.speed > 0.0 ? 0.0f : 1f, Engine.DeltaTime * 4f);
                breathingMinigame.currentTargetBounds = Calc.Approach(breathingMinigame.currentTargetBounds, (float) (160.0 + -60.0 * t), Engine.DeltaTime * 16f);
                breathingMinigame.feather += breathingMinigame.speed * breathingMinigame.speedMultiplier * Engine.DeltaTime;
                if (breathingMinigame.boxEnabled)
                {
                    breathingMinigame.currentTargetCenter = (float) (-(double) breathingMinigame.sine.Value * 300.0) * MathHelper.Lerp(1f, 0.0f, Ease.CubeIn(t));
                    float num1 = breathingMinigame.currentTargetCenter - breathingMinigame.currentTargetBounds;
                    float num2 = breathingMinigame.currentTargetCenter + breathingMinigame.currentTargetBounds;
                    if (breathingMinigame.feather > (double) num1 && breathingMinigame.feather < (double) num2)
                    {
                        breathingMinigame.insideTargetTimer += Engine.DeltaTime;
                        if (breathingMinigame.insideTargetTimer > 0.20000000298023224)
                            breathingMinigame.stablizedTimer += Engine.DeltaTime;
                        if (breathingMinigame.rumbler != null)
                            breathingMinigame.rumbler.Strength = (float) (0.30000001192092896 * (1.0 - t));
                    }
                    else
                    {
                        if (breathingMinigame.insideTargetTimer > 0.20000000298023224)
                            breathingMinigame.stablizedTimer = Math.Max(0.0f, breathingMinigame.stablizedTimer - 0.5f);
                        if (breathingMinigame.stablizedTimer > 0.0)
                            breathingMinigame.stablizedTimer -= 0.5f * Engine.DeltaTime;
                        breathingMinigame.insideTargetTimer = 0.0f;
                        if (breathingMinigame.rumbler != null)
                            breathingMinigame.rumbler.Strength = (float) (0.5 * (1.0 - t));
                    }
                }
                else if (breathingMinigame.rumbler != null)
                    breathingMinigame.rumbler.Strength = 0.2f;
                float target = (float) (0.64999997615814209 + Math.Min(1f, t / 0.8f) * 0.35000002384185791);
                breathingMinigame.bgAlpha = Calc.Approach(breathingMinigame.bgAlpha, target, Engine.DeltaTime);
                breathingMinigame.featherSprite.Position = breathingMinigame.screenCenter + Vector2.UnitY * (breathingMinigame.feather - 128f);
                breathingMinigame.featherSprite.Play(breathingMinigame.insideTargetTimer > 0.0 || !breathingMinigame.boxEnabled ? "hover" : "flutter");
                breathingMinigame.particleAlpha = Calc.Approach(breathingMinigame.particleAlpha, 1f, Engine.DeltaTime);
                if (!breathingMinigame.winnable && breathingMinigame.stablizedTimer > 12.0)
                    breathingMinigame.losing = true;
                if (breathingMinigame.losing)
                {
                    breathingMinigame.losingTimer += Engine.DeltaTime / 5f;
                    if (breathingMinigame.losingTimer > 1.0)
                        break;
                }
                yield return null;
            }
            if (!breathingMinigame.winnable)
            {
                breathingMinigame.Pausing = true;
                while (breathingMinigame.Pausing)
                {
                    if (breathingMinigame.rumbler != null)
                        breathingMinigame.rumbler.Strength = Calc.Approach(breathingMinigame.rumbler.Strength, 1f, 2f * Engine.DeltaTime);
                    Sprite featherSprite = breathingMinigame.featherSprite;
                    featherSprite.Position += (breathingMinigame.screenCenter - breathingMinigame.featherSprite.Position) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                    breathingMinigame.boxAlpha -= Engine.DeltaTime * 10f;
                    breathingMinigame.particleAlpha = breathingMinigame.boxAlpha;
                    yield return null;
                }
                breathingMinigame.losing = false;
                breathingMinigame.losingTimer = 0.0f;
                yield return breathingMinigame.PopFeather();
            }
            else
            {
                breathingMinigame.bgAlpha = 1f;
                if (breathingMinigame.rumbler != null)
                {
                    breathingMinigame.rumbler.RemoveSelf();
                    breathingMinigame.rumbler = null;
                }
                while (breathingMinigame.boxAlpha > 0.0)
                {
                    yield return null;
                    breathingMinigame.boxAlpha -= Engine.DeltaTime;
                    breathingMinigame.particleAlpha = breathingMinigame.boxAlpha;
                }
                breathingMinigame.particleAlpha = 0.0f;
                yield return 2f;
                for (; breathingMinigame.featherAlpha > 0.0; breathingMinigame.featherAlpha -= Engine.DeltaTime)
                    yield return null;
                yield return 1f;
            }
            breathingMinigame.Completed = true;
            for (; breathingMinigame.bgAlpha > 0.0; breathingMinigame.bgAlpha -= Engine.DeltaTime * (breathingMinigame.winnable ? 1f : 10f))
                yield return null;
            breathingMinigame.RemoveSelf();
        }

        private IEnumerator ShowText(int num)
        {
            yield return FadeTextTo(0.0f);
            text = Dialog.Clean("CH4_GONDOLA_FEATHER_" + num);
            yield return 0.1f;
            yield return FadeTextTo(1f);
            while (!Input.MenuConfirm.Pressed)
                yield return null;
            yield return FadeTextTo(0.0f);
        }

        private IEnumerator FadeGameIn()
        {
            while (featherAlpha < 1.0)
            {
                featherAlpha += Engine.DeltaTime;
                yield return null;
            }
            featherAlpha = 1f;
        }

        private IEnumerator FadeBoxIn()
        {
            BreathingMinigame breathingMinigame = this;
            yield return (float) (breathingMinigame.winnable ? 5.0 : 2.0);
            while (Math.Abs(breathingMinigame.feather) > 300.0)
                yield return null;
            breathingMinigame.boxEnabled = true;
            breathingMinigame.Add(breathingMinigame.sine = new SineWave(0.12f));
            while (breathingMinigame.boxAlpha < 1.0)
            {
                breathingMinigame.boxAlpha += Engine.DeltaTime;
                yield return null;
            }
            breathingMinigame.boxAlpha = 1f;
        }

        private IEnumerator FadeTextTo(float v)
        {
            if (textAlpha != (double) v)
            {
                float from = textAlpha;
                for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime * 4f)
                {
                    yield return null;
                    textAlpha = from + (v - from) * p;
                }
                textAlpha = v;
            }
        }

        private IEnumerator PopFeather()
        {
            BreathingMinigame breathingMinigame = this;
            Audio.Play("event:/game/06_reflection/badeline_feather_slice");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            if (breathingMinigame.rumbler != null)
            {
                breathingMinigame.rumbler.RemoveSelf();
                breathingMinigame.rumbler = null;
            }
            breathingMinigame.featherSprite.Rotation = 0.0f;
            breathingMinigame.featherSprite.Play("hover");
            breathingMinigame.featherSprite.CenterOrigin();
            breathingMinigame.featherSprite.Y += breathingMinigame.featherSprite.Height / 2f;
            yield return 0.25f;
            breathingMinigame.featherSlice = new Image(GFX.Gui["feather/slice"]);
            breathingMinigame.featherSlice.CenterOrigin();
            breathingMinigame.featherSlice.Position = breathingMinigame.featherSprite.Position;
            breathingMinigame.featherSlice.Rotation = Calc.Angle(new Vector2(96f, 165f), new Vector2(140f, 112f));
            float p;
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime * 8f)
            {
                breathingMinigame.featherSlice.Scale.X = (float) ((0.25 + Calc.YoYo(p) * 0.75) * 8.0);
                breathingMinigame.featherSlice.Scale.Y = (float) ((0.5 + (1.0 - Calc.YoYo(p)) * 0.5) * 8.0);
                breathingMinigame.featherSlice.Position = breathingMinigame.featherSprite.Position + Vector2.Lerp(new Vector2(128f, sbyte.MinValue), new Vector2(sbyte.MinValue, 128f), p);
                yield return null;
            }
            breathingMinigame.featherSlice.Visible = false;
            (breathingMinigame.Scene as Level).Shake();
            (breathingMinigame.Scene as Level).Flash(Color.White);
            breathingMinigame.featherSprite.Visible = false;
            breathingMinigame.featherHalfLeft = new Image(GFX.Gui["feather/feather_half0"]);
            breathingMinigame.featherHalfLeft.CenterOrigin();
            breathingMinigame.featherHalfRight = new Image(GFX.Gui["feather/feather_half1"]);
            breathingMinigame.featherHalfRight.CenterOrigin();
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime)
            {
                breathingMinigame.featherHalfLeft.Position = breathingMinigame.featherSprite.Position + Vector2.Lerp(Vector2.Zero, new Vector2(sbyte.MinValue, -32f), p);
                breathingMinigame.featherHalfRight.Position = breathingMinigame.featherSprite.Position + Vector2.Lerp(Vector2.Zero, new Vector2(128f, 32f), p);
                breathingMinigame.featherAlpha = 1f - p;
                yield return null;
            }
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
            trailSpeed = Calc.Approach(trailSpeed, speed, (float) (Engine.DeltaTime * 200.0 * 8.0));
            if (featherWave != null)
                featherSprite.Rotation = (float) (featherWave.Value * 0.25 + 0.10000000149011612);
            for (int index = 0; index < particles.Length; ++index)
            {
                particles[index].Position.Y += particles[index].Speed * particleSpeed * Engine.DeltaTime;
                if (particleSpeed > -400.0)
                    particles[index].Position.X += (float) ((particleSpeed + 400.0) * Math.Sin(particles[index].Sin) * 0.10000000149011612) * Engine.DeltaTime;
                particles[index].Sin += Engine.DeltaTime;
                if (particles[index].Position.Y < (double) sbyte.MinValue || particles[index].Position.Y > 1208.0)
                {
                    particles[index].Reset();
                    particles[index].Position.Y = particleSpeed >= 0.0 ? sbyte.MinValue : 1208f;
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
                Sprite featherSprite = this.featherSprite;
                featherSprite.Position += new Vector2(Calc.Random.Range(-1, 1), Calc.Random.Range(-1, 1)).SafeNormalize() * losingTimer * 10f;
                this.featherSprite.Rotation += (float) (Calc.Random.Range(-1, 1) * (double) losingTimer * 0.10000000149011612);
            }
            this.featherSprite.Color = Color.White * featherAlpha;
            if (this.featherSprite.Visible)
                this.featherSprite.Render();
            if (featherSlice != null && featherSlice.Visible)
                featherSlice.Render();
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
            MagicGlow.Render(featherBuffer.Target, timer, (float) (-(double) trailSpeed / 200.0 * 2.0), Matrix.CreateScale(0.5f));
            GaussianBlur.Blur(smokeBuffer.Target, tempBuffer, smokeBuffer);
        }

        public override void Render()
        {
            Color color = insideTargetTimer > 0.20000000298023224 ? Color.White : Color.White * 0.6f;
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * bgAlpha);
            if (Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene))
                return;
            MTexture mtexture1 = GFX.Gui["feather/border"];
            MTexture mtexture2 = GFX.Gui["feather/box"];
            Vector2 scale1 = new Vector2((float) (mtexture1.Width * 2.0 - 32.0) / mtexture2.Width, (float) (currentTargetBounds * 2.0 - 32.0) / mtexture2.Height);
            mtexture2.DrawCentered(screenCenter + new Vector2(0.0f, currentTargetCenter), Color.White * boxAlpha * 0.25f, scale1);
            mtexture1.Draw(screenCenter + new Vector2(-mtexture1.Width, currentTargetCenter - currentTargetBounds), Vector2.Zero, color * boxAlpha, Vector2.One);
            mtexture1.Draw(screenCenter + new Vector2(mtexture1.Width, currentTargetCenter + currentTargetBounds), Vector2.Zero, color * boxAlpha, new Vector2(-1f, -1f));
            if (featherBuffer != null && !featherBuffer.IsDisposed)
            {
                float scale2 = 1920f / featherBuffer.Width;
                Draw.SpriteBatch.Draw(smokeBuffer.Target, Vector2.Zero, smokeBuffer.Bounds, Color.White * 0.3f, 0.0f, Vector2.Zero, scale2 * 2f, SpriteEffects.None, 0.0f);
                Draw.SpriteBatch.Draw(featherBuffer.Target, Vector2.Zero, featherBuffer.Bounds, Color.White, 0.0f, Vector2.Zero, scale2, SpriteEffects.None, 0.0f);
            }
            Vector2 vector2_1 = new Vector2(1f, 1f);
            if (particleSpeed < 0.0)
                vector2_1 = new Vector2(Math.Min(1f, (float) (1.0 / (-(double) particleSpeed * 0.0040000001899898052))), Math.Max(1f, (float) (1.0 * -(double) particleSpeed * 0.0040000001899898052)));
            for (int index = 0; index < particles.Length; ++index)
                particleTexture.DrawCentered(particles[index].Position, Color.White * (0.5f * particleAlpha), particles[index].Scale * vector2_1);
            if (!string.IsNullOrEmpty(text) && textAlpha > 0.0)
                ActiveFont.Draw(text, new Vector2(960f, 920f), new Vector2(0.5f, 0.5f), Vector2.One, Color.White * textAlpha);
            if (string.IsNullOrEmpty(text) || textAlpha < 1.0)
                return;
            Vector2 vector2_2 = ActiveFont.Measure(text);
            Vector2 position = new Vector2((float) ((1920.0 + vector2_2.X) / 2.0 + 40.0), (float) (920.0 + vector2_2.Y / 2.0 - 16.0)) + new Vector2(0.0f, timer % 1.0 < 0.25 ? 6f : 0.0f);
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
                return;
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
                float val = num * (num * num * num);
                Position = new Vector2(Calc.Random.NextFloat() * 1920f, Calc.Random.NextFloat() * 1080f);
                Scale = Calc.Map(val, 0.0f, 1f, 0.05f, 0.8f);
                Speed = Scale * Calc.Random.Range(2f, 8f);
                Sin = Calc.Random.NextFloat(6.28318548f);
            }
        }
    }
}
