using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS10_FreeBird : CutsceneEntity
    {
        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS10_FreeBird cs10FreeBird = this;
            yield return Textbox.Say("CH9_FREE_BIRD");
            FadeWipe fadeWipe = new FadeWipe(level, false);
            fadeWipe.Duration = 3f;
            yield return fadeWipe.Duration;
            cs10FreeBird.EndCutscene(level);
        }

        public override void OnEnd(Level level) => level.CompleteArea(false, true);
    }
}
