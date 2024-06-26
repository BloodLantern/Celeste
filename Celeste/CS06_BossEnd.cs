﻿using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS06_BossEnd : CutsceneEntity
    {
        public const string Flag = "badeline_connection";
        private Player player;
        private NPC06_Badeline_Crying badeline;
        private float fade;
        private float pictureFade;
        private float pictureGlow;
        private MTexture picture;
        private bool waitForKeyPress;
        private float timer;
        private EventInstance sfx;

        public CS06_BossEnd(Player player, NPC06_Badeline_Crying badeline)
        {
            Tag = (int) Tags.HUD;
            this.player = player;
            this.badeline = badeline;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS06_BossEnd cs06BossEnd = this;
            cs06BossEnd.player.StateMachine.State = 11;
            cs06BossEnd.player.StateMachine.Locked = true;
            while (!cs06BossEnd.player.OnGround())
                yield return null;
            cs06BossEnd.player.Facing = Facings.Right;
            yield return 1f;
            Level level1 = cs06BossEnd.SceneAs<Level>();
            level1.Session.Audio.Music.Event = "event:/music/lvl6/badeline_acoustic";
            level1.Session.Audio.Apply();
            yield return Textbox.Say("ch6_boss_ending", cs06BossEnd.StartMusic, cs06BossEnd.PlayerHug, cs06BossEnd.BadelineCalmDown);
            yield return 0.5f;
            while ((cs06BossEnd.fade += Engine.DeltaTime) < 1.0)
                yield return null;
            cs06BossEnd.picture = GFX.Portraits["hug1"];
            cs06BossEnd.sfx = Audio.Play("event:/game/06_reflection/hug_image_1");
            yield return cs06BossEnd.PictureFade(1f);
            yield return cs06BossEnd.WaitForPress();
            cs06BossEnd.sfx = Audio.Play("event:/game/06_reflection/hug_image_2");
            yield return cs06BossEnd.PictureFade(0.0f, 0.5f);
            cs06BossEnd.picture = GFX.Portraits["hug2"];
            yield return cs06BossEnd.PictureFade(1f);
            yield return cs06BossEnd.WaitForPress();
            cs06BossEnd.sfx = Audio.Play("event:/game/06_reflection/hug_image_3");
            while ((cs06BossEnd.pictureGlow += Engine.DeltaTime / 2f) < 1.0)
                yield return null;
            yield return 0.2f;
            yield return cs06BossEnd.PictureFade(0.0f, 0.5f);
            while ((cs06BossEnd.fade -= Engine.DeltaTime * 12f) > 0.0)
                yield return null;
            level.Session.Audio.Music.Param("levelup", 1f);
            level.Session.Audio.Apply();
            cs06BossEnd.Add(new Coroutine(cs06BossEnd.badeline.TurnWhite(1f)));
            yield return 0.5f;
            cs06BossEnd.player.Sprite.Play("idle");
            yield return 0.25f;
            yield return cs06BossEnd.player.DummyWalkToExact((int) cs06BossEnd.player.X - 8, true);
            cs06BossEnd.Add(new Coroutine(cs06BossEnd.CenterCameraOnPlayer()));
            yield return cs06BossEnd.badeline.Disperse();
            (cs06BossEnd.Scene as Level).Session.SetFlag("badeline_connection");
            level.Flash(Color.White);
            level.Session.Inventory.Dashes = 2;
            cs06BossEnd.badeline.RemoveSelf();
            yield return 0.1f;
            level.Add(new LevelUpEffect(cs06BossEnd.player.Position));
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            yield return 2f;
            yield return level.ZoomBack(0.5f);
            cs06BossEnd.EndCutscene(level);
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator StartMusic()
                {
                Level level = SceneAs<Level>();
                level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_acoustic";
                level.Session.Audio.Apply();
                yield return 0.5f;
                }

                private IEnumerator PlayerHug()
        {
            CS06_BossEnd cs06BossEnd = this;
            cs06BossEnd.Add(new Coroutine(cs06BossEnd.Level.ZoomTo(cs06BossEnd.badeline.Center + new Vector2(0.0f, -24f) - cs06BossEnd.Level.Camera.Position, 2f, 0.5f)));
            yield return 0.6f;
            yield return cs06BossEnd.player.DummyWalkToExact((int) cs06BossEnd.badeline.X - 10);
            cs06BossEnd.player.Facing = Facings.Right;
            yield return 0.25f;
            cs06BossEnd.player.DummyAutoAnimate = false;
            cs06BossEnd.player.Sprite.Play("hug");
            yield return 0.5f;
        }

        private IEnumerator BadelineCalmDown()
        {
            CS06_BossEnd cs06BossEnd = this;
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 0.0f);
            cs06BossEnd.badeline.LoopingSfx.Param("end", 1f);
            yield return 0.5f;
            cs06BossEnd.badeline.Sprite.Play("scaredTransition");
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            FinalBossStarfield bossBg = cs06BossEnd.Level.Background.Get<FinalBossStarfield>();
            if (bossBg != null)
            {
                while (bossBg.Alpha > 0.0)
                {
                    bossBg.Alpha -= Engine.DeltaTime;
                    yield return null;
                }
            }
            yield return 1.5f;
        }

        private IEnumerator CenterCameraOnPlayer()
        {
            CS06_BossEnd cs06BossEnd = this;
            yield return 0.5f;
            Vector2 from = cs06BossEnd.Level.ZoomFocusPoint;
            Vector2 to = new Vector2(cs06BossEnd.Level.Bounds.Left + 580, cs06BossEnd.Level.Bounds.Top + 124) - cs06BossEnd.Level.Camera.Position;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime)
            {
                cs06BossEnd.Level.ZoomFocusPoint = from + (to - from) * Ease.SineInOut(p);
                yield return null;
            }
        }

        private IEnumerator PictureFade(float to, float duration = 1f)
        {
            while ((pictureFade = Calc.Approach(pictureFade, to, Engine.DeltaTime / duration)) != (double) to)
                yield return null;
        }

        private IEnumerator WaitForPress()
        {
            waitForKeyPress = true;
            while (!Input.MenuConfirm.Pressed)
                yield return null;
            waitForKeyPress = false;
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped && sfx != null)
                Audio.Stop(sfx);
            Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 0.0f);
            level.ResetZoom();
            level.Session.Inventory.Dashes = 2;
            level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_acoustic";
            if (WasSkipped)
                level.Session.Audio.Music.Param("levelup", 2f);
            level.Session.Audio.Apply();
            if (WasSkipped)
                level.Add(new LevelUpEffect(player.Position));
            player.DummyAutoAnimate = true;
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            FinalBossStarfield finalBossStarfield = Level.Background.Get<FinalBossStarfield>();
            if (finalBossStarfield != null)
                finalBossStarfield.Alpha = 0.0f;
            badeline.RemoveSelf();
            level.Session.SetFlag("badeline_connection");
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
            base.Update();
        }

        public override void Render()
        {
            if (fade <= 0.0)
                return;
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(fade) * 0.8f);
            if (picture == null || pictureFade <= 0.0)
                return;
            float num = Ease.CubeOut(pictureFade);
            Vector2 position = new Vector2(960f, 540f);
            float scale = (float) (1.0 + (1.0 - num) * 0.02500000037252903);
            picture.DrawCentered(position, Color.White * Ease.CubeOut(pictureFade), scale, 0.0f);
            if (pictureGlow > 0.0)
            {
                GFX.Portraits["hug-light2a"].DrawCentered(position, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
                GFX.Portraits["hug-light2b"].DrawCentered(position, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
                GFX.Portraits["hug-light2c"].DrawCentered(position, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
                HiresRenderer.EndRender();
                HiresRenderer.BeginRender(BlendState.Additive);
                GFX.Portraits["hug-light2a"].DrawCentered(position, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
                GFX.Portraits["hug-light2b"].DrawCentered(position, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
                GFX.Portraits["hug-light2c"].DrawCentered(position, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
                HiresRenderer.EndRender();
                HiresRenderer.BeginRender();
            }
            if (!waitForKeyPress)
                return;
            GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1520f, 880 + (timer % 1.0 < 0.25 ? 6 : 0)));
        }
    }
}
