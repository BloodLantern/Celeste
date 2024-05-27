using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS05_Reflection1 : CutsceneEntity
    {
        public const string Flag = "reflection";
        private Player player;

        public CS05_Reflection1(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS05_Reflection1 cs05Reflection1 = this;
            cs05Reflection1.player.StateMachine.State = 11;
            cs05Reflection1.player.StateMachine.Locked = true;
            cs05Reflection1.player.ForceCameraUpdate = true;
            TempleMirror first = cs05Reflection1.Scene.Entities.FindFirst<TempleMirror>();
            yield return cs05Reflection1.player.DummyWalkTo(first.Center.X + 8f);
            yield return 0.2f;
            cs05Reflection1.player.Facing = Facings.Left;
            yield return 0.3f;
            if (!cs05Reflection1.player.Dead)
                yield return Textbox.Say("ch5_reflection", cs05Reflection1.MadelineFallsToKnees, cs05Reflection1.MadelineStopsPanicking, cs05Reflection1.MadelineGetsUp);
            else
                yield return 100f;
            yield return cs05Reflection1.Level.ZoomBack(0.5f);
            cs05Reflection1.EndCutscene(level);
        }

        private IEnumerator MadelineFallsToKnees()
        {
            CS05_Reflection1 cs05Reflection1 = this;
            yield return 0.2f;
            cs05Reflection1.player.DummyAutoAnimate = false;
            cs05Reflection1.player.Sprite.Play("tired");
            yield return 0.2f;
            yield return cs05Reflection1.Level.ZoomTo(new Vector2(90f, 116f), 2f, 0.5f);
            yield return 0.2f;
        }

        private IEnumerator MadelineStopsPanicking()
        {
            yield return 0.8f;
            player.Sprite.Play("tiredStill");
            yield return 0.4f;
        }

        private IEnumerator MadelineGetsUp()
        {
            player.DummyAutoAnimate = true;
            player.Sprite.Play("idle");
            yield break;
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            player.ForceCameraUpdate = false;
            player.FlipInReflection = false;
            level.Session.SetFlag("reflection");
        }
    }
}
