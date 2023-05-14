// Decompiled with JetBrains decompiler
// Type: Celeste.CS02_Ending
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS02_Ending : CutsceneEntity
    {
        private readonly Player player;
        private Payphone payphone;
        private readonly SoundSource phoneSfx;

        public CS02_Ending(Player player)
            : base(false, true)
        {
            this.player = player;
            Add(phoneSfx = new SoundSource());
        }

        public override void OnBegin(Level level)
        {
            level.RegisterAreaComplete();
            payphone = Scene.Tracker.GetEntity<Payphone>();
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS02_Ending cs02Ending = this;
            cs02Ending.player.StateMachine.State = 11;
            cs02Ending.player.Dashes = 1;
            while (cs02Ending.player.Light.Alpha > 0.0)
            {
                cs02Ending.player.Light.Alpha -= Engine.DeltaTime * 1.25f;
                yield return null;
            }
            yield return 1f;
            yield return cs02Ending.player.DummyWalkTo(cs02Ending.payphone.X - 4f);
            yield return 0.2f;
            cs02Ending.player.Facing = Facings.Right;
            yield return 0.5f;
            cs02Ending.player.Visible = false;
            _ = Audio.Play("event:/game/02_old_site/sequence_phone_pickup", cs02Ending.player.Position);
            yield return cs02Ending.payphone.Sprite.PlayRoutine("pickUp");
            yield return 0.25f;
            cs02Ending.phoneSfx.Position = cs02Ending.player.Position;
            _ = cs02Ending.phoneSfx.Play("event:/game/02_old_site/sequence_phone_ringtone_loop");
            yield return 6f;
            _ = cs02Ending.phoneSfx.Stop();
            cs02Ending.payphone.Sprite.Play("talkPhone");
            yield return Textbox.Say("CH2_END_PHONECALL");
            yield return 0.3f;
            cs02Ending.EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            _ = level.CompleteArea();
        }
    }
}
