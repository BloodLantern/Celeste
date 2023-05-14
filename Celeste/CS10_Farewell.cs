// Decompiled with JetBrains decompiler
// Type: Celeste.CS10_Farewell
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
    public class CS10_Farewell : CutsceneEntity
    {
        private readonly Player player;
        private NPC granny;
        private float fade;
        private Coroutine grannyWalk;
        private EventInstance snapshot;
        private EventInstance dissipate;

        public CS10_Farewell(Player player)
            : base(false)
        {
            this.player = player;
            Depth = -1000000;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            level.TimerStopped = true;
            level.TimerHidden = true;
            level.SaveQuitDisabled = true;
            level.SnapColorGrade("none");
            snapshot = Audio.CreateSnapshot("snapshot:/game_10_granny_clouds_dialogue");
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS10_Farewell cs10Farewell = this;
            cs10Farewell.player.Dashes = 1;
            cs10Farewell.player.StateMachine.State = 11;
            cs10Farewell.player.Sprite.Play("idle");
            cs10Farewell.player.Visible = false;
            _ = Audio.SetMusic("event:/new_content/music/lvl10/granny_farewell");
            FadeWipe fadeWipe = new(cs10Farewell.Level, true)
            {
                Duration = 2f
            };
            ScreenWipe.WipeColor = Color.White;
            yield return fadeWipe.Duration;
            yield return 1.5f;
            cs10Farewell.Add(new Coroutine(cs10Farewell.Level.ZoomTo(new Vector2(160f, 125f), 2f, 5f)));
            yield return 0.2f;
            _ = Audio.Play("event:/new_content/char/madeline/screenentry_gran");
            yield return 0.3f;
            Vector2 position = cs10Farewell.player.Position;
            cs10Farewell.player.Position = new Vector2(cs10Farewell.player.X, level.Bounds.Bottom + 8);
            cs10Farewell.player.Speed.Y = -160f;
            cs10Farewell.player.Visible = true;
            cs10Farewell.player.DummyGravity = false;
            cs10Farewell.player.MuffleLanding = true;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            while (!cs10Farewell.player.OnGround() || cs10Farewell.player.Speed.Y < 0.0)
            {
                double y = cs10Farewell.player.Speed.Y;
                cs10Farewell.player.Speed.Y += (float)((double)Engine.DeltaTime * 900.0 * 0.20000000298023224);
                if (y < 0.0 && cs10Farewell.player.Speed.Y >= 0.0)
                {
                    cs10Farewell.player.Speed.Y = 0.0f;
                    yield return 0.2f;
                }
                yield return null;
            }
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            _ = Audio.Play("event:/new_content/char/madeline/screenentry_gran_landing", cs10Farewell.player.Position);
            cs10Farewell.granny = new NPC(cs10Farewell.player.Position + new Vector2(164f, 0.0f))
            {
                IdleAnim = "idle",
                MoveAnim = "walk",
                Maxspeed = 15f
            };
            cs10Farewell.granny.Add(cs10Farewell.granny.Sprite = GFX.SpriteBank.Create("granny"));
            cs10Farewell.granny.Add(new GrannyLaughSfx(cs10Farewell.granny.Sprite)
            {
                FirstPlay = false
            });
            // ISSUE: reference to a compiler-generated method
            cs10Farewell.granny.Sprite.OnFrameChange = delegate (string anim)
            {
                int currentAnimationFrame = granny.Sprite.CurrentAnimationFrame;
                if (anim == "walk" && currentAnimationFrame == 2)
                {
                    float volume = Calc.ClampedMap((player.Position - granny.Position).Length(), 64f, 128f, 1f, 0f);
                    _ = Audio.Play("event:/new_content/char/granny/cane_tap_ending", granny.Position).setVolume(volume);
                }
            };
            cs10Farewell.Scene.Add(cs10Farewell.granny);
            cs10Farewell.grannyWalk = new Coroutine(cs10Farewell.granny.MoveTo(cs10Farewell.player.Position + new Vector2(32f, 0.0f)));
            cs10Farewell.Add(cs10Farewell.grannyWalk);
            yield return 2f;
            cs10Farewell.player.Facing = Facings.Left;
            yield return 1.6f;
            cs10Farewell.player.Facing = Facings.Right;
            yield return 0.8f;
            yield return cs10Farewell.player.DummyWalkToExact((int)cs10Farewell.player.X + 4, speedMultiplier: 0.4f);
            yield return 0.8f;
            yield return Textbox.Say("CH9_FAREWELL", new Func<IEnumerator>(cs10Farewell.Laugh), new Func<IEnumerator>(cs10Farewell.StopLaughing), new Func<IEnumerator>(cs10Farewell.StepForward), new Func<IEnumerator>(cs10Farewell.GrannyDisappear), new Func<IEnumerator>(cs10Farewell.FadeToWhite), new Func<IEnumerator>(cs10Farewell.WaitForGranny));
            yield return 2f;
            while (cs10Farewell.fade < 1.0)
            {
                yield return null;
            }

            cs10Farewell.EndCutscene(level);
        }

        private IEnumerator WaitForGranny()
        {
            while (grannyWalk != null && !grannyWalk.Finished)
            {
                yield return null;
            }
        }

        private IEnumerator Laugh()
        {
            granny.Sprite.Play("laugh");
            yield break;
        }

        private IEnumerator StopLaughing()
        {
            granny.Sprite.Play("idle");
            yield break;
        }

        private IEnumerator StepForward()
        {
            yield return player.DummyWalkToExact((int)player.X + 8, speedMultiplier: 0.4f);
        }

        private IEnumerator GrannyDisappear()
        {
            CS10_Farewell cs10Farewell = this;
            Audio.SetMusicParam("end", 1f);
            cs10Farewell.Add(new Coroutine(cs10Farewell.player.DummyWalkToExact((int)cs10Farewell.player.X + 8, speedMultiplier: 0.4f)));
            yield return 0.1f;
            cs10Farewell.dissipate = Audio.Play("event:/new_content/char/granny/dissipate", cs10Farewell.granny.Position);
            MTexture frame = cs10Farewell.granny.Sprite.GetFrame(cs10Farewell.granny.Sprite.CurrentAnimationID, cs10Farewell.granny.Sprite.CurrentAnimationFrame);
            cs10Farewell.Level.Add(new DisperseImage(cs10Farewell.granny.Position, new Vector2(1f, -0.1f), cs10Farewell.granny.Sprite.Origin, cs10Farewell.granny.Sprite.Scale, frame));
            yield return null;
            cs10Farewell.granny.Visible = false;
            yield return 3.5f;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator FadeToWhite()
        {
            base.Add(new Coroutine(DoFadeToWhite(), true));
            yield break;
        }

        private IEnumerator DoFadeToWhite()
        {
            CS10_Farewell cs10Farewell = this;
            cs10Farewell.Add(new Coroutine(cs10Farewell.Level.ZoomBack(8f)));
            while (cs10Farewell.fade < 1.0)
            {
                cs10Farewell.fade = Calc.Approach(cs10Farewell.fade, 1f, Engine.DeltaTime / 8f);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            Dispose();
            if (WasSkipped)
            {
                Audio.Stop(dissipate);
            }

            Level.OnEndOfFrame += () =>
            {
                //Achievements.Register(Achievement.FAREWELL);
                Level.TeleportTo(player, "end-cinematic", Player.IntroTypes.Transition);
            };
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        private void Dispose()
        {
            Audio.ReleaseSnapshot(snapshot);
            snapshot = null;
        }

        public override void Render()
        {
            if (fade <= 0.0)
            {
                return;
            }

            Draw.Rect(Level.Camera.X - 1f, Level.Camera.Y - 1f, 322f, 182f, Color.White * fade);
        }
    }
}
