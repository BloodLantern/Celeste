using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS03_Guestbook : CutsceneEntity
    {
        private Player player;

        public CS03_Guestbook(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Routine()));

        private IEnumerator Routine()
        {
            CS03_Guestbook cs03Guestbook = this;
            cs03Guestbook.player.StateMachine.State = 11;
            cs03Guestbook.player.StateMachine.Locked = true;
            yield return Textbox.Say("ch3_guestbook");
            yield return 0.1f;
            cs03Guestbook.EndCutscene(cs03Guestbook.Level);
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }
    }
}
