using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS10_MissTheBird : CutsceneEntity
    {
        public const string Flag = "MissTheBird";
        private Player player;
        private FlingBirdIntro flingBird;
        private BirdNPC bird;
        private Coroutine zoomRoutine;
        private EventInstance crashMusicSfx;

        public CS10_MissTheBird(Player player, FlingBirdIntro flingBird)
        {
            this.player = player;
            this.flingBird = flingBird;
            Add(new LevelEndingHook(() => Audio.Stop(crashMusicSfx)));
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS10_MissTheBird cs10MissTheBird = this;
            Audio.SetMusicParam("bird_grab", 1f);
            cs10MissTheBird.crashMusicSfx = Audio.Play("event:/new_content/music/lvl10/cinematic/bird_crash_first");
            yield return cs10MissTheBird.flingBird.DoGrabbingRoutine(cs10MissTheBird.player);
            cs10MissTheBird.bird = new BirdNPC(cs10MissTheBird.flingBird.Position, BirdNPC.Modes.None);
            level.Add(cs10MissTheBird.bird);
            cs10MissTheBird.flingBird.RemoveSelf();
            yield return null;
            level.ResetZoom();
            level.Shake(0.5f);
            cs10MissTheBird.player.Position = cs10MissTheBird.player.Position.Floor();
            cs10MissTheBird.player.DummyGravity = true;
            cs10MissTheBird.player.DummyAutoAnimate = false;
            cs10MissTheBird.player.DummyFriction = false;
            cs10MissTheBird.player.ForceCameraUpdate = true;
            cs10MissTheBird.player.Speed = new Vector2(200f, 200f);
            BirdNPC bird = cs10MissTheBird.bird;
            bird.Position += Vector2.UnitX * 16f;
            cs10MissTheBird.bird.Add(new Coroutine(cs10MissTheBird.bird.Startle(null, 0.5f, new Vector2(3f, 0.25f))));
            // ISSUE: reference to a compiler-generated method
            cs10MissTheBird.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
            {
                    Add(new Coroutine(this.bird.FlyAway(0.2f)));
                    this.bird.Position += new Vector2(0f, -4f);
            }, 0.8f, true));
            while (!cs10MissTheBird.player.OnGround())
            cs10MissTheBird.player.MoveVExact(1);
            Engine.TimeRate = 0.5f;
            cs10MissTheBird.player.Sprite.Play("roll");
            while (cs10MissTheBird.player.Speed.X != 0.0)
            {
                cs10MissTheBird.player.Speed.X = Calc.Approach(cs10MissTheBird.player.Speed.X, 0.0f, 120f * Engine.DeltaTime);
                if (cs10MissTheBird.Scene.OnInterval(0.1f))
                    Dust.BurstFG(cs10MissTheBird.player.Position, -1.57079637f, 2);
                yield return null;
            }
            while (Engine.TimeRate < 1.0)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 4f * Engine.DeltaTime);
                yield return null;
            }
            cs10MissTheBird.player.Speed.X = 0.0f;
            cs10MissTheBird.player.DummyFriction = true;
            yield return 0.25f;
            cs10MissTheBird.Add(cs10MissTheBird.zoomRoutine = new Coroutine(level.ZoomTo(new Vector2(160f, 110f), 1.5f, 6f)));
            yield return 1.5f;
            cs10MissTheBird.player.Sprite.Play("rollGetUp");
            yield return 0.5f;
            cs10MissTheBird.player.ForceCameraUpdate = false;
            yield return Textbox.Say("CH9_MISS_THE_BIRD", cs10MissTheBird.StandUpFaceLeft, cs10MissTheBird.TakeStepLeft, cs10MissTheBird.TakeStepRight, cs10MissTheBird.FlickerBlackhole, cs10MissTheBird.OpenBlackhole);
            cs10MissTheBird.StartMusic();
            cs10MissTheBird.EndCutscene(level);
        }

        private IEnumerator StandUpFaceLeft()
        {
            while (!zoomRoutine.Finished)
                yield return null;
            yield return 0.2f;
            Audio.Play("event:/char/madeline/stand", player.Position);
            player.DummyAutoAnimate = true;
            player.Sprite.Play("idle");
            yield return 0.2f;
            player.Facing = Facings.Left;
            yield return 0.5f;
        }

        private IEnumerator TakeStepLeft()
        {
            yield return player.DummyWalkTo(player.X - 16f);
        }

        private IEnumerator TakeStepRight()
        {
            yield return player.DummyWalkTo(player.X + 32f);
        }

        private IEnumerator FlickerBlackhole()
        {
            yield return 0.5f;
            Audio.Play("event:/new_content/game/10_farewell/glitch_medium");
            yield return MoonGlitchBackgroundTrigger.GlitchRoutine(0.5f, false);
            yield return player.DummyWalkTo(player.X - 8f, true);
            yield return 0.4f;
        }

        private IEnumerator OpenBlackhole()
        {
            CS10_MissTheBird cs10MissTheBird = this;
            yield return 0.2f;
            cs10MissTheBird.Level.ResetZoom();
            cs10MissTheBird.Level.Flash(Color.White);
            cs10MissTheBird.Level.Shake(0.4f);
            Level level1 = cs10MissTheBird.Level;
            double x1 = cs10MissTheBird.player.X;
            Rectangle bounds = cs10MissTheBird.Level.Bounds;
            double top1 = bounds.Top;
            LightningStrike lightningStrike1 = new LightningStrike(new Vector2((float) x1, (float) top1), 80, 240f);
            level1.Add(lightningStrike1);
            Level level2 = cs10MissTheBird.Level;
            double x2 = cs10MissTheBird.player.X - 100.0;
            bounds = cs10MissTheBird.Level.Bounds;
            double top2 = bounds.Top;
            LightningStrike lightningStrike2 = new LightningStrike(new Vector2((float) x2, (float) top2), 90, 240f, 0.5f);
            level2.Add(lightningStrike2);
            Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
            cs10MissTheBird.TriggerEnvironmentalEvents();
            cs10MissTheBird.StartMusic();
            yield return 1.2f;
        }

        private void StartMusic()
        {
            Level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/part03";
            Level.Session.Audio.Ambience.Event = "event:/new_content/env/10_voidspiral";
            Level.Session.Audio.Apply();
        }

        private void TriggerEnvironmentalEvents()
        {
            CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
            if (cutsceneNode != null)
                RumbleTrigger.ManuallyTrigger(cutsceneNode.X, 0.0f);
            Scene.Entities.FindFirst<MoonGlitchBackgroundTrigger>()?.Invoke();
        }

        public override void OnEnd(Level level)
        {
            Audio.Stop(crashMusicSfx);
            Engine.TimeRate = 1f;
            level.Session.SetFlag("MissTheBird");
            if (WasSkipped)
            {
                player.Sprite.Play("idle");
                CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
                if (cutsceneNode != null)
                {
                    player.Position = cutsceneNode.Position.Floor();
                    level.Camera.Position = player.CameraTarget;
                }
                if (flingBird != null)
                {
                    if (flingBird.CrashSfxEmitter != null)
                        flingBird.CrashSfxEmitter.RemoveSelf();
                    flingBird.RemoveSelf();
                }
                if (bird != null)
                    bird.RemoveSelf();
                TriggerEnvironmentalEvents();
                StartMusic();
            }
            player.Speed = Vector2.Zero;
            player.DummyAutoAnimate = true;
            player.DummyFriction = true;
            player.DummyGravity = true;
            player.ForceCameraUpdate = false;
            player.StateMachine.State = 0;
        }
    }
}
