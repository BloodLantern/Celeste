using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS06_Reflection : CutsceneEntity
    {
        public const string Flag = "reflection";
        private Player player;
        private float targetX;

        public CS06_Reflection(Player player, float targetX)
        {
            this.player = player;
            this.targetX = targetX;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS06_Reflection cs06Reflection = this;
            cs06Reflection.player.StateMachine.State = 11;
            cs06Reflection.player.StateMachine.Locked = true;
            cs06Reflection.player.ForceCameraUpdate = true;
            yield return cs06Reflection.player.DummyWalkToExact((int) cs06Reflection.targetX);
            yield return 0.1f;
            cs06Reflection.player.Facing = Facings.Right;
            yield return 0.1f;
            yield return cs06Reflection.Level.ZoomTo(new Vector2(200f, 90f), 2f, 1f);
            yield return Textbox.Say("CH6_REFLECT_AFTER");
            yield return cs06Reflection.Level.ZoomBack(0.5f);
            cs06Reflection.EndCutscene(level);
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
