// Decompiled with JetBrains decompiler
// Type: Celeste.CS03_OshiroHallway2
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS03_OshiroHallway2 : CutsceneEntity
    {
        public const string Flag = "oshiro_resort_talked_3";
        private readonly Player player;
        private readonly NPC oshiro;

        public CS03_OshiroHallway2(Player player, NPC oshiro)
            : base()
        {
            this.player = player;
            this.oshiro = oshiro;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS03_OshiroHallway2 cs03OshiroHallway2 = this;
            _ = level.Session.Audio.Music.Layer(1, false);
            _ = level.Session.Audio.Music.Layer(2, true);
            level.Session.Audio.Apply();
            cs03OshiroHallway2.player.StateMachine.State = 11;
            cs03OshiroHallway2.player.StateMachine.Locked = true;
            yield return Textbox.Say("CH3_OSHIRO_HALLWAY_B");
            cs03OshiroHallway2.oshiro.MoveToAndRemove(new Vector2(level.Bounds.Right + 64, cs03OshiroHallway2.oshiro.Y));
            cs03OshiroHallway2.oshiro.Add(new SoundSource("event:/char/oshiro/move_03_08a_exit"));
            yield return 1f;
            cs03OshiroHallway2.EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            _ = level.Session.Audio.Music.Layer(1, true);
            _ = level.Session.Audio.Music.Layer(2, false);
            level.Session.Audio.Apply();
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            level.Session.SetFlag("oshiro_resort_talked_3");
            if (!WasSkipped)
            {
                return;
            }

            level.Remove(oshiro);
        }
    }
}
