using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS05_TheoPhone : CutsceneEntity
    {
        private Player player;
        private float targetX;

        public CS05_TheoPhone(Player player, float targetX)
        {
            this.player = player;
            this.targetX = targetX;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Routine()));

        private IEnumerator Routine()
        {
            CS05_TheoPhone cs05TheoPhone = this;
            cs05TheoPhone.player.StateMachine.State = 11;
            if (cs05TheoPhone.player.X != (double) cs05TheoPhone.targetX)
                cs05TheoPhone.player.Facing = (Facings) Math.Sign(cs05TheoPhone.targetX - cs05TheoPhone.player.X);
            yield return 0.5f;
            yield return cs05TheoPhone.Level.ZoomTo(new Vector2(80f, 60f), 2f, 0.5f);
            yield return Textbox.Say("CH5_PHONE", cs05TheoPhone.WalkToPhone, cs05TheoPhone.StandBackUp);
            yield return cs05TheoPhone.Level.ZoomBack(0.5f);
            cs05TheoPhone.EndCutscene(cs05TheoPhone.Level);
        }

        private IEnumerator WalkToPhone()
        {
            yield return 0.25f;
            yield return player.DummyWalkToExact((int) targetX);
            player.Facing = Facings.Left;
            yield return 0.5f;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("duck");
            yield return 0.5f;
        }

        private IEnumerator StandBackUp()
        {
            RemovePhone();
            yield return 0.6f;
            player.Sprite.Play("idle");
            yield return 0.2f;
        }

        public override void OnEnd(Level level)
        {
            RemovePhone();
            player.StateMachine.State = 0;
        }

        private void RemovePhone() => Scene.Entities.FindFirst<TheoPhone>()?.RemoveSelf();
    }
}
