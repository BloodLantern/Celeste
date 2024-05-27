using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS00_Ending : CutsceneEntity
    {
        private Player player;
        private BirdNPC bird;
        private Bridge bridge;
        private bool keyOffed;
        private PrologueEndingText endingText;

        public CS00_Ending(Player player, BirdNPC bird, Bridge bridge)
            : base(false, true)
        {
            this.player = player;
            this.bird = bird;
            this.bridge = bridge;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS00_Ending cs00Ending = this;
            for (; Engine.TimeRate > 0.0; Engine.TimeRate -= Engine.RawDeltaTime * 2f)
            {
                yield return null;
                if (Engine.TimeRate < 0.5 && cs00Ending.bridge != null)
                    cs00Ending.bridge.StopCollapseLoop();
                level.StopShake();
                MInput.GamePads[Input.Gamepad].StopRumble();
            }
            Engine.TimeRate = 0.0f;
            cs00Ending.player.StateMachine.State = 11;
            cs00Ending.player.Facing = Facings.Right;
            yield return cs00Ending.WaitFor(1f);
            EventInstance instance = Audio.Play("event:/game/general/bird_in", cs00Ending.bird.Position);
            cs00Ending.bird.Facing = Facings.Left;
            cs00Ending.bird.Sprite.Play("fall");
            float percent = 0.0f;
            Vector2 from = cs00Ending.bird.Position;
            Vector2 to = cs00Ending.bird.StartPosition;
            while (percent < 1.0)
            {
                cs00Ending.bird.Position = from + (to - from) * Ease.QuadOut(percent);
                Audio.Position(instance, cs00Ending.bird.Position);
                if (percent > 0.5)
                    cs00Ending.bird.Sprite.Play("fly");
                percent += Engine.RawDeltaTime * 0.5f;
                yield return null;
            }
            cs00Ending.bird.Position = to;
            instance = null;
            from = new Vector2();
            to = new Vector2();
            Audio.Play("event:/game/general/bird_land_dirt", cs00Ending.bird.Position);
            Dust.Burst(cs00Ending.bird.Position, -MathHelper.Pi / 2, 12);
            cs00Ending.bird.Sprite.Play("idle");
            yield return cs00Ending.WaitFor(0.5f);
            cs00Ending.bird.Sprite.Play("peck");
            yield return cs00Ending.WaitFor(1.1f);
            yield return cs00Ending.bird.ShowTutorial(new BirdTutorialGui(cs00Ending.bird, new Vector2(0.0f, -16f), Dialog.Clean("tutorial_dash"), new Vector2(1f, -1f), "+", BirdTutorialGui.ButtonPrompt.Dash), true);
            while (true)
            {
                Vector2 aimVector = Input.GetAimVector();
                if (aimVector.X <= 0.0 || aimVector.Y >= 0.0 || !Input.Dash.Pressed)
                    yield return null;
                else
                    break;
            }
            cs00Ending.player.StateMachine.State = 16;
            cs00Ending.player.Dashes = 0;
            level.Session.Inventory.Dashes = 1;
            Engine.TimeRate = 1f;
            cs00Ending.keyOffed = true;
            int num1 = (int) Audio.CurrentMusicEventInstance.triggerCue();
            cs00Ending.bird.Add(new Coroutine(cs00Ending.bird.HideTutorial()));
            yield return 0.25f;
            cs00Ending.bird.Add(new Coroutine(cs00Ending.bird.StartleAndFlyAway()));
            while (!cs00Ending.player.Dead && !cs00Ending.player.OnGround())
                yield return null;
            yield return 2f;
            Audio.SetMusic("event:/music/lvl0/title_ping");
            yield return 2f;
            cs00Ending.endingText = new PrologueEndingText(false);
            cs00Ending.Scene.Add(cs00Ending.endingText);
            Snow bgSnow = level.Background.Get<Snow>();
            Snow fgSnow = level.Foreground.Get<Snow>();
            level.Add(level.HiresSnow = new HiresSnow());
            level.HiresSnow.Alpha = 0f;
            float ease = 0f;
            while (ease < 1)
            {
                ease += Engine.DeltaTime * 0.25f;
                float num2 = Ease.CubeInOut(ease);
                if (fgSnow != null)
                    fgSnow.Alpha -= Engine.DeltaTime * 0.5f;
                if (bgSnow != null)
                    bgSnow.Alpha -= Engine.DeltaTime * 0.5f;
                level.HiresSnow.Alpha = Calc.Approach(level.HiresSnow.Alpha, 1f, Engine.DeltaTime * 0.5f);
                cs00Ending.endingText.Position = new Vector2(960f, (float) (540.0 - 1080.0 * (1.0 - num2)));
                level.Camera.Y = level.Bounds.Top - 3900f * num2;
                yield return null;
            }
            cs00Ending.EndCutscene(level);
        }

        private IEnumerator WaitFor(float time)
        {
            for (float t = 0.0f; t < (double) time; t += Engine.RawDeltaTime)
                yield return null;
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                if (bird != null)
                    bird.Visible = false;
                if (player != null)
                {
                    player.Position = new Vector2(2120f, 40f);
                    player.StateMachine.State = 11;
                    player.DummyAutoAnimate = false;
                    player.Sprite.Play("tired");
                    player.Speed = Vector2.Zero;
                }
                if (!keyOffed)
                {
                    int num = (int) Audio.CurrentMusicEventInstance.triggerCue();
                }
                if (level.HiresSnow == null)
                    level.Add(level.HiresSnow = new HiresSnow());
                level.HiresSnow.Alpha = 1f;
                Snow snow1 = level.Background.Get<Snow>();
                if (snow1 != null)
                    snow1.Alpha = 0.0f;
                Snow snow2 = level.Foreground.Get<Snow>();
                if (snow2 != null)
                    snow2.Alpha = 0.0f;
                if (endingText != null)
                    level.Remove(endingText);
                level.Add(endingText = new PrologueEndingText(true));
                endingText.Position = new Vector2(960f, 540f);
                level.Camera.Y = level.Bounds.Top - 3900;
            }
            Engine.TimeRate = 1f;
            level.PauseLock = true;
            level.Entities.FindFirst<SpeedrunTimerDisplay>().CompleteTimer = 10f;
            level.Add(new EndingCutsceneDelay());
        }

                // ISSUE: reference to a compiler-generated field
                private class EndingCutsceneDelay : Entity
                {
                        // Token: 0x0600286F RID: 10351 RVA: 0x00106847 File Offset: 0x00104A47
                        public EndingCutsceneDelay()
                        {
                                Add(new Coroutine(Routine()));
                        }

                        // Token: 0x06002870 RID: 10352 RVA: 0x00106861 File Offset: 0x00104A61
                        private IEnumerator Routine()
                        {
                                yield return 3f;
                                (Scene as Level).CompleteArea(false);
                        }
                }
        }
}
