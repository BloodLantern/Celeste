using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS03_Diary : CutsceneEntity
    {
        private Player player;

        public CS03_Diary(Player player)
            : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Routine()));

        private IEnumerator Routine()
        {
            CS03_Diary cs03Diary = this;
            cs03Diary.player.StateMachine.State = 11;
            cs03Diary.player.StateMachine.Locked = true;
            yield return (object) Textbox.Say("CH3_DIARY");
            yield return (object) 0.1f;
            cs03Diary.EndCutscene(cs03Diary.Level);
        }

        public override void OnEnd(Level level)
        {
            this.player.StateMachine.Locked = false;
            this.player.StateMachine.State = 0;
        }
    }
}
