using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS00_Granny : CutsceneEntity
    {
        public const string Flag = "granny";
        private NPC00_Granny granny;
        private Player player;
        private Vector2 endPlayerPosition;
        private Coroutine zoomCoroutine;

        public CS00_Granny(NPC00_Granny granny, Player player)
        {
            this.granny = granny;
            this.player = player;
            endPlayerPosition = granny.Position + new Vector2(48f, 0.0f);
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene()));

        private IEnumerator Cutscene()
        {
            CS00_Granny cs00Granny = this;
            cs00Granny.player.StateMachine.State = 11;
            if (Math.Abs(cs00Granny.player.X - cs00Granny.granny.X) < 20.0)
                yield return cs00Granny.player.DummyWalkTo(cs00Granny.granny.X - 48f);
            cs00Granny.player.Facing = Facings.Right;
            yield return 0.5f;
            yield return Textbox.Say("CH0_GRANNY", cs00Granny.Meet, cs00Granny.RunAlong, cs00Granny.LaughAndAirQuotes, cs00Granny.Laugh, cs00Granny.StopLaughing, cs00Granny.OminousZoom, cs00Granny.PanToMaddy);
            yield return cs00Granny.Level.ZoomBack(0.5f);
            cs00Granny.EndCutscene(cs00Granny.Level);
        }

        private IEnumerator Meet()
        {
            yield return 0.25f;
            granny.Sprite.Scale.X = Math.Sign(player.X - granny.X);
            yield return player.DummyWalkTo(granny.X - 20f);
            player.Facing = Facings.Right;
            yield return 0.8f;
        }

        private IEnumerator RunAlong()
        {
            CS00_Granny cs00Granny = this;
            yield return cs00Granny.player.DummyWalkToExact((int) cs00Granny.endPlayerPosition.X);
            yield return 0.8f;
            cs00Granny.player.Facing = Facings.Left;
            yield return 0.4f;
            cs00Granny.granny.Sprite.Scale.X = 1f;
            yield return cs00Granny.Level.ZoomTo(new Vector2(210f, 90f), 2f, 0.5f);
            yield return 0.2f;
        }

        private IEnumerator LaughAndAirQuotes()
        {
            yield return 0.6f;
            granny.LaughSfx.FirstPlay = true;
            granny.Sprite.Play("laugh");
            yield return 2f;
            granny.Sprite.Play("airQuotes");
        }

        private IEnumerator Laugh()
        {
            granny.LaughSfx.FirstPlay = false;
            yield return null;
            granny.Sprite.Play("laugh");
        }

        private IEnumerator StopLaughing()
        {
            granny.Sprite.Play("idle");
            yield break;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator OminousZoom()
        {
                Vector2 screenSpaceFocusPoint = new Vector2(210f, 100f);
                zoomCoroutine = new Coroutine(Level.ZoomAcross(screenSpaceFocusPoint, 4f, 3f));
                Add(zoomCoroutine);
                granny.Sprite.Play("idle");
                yield return 0.2f;
        }

        private IEnumerator PanToMaddy()
        {
            CS00_Granny cs00Granny = this;
            while (cs00Granny.zoomCoroutine != null && cs00Granny.zoomCoroutine.Active)
                yield return null;
            yield return 0.2f;
            yield return cs00Granny.Level.ZoomAcross(new Vector2(210f, 90f), 2f, 0.5f);
            yield return 0.2f;
        }

        public override void OnEnd(Level level)
        {
            granny.Hahaha.Enabled = true;
            granny.Sprite.Play("laugh");
            granny.Sprite.Scale.X = 1f;
            player.Position.X = endPlayerPosition.X;
            player.Facing = Facings.Left;
            player.StateMachine.State = 0;
            level.Session.SetFlag("granny");
            level.ResetZoom();
        }
    }
}
