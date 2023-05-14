// Decompiled with JetBrains decompiler
// Type: Celeste.CS10_CatchTheBird
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
    public class CS10_CatchTheBird : CutsceneEntity
    {
        private readonly Player player;
        private readonly FlingBirdIntro flingBird;
        private BadelineDummy badeline;
        private BirdNPC bird;
        private Vector2 birdWaitPosition;
        private EventInstance snapshot;

        public CS10_CatchTheBird(Player player, FlingBirdIntro flingBird)
            : base()
        {
            this.player = player;
            this.flingBird = flingBird;
            birdWaitPosition = flingBird.BirdEndPosition;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS10_CatchTheBird cs10CatchTheBird = this;
            _ = Audio.SetMusic("event:/new_content/music/lvl10/cinematic/bird_crash_second");
            BadelineBoost boost = cs10CatchTheBird.Scene.Entities.FindFirst<BadelineBoost>();
            if (boost != null)
            {
                boost.Active = boost.Visible = boost.Collidable = false;
            }

            yield return cs10CatchTheBird.flingBird.DoGrabbingRoutine(cs10CatchTheBird.player);
            cs10CatchTheBird.flingBird.Sprite.Play("hurt");
            cs10CatchTheBird.flingBird.X += 8f;
            while (!cs10CatchTheBird.player.OnGround())
            {
                _ = cs10CatchTheBird.player.MoveVExact(1);
            }

            while (cs10CatchTheBird.player.CollideCheck<Solid>())
            {
                --cs10CatchTheBird.player.Y;
            }

            Engine.TimeRate = 0.65f;
            float ground = cs10CatchTheBird.player.Position.Y;
            cs10CatchTheBird.player.Dashes = 1;
            cs10CatchTheBird.player.Sprite.Play("roll");
            cs10CatchTheBird.player.Speed.X = 200f;
            cs10CatchTheBird.player.DummyFriction = false;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime)
            {
                cs10CatchTheBird.player.Speed.X = Calc.Approach(cs10CatchTheBird.player.Speed.X, 0.0f, 160f * Engine.DeltaTime);
                if (cs10CatchTheBird.player.Speed.X != 0.0 && cs10CatchTheBird.Scene.OnInterval(0.1f))
                {
                    Dust.BurstFG(cs10CatchTheBird.player.Position, -1.57079637f, 2);
                }

                cs10CatchTheBird.flingBird.Position.X += Engine.DeltaTime * 80f * Ease.CubeOut(1f - p);
                cs10CatchTheBird.flingBird.Position.Y = ground;
                yield return null;
            }
            cs10CatchTheBird.player.Speed.X = 0.0f;
            cs10CatchTheBird.player.DummyFriction = true;
            cs10CatchTheBird.player.DummyGravity = true;
            yield return 0.25f;
            while (Engine.TimeRate < 1.0)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 4f * Engine.DeltaTime);
                yield return null;
            }
            cs10CatchTheBird.player.ForceCameraUpdate = false;
            yield return 0.6f;
            cs10CatchTheBird.player.Sprite.Play("rollGetUp");
            yield return 0.8f;
            level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/reconciliation";
            level.Session.Audio.Apply();
            yield return Textbox.Say("CH9_CATCH_THE_BIRD", new Func<IEnumerator>(cs10CatchTheBird.BirdLooksHurt), new Func<IEnumerator>(cs10CatchTheBird.BirdSquakOnGround), new Func<IEnumerator>(cs10CatchTheBird.ApproachBird), new Func<IEnumerator>(cs10CatchTheBird.ApproachBirdAgain), new Func<IEnumerator>(cs10CatchTheBird.BadelineAppears), new Func<IEnumerator>(cs10CatchTheBird.WaitABeat), new Func<IEnumerator>(cs10CatchTheBird.MadelineSits), new Func<IEnumerator>(cs10CatchTheBird.BadelineHugs), new Func<IEnumerator>(cs10CatchTheBird.StandUp), new Func<IEnumerator>(cs10CatchTheBird.ShiftCameraToBird));
            yield return level.ZoomBack(0.5f);
            cs10CatchTheBird.badeline?.Vanish();
            yield return 0.5f;
            if (boost != null)
            {
                _ = cs10CatchTheBird.Level.Displacement.AddBurst(boost.Center, 0.5f, 8f, 32f, 0.5f);
                _ = Audio.Play("event:/new_content/char/badeline/booster_first_appear", boost.Center);
                boost.Active = boost.Visible = boost.Collidable = true;
                yield return 0.2f;
            }
            cs10CatchTheBird.EndCutscene(level);
        }

        private IEnumerator BirdTwitches(string sfx = null)
        {
            flingBird.Sprite.Scale.Y = 1.6f;
            if (!string.IsNullOrWhiteSpace(sfx))
            {
                _ = Audio.Play(sfx, flingBird.Position);
            }

            while (flingBird.Sprite.Scale.Y > 1.0)
            {
                flingBird.Sprite.Scale.Y = Calc.Approach(flingBird.Sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
                yield return null;
            }
        }

        private IEnumerator BirdLooksHurt()
        {
            yield return 0.8f;
            yield return BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_1");
            yield return 0.4f;
            yield return BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_2");
            yield return 0.5f;
        }

        private IEnumerator BirdSquakOnGround()
        {
            CS10_CatchTheBird cs10CatchTheBird = this;
            yield return 0.6f;
            yield return cs10CatchTheBird.BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_3");
            yield return 0.8f;
            _ = Audio.Play("event:/new_content/game/10_farewell/bird_crashscene_recover", cs10CatchTheBird.flingBird.Position);
            cs10CatchTheBird.flingBird.RemoveSelf();
            cs10CatchTheBird.Scene.Add(cs10CatchTheBird.bird = new BirdNPC(cs10CatchTheBird.flingBird.Position, BirdNPC.Modes.None));
            cs10CatchTheBird.bird.Facing = Facings.Right;
            cs10CatchTheBird.bird.Sprite.Play("recover");
            yield return 0.6f;
            cs10CatchTheBird.bird.Facing = Facings.Left;
            cs10CatchTheBird.bird.Sprite.Play("idle");
            cs10CatchTheBird.bird.X += 3f;
            yield return 0.4f;
            yield return cs10CatchTheBird.bird.Caw();
        }

        private IEnumerator ApproachBird()
        {
            CS10_CatchTheBird cs10CatchTheBird = this;
            cs10CatchTheBird.player.DummyAutoAnimate = true;
            yield return 0.25f;
            yield return cs10CatchTheBird.bird.Caw();
            cs10CatchTheBird.Add(new Coroutine(cs10CatchTheBird.player.DummyWalkTo(cs10CatchTheBird.player.X + 20f)));
            yield return 0.1f;
            _ = Audio.Play("event:/game/general/bird_startle", cs10CatchTheBird.bird.Position);
            yield return cs10CatchTheBird.bird.Startle("event:/new_content/game/10_farewell/bird_crashscene_relocate");
            yield return cs10CatchTheBird.bird.FlyTo(new Vector2(cs10CatchTheBird.player.X + 80f, cs10CatchTheBird.player.Y), 3f, false);
        }

        private IEnumerator ApproachBirdAgain()
        {
            CS10_CatchTheBird cs10CatchTheBird = this;
            _ = Audio.Play("event:/new_content/game/10_farewell/bird_crashscene_leave", cs10CatchTheBird.bird.Position);
            cs10CatchTheBird.Add(new Coroutine(cs10CatchTheBird.bird.FlyTo(cs10CatchTheBird.birdWaitPosition, 2f, false)));
            yield return cs10CatchTheBird.player.DummyWalkTo(cs10CatchTheBird.player.X + 20f);
            cs10CatchTheBird.snapshot = Audio.CreateSnapshot("snapshot:/game_10_bird_wings_silenced");
            yield return 0.8f;
            cs10CatchTheBird.bird.RemoveSelf();
            cs10CatchTheBird.Scene.Add(cs10CatchTheBird.bird = new BirdNPC(cs10CatchTheBird.birdWaitPosition, BirdNPC.Modes.WaitForLightningOff));
            cs10CatchTheBird.bird.Facing = Facings.Right;
            cs10CatchTheBird.bird.FlyAwayUp = false;
            cs10CatchTheBird.bird.WaitForLightningPostDelay = 1f;
        }

        private IEnumerator BadelineAppears()
        {
            CS10_CatchTheBird cs10CatchTheBird = this;
            yield return cs10CatchTheBird.player.DummyWalkToExact((int)cs10CatchTheBird.player.X + 20, speedMultiplier: 0.5f);
            cs10CatchTheBird.Level.Add(cs10CatchTheBird.badeline = new BadelineDummy(cs10CatchTheBird.player.Position + new Vector2(24f, -8f)));
            _ = cs10CatchTheBird.Level.Displacement.AddBurst(cs10CatchTheBird.badeline.Center, 0.5f, 8f, 32f, 0.5f);
            _ = Audio.Play("event:/char/badeline/maddy_split", cs10CatchTheBird.player.Position);
            cs10CatchTheBird.badeline.Sprite.Scale.X = -1f;
            yield return 0.2f;
        }

        private IEnumerator WaitABeat()
        {
            yield return player.DummyWalkToExact((int)player.X - 4, true, 0.5f);
            yield return 0.8f;
        }

        private IEnumerator MadelineSits()
        {
            yield return 0.5f;
            yield return player.DummyWalkToExact((int)player.X - 16, speedMultiplier: 0.25f);
            player.DummyAutoAnimate = false;
            player.Sprite.Play("sitDown");
            yield return 1.5f;
        }

        private IEnumerator BadelineHugs()
        {
            yield return 1f;
            yield return badeline.FloatTo(badeline.Position + new Vector2(0.0f, 8f), quickEnd: true);
            badeline.Floatness = 0.0f;
            badeline.AutoAnimator.Enabled = false;
            badeline.Sprite.Play("idle");
            _ = Audio.Play("event:/char/badeline/landing", badeline.Position);
            yield return 0.5f;
            yield return badeline.WalkTo(player.X - 9f, 40f);
            badeline.Sprite.Scale.X = 1f;
            yield return 0.2f;
            _ = Audio.Play("event:/char/badeline/duck", badeline.Position);
            badeline.Depth = player.Depth + 5;
            badeline.Sprite.Play("hug");
            yield return 1f;
        }

        private IEnumerator StandUp()
        {
            CS10_CatchTheBird cs10CatchTheBird = this;
            _ = Audio.Play("event:/char/badeline/stand", cs10CatchTheBird.badeline.Position);
            yield return cs10CatchTheBird.badeline.WalkTo(cs10CatchTheBird.badeline.X - 8f);
            cs10CatchTheBird.badeline.Sprite.Scale.X = 1f;
            yield return 0.2f;
            cs10CatchTheBird.player.DummyAutoAnimate = true;
            cs10CatchTheBird.Level.NextColorGrade("none", 0.25f);
            yield return 0.25f;
        }

        private IEnumerator ShiftCameraToBird()
        {
            CS10_CatchTheBird cs10CatchTheBird = this;
            Audio.ReleaseSnapshot(cs10CatchTheBird.snapshot);
            cs10CatchTheBird.snapshot = null;
            _ = Audio.Play("event:/new_content/char/badeline/birdcrash_scene_float", cs10CatchTheBird.badeline.Position);
            cs10CatchTheBird.Add(new Coroutine(cs10CatchTheBird.badeline.FloatTo(cs10CatchTheBird.player.Position + new Vector2(-16f, -16f), new int?(1))));
            Level scene = cs10CatchTheBird.Scene as Level;
            cs10CatchTheBird.player.Facing = Facings.Right;
            yield return scene.ZoomAcross(scene.ZoomFocusPoint + new Vector2(70f, 0.0f), 1.5f, 1f);
            yield return 0.4;
        }

        public override void OnEnd(Level level)
        {
            Audio.ReleaseSnapshot(snapshot);
            snapshot = null;
            if (WasSkipped)
            {
                CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
                if (cutsceneNode != null)
                {
                    player.Sprite.Play("idle");
                    player.Position = cutsceneNode.Position.Floor();
                    level.Camera.Position = player.CameraTarget;
                }
                foreach (Lightning lightning in Scene.Entities.FindAll<Lightning>())
                {
                    lightning.ToggleCheck();
                }

                Scene.Tracker.GetEntity<LightningRenderer>()?.ToggleEdges(true);
                level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/reconciliation";
                level.Session.Audio.Apply();
            }
            player.Speed = Vector2.Zero;
            player.DummyGravity = true;
            player.DummyFriction = true;
            player.DummyAutoAnimate = true;
            player.ForceCameraUpdate = false;
            player.StateMachine.State = 0;
            BadelineBoost first = Scene.Entities.FindFirst<BadelineBoost>();
            if (first != null)
            {
                first.Active = first.Visible = first.Collidable = true;
            }

            badeline?.RemoveSelf();
            if (flingBird != null)
            {
                flingBird.CrashSfxEmitter?.RemoveSelf();
                flingBird.RemoveSelf();
            }
            if (WasSkipped)
            {
                bird?.RemoveSelf();
                Scene.Add(bird = new BirdNPC(birdWaitPosition, BirdNPC.Modes.WaitForLightningOff));
                bird.Facing = Facings.Right;
                bird.FlyAwayUp = false;
                bird.WaitForLightningPostDelay = 1f;
                level.SnapColorGrade("none");
            }
            level.ResetZoom();
        }

        public override void Removed(Scene scene)
        {
            Audio.ReleaseSnapshot(snapshot);
            snapshot = null;
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Audio.ReleaseSnapshot(snapshot);
            snapshot = null;
            base.SceneEnd(scene);
        }

        public static void HandlePostCutsceneSpawn(FlingBirdIntro flingBird, Level level)
        {
            BadelineBoost first = level.Entities.FindFirst<BadelineBoost>();
            if (first != null)
            {
                first.Active = first.Visible = first.Collidable = true;
            }

            flingBird?.RemoveSelf();
            BirdNPC birdNpc;
            level.Add(birdNpc = new BirdNPC(flingBird.BirdEndPosition, BirdNPC.Modes.WaitForLightningOff));
            birdNpc.Facing = Facings.Right;
            birdNpc.FlyAwayUp = false;
        }
    }
}
