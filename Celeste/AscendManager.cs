using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class AscendManager : Entity
    {
        private const string BeginSwapFlag = "beginswap_";
        private const string BgSwapFlag = "bgswap_";
        public readonly bool Dark;
        public readonly bool Ch9Ending;
        private readonly bool introLaunch;
        private readonly int index;
        private readonly string cutscene;
        private Level level;
        private float fade;
        private float scroll;
        private bool outTheTop;
        private Color background;
        private readonly string ambience;

        public AscendManager(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = (int) Tags.TransitionUpdate;
            Depth = 8900;
            index = data.Int(nameof (index));
            cutscene = data.Attr(nameof (cutscene));
            introLaunch = data.Bool("intro_launch");
            Dark = data.Bool("dark");
            Ch9Ending = cutscene.Equals("CH9_FREE_BIRD", StringComparison.InvariantCultureIgnoreCase);
            ambience = data.Attr(nameof (ambience));
            background = Dark ? Color.Black : Calc.HexToColor("75a0ab");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = Scene as Level;
            Add(new Coroutine(Routine()));
        }

        private IEnumerator Routine()
        {
            AscendManager manager = this;
            Player player = manager.Scene.Tracker.GetEntity<Player>();
            while (player == null || player.Y > (double) manager.Y)
            {
                player = manager.Scene.Tracker.GetEntity<Player>();
                yield return null;
            }
            if (manager.index == 9)
                yield return 1.6f;
            Streaks streaks = new(manager);
            manager.Scene.Add(streaks);
            if (!manager.Dark)
            {
                Clouds clouds = new(manager);
                manager.Scene.Add(clouds);
            }
            manager.level.Session.SetFlag("beginswap_" + manager.index);
            player.Sprite.Play("launch");
            player.Speed = Vector2.Zero;
            player.StateMachine.State = 11;
            player.DummyGravity = false;
            player.DummyAutoAnimate = false;
            if (!string.IsNullOrWhiteSpace(manager.ambience))
            {
                if (manager.ambience.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                    Audio.SetAmbience(null);
                else
                    Audio.SetAmbience(SFX.EventnameByHandle(manager.ambience));
            }
            if (manager.introLaunch)
            {
                manager.FadeSnapTo(1f);
                manager.level.Camera.Position = player.Center + new Vector2(-160f, -90f);
                yield return 2.3f;
            }
            else
            {
                yield return manager.FadeTo(1f, manager.Dark ? 2f : 0.8f);
                if (manager.Ch9Ending)
                {
                    manager.level.Add(new CS10_FreeBird());
                    while (true)
                        yield return null;
                }
                if (!string.IsNullOrEmpty(manager.cutscene))
                {
                    yield return 0.25f;
                    CS07_Ascend cs = new(manager.index, manager.cutscene, manager.Dark);
                    manager.level.Add(cs);
                    yield return null;
                    while (cs.Running)
                        yield return null;
                    cs = null;
                }
                else
                    yield return 0.5f;
            }
            manager.level.CanRetry = false;
            player.Sprite.Play("launch");
            Audio.Play("event:/char/madeline/summit_flytonext", player.Position);
            yield return 0.25f;
            Vector2 from = player.Position;
            float p;
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime / 1f)
            {
                player.Position = Vector2.Lerp(from, from + new Vector2(0.0f, 60f), Ease.CubeInOut(p)) + Calc.Random.ShakeVector();
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                yield return null;
            }
            Fader fader = new(manager);
            manager.Scene.Add(fader);
            from = player.Position;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime / 0.5f)
            {
                float y = player.Y;
                player.Position = Vector2.Lerp(from, from + new Vector2(0.0f, -160f), Ease.SineIn(p));
                if (p == 0.0 || Calc.OnInterval(player.Y, y, 16f))
                    manager.level.Add(Engine.Pooler.Create<SpeedRing>().Init(player.Center, new Vector2(0.0f, -1f).Angle(), Color.White));
                fader.Fade = p < 0.5 ? 0.0f : (float) ((p - 0.5) * 2.0);
                yield return null;
            }
            from = new Vector2();
            fader = null;
            manager.level.CanRetry = true;
            manager.outTheTop = true;
            player.Y = manager.level.Bounds.Top;
            player.SummitLaunch(player.X);
            player.DummyGravity = true;
            player.DummyAutoAnimate = true;
            manager.level.Session.SetFlag("bgswap_" + manager.index);
            manager.level.NextTransitionDuration = 0.05f;
            if (manager.introLaunch)
                manager.level.Add(new HeightDisplay(-1));
        }

        public override void Update()
        {
            scroll += Engine.DeltaTime * 240f;
            base.Update();
        }

        public override void Render() => Draw.Rect(level.Camera.X - 10f, level.Camera.Y - 10f, 340f, 200f, background * fade);

        public override void Removed(Scene scene)
        {
            FadeSnapTo(0.0f);
            level.Session.SetFlag("bgswap_" + index, false);
            level.Session.SetFlag("beginswap_" + index, false);
            if (outTheTop)
            {
                ScreenWipe.WipeColor = Dark ? Color.Black : Color.White;
                if (introLaunch)
                {
                    MountainWipe mountainWipe = new(Scene, true);
                }
                else if (index == 0)
                    AreaData.Get(1).DoScreenWipe(Scene, true);
                else if (index == 1)
                    AreaData.Get(2).DoScreenWipe(Scene, true);
                else if (index == 2)
                    AreaData.Get(3).DoScreenWipe(Scene, true);
                else if (index == 3)
                    AreaData.Get(4).DoScreenWipe(Scene, true);
                else if (index == 4)
                    AreaData.Get(5).DoScreenWipe(Scene, true);
                else if (index == 5)
                    AreaData.Get(7).DoScreenWipe(Scene, true);
                else if (index >= 9)
                    AreaData.Get(10).DoScreenWipe(Scene, true);
                ScreenWipe.WipeColor = Color.Black;
            }
            base.Removed(scene);
        }

        private IEnumerator FadeTo(float target, float duration = 0.8f)
        {
            while ((fade = Calc.Approach(fade, target, Engine.DeltaTime / duration)) != (double) target)
            {
                FadeSnapTo(fade);
                yield return null;
            }
            FadeSnapTo(target);
        }

        private void FadeSnapTo(float target)
        {
            fade = target;
            SetSnowAlpha(1f - fade);
            SetBloom(fade * 0.1f);
            if (!Dark)
                return;
            foreach (Parallax parallax in level.Background.GetEach<Parallax>())
                parallax.CameraOffset.Y -= 25f * target;
            foreach (Parallax parallax in level.Foreground.GetEach<Parallax>())
                parallax.Alpha = 1f - fade;
        }

        private void SetBloom(float add) => level.Bloom.Base = AreaData.Get(level).BloomBase + add;

        private void SetSnowAlpha(float value)
        {
            Snow snow = level.Foreground.Get<Snow>();
            if (snow != null)
                snow.Alpha = value;
            RainFG rainFg = level.Foreground.Get<RainFG>();
            if (rainFg != null)
                rainFg.Alpha = value;
            WindSnowFG windSnowFg = level.Foreground.Get<WindSnowFG>();
            if (windSnowFg == null)
                return;
            windSnowFg.Alpha = value;
        }

        private static float Mod(float x, float m) => (x % m + m) % m;

        public class Streaks : Entity
        {
            private const float MinSpeed = 600f;
            private const float MaxSpeed = 2000f;
            public float Alpha = 1f;
            private readonly Particle[] particles = new Particle[80];
            private readonly List<MTexture> textures;
            private readonly Color[] colors;
            private readonly Color[] alphaColors;
            private readonly AscendManager manager;

            public Streaks(AscendManager manager)
            {
                this.manager = manager;
                if (manager == null || !manager.Dark)
                    colors = new Color[2]
                    {
                        Color.White,
                        Calc.HexToColor("e69ecb")
                    };
                else
                    colors = new Color[2]
                    {
                        Calc.HexToColor("041b44"),
                        Calc.HexToColor("011230")
                    };
                Depth = 20;
                textures = GFX.Game.GetAtlasSubtextures("scenery/launch/slice");
                alphaColors = new Color[colors.Length];
                for (int index = 0; index < particles.Length; ++index)
                {
                    float x = (float) (160.0 + (double) Calc.Random.Range(24f, 144f) * Calc.Random.Choose(-1, 1));
                    float y = Calc.Random.NextFloat(436f);
                    float num = Calc.ClampedMap(Math.Abs(x - 160f), 0.0f, 160f, 0.25f) * Calc.Random.Range(600f, 2000f);
                    particles[index] = new Particle
                    {
                        Position = new Vector2(x, y),
                        Speed = num,
                        Index = Calc.Random.Next(textures.Count),
                        Color = Calc.Random.Next(colors.Length)
                    };
                }
            }

            public override void Update()
            {
                base.Update();
                for (int index = 0; index < particles.Length; ++index)
                    particles[index].Position.Y += particles[index].Speed * Engine.DeltaTime;
            }

            public override void Render()
            {
                float num = Ease.SineInOut((manager != null ? manager.fade : 1f) * Alpha);
                Vector2 position1 = (Scene as Level).Camera.Position;
                for (int index = 0; index < colors.Length; ++index)
                    alphaColors[index] = colors[index] * num;
                for (int index = 0; index < particles.Length; ++index)
                {
                    Vector2 position2 = particles[index].Position;
                    position2.X = AscendManager.Mod(position2.X, 320f);
                    position2.Y = AscendManager.Mod(position2.Y, 436f) - 128f;
                    Vector2 vector2_1 = position2 + position1;
                    Vector2 vector2_2 = new Vector2
                    {
                        X = Calc.ClampedMap(particles[index].Speed, 600f, 2000f, 1f, 0.25f),
                        Y = Calc.ClampedMap(particles[index].Speed, 600f, 2000f, 1f, 2f)
                    } * Calc.ClampedMap(particles[index].Speed, 600f, 2000f, 1f, 4f);
                    MTexture texture = textures[particles[index].Index];
                    Color alphaColor = alphaColors[particles[index].Color];
                    Vector2 position3 = vector2_1;
                    Color color = alphaColor;
                    Vector2 scale = vector2_2;
                    texture.DrawCentered(position3, color, scale);
                }
                Draw.Rect(position1.X - 10f, position1.Y - 10f, 26f, 200f, alphaColors[0]);
                Draw.Rect((float) (position1.X + 320.0 - 16.0), position1.Y - 10f, 26f, 200f, alphaColors[0]);
            }

            private class Particle
            {
                public Vector2 Position;
                public float Speed;
                public int Index;
                public int Color;
            }
        }

        public class Clouds : Entity
        {
            public float Alpha;
            private readonly AscendManager manager;
            private readonly List<MTexture> textures;
            private readonly Particle[] particles = new Particle[10];
            private Color color;

            public Clouds(AscendManager manager)
            {
                this.manager = manager;
                color = manager == null || !manager.Dark ? Calc.HexToColor("b64a86") : Calc.HexToColor("082644");
                Depth = -1000000;
                textures = GFX.Game.GetAtlasSubtextures("scenery/launch/cloud");
                for (int index = 0; index < particles.Length; ++index)
                    particles[index] = new Particle
                    {
                        Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(900f)),
                        Speed = Calc.Random.Range(400, 800),
                        Index = Calc.Random.Next(textures.Count)
                    };
            }

            public override void Update()
            {
                base.Update();
                for (int index = 0; index < particles.Length; ++index)
                    particles[index].Position.Y += particles[index].Speed * Engine.DeltaTime;
            }

            public override void Render()
            {
                Color color = this.color * ((manager != null ? manager.fade : 1f) * Alpha);
                Vector2 position1 = (Scene as Level).Camera.Position;
                for (int index = 0; index < particles.Length; ++index)
                {
                    Vector2 position2 = particles[index].Position;
                    position2.Y = AscendManager.Mod(position2.Y, 900f) - 360f;
                    Vector2 position3 = position2 + position1;
                    textures[particles[index].Index].DrawCentered(position3, color);
                }
            }

            private class Particle
            {
                public Vector2 Position;
                public float Speed;
                public int Index;
            }
        }

        private class Fader : Entity
        {
            public float Fade;
            private readonly AscendManager manager;

            public Fader(AscendManager manager)
            {
                this.manager = manager;
                Depth = -1000010;
            }

            public override void Render()
            {
                if (Fade <= 0.0)
                    return;
                Vector2 position = (Scene as Level).Camera.Position;
                Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, (manager.Dark ? Color.Black : Color.White) * Fade);
            }
        }
    }
}
