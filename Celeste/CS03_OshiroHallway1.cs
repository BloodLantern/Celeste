// Decompiled with JetBrains decompiler
// Type: Celeste.CS03_OshiroHallway1
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS03_OshiroHallway1 : CutsceneEntity
    {
        public const string Flag = "oshiro_resort_talked_2";
        private Player player;
        private NPC oshiro;

        public CS03_OshiroHallway1(Player player, NPC oshiro)
            : base()
        {
            this.player = player;
            this.oshiro = oshiro;
        }

        public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS03_OshiroHallway1 cs03OshiroHallway1 = this;
            level.Session.Audio.Music.Layer(1, false);
            level.Session.Audio.Music.Layer(2, true);
            level.Session.Audio.Apply();
            cs03OshiroHallway1.player.StateMachine.State = 11;
            cs03OshiroHallway1.player.StateMachine.Locked = true;
            yield return (object) Textbox.Say("CH3_OSHIRO_HALLWAY_A");
            cs03OshiroHallway1.oshiro.MoveToAndRemove(new Vector2((float) (cs03OshiroHallway1.SceneAs<Level>().Bounds.Right + 64), cs03OshiroHallway1.oshiro.Y));
            cs03OshiroHallway1.oshiro.Add((Component) new SoundSource("event:/char/oshiro/move_02_03a_exit"));
            yield return (object) 1f;
            cs03OshiroHallway1.EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            level.Session.Audio.Music.Layer(1, true);
            level.Session.Audio.Music.Layer(2, false);
            level.Session.Audio.Apply();
            this.player.StateMachine.Locked = false;
            this.player.StateMachine.State = 0;
            level.Session.SetFlag("oshiro_resort_talked_2");
            if (!this.WasSkipped)
                return;
            level.Remove((Entity) this.oshiro);
        }
    }
}
