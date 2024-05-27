using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste
{
    public class ForsakenCitySatellite : Entity
    {
        private const string UnlockedFlag = "unlocked_satellite";
        public static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>
        {
            {
                "U",
                Calc.HexToColor("f0f0f0")
            },
            {
                "L",
                Calc.HexToColor("9171f2")
            },
            {
                "DR",
                Calc.HexToColor("0a44e0")
            },
            {
                "UR",
                Calc.HexToColor("b32d00")
            },
            {
                "UL",
                Calc.HexToColor("ffcd37")
            }
        };
        public static readonly Dictionary<string, string> Sounds = new Dictionary<string, string>
        {
            {
                "U",
                "event:/game/01_forsaken_city/console_white"
            },
            {
                "L",
                "event:/game/01_forsaken_city/console_purple"
            },
            {
                "DR",
                "event:/game/01_forsaken_city/console_blue"
            },
            {
                "UR",
                "event:/game/01_forsaken_city/console_red"
            },
            {
                "UL",
                "event:/game/01_forsaken_city/console_yellow"
            }
        };
        public static readonly Dictionary<string, ParticleType> Particles = new Dictionary<string, ParticleType>();
        private static readonly string[] Code = new string[6]
        {
            "U",
            "L",
            "DR",
            "UR",
            "L",
            "UL"
        };
        private static List<string> uniqueCodes = new List<string>();
        private bool enabled;
        private List<string> currentInputs = new List<string>();
        private List<CodeBird> birds = new List<CodeBird>();
        private Vector2 gemSpawnPosition;
        private Vector2 birdFlyPosition;
        private Image sprite;
        private Image pulse;
        private Image computer;
        private Image computerScreen;
        private Sprite computerScreenNoise;
        private Image computerScreenShine;
        private BloomPoint pulseBloom;
        private BloomPoint screenBloom;
        private Level level;
        private DashListener dashListener;
        private SoundSource birdFlyingSfx;
        private SoundSource birdThrustSfx;
        private SoundSource birdFinishSfx;
        private SoundSource staticLoopSfx;

        public ForsakenCitySatellite(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sprite = new Image(GFX.Game["objects/citysatellite/dish"]));
            Add(pulse = new Image(GFX.Game["objects/citysatellite/light"]));
            Add(computer = new Image(GFX.Game["objects/citysatellite/computer"]));
            Add(computerScreen = new Image(GFX.Game["objects/citysatellite/computerscreen"]));
            Add(computerScreenNoise = new Sprite(GFX.Game, "objects/citysatellite/computerScreenNoise"));
            Add(computerScreenShine = new Image(GFX.Game["objects/citysatellite/computerscreenShine"]));
            sprite.JustifyOrigin(0.5f, 1f);
            pulse.JustifyOrigin(0.5f, 1f);
            Add(new Coroutine(PulseRoutine()));
            Add(pulseBloom = new BloomPoint(new Vector2(-12f, -44f), 1f, 8f));
            Add(screenBloom = new BloomPoint(new Vector2(32f, 20f), 1f, 8f));
            computerScreenNoise.AddLoop("static", "", 0.05f);
            computerScreenNoise.Play("static");
            computer.Position = computerScreen.Position = computerScreenShine.Position = computerScreenNoise.Position = new Vector2(8f, 8f);
            birdFlyPosition = offset + data.Nodes[0];
            gemSpawnPosition = offset + data.Nodes[1];
            Add(dashListener = new DashListener());
            dashListener.OnDash = dir =>
            {
                string str = "";
                if (dir.Y < 0.0)
                    str = "U";
                else if (dir.Y > 0.0)
                    str = "D";
                if (dir.X < 0.0)
                    str += "L";
                else if (dir.X > 0.0)
                    str += "R";
                currentInputs.Add(str);
                if (currentInputs.Count > ForsakenCitySatellite.Code.Length)
                    currentInputs.RemoveAt(0);
                if (currentInputs.Count != ForsakenCitySatellite.Code.Length)
                    return;
                bool flag = true;
                for (int index = 0; index < ForsakenCitySatellite.Code.Length; ++index)
                {
                    if (!currentInputs[index].Equals(ForsakenCitySatellite.Code[index]))
                        flag = false;
                }
                if (!flag || level.Camera.Left + 32.0 >= gemSpawnPosition.X || !enabled)
                    return;
                Add(new Coroutine(UnlockGem()));
            };
            foreach (string str in ForsakenCitySatellite.Code)
            {
                if (!ForsakenCitySatellite.uniqueCodes.Contains(str))
                    ForsakenCitySatellite.uniqueCodes.Add(str);
            }
            Depth = 8999;
            Add(staticLoopSfx = new SoundSource());
            staticLoopSfx.Position = computer.Position;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            enabled = !level.Session.HeartGem && !level.Session.GetFlag("unlocked_satellite");
            if (enabled)
            {
                foreach (string uniqueCode in ForsakenCitySatellite.uniqueCodes)
                {
                    CodeBird codeBird = new CodeBird(birdFlyPosition, uniqueCode);
                    birds.Add(codeBird);
                    level.Add(codeBird);
                }
                Add(birdFlyingSfx = new SoundSource());
                Add(birdFinishSfx = new SoundSource());
                Add(birdThrustSfx = new SoundSource());
                birdFlyingSfx.Position = birdFlyPosition - Position;
                birdFlyingSfx.Play("event:/game/01_forsaken_city/birdbros_fly_loop");
            }
            else
                staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
            if (level.Session.HeartGem || !level.Session.GetFlag("unlocked_satellite"))
                return;
            level.Add(new HeartGem(gemSpawnPosition));
        }

        public override void Update()
        {
            base.Update();
            computerScreenNoise.Visible = !pulse.Visible;
            computerScreen.Visible = pulse.Visible;
            screenBloom.Visible = pulseBloom.Visible;
        }

        private IEnumerator PulseRoutine()
        {
            ForsakenCitySatellite forsakenCitySatellite = this;
            forsakenCitySatellite.pulseBloom.Visible = forsakenCitySatellite.pulse.Visible = false;
            while (forsakenCitySatellite.enabled)
            {
                yield return 2f;
                for (int i = 0; i < ForsakenCitySatellite.Code.Length && forsakenCitySatellite.enabled; ++i)
                {
                    forsakenCitySatellite.pulse.Color = forsakenCitySatellite.computerScreen.Color = ForsakenCitySatellite.Colors[ForsakenCitySatellite.Code[i]];
                    forsakenCitySatellite.pulseBloom.Visible = forsakenCitySatellite.pulse.Visible = true;
                    Audio.Play(ForsakenCitySatellite.Sounds[ForsakenCitySatellite.Code[i]], forsakenCitySatellite.Position + forsakenCitySatellite.computer.Position);
                    yield return 0.5f;
                    forsakenCitySatellite.pulseBloom.Visible = forsakenCitySatellite.pulse.Visible = false;
                    Audio.Play(i < ForsakenCitySatellite.Code.Length - 1 ? "event:/game/01_forsaken_city/console_static_short" : "event:/game/01_forsaken_city/console_static_long", forsakenCitySatellite.Position + forsakenCitySatellite.computer.Position);
                    yield return 0.2f;
                }
                // ISSUE: reference to a compiler-generated method
                forsakenCitySatellite.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
                {
                        if (enabled)
                        {
                                birdThrustSfx.Position = birdFlyPosition - Position;
                                birdThrustSfx.Play("event:/game/01_forsaken_city/birdbros_thrust");
                        }
                }, 1.1f, true));
                forsakenCitySatellite.birds.Shuffle();
                foreach (CodeBird bird in forsakenCitySatellite.birds)
                {
                    if (forsakenCitySatellite.enabled)
                    {
                        bird.Dash();
                        yield return 0.02f;
                    }
                }
            }
            forsakenCitySatellite.pulseBloom.Visible = forsakenCitySatellite.pulse.Visible = false;
        }

        private IEnumerator UnlockGem()
        {
            ForsakenCitySatellite forsakenCitySatellite = this;
            forsakenCitySatellite.level.Session.SetFlag("unlocked_satellite");
            forsakenCitySatellite.birdFinishSfx.Position = forsakenCitySatellite.birdFlyPosition - forsakenCitySatellite.Position;
            forsakenCitySatellite.birdFinishSfx.Play("event:/game/01_forsaken_city/birdbros_finish");
            forsakenCitySatellite.staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
            forsakenCitySatellite.enabled = false;
            yield return 0.25f;
            forsakenCitySatellite.level.Displacement.Clear();
            yield return null;
            forsakenCitySatellite.birdFlyingSfx.Stop();
            forsakenCitySatellite.level.Frozen = true;
            forsakenCitySatellite.Tag = (int) Tags.FrozenUpdate;
            BloomPoint bloom = new BloomPoint(forsakenCitySatellite.birdFlyPosition - forsakenCitySatellite.Position, 0.0f, 32f);
            forsakenCitySatellite.Add(bloom);
            foreach (CodeBird bird in forsakenCitySatellite.birds)
                bird.Transform(3f);
            while (bloom.Alpha < 1.0)
            {
                bloom.Alpha += Engine.DeltaTime / 3f;
                yield return null;
            }
            yield return 0.25f;
            foreach (Entity bird in forsakenCitySatellite.birds)
                bird.RemoveSelf();
            ParticleSystem particles = new ParticleSystem(-10000, 100);
            particles.Tag = (int) Tags.FrozenUpdate;
            particles.Emit(BirdNPC.P_Feather, 24, forsakenCitySatellite.birdFlyPosition, new Vector2(4f, 4f));
            forsakenCitySatellite.level.Add(particles);
            HeartGem gem = new HeartGem(forsakenCitySatellite.birdFlyPosition);
            gem.Tag = (int) Tags.FrozenUpdate;
            forsakenCitySatellite.level.Add(gem);
            yield return null;
            gem.ScaleWiggler.Start();
            yield return 0.85f;
            SimpleCurve curve = new SimpleCurve(gem.Position, forsakenCitySatellite.gemSpawnPosition, (gem.Position + forsakenCitySatellite.gemSpawnPosition) / 2f + new Vector2(0.0f, -64f));
            for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime)
            {
                yield return null;
                gem.Position = curve.GetPoint(Ease.CubeInOut(t));
            }
            yield return 0.5f;
            particles.RemoveSelf();
            forsakenCitySatellite.Remove(bloom);
            forsakenCitySatellite.level.Frozen = false;
        }

        private class CodeBird : Entity
        {
            private Sprite sprite;
            private Coroutine routine;
            private float timer = Calc.Random.NextFloat();
            private Vector2 speed;
            private Image heartImage;
            private readonly string code;
            private readonly Vector2 origin;
            private readonly Vector2 dash;

            public CodeBird(Vector2 origin, string code)
                : base(origin)
            {
                this.code = code;
                this.origin = origin;
                Add(sprite = new Sprite(GFX.Game, "scenery/flutterbird/"));
                sprite.AddLoop("fly", "flap", 0.08f);
                sprite.Play("fly");
                sprite.CenterOrigin();
                sprite.Color = ForsakenCitySatellite.Colors[code];
                dash = (Vector2.Zero with
                {
                    X = (code.Contains('L') ? -1f : (code.Contains('R') ? 1f : 0.0f)),
                    Y = (code.Contains('U') ? -1f : (code.Contains('D') ? 1f : 0.0f))
                }).SafeNormalize();
                Add(routine = new Coroutine(AimlessFlightRoutine()));
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                sprite.Y = (float) Math.Sin(timer * 2.0);
                base.Update();
            }

            public void Dash() => routine.Replace(DashRoutine());

            public void Transform(float duration)
            {
                Tag = (int) Tags.FrozenUpdate;
                routine.Replace(TransformRoutine(duration));
            }

            private IEnumerator AimlessFlightRoutine()
            {
                CodeBird codeBird = this;
                codeBird.speed = Vector2.Zero;
                while (true)
                {
                    Vector2 target = codeBird.origin + Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 16f + Calc.Random.NextFloat(40f));
                    float reset = 0.0f;
                    while (reset < 1.0 && (target - codeBird.Position).Length() > 8.0)
                    {
                        Vector2 vector2 = (target - codeBird.Position).SafeNormalize();
                        codeBird.speed += vector2 * 420f * Engine.DeltaTime;
                        if (codeBird.speed.Length() > 90.0)
                            codeBird.speed = codeBird.speed.SafeNormalize(90f);
                        codeBird.Position += codeBird.speed * Engine.DeltaTime;
                        reset += Engine.DeltaTime;
                        if (Math.Sign(vector2.X) != 0)
                            codeBird.sprite.Scale.X = Math.Sign(vector2.X);
                        yield return null;
                    }
                    target = new Vector2();
                }
            }

            private IEnumerator DashRoutine()
            {
                CodeBird codeBird = this;
                float t;
                for (t = 0.25f; t > 0.0; t -= Engine.DeltaTime)
                {
                    codeBird.speed = Calc.Approach(codeBird.speed, Vector2.Zero, 200f * Engine.DeltaTime);
                    codeBird.Position += codeBird.speed * Engine.DeltaTime;
                    yield return null;
                }
                Vector2 from = codeBird.Position;
                Vector2 to = codeBird.origin + codeBird.dash * 8f;
                if (Math.Sign(to.X - from.X) != 0)
                    codeBird.sprite.Scale.X = Math.Sign(to.X - from.X);
                for (t = 0.0f; t < 1.0; t += Engine.DeltaTime * 1.5f)
                {
                    codeBird.Position = from + (to - from) * Ease.CubeInOut(t);
                    yield return null;
                }
                codeBird.Position = to;
                yield return 0.2f;
                from = new Vector2();
                to = new Vector2();
                if (codeBird.dash.X != 0.0)
                    codeBird.sprite.Scale.X = Math.Sign(codeBird.dash.X);
                (codeBird.Scene as Level).Displacement.AddBurst(codeBird.Position, 0.25f, 4f, 24f, 0.4f);
                codeBird.speed = codeBird.dash * 300f;
                for (t = 0.4f; t > 0.0; t -= Engine.DeltaTime)
                {
                    if (t > 0.10000000149011612 && codeBird.Scene.OnInterval(0.02f))
                        codeBird.SceneAs<Level>().ParticlesBG.Emit(ForsakenCitySatellite.Particles[codeBird.code], 1, codeBird.Position, Vector2.One * 2f, codeBird.dash.Angle());
                    codeBird.speed = Calc.Approach(codeBird.speed, Vector2.Zero, 800f * Engine.DeltaTime);
                    codeBird.Position += codeBird.speed * Engine.DeltaTime;
                    yield return null;
                }
                yield return 0.4f;
                codeBird.routine.Replace(codeBird.AimlessFlightRoutine());
            }

            private IEnumerator TransformRoutine(float duration)
            {
                CodeBird codeBird = this;
                Color colorFrom = codeBird.sprite.Color;
                Color colorTo = Color.White;
                Vector2 target = codeBird.origin;
                codeBird.Add(codeBird.heartImage = new Image(GFX.Game["collectables/heartGem/shape"]));
                codeBird.heartImage.CenterOrigin();
                codeBird.heartImage.Scale = Vector2.Zero;
                for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime / duration)
                {
                    Vector2 vector2 = (target - codeBird.Position).SafeNormalize();
                    codeBird.speed += 400f * vector2 * Engine.DeltaTime;
                    float length = Math.Max(20f, (float) ((1.0 - t) * 200.0));
                    if (codeBird.speed.Length() > (double) length)
                        codeBird.speed = codeBird.speed.SafeNormalize(length);
                    codeBird.Position += codeBird.speed * Engine.DeltaTime;
                    codeBird.sprite.Color = Color.Lerp(colorFrom, colorTo, t);
                    codeBird.heartImage.Scale = Vector2.One * Math.Max(0.0f, (float) ((t - 0.75) * 4.0));
                    if (vector2.X != 0.0)
                        codeBird.sprite.Scale.X = Math.Abs(codeBird.sprite.Scale.X) * Math.Sign(vector2.X);
                    codeBird.sprite.Scale.X = Math.Sign(codeBird.sprite.Scale.X) * (1f - codeBird.heartImage.Scale.X);
                    codeBird.sprite.Scale.Y = 1f - codeBird.heartImage.Scale.X;
                    yield return null;
                }
            }
        }
    }
}
