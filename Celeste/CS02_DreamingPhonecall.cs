// Decompiled with JetBrains decompiler
// Type: Celeste.CS02_DreamingPhonecall
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS02_DreamingPhonecall : CutsceneEntity
    {
        private BadelineDummy evil;
        private Player player;
        private Payphone payphone;
        private SoundSource ringtone;

        public CS02_DreamingPhonecall(Player player)
            : base(false)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            this.payphone = this.Scene.Tracker.GetEntity<Payphone>();
            this.Add((Component) new Coroutine(this.Cutscene(level)));
            this.Add((Component) (this.ringtone = new SoundSource()));
            this.ringtone.Position = this.payphone.Position;
        }

        private IEnumerator Cutscene(Level level)
        {
            CS02_DreamingPhonecall dreamingPhonecall = this;
            dreamingPhonecall.player.StateMachine.State = 11;
            dreamingPhonecall.player.Dashes = 1;
            yield return (object) 0.3f;
            dreamingPhonecall.ringtone.Play("event:/game/02_old_site/sequence_phone_ring_loop");
            while ((double) dreamingPhonecall.player.Light.Alpha > 0.0)
            {
                dreamingPhonecall.player.Light.Alpha -= Engine.DeltaTime * 2f;
                yield return (object) null;
            }
            yield return (object) 3.2f;
            yield return (object) dreamingPhonecall.player.DummyWalkTo(dreamingPhonecall.payphone.X - 24f);
            yield return (object) 1.5f;
            dreamingPhonecall.player.Facing = Facings.Left;
            yield return (object) 1.5f;
            dreamingPhonecall.player.Facing = Facings.Right;
            yield return (object) 0.25f;
            yield return (object) dreamingPhonecall.player.DummyWalkTo(dreamingPhonecall.payphone.X - 4f);
            yield return (object) 1.5f;
            dreamingPhonecall.Add((Component) Alarm.Create(Alarm.AlarmMode.Oneshot, (Action) (() => this.ringtone.Param("end", 1f)), 0.43f, true));
            dreamingPhonecall.player.Visible = false;
            Audio.Play("event:/game/02_old_site/sequence_phone_pickup", dreamingPhonecall.player.Position);
            yield return (object) dreamingPhonecall.payphone.Sprite.PlayRoutine("pickUp");
            yield return (object) 1f;
            if (level.Session.Area.Mode == AreaMode.Normal)
                Audio.SetMusic("event:/music/lvl2/phone_loop");
            dreamingPhonecall.payphone.Sprite.Play("talkPhone");
            yield return (object) Textbox.Say("CH2_DREAM_PHONECALL", new Func<IEnumerator>(dreamingPhonecall.ShowShadowMadeline));
            if (dreamingPhonecall.evil != null)
            {
                if (level.Session.Area.Mode == AreaMode.Normal)
                    Audio.SetMusic("event:/music/lvl2/phone_end");
                dreamingPhonecall.evil.Vanish();
                dreamingPhonecall.evil = (BadelineDummy) null;
                yield return (object) 1f;
            }
            dreamingPhonecall.Add((Component) new Coroutine(dreamingPhonecall.WireFalls()));
            dreamingPhonecall.payphone.Broken = true;
            level.Shake(0.2f);
            VertexLight light = new VertexLight(new Vector2(16f, -28f), Color.White, 0.0f, 32, 48);
            dreamingPhonecall.payphone.Add((Component) light);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 2f, start: true);
            tween.OnUpdate = (Action<Tween>) (t => light.Alpha = t.Eased);
            dreamingPhonecall.Add((Component) tween);
            Audio.Play("event:/game/02_old_site/sequence_phone_transform", dreamingPhonecall.payphone.Position);
            yield return (object) dreamingPhonecall.payphone.Sprite.PlayRoutine("transform");
            yield return (object) 0.4f;
            yield return (object) dreamingPhonecall.payphone.Sprite.PlayRoutine("eat");
            dreamingPhonecall.payphone.Sprite.Play("monsterIdle");
            yield return (object) 1.2f;
            level.EndCutscene();
            FadeWipe fadeWipe = new FadeWipe((Scene) level, false, (Action) (() => this.EndCutscene(level)));
        }

        private IEnumerator ShowShadowMadeline()
        {
            CS02_DreamingPhonecall dreamingPhonecall = this;
            Payphone payphone = dreamingPhonecall.Scene.Tracker.GetEntity<Payphone>();
            Level level = dreamingPhonecall.Scene as Level;
            yield return (object) level.ZoomTo(new Vector2(240f, 116f), 2f, 0.5f);
            dreamingPhonecall.evil = new BadelineDummy(payphone.Position + new Vector2(32f, -24f));
            dreamingPhonecall.evil.Appear(level);
            dreamingPhonecall.Scene.Add((Entity) dreamingPhonecall.evil);
            yield return (object) 0.2f;
            ++payphone.Blink.X;
            yield return (object) payphone.Sprite.PlayRoutine("jumpBack");
            yield return (object) payphone.Sprite.PlayRoutine("scare");
            yield return (object) 1.2f;
        }

        private IEnumerator WireFalls()
        {
            CS02_DreamingPhonecall dreamingPhonecall = this;
            yield return (object) 0.5f;
            Wire wire = dreamingPhonecall.Scene.Entities.FindFirst<Wire>();
            Vector2 speed = Vector2.Zero;
            Level level = dreamingPhonecall.SceneAs<Level>();
            while (wire != null && (double) wire.Curve.Begin.X < (double) level.Bounds.Right)
            {
                speed += new Vector2(0.7f, 1f) * 200f * Engine.DeltaTime;
                wire.Curve.Begin += speed * Engine.DeltaTime;
                yield return (object) null;
            }
        }

        public override void OnEnd(Level level)
        {
            Leader.StoreStrawberries(this.player.Leader);
            level.ResetZoom();
            level.Bloom.Base = 0.0f;
            level.Remove((Entity) this.player);
            level.UnloadLevel();
            level.Session.Dreaming = false;
            level.Session.Level = "end_0";
            Session session = level.Session;
            Level level1 = level;
            Rectangle bounds = level.Bounds;
            double left = (double) bounds.Left;
            bounds = level.Bounds;
            double bottom = (double) bounds.Bottom;
            Vector2 from = new Vector2((float) left, (float) bottom);
            Vector2? nullable = new Vector2?(level1.GetSpawnPoint(from));
            session.RespawnPoint = nullable;
            level.Session.Audio.Music.Event = "event:/music/lvl2/awake";
            level.Session.Audio.Ambience.Event = "event:/env/amb/02_awake";
            level.LoadLevel(Player.IntroTypes.WakeUp);
            level.EndCutscene();
            Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
        }
    }
}
