// Decompiled with JetBrains decompiler
// Type: Celeste.CS02_BadelineIntro
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS02_BadelineIntro : CutsceneEntity
    {
        public const string Flag = "evil_maddy_intro";
        private Player player;
        private readonly BadelineOldsite badeline;
        private Vector2 badelineEndPosition;
        private float anxietyFade;
        private float anxietyFadeTarget;
        private readonly SineWave anxietySine;
        private float anxietyJitter;

        public CS02_BadelineIntro(BadelineOldsite badeline)
            : base()
        {
            this.badeline = badeline;
            badelineEndPosition = badeline.Position + new Vector2(8f, -24f);
            Add(anxietySine = new SineWave(0.3f));
            Distort.AnxietyOrigin = new Vector2(0.5f, 0.75f);
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public override void Update()
        {
            base.Update();
            anxietyFade = Calc.Approach(anxietyFade, anxietyFadeTarget, 2.5f * Engine.DeltaTime);
            if (Scene.OnInterval(0.1f))
            {
                anxietyJitter = Calc.Random.Range(-0.1f, 0.1f);
            }

            Distort.Anxiety = anxietyFade * Math.Max(0.0f, (float)(0.0 + anxietyJitter + ((double)anxietySine.Value * 0.30000001192092896)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS02_BadelineIntro cs02BadelineIntro = this;
            cs02BadelineIntro.anxietyFadeTarget = 1f;
            while (true)
            {
                cs02BadelineIntro.player = level.Tracker.GetEntity<Player>();
                if (cs02BadelineIntro.player == null)
                {
                    yield return null;
                }
                else
                {
                    break;
                }
            }
            while (!cs02BadelineIntro.player.OnGround())
            {
                yield return null;
            }

            cs02BadelineIntro.player.StateMachine.State = 11;
            cs02BadelineIntro.player.StateMachine.Locked = true;
            yield return 1f;
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                _ = Audio.SetMusic("event:/music/lvl2/evil_madeline");
            }

            yield return Textbox.Say("CH2_BADELINE_INTRO", new Func<IEnumerator>(cs02BadelineIntro.TurnAround), new Func<IEnumerator>(cs02BadelineIntro.RevealBadeline), new Func<IEnumerator>(cs02BadelineIntro.StartLaughing), new Func<IEnumerator>(cs02BadelineIntro.StopLaughing));
            cs02BadelineIntro.anxietyFadeTarget = 0.0f;
            yield return cs02BadelineIntro.Level.ZoomBack(0.5f);
            cs02BadelineIntro.EndCutscene(level);
        }

        private IEnumerator TurnAround()
        {
            CS02_BadelineIntro cs02BadelineIntro = this;
            cs02BadelineIntro.player.Facing = Facings.Left;
            yield return 0.2f;
            cs02BadelineIntro.Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(cs02BadelineIntro.Level.Bounds.X, cs02BadelineIntro.Level.Camera.Y), 0.5f)));
            yield return cs02BadelineIntro.Level.ZoomTo(new Vector2(84f, 135f), 2f, 0.5f);
            yield return 0.2f;
        }

        private IEnumerator RevealBadeline()
        {
            CS02_BadelineIntro cs02BadelineIntro = this;
            _ = Audio.Play("event:/game/02_old_site/sequence_badeline_intro", cs02BadelineIntro.badeline.Position);
            yield return 0.1f;
            _ = cs02BadelineIntro.Level.Displacement.AddBurst(cs02BadelineIntro.badeline.Position + new Vector2(0.0f, -4f), 0.8f, 8f, 48f, 0.5f);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            yield return 0.1f;
            cs02BadelineIntro.badeline.Hovering = true;
            cs02BadelineIntro.badeline.Hair.Visible = true;
            cs02BadelineIntro.badeline.Sprite.Play("fallSlow");
            Vector2 from = cs02BadelineIntro.badeline.Position;
            Vector2 to = cs02BadelineIntro.badelineEndPosition;
            for (float t = 0.0f; (double)t < 1.0; t += Engine.DeltaTime)
            {
                cs02BadelineIntro.badeline.Position = from + ((to - from) * Ease.CubeInOut(t));
                yield return null;
            }
            cs02BadelineIntro.player.Facing = (Facings)Math.Sign(cs02BadelineIntro.badeline.X - cs02BadelineIntro.player.X);
            yield return 1f;
        }

        private IEnumerator StartLaughing()
        {
            yield return 0.2f;
            badeline.Sprite.Play("laugh", true);
            yield return null;
        }

        private IEnumerator StopLaughing()
        {
            badeline.Sprite.Play("fallSlow", true);
            yield return null;
        }

        public override void OnEnd(Level level)
        {
            _ = Audio.SetMusic(null);
            Distort.Anxiety = 0.0f;
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.Facing = Facings.Left;
                player.StateMachine.State = 0;
                player.JustRespawned = true;
            }
            badeline.Position = badelineEndPosition;
            badeline.Visible = true;
            badeline.Hair.Visible = true;
            badeline.Sprite.Play("fallSlow");
            badeline.Hovering = false;
            badeline.Add(new Coroutine(badeline.StartChasingRoutine(level)));
            level.Session.SetFlag("evil_maddy_intro");
        }
    }
}
