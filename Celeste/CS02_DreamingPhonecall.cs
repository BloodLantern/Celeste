using Microsoft.Xna.Framework;
using Monocle;
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
            payphone = Scene.Tracker.GetEntity<Payphone>();
            Add(new Coroutine(Cutscene(level)));
            Add(ringtone = new SoundSource());
            ringtone.Position = payphone.Position;
        }

        private IEnumerator Cutscene(Level level)
        {
            CS02_DreamingPhonecall dreamingPhonecall = this;
            dreamingPhonecall.player.StateMachine.State = 11;
            dreamingPhonecall.player.Dashes = 1;
            yield return 0.3f;
            dreamingPhonecall.ringtone.Play("event:/game/02_old_site/sequence_phone_ring_loop");
            while (dreamingPhonecall.player.Light.Alpha > 0.0)
            {
                dreamingPhonecall.player.Light.Alpha -= Engine.DeltaTime * 2f;
                yield return null;
            }
            yield return 3.2f;
            yield return dreamingPhonecall.player.DummyWalkTo(dreamingPhonecall.payphone.X - 24f);
            yield return 1.5f;
            dreamingPhonecall.player.Facing = Facings.Left;
            yield return 1.5f;
            dreamingPhonecall.player.Facing = Facings.Right;
            yield return 0.25f;
            yield return dreamingPhonecall.player.DummyWalkTo(dreamingPhonecall.payphone.X - 4f);
            yield return 1.5f;
            dreamingPhonecall.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => ringtone.Param("end", 1f), 0.43f, true));
            dreamingPhonecall.player.Visible = false;
            Audio.Play("event:/game/02_old_site/sequence_phone_pickup", dreamingPhonecall.player.Position);
            yield return dreamingPhonecall.payphone.Sprite.PlayRoutine("pickUp");
            yield return 1f;
            if (level.Session.Area.Mode == AreaMode.Normal)
                Audio.SetMusic("event:/music/lvl2/phone_loop");
            dreamingPhonecall.payphone.Sprite.Play("talkPhone");
            yield return Textbox.Say("CH2_DREAM_PHONECALL", dreamingPhonecall.ShowShadowMadeline);
            if (dreamingPhonecall.evil != null)
            {
                if (level.Session.Area.Mode == AreaMode.Normal)
                    Audio.SetMusic("event:/music/lvl2/phone_end");
                dreamingPhonecall.evil.Vanish();
                dreamingPhonecall.evil = null;
                yield return 1f;
            }
            dreamingPhonecall.Add(new Coroutine(dreamingPhonecall.WireFalls()));
            dreamingPhonecall.payphone.Broken = true;
            level.Shake(0.2f);
            VertexLight light = new VertexLight(new Vector2(16f, -28f), Color.White, 0.0f, 32, 48);
            dreamingPhonecall.payphone.Add(light);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 2f, start: true);
            tween.OnUpdate = t => light.Alpha = t.Eased;
            dreamingPhonecall.Add(tween);
            Audio.Play("event:/game/02_old_site/sequence_phone_transform", dreamingPhonecall.payphone.Position);
            yield return dreamingPhonecall.payphone.Sprite.PlayRoutine("transform");
            yield return 0.4f;
            yield return dreamingPhonecall.payphone.Sprite.PlayRoutine("eat");
            dreamingPhonecall.payphone.Sprite.Play("monsterIdle");
            yield return 1.2f;
            level.EndCutscene();
            FadeWipe fadeWipe = new FadeWipe(level, false, () => EndCutscene(level));
        }

        private IEnumerator ShowShadowMadeline()
        {
            CS02_DreamingPhonecall dreamingPhonecall = this;
            Payphone payphone = dreamingPhonecall.Scene.Tracker.GetEntity<Payphone>();
            Level level = dreamingPhonecall.Scene as Level;
            yield return level.ZoomTo(new Vector2(240f, 116f), 2f, 0.5f);
            dreamingPhonecall.evil = new BadelineDummy(payphone.Position + new Vector2(32f, -24f));
            dreamingPhonecall.evil.Appear(level);
            dreamingPhonecall.Scene.Add(dreamingPhonecall.evil);
            yield return 0.2f;
            ++payphone.Blink.X;
            yield return payphone.Sprite.PlayRoutine("jumpBack");
            yield return payphone.Sprite.PlayRoutine("scare");
            yield return 1.2f;
        }

        private IEnumerator WireFalls()
        {
            CS02_DreamingPhonecall dreamingPhonecall = this;
            yield return 0.5f;
            Wire wire = dreamingPhonecall.Scene.Entities.FindFirst<Wire>();
            Vector2 speed = Vector2.Zero;
            Level level = dreamingPhonecall.SceneAs<Level>();
            while (wire != null && wire.Curve.Begin.X < (double) level.Bounds.Right)
            {
                speed += new Vector2(0.7f, 1f) * 200f * Engine.DeltaTime;
                wire.Curve.Begin += speed * Engine.DeltaTime;
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            Leader.StoreStrawberries(player.Leader);
            level.ResetZoom();
            level.Bloom.Base = 0.0f;
            level.Remove(player);
            level.UnloadLevel();
            level.Session.Dreaming = false;
            level.Session.Level = "end_0";
            Session session = level.Session;
            Level level1 = level;
            Rectangle bounds = level.Bounds;
            double left = bounds.Left;
            bounds = level.Bounds;
            double bottom = bounds.Bottom;
            Vector2 from = new Vector2((float) left, (float) bottom);
            Vector2? nullable = level1.GetSpawnPoint(from);
            session.RespawnPoint = nullable;
            level.Session.Audio.Music.Event = "event:/music/lvl2/awake";
            level.Session.Audio.Ambience.Event = "event:/env/amb/02_awake";
            level.LoadLevel(Player.IntroTypes.WakeUp);
            level.EndCutscene();
            Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
        }
    }
}
