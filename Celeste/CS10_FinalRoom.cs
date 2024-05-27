using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS10_FinalRoom : CutsceneEntity
    {
        private Player player;
        private BadelineDummy badeline;
        private bool first;

        public CS10_FinalRoom(Player player, bool first)
        {
            Depth = -8500;
            this.player = player;
            this.first = first;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS10_FinalRoom cs10FinalRoom = this;
            cs10FinalRoom.player.StateMachine.State = 11;
            if (cs10FinalRoom.first)
            {
                yield return cs10FinalRoom.player.DummyWalkToExact((int) (cs10FinalRoom.player.X + 16.0));
                yield return 0.5f;
            }
            else
            {
                cs10FinalRoom.player.DummyAutoAnimate = false;
                cs10FinalRoom.player.Sprite.Play("sitDown");
                cs10FinalRoom.player.Sprite.SetAnimationFrame(cs10FinalRoom.player.Sprite.CurrentAnimationTotalFrames - 1);
                yield return 1.25f;
            }
            yield return cs10FinalRoom.BadelineAppears();
            if (cs10FinalRoom.first)
                yield return Textbox.Say("CH9_LAST_ROOM");
            else
                yield return Textbox.Say("CH9_LAST_ROOM_ALT");
            yield return cs10FinalRoom.BadelineVanishes();
            cs10FinalRoom.EndCutscene(level);
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator BadelineAppears()
        {
                Level.Add(badeline = new BadelineDummy(player.Position + new Vector2(18f, -8f)));
                Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
                Audio.Play("event:/char/badeline/maddy_split", badeline.Position);
                badeline.Sprite.Scale.X = -1f;
                yield return null;
        }

        private IEnumerator BadelineVanishes()
        {
            yield return 0.2f;
            badeline.Vanish();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            badeline = null;
            yield return 0.5f;
            player.Facing = Facings.Right;
        }

        public override void OnEnd(Level level)
        {
            Level.Session.Inventory.Dashes = 1;
            player.StateMachine.State = 0;
            if (!first && !WasSkipped)
                Audio.Play("event:/char/madeline/stand", player.Position);
            if (badeline == null)
                return;
            badeline.RemoveSelf();
        }
    }
}
