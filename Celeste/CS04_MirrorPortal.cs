using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS04_MirrorPortal : CutsceneEntity
    {
        private Player player;
        private TempleMirrorPortal portal;
        private Fader fader;
        private SoundSource sfx;

        public CS04_MirrorPortal(Player player, TempleMirrorPortal portal)
        {
            this.player = player;
            this.portal = portal;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
            level.Add(fader = new Fader());
        }

        private IEnumerator Cutscene(Level level)
        {
            CS04_MirrorPortal cs04MirrorPortal = this;
            cs04MirrorPortal.player.StateMachine.State = 11;
            cs04MirrorPortal.player.StateMachine.Locked = true;
            cs04MirrorPortal.player.Dashes = 1;
            if (level.Session.Area.Mode == AreaMode.Normal)
                Audio.SetMusic(null);
            else
                cs04MirrorPortal.Add(new Coroutine(cs04MirrorPortal.MusicFadeOutBSide()));
            cs04MirrorPortal.Add(cs04MirrorPortal.sfx = new SoundSource());
            cs04MirrorPortal.sfx.Position = cs04MirrorPortal.portal.Center;
            cs04MirrorPortal.sfx.Play("event:/music/lvl5/mirror_cutscene");
            cs04MirrorPortal.Add(new Coroutine(cs04MirrorPortal.CenterCamera()));
            yield return cs04MirrorPortal.player.DummyWalkToExact((int) cs04MirrorPortal.portal.X);
            yield return 0.25f;
            yield return cs04MirrorPortal.player.DummyWalkToExact((int) cs04MirrorPortal.portal.X - 16);
            yield return 0.5f;
            yield return cs04MirrorPortal.player.DummyWalkToExact((int) cs04MirrorPortal.portal.X + 16);
            yield return 0.25f;
            cs04MirrorPortal.player.Facing = Facings.Left;
            yield return 0.25f;
            yield return cs04MirrorPortal.player.DummyWalkToExact((int) cs04MirrorPortal.portal.X);
            yield return 0.1f;
            cs04MirrorPortal.player.DummyAutoAnimate = false;
            cs04MirrorPortal.player.Sprite.Play("lookUp");
            yield return 1f;
            cs04MirrorPortal.player.DummyAutoAnimate = true;
            cs04MirrorPortal.portal.Activate();
            cs04MirrorPortal.Add(new Coroutine(level.ZoomTo(new Vector2(160f, 90f), 3f, 12f)));
            yield return 0.25f;
            cs04MirrorPortal.player.ForceStrongWindHair.X = -1f;
            yield return cs04MirrorPortal.player.DummyWalkToExact((int) cs04MirrorPortal.player.X + 12, true);
            yield return 0.5f;
            cs04MirrorPortal.player.Facing = Facings.Right;
            cs04MirrorPortal.player.DummyAutoAnimate = false;
            cs04MirrorPortal.player.DummyGravity = false;
            cs04MirrorPortal.player.Sprite.Play("runWind");
            while (cs04MirrorPortal.player.Sprite.Rate > 0.0)
            {
                cs04MirrorPortal.player.MoveH(cs04MirrorPortal.player.Sprite.Rate * 10f * Engine.DeltaTime);
                cs04MirrorPortal.player.MoveV((float) (-(1.0 - cs04MirrorPortal.player.Sprite.Rate) * 6.0) * Engine.DeltaTime);
                cs04MirrorPortal.player.Sprite.Rate -= Engine.DeltaTime * 0.15f;
                yield return null;
            }
            yield return 0.5f;
            cs04MirrorPortal.player.Sprite.Play("fallFast");
            cs04MirrorPortal.player.Sprite.Rate = 1f;
            Vector2 target = cs04MirrorPortal.portal.Center + new Vector2(0.0f, 8f);
            Vector2 from = cs04MirrorPortal.player.Position;
            float p;
            for (p = 0.0f; p < 1.0; p += Engine.DeltaTime * 2f)
            {
                cs04MirrorPortal.player.Position = from + (target - from) * Ease.SineInOut(p);
                yield return null;
            }
            cs04MirrorPortal.player.ForceStrongWindHair.X = 0.0f;
            target = new Vector2();
            from = new Vector2();
            cs04MirrorPortal.fader.Target = 1f;
            yield return 2f;
            cs04MirrorPortal.player.Sprite.Play("sleep");
            yield return 1f;
            yield return level.ZoomBack(1f);
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                level.Session.ColorGrade = "templevoid";
                for (p = 0.0f; p < 1.0; p += Engine.DeltaTime)
                {
                    Glitch.Value = p * 0.05f;
                    level.ScreenPadding = 32f * p;
                    yield return null;
                }
            }
            while ((cs04MirrorPortal.portal.DistortionFade -= Engine.DeltaTime * 2f) > 0.0)
                yield return null;
            cs04MirrorPortal.EndCutscene(level);
        }

        private IEnumerator CenterCamera()
        {
            CS04_MirrorPortal cs04MirrorPortal = this;
            Camera camera = cs04MirrorPortal.Level.Camera;
            Vector2 target = cs04MirrorPortal.portal.Center - new Vector2(160f, 90f);
            while ((camera.Position - target).Length() > 1.0)
            {
                camera.Position += (target - camera.Position) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                yield return null;
            }
        }

        private IEnumerator MusicFadeOutBSide()
        {
            for (float p = 1f; p > 0.0; p -= Engine.DeltaTime)
            {
                Audio.SetMusicParam("fade", p);
                yield return null;
            }
            Audio.SetMusicParam("fade", 0.0f);
        }

        public override void OnEnd(Level level) => level.OnEndOfFrame += () =>
        {
            if (fader != null && !WasSkipped)
            {
                fader.Tag = (int) Tags.Global;
                fader.Target = 0.0f;
                fader.Ended = true;
            }
            Leader.StoreStrawberries(player.Leader);
            level.Remove(player);
            level.UnloadLevel();
            level.Session.Dreaming = true;
            level.Session.Keys.Clear();
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                level.Session.Level = "void";
                level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                level.LoadLevel(Player.IntroTypes.TempleMirrorVoid);
            }
            else
            {
                level.Session.Level = "c-00";
                level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                level.LoadLevel(Player.IntroTypes.WakeUp);
                Audio.SetMusicParam("fade", 1f);
            }
            Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
            level.Camera.Y -= 8f;
            if (!WasSkipped && level.Wipe != null)
                level.Wipe.Cancel();
            if (fader == null)
                return;
            fader.RemoveTag((int) Tags.Global);
        };

        private class Fader : Entity
        {
            public float Target;
            public bool Ended;
            private float fade;

            public Fader() => Depth = -1000000;

            public override void Update()
            {
                fade = Calc.Approach(fade, Target, Engine.DeltaTime * 0.5f);
                if (Target <= 0.0 && fade <= 0.0 && Ended)
                    RemoveSelf();
                base.Update();
            }

            public override void Render()
            {
                Camera camera = (Scene as Level).Camera;
                if (fade > 0.0)
                    Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity == null || entity.OnGround(2))
                    return;
                entity.Render();
            }
        }
    }
}
