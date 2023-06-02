using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS03_OshiroHallway2 : CutsceneEntity
    {
        public const string Flag = "oshiro_resort_talked_3";
        private Player player;
        private NPC oshiro;

        public CS03_OshiroHallway2(Player player, NPC oshiro)
            : base()
        {
            this.player = player;
            this.oshiro = oshiro;
        }

        public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS03_OshiroHallway2 cs03OshiroHallway2 = this;
            level.Session.Audio.Music.Layer(1, false);
            level.Session.Audio.Music.Layer(2, true);
            level.Session.Audio.Apply();
            cs03OshiroHallway2.player.StateMachine.State = 11;
            cs03OshiroHallway2.player.StateMachine.Locked = true;
            yield return (object) Textbox.Say("CH3_OSHIRO_HALLWAY_B");
            cs03OshiroHallway2.oshiro.MoveToAndRemove(new Vector2((float) (level.Bounds.Right + 64), cs03OshiroHallway2.oshiro.Y));
            cs03OshiroHallway2.oshiro.Add((Component) new SoundSource("event:/char/oshiro/move_03_08a_exit"));
            yield return (object) 1f;
            cs03OshiroHallway2.EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            level.Session.Audio.Music.Layer(1, true);
            level.Session.Audio.Music.Layer(2, false);
            level.Session.Audio.Apply();
            this.player.StateMachine.Locked = false;
            this.player.StateMachine.State = 0;
            level.Session.SetFlag("oshiro_resort_talked_3");
            if (!this.WasSkipped)
                return;
            level.Remove((Entity) this.oshiro);
        }
    }
}
