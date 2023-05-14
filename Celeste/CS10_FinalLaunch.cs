// Decompiled with JetBrains decompiler
// Type: Celeste.CS10_FinalLaunch
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS10_FinalLaunch : CutsceneEntity
    {
        private readonly Player player;
        private readonly BadelineBoost boost;
        private BirdNPC bird;
        private float fadeToWhite;
        private Vector2 birdScreenPosition;
        private AscendManager.Streaks streaks;
        private Vector2 cameraWaveOffset;
        private Vector2 cameraOffset;
        private float timer;
        private Coroutine wave;
        private bool hasGolden;
        private readonly bool sayDialog;

        public CS10_FinalLaunch(Player player, BadelineBoost boost, bool sayDialog = true)
            : base(false)
        {
            this.player = player;
            this.boost = boost;
            this.sayDialog = sayDialog;
            Depth = 10010;
        }

        public override void OnBegin(Level level)
        {
            _ = Audio.SetMusic(null);
            ScreenWipe.WipeColor = Color.White;
            foreach (Component follower in player.Leader.Followers)
            {
                if (follower.Entity is Strawberry entity && entity.Golden)
                {
                    hasGolden = true;
                    break;
                }
            }
            Add(new Coroutine(Cutscene()));
        }

        private IEnumerator Cutscene()
        {
            CS10_FinalLaunch cs10FinalLaunch = this;
            Engine.TimeRate = 1f;
            cs10FinalLaunch.boost.Active = false;
            yield return null;
            yield return cs10FinalLaunch.sayDialog ? Textbox.Say("CH9_LAST_BOOST") : 0.152f;
            cs10FinalLaunch.cameraOffset = new Vector2(0.0f, -20f);
            cs10FinalLaunch.boost.Active = true;
            cs10FinalLaunch.player.EnforceLevelBounds = false;
            yield return null;
            BlackholeBG blackholeBg = cs10FinalLaunch.Level.Background.Get<BlackholeBG>();
            blackholeBg.Direction = -2.5f;
            blackholeBg.SnapStrength(cs10FinalLaunch.Level, BlackholeBG.Strengths.High);
            blackholeBg.CenterOffset.Y = 100f;
            blackholeBg.OffsetOffset.Y = -50f;
            cs10FinalLaunch.Add(cs10FinalLaunch.wave = new Coroutine(cs10FinalLaunch.WaveCamera()));
            cs10FinalLaunch.Add(new Coroutine(cs10FinalLaunch.BirdRoutine(0.8f)));
            cs10FinalLaunch.Level.Add(cs10FinalLaunch.streaks = new AscendManager.Streaks(null));
            float p1;
            for (p1 = 0.0f; (double)p1 < 1.0; p1 += Engine.DeltaTime / 12f)
            {
                cs10FinalLaunch.fadeToWhite = p1;
                cs10FinalLaunch.streaks.Alpha = p1;
                foreach (Backdrop backdrop in cs10FinalLaunch.Level.Foreground.GetEach<Parallax>("blackhole"))
                {
                    backdrop.FadeAlphaMultiplier = 1f - p1;
                }

                yield return null;
            }
            while (cs10FinalLaunch.bird != null)
            {
                yield return null;
            }

            FadeWipe wipe = new(cs10FinalLaunch.Level, false)
            {
                Duration = 4f
            };
            ScreenWipe.WipeColor = Color.White;
            if (!cs10FinalLaunch.hasGolden)
            {
                _ = Audio.SetMusic("event:/new_content/music/lvl10/granny_farewell");
            }

            p1 = cs10FinalLaunch.cameraOffset.Y;
            int to = 180;
            for (float p2 = 0.0f; (double)p2 < 1.0; p2 += Engine.DeltaTime / 2f)
            {
                cs10FinalLaunch.cameraOffset.Y = p1 + ((to - p1) * Ease.BigBackIn(p2));
                yield return null;
            }
            while (wipe.Percent < 1.0)
            {
                yield return null;
            }

            cs10FinalLaunch.EndCutscene(cs10FinalLaunch.Level);
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped && boost != null && boost.Ch9FinalBoostSfx != null)
            {
                int num1 = (int)boost.Ch9FinalBoostSfx.stop(STOP_MODE.ALLOWFADEOUT);
                int num2 = (int)boost.Ch9FinalBoostSfx.release();
            }
            string nextLevelName = "end-granny";
            Player.IntroTypes nextLevelIntro = Player.IntroTypes.Transition;
            if (hasGolden)
            {
                nextLevelName = "end-golden";
                nextLevelIntro = Player.IntroTypes.Jump;
            }
            player.Active = true;
            player.Speed = Vector2.Zero;
            player.EnforceLevelBounds = true;
            player.StateMachine.State = 0;
            player.DummyFriction = true;
            player.DummyGravity = true;
            player.DummyAutoAnimate = true;
            player.ForceCameraUpdate = false;
            Engine.TimeRate = 1f;
            Level.OnEndOfFrame += () =>
            {
                Level.TeleportTo(player, nextLevelName, nextLevelIntro);
                if (!hasGolden)
                {
                    return;
                }

                Level.Wipe?.Cancel();
                Level.SnapColorGrade("golden");
                new FadeWipe(level, true).Duration = 2f;
                ScreenWipe.WipeColor = Color.White;
            };
        }

        private IEnumerator WaveCamera()
        {
            float timer = 0.0f;
            while (true)
            {
                cameraWaveOffset.X = (float)Math.Sin((double)timer) * 16f;
                cameraWaveOffset.Y = (float)((Math.Sin((double)timer * 0.5) * 16.0) + (Math.Sin((double)timer * 0.25) * 8.0));
                timer += Engine.DeltaTime * 2f;
                yield return null;
            }
        }

        private IEnumerator BirdRoutine(float delay)
        {
            CS10_FinalLaunch cs10FinalLaunch = this;
            yield return delay;
            cs10FinalLaunch.Level.Add(cs10FinalLaunch.bird = new BirdNPC(Vector2.Zero, BirdNPC.Modes.None));
            cs10FinalLaunch.bird.Sprite.Play("flyupIdle");
            Vector2 vector2 = new Vector2(320f, 180f) / 2f;
            Vector2 topCenter = new(vector2.X, 0.0f);
            Vector2 from = new Vector2(vector2.X, 180f) + new Vector2(40f, 40f);
            Vector2 to = vector2 + new Vector2(-32f, -24f);
            float t;
            for (t = 0.0f; (double)t < 1.0; t += Engine.DeltaTime / 4f)
            {
                cs10FinalLaunch.birdScreenPosition = from + ((to - from) * Ease.BackOut(t));
                yield return null;
            }
            cs10FinalLaunch.bird.Sprite.Play("flyupRoll");
            for (t = 0.0f; (double)t < 1.0; t += Engine.DeltaTime / 2f)
            {
                cs10FinalLaunch.birdScreenPosition = to + (new Vector2(64f, 0.0f) * Ease.CubeInOut(t));
                yield return null;
            }
            _ = new Vector2();
            _ = new Vector2();
            to = cs10FinalLaunch.birdScreenPosition;
            from = topCenter + new Vector2(-40f, -100f);
            bool playedAnim = false;
            for (t = 0.0f; (double)t < 1.0; t += Engine.DeltaTime / 4f)
            {
                if ((double)t >= 0.34999999403953552 && !playedAnim)
                {
                    cs10FinalLaunch.bird.Sprite.Play("flyupRoll");
                    playedAnim = true;
                }
                cs10FinalLaunch.birdScreenPosition = to + ((from - to) * Ease.BigBackIn(t));
                cs10FinalLaunch.birdScreenPosition.X += t * 32f;
                yield return null;
            }
            cs10FinalLaunch.bird.RemoveSelf();
            cs10FinalLaunch.bird = null;
            _ = new Vector2();
            _ = new Vector2();
        }

        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime;
            if (bird != null)
            {
                bird.Position = Level.Camera.Position + birdScreenPosition;
                bird.Position.X += (float)Math.Sin(timer) * 4f;
                bird.Position.Y += (float)((Math.Sin(timer * 0.10000000149011612) * 4.0) + (Math.Sin(timer * 0.25) * 4.0));
            }
            Level.CameraOffset = cameraOffset + cameraWaveOffset;
        }

        public override void Render()
        {
            Camera camera = Level.Camera;
            Draw.Rect(camera.X - 1f, camera.Y - 1f, 322f, 322f, Color.White * fadeToWhite);
        }
    }
}
