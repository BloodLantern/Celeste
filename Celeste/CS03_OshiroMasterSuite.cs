using Microsoft.Xna.Framework;
using Monocle;
using System;
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
            : base()
        {
            this.oshiro = oshiro;
        }

        public override void OnBegin(Level level)
        {
            this.mirror = this.Scene.Entities.FindFirst<ResortMirror>();
            this.Add((Component) new Coroutine(this.Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS03_OshiroMasterSuite oshiroMasterSuite = this;
            while (true)
            {
                oshiroMasterSuite.player = oshiroMasterSuite.Scene.Tracker.GetEntity<Player>();
                if (oshiroMasterSuite.player == null)
                    yield return (object) null;
                else
                    break;
            }
            Audio.SetMusic((string) null);
            yield return (object) 0.4f;
            oshiroMasterSuite.player.StateMachine.State = 11;
            oshiroMasterSuite.player.StateMachine.Locked = true;
            oshiroMasterSuite.Add((Component) new Coroutine(oshiroMasterSuite.player.DummyWalkTo(oshiroMasterSuite.oshiro.X + 32f)));
            yield return (object) 1f;
            Audio.SetMusic("event:/music/lvl3/oshiro_theme");
            yield return (object) Textbox.Say("CH3_OSHIRO_SUITE", new Func<IEnumerator>(oshiroMasterSuite.SuiteShadowAppear), new Func<IEnumerator>(oshiroMasterSuite.SuiteShadowDisrupt), new Func<IEnumerator>(oshiroMasterSuite.SuiteShadowCeiling), new Func<IEnumerator>(oshiroMasterSuite.Wander), new Func<IEnumerator>(oshiroMasterSuite.Console), new Func<IEnumerator>(oshiroMasterSuite.JumpBack), new Func<IEnumerator>(oshiroMasterSuite.Collapse), new Func<IEnumerator>(oshiroMasterSuite.AwkwardPause));
            oshiroMasterSuite.evil.Add((Component) new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_exittop"));
            yield return (object) oshiroMasterSuite.evil.FloatTo(new Vector2(oshiroMasterSuite.evil.X, (float) (level.Bounds.Top - 32)));
            oshiroMasterSuite.Scene.Remove((Entity) oshiroMasterSuite.evil);
            while ((double) level.Lighting.Alpha != (double) level.BaseLightingAlpha)
            {
                level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, level.BaseLightingAlpha, Engine.DeltaTime * 0.5f);
                yield return (object) null;
            }
            oshiroMasterSuite.EndCutscene(level);
        }

        private IEnumerator Wander()
        {
            CS03_OshiroMasterSuite oshiroMasterSuite = this;
            yield return (object) 0.5f;
            oshiroMasterSuite.player.Facing = Facings.Right;
            yield return (object) 0.1f;
            yield return (object) oshiroMasterSuite.player.DummyWalkToExact((int) oshiroMasterSuite.oshiro.X + 48);
            yield return (object) 1f;
            oshiroMasterSuite.player.Facing = Facings.Left;
            yield return (object) 0.2f;
            yield return (object) oshiroMasterSuite.player.DummyWalkToExact((int) oshiroMasterSuite.oshiro.X - 32);
            yield return (object) 0.1f;
            oshiroMasterSuite.oshiro.Sprite.Scale.X = -1f;
            yield return (object) 0.2f;
            oshiroMasterSuite.player.DummyAutoAnimate = false;
            oshiroMasterSuite.player.Sprite.Play("lookUp");
            yield return (object) 1f;
            oshiroMasterSuite.player.DummyAutoAnimate = true;
            yield return (object) 0.4f;
            oshiroMasterSuite.player.Facing = Facings.Right;
            yield return (object) 0.2f;
            yield return (object) oshiroMasterSuite.player.DummyWalkToExact((int) oshiroMasterSuite.oshiro.X - 24);
            yield return (object) 0.5f;
            yield return (object) oshiroMasterSuite.SceneAs<Level>().ZoomTo(new Vector2(190f, 110f), 2f, 0.5f);
        }

        private IEnumerator AwkwardPause()
        {
            yield return (object) 2f;
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
                for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 2f)
                {
                    oshiroMasterSuite.Level.ZoomFocusPoint = from + (to - from) * Ease.SineInOut(p);
                    yield return (object) null;
                }
                yield return (object) null;
            }
        }

        private IEnumerator SuiteShadowDisrupt()
        {
            CS03_OshiroMasterSuite oshiroMasterSuite = this;
            if (oshiroMasterSuite.mirror != null)
            {
                Audio.Play("event:/game/03_resort/suite_bad_mirrorbreak", oshiroMasterSuite.mirror.Position);
                yield return (object) oshiroMasterSuite.mirror.SmashRoutine();
                oshiroMasterSuite.evil = new BadelineDummy(oshiroMasterSuite.mirror.Position + new Vector2(0.0f, -8f));
                oshiroMasterSuite.Scene.Add((Entity) oshiroMasterSuite.evil);
                yield return (object) 1.2f;
                oshiroMasterSuite.oshiro.Sprite.Scale.X = 1f;
                yield return (object) oshiroMasterSuite.evil.FloatTo(oshiroMasterSuite.oshiro.Position + new Vector2(32f, -24f));
            }
        }

        private IEnumerator Collapse()
        {
            this.oshiro.Sprite.Play("fall");
            Audio.Play("event:/char/oshiro/chat_collapse", this.oshiro.Position);
            yield return (object) null;
        }

        private IEnumerator Console()
        {
            yield return (object) this.player.DummyWalkToExact((int) this.oshiro.X - 16);
        }

        private IEnumerator JumpBack()
        {
            yield return (object) this.player.DummyWalkToExact((int) this.oshiro.X - 24, true);
            yield return (object) 0.8f;
        }

        private IEnumerator SuiteShadowCeiling()
        {
            CS03_OshiroMasterSuite oshiroMasterSuite = this;
            yield return (object) oshiroMasterSuite.SceneAs<Level>().ZoomBack(0.5f);
            oshiroMasterSuite.evil.Add((Component) new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_movestageleft"));
            yield return (object) oshiroMasterSuite.evil.FloatTo(new Vector2((float) (oshiroMasterSuite.Level.Bounds.Left + 96), oshiroMasterSuite.evil.Y - 16f), new int?(1));
            oshiroMasterSuite.player.Facing = Facings.Left;
            yield return (object) 0.25f;
            oshiroMasterSuite.evil.Add((Component) new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_ceilingbreak"));
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
            oshiroMasterSuite.Level.DirectionalShake(-Vector2.UnitY);
            yield return (object) oshiroMasterSuite.evil.SmashBlock(oshiroMasterSuite.evil.Position + new Vector2(0.0f, -32f));
            yield return (object) 0.8f;
        }

        private void SetMusic()
        {
            if (this.Level.Session.Area.Mode != AreaMode.Normal)
                return;
            this.Level.Session.Audio.Music.Event = "event:/music/lvl2/evil_madeline";
            this.Level.Session.Audio.Apply();
        }

        public override void OnEnd(Level level)
        {
            if (this.WasSkipped)
            {
                if (this.evil != null)
                    this.Scene.Remove((Entity) this.evil);
                if (this.mirror != null)
                    this.mirror.Broken();
                this.Scene.Entities.FindFirst<DashBlock>()?.RemoveAndFlagAsGone();
                this.oshiro.Sprite.Play("idle_ground");
            }
            this.oshiro.Talker.Enabled = true;
            if (this.player != null)
            {
                this.player.StateMachine.Locked = false;
                this.player.StateMachine.State = 0;
            }
            level.Lighting.Alpha = level.BaseLightingAlpha;
            level.Session.SetFlag("oshiro_resort_suite");
            this.SetMusic();
        }
    }
}
