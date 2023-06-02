using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS06_Granny : CutsceneEntity
    {
        public const string FlagPrefix = "granny_";
        private NPC06_Granny granny;
        private Player player;
        private float startX;
        private int index;
        private bool firstLaugh;

        public CS06_Granny(NPC06_Granny granny, Player player, int index)
            : base()
        {
            this.granny = granny;
            this.player = player;
            this.index = index;
        }

        public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS06_Granny cs06Granny = this;
            cs06Granny.player.StateMachine.State = 11;
            cs06Granny.player.StateMachine.Locked = true;
            cs06Granny.player.ForceCameraUpdate = true;
            if (cs06Granny.index == 0)
            {
                yield return (object) cs06Granny.player.DummyWalkTo(cs06Granny.granny.X - 40f);
                cs06Granny.startX = cs06Granny.player.X;
                cs06Granny.player.Facing = Facings.Right;
                cs06Granny.firstLaugh = true;
                yield return (object) Textbox.Say("ch6_oldlady", new Func<IEnumerator>(cs06Granny.ZoomIn), new Func<IEnumerator>(cs06Granny.Laughs), new Func<IEnumerator>(cs06Granny.StopLaughing), new Func<IEnumerator>(cs06Granny.MaddyWalksRight), new Func<IEnumerator>(cs06Granny.MaddyWalksLeft), new Func<IEnumerator>(cs06Granny.WaitABit), new Func<IEnumerator>(cs06Granny.MaddyTurnsRight));
            }
            else if (cs06Granny.index == 1)
            {
                yield return (object) cs06Granny.ZoomIn();
                yield return (object) cs06Granny.player.DummyWalkTo(cs06Granny.granny.X - 20f);
                cs06Granny.player.Facing = Facings.Right;
                yield return (object) Textbox.Say("ch6_oldlady_b");
            }
            else if (cs06Granny.index == 2)
            {
                yield return (object) cs06Granny.ZoomIn();
                yield return (object) cs06Granny.player.DummyWalkTo(cs06Granny.granny.X - 20f);
                cs06Granny.player.Facing = Facings.Right;
                yield return (object) Textbox.Say("ch6_oldlady_c");
            }
            yield return (object) cs06Granny.Level.ZoomBack(0.5f);
            cs06Granny.EndCutscene(level);
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator ZoomIn()
        {
                Vector2 screenSpaceFocusPoint = Vector2.Lerp(this.granny.Position, this.player.Position, 0.5f) - this.Level.Camera.Position + new Vector2(0f, -20f);
                yield return this.Level.ZoomTo(screenSpaceFocusPoint, 2f, 0.5f);
                yield break;
        }

        private IEnumerator Laughs()
        {
            if (this.firstLaugh)
            {
                this.firstLaugh = false;
                yield return (object) 0.5f;
            }
            this.granny.Sprite.Play("laugh");
            yield return (object) 1f;
        }

        private IEnumerator StopLaughing()
        {
            this.granny.Sprite.Play("idle");
            yield return (object) 0.25f;
        }

        private IEnumerator MaddyWalksLeft()
        {
            yield return (object) 0.1f;
            this.player.Facing = Facings.Left;
            yield return (object) this.player.DummyWalkToExact((int) this.player.X - 8);
            yield return (object) 0.1f;
        }

        private IEnumerator MaddyWalksRight()
        {
            yield return (object) 0.1f;
            this.player.Facing = Facings.Right;
            yield return (object) this.player.DummyWalkToExact((int) this.player.X + 8);
            yield return (object) 0.1f;
        }

        private IEnumerator WaitABit()
        {
            yield return (object) 0.8f;
        }

        private IEnumerator MaddyTurnsRight()
        {
            yield return (object) 0.1f;
            this.player.Facing = Facings.Right;
            yield return (object) 0.1f;
        }

        public override void OnEnd(Level level)
        {
            this.player.StateMachine.Locked = false;
            this.player.StateMachine.State = 0;
            this.player.ForceCameraUpdate = false;
            this.granny.Sprite.Play("idle");
            level.Session.SetFlag("granny_" + (object) this.index);
        }
    }
}
