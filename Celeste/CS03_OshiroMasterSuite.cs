using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS03_OshiroMasterSuite : CutsceneEntity
    {
        public const string Flag = "oshiro_resort_suite";
        private Player player;
        private NPC oshiro;
        private BadelineDummy evil;
        private ResortMirror mirror;

        public CS03_OshiroMasterSuite(NPC oshiro)
        {
            this.oshiro = oshiro;
        }

        public override void OnBegin(Level level)
        {
            mirror = Scene.Entities.FindFirst<ResortMirror>();
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS03_OshiroMasterSuite oshiroMasterSuite = this;
            while (true)
            {
                oshiroMasterSuite.player = oshiroMasterSuite.Scene.Tracker.GetEntity<Player>();
                if (oshiroMasterSuite.player == null)
                    yield return null;
                else
                    break;
            }
            Audio.SetMusic(null);
            yield return 0.4f;
            oshiroMasterSuite.player.StateMachine.State = 11;
            oshiroMasterSuite.player.StateMachine.Locked = true;
            oshiroMasterSuite.Add(new Coroutine(oshiroMasterSuite.player.DummyWalkTo(oshiroMasterSuite.oshiro.X + 32f)));
            yield return 1f;
            Audio.SetMusic("event:/music/lvl3/oshiro_theme");
            yield return Textbox.Say("CH3_OSHIRO_SUITE", oshiroMasterSuite.SuiteShadowAppear, oshiroMasterSuite.SuiteShadowDisrupt, oshiroMasterSuite.SuiteShadowCeiling, oshiroMasterSuite.Wander, oshiroMasterSuite.Console, oshiroMasterSuite.JumpBack, oshiroMasterSuite.Collapse, oshiroMasterSuite.AwkwardPause);
            oshiroMasterSuite.evil.Add(new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_exittop"));
            yield return oshiroMasterSuite.evil.FloatTo(new Vector2(oshiroMasterSuite.evil.X, level.Bounds.Top - 32));
            oshiroMasterSuite.Scene.Remove(oshiroMasterSuite.evil);
            while (level.Lighting.Alpha != (double) level.BaseLightingAlpha)
            {
                level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, level.BaseLightingAlpha, Engine.DeltaTime * 0.5f);
                yield return null;
            }
            oshiroMasterSuite.EndCutscene(level);
        }

        private IEnumerator Wander()
        {
            CS03_OshiroMasterSuite oshiroMasterSuite = this;
            yield return 0.5f;
            oshiroMasterSuite.player.Facing = Facings.Right;
            yield return 0.1f;
            yield return oshiroMasterSuite.player.DummyWalkToExact((int) oshiroMasterSuite.oshiro.X + 48);
            yield return 1f;
            oshiroMasterSuite.player.Facing = Facings.Left;
            yield return 0.2f;
            yield return oshiroMasterSuite.player.DummyWalkToExact((int) oshiroMasterSuite.oshiro.X - 32);
            yield return 0.1f;
            oshiroMasterSuite.oshiro.Sprite.Scale.X = -1f;
            yield return 0.2f;
            oshiroMasterSuite.player.DummyAutoAnimate = false;
            oshiroMasterSuite.player.Sprite.Play("lookUp");
            yield return 1f;
            oshiroMasterSuite.player.DummyAutoAnimate = true;
            yield return 0.4f;
            oshiroMasterSuite.player.Facing = Facings.Right;
            yield return 0.2f;
            yield return oshiroMasterSuite.player.DummyWalkToExact((int) oshiroMasterSuite.oshiro.X - 24);
            yield return 0.5f;
            yield return oshiroMasterSuite.SceneAs<Level>().ZoomTo(new Vector2(190f, 110f), 2f, 0.5f);
        }

        private IEnumerator AwkwardPause()
        {
            yield return 2f;
        }

        private IEnumerator SuiteShadowAppear()
        {
            CS03_OshiroMasterSuite oshiroMasterSuite = this;
            if (oshiroMasterSuite.mirror != null)
            {
                oshiroMasterSuite.mirror.EvilAppear();
                oshiroMasterSuite.SetMusic();
                Audio.Play("event:/game/03_resort/suite_bad_intro", oshiroMasterSuite.mirror.Position);
                Vector2 from = oshiroMasterSuite.Level.ZoomFocusPoint;
                Vector2 to = new Vector2(216f, 110f);
                for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime * 2f)
                {
                    oshiroMasterSuite.Level.ZoomFocusPoint = from + (to - from) * Ease.SineInOut(p);
                    yield return null;
                }
                yield return null;
            }
        }

        private IEnumerator SuiteShadowDisrupt()
        {
            CS03_OshiroMasterSuite oshiroMasterSuite = this;
            if (oshiroMasterSuite.mirror != null)
            {
                Audio.Play("event:/game/03_resort/suite_bad_mirrorbreak", oshiroMasterSuite.mirror.Position);
                yield return oshiroMasterSuite.mirror.SmashRoutine();
                oshiroMasterSuite.evil = new BadelineDummy(oshiroMasterSuite.mirror.Position + new Vector2(0.0f, -8f));
                oshiroMasterSuite.Scene.Add(oshiroMasterSuite.evil);
                yield return 1.2f;
                oshiroMasterSuite.oshiro.Sprite.Scale.X = 1f;
                yield return oshiroMasterSuite.evil.FloatTo(oshiroMasterSuite.oshiro.Position + new Vector2(32f, -24f));
            }
        }

        private IEnumerator Collapse()
        {
            oshiro.Sprite.Play("fall");
            Audio.Play("event:/char/oshiro/chat_collapse", oshiro.Position);
            yield return null;
        }

        private IEnumerator Console()
        {
            yield return player.DummyWalkToExact((int) oshiro.X - 16);
        }

        private IEnumerator JumpBack()
        {
            yield return player.DummyWalkToExact((int) oshiro.X - 24, true);
            yield return 0.8f;
        }

        private IEnumerator SuiteShadowCeiling()
        {
            CS03_OshiroMasterSuite oshiroMasterSuite = this;
            yield return oshiroMasterSuite.SceneAs<Level>().ZoomBack(0.5f);
            oshiroMasterSuite.evil.Add(new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_movestageleft"));
            yield return oshiroMasterSuite.evil.FloatTo(new Vector2(oshiroMasterSuite.Level.Bounds.Left + 96, oshiroMasterSuite.evil.Y - 16f), 1);
            oshiroMasterSuite.player.Facing = Facings.Left;
            yield return 0.25f;
            oshiroMasterSuite.evil.Add(new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_ceilingbreak"));
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            oshiroMasterSuite.Level.DirectionalShake(-Vector2.UnitY);
            yield return oshiroMasterSuite.evil.SmashBlock(oshiroMasterSuite.evil.Position + new Vector2(0.0f, -32f));
            yield return 0.8f;
        }

        private void SetMusic()
        {
            if (Level.Session.Area.Mode != AreaMode.Normal)
                return;
            Level.Session.Audio.Music.Event = "event:/music/lvl2/evil_madeline";
            Level.Session.Audio.Apply();
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                if (evil != null)
                    Scene.Remove(evil);
                if (mirror != null)
                    mirror.Broken();
                Scene.Entities.FindFirst<DashBlock>()?.RemoveAndFlagAsGone();
                oshiro.Sprite.Play("idle_ground");
            }
            oshiro.Talker.Enabled = true;
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = 0;
            }
            level.Lighting.Alpha = level.BaseLightingAlpha;
            level.Session.SetFlag("oshiro_resort_suite");
            SetMusic();
        }
    }
}
