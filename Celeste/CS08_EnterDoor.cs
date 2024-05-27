using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS08_EnterDoor : CutsceneEntity
    {
        private Player player;
        private float targetX;

        public CS08_EnterDoor(Player player, float targetX)
        {
            this.player = player;
            this.targetX = targetX;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        // ISSUE: reference to a compiler-generated field
        private IEnumerator Cutscene(Level level)
        {
                player.StateMachine.State = 11;
                Add(new Coroutine(player.DummyWalkToExact((int)targetX, false, 0.7f)));
                Add(new Coroutine(level.ZoomTo(new Vector2(targetX - level.Camera.X, 90f), 2f, 2f)));
                yield return new FadeWipe(level, false)
                {
                        Duration = 2f
                }.Wait();
                EndCutscene(level);
        }

        public override void OnEnd(Level level) => level.OnEndOfFrame += () =>
        {
            level.Remove(player);
            level.UnloadLevel();
            level.Session.Level = "inside";
            Session session = level.Session;
            Level level1 = level;
            Rectangle bounds = level.Bounds;
            double left = bounds.Left;
            bounds = level.Bounds;
            double top = bounds.Top;
            Vector2 from = new Vector2((float) left, (float) top);
            Vector2? nullable = level1.GetSpawnPoint(from);
            session.RespawnPoint = nullable;
            level.LoadLevel(Player.IntroTypes.None);
            level.Add(new CS08_Ending());
        };
    }
}
