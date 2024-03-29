﻿using Microsoft.Xna.Framework;
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
            : base()
        {
            this.granny = granny;
            this.player = player;
            this.endPlayerPosition = granny.Position + new Vector2(48f, 0.0f);
        }

        public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene()));

        private IEnumerator Cutscene()
        {
            CS00_Granny cs00Granny = this;
            cs00Granny.player.StateMachine.State = 11;
            if ((double) Math.Abs(cs00Granny.player.X - cs00Granny.granny.X) < 20.0)
                yield return (object) cs00Granny.player.DummyWalkTo(cs00Granny.granny.X - 48f);
            cs00Granny.player.Facing = Facings.Right;
            yield return (object) 0.5f;
            yield return (object) Textbox.Say("CH0_GRANNY", new Func<IEnumerator>(cs00Granny.Meet), new Func<IEnumerator>(cs00Granny.RunAlong), new Func<IEnumerator>(cs00Granny.LaughAndAirQuotes), new Func<IEnumerator>(cs00Granny.Laugh), new Func<IEnumerator>(cs00Granny.StopLaughing), new Func<IEnumerator>(cs00Granny.OminousZoom), new Func<IEnumerator>(cs00Granny.PanToMaddy));
            yield return (object) cs00Granny.Level.ZoomBack(0.5f);
            cs00Granny.EndCutscene(cs00Granny.Level);
        }

        private IEnumerator Meet()
        {
            yield return (object) 0.25f;
            this.granny.Sprite.Scale.X = (float) Math.Sign(this.player.X - this.granny.X);
            yield return (object) this.player.DummyWalkTo(this.granny.X - 20f);
            this.player.Facing = Facings.Right;
            yield return (object) 0.8f;
        }

        private IEnumerator RunAlong()
        {
            CS00_Granny cs00Granny = this;
            yield return (object) cs00Granny.player.DummyWalkToExact((int) cs00Granny.endPlayerPosition.X);
            yield return (object) 0.8f;
            cs00Granny.player.Facing = Facings.Left;
            yield return (object) 0.4f;
            cs00Granny.granny.Sprite.Scale.X = 1f;
            yield return (object) cs00Granny.Level.ZoomTo(new Vector2(210f, 90f), 2f, 0.5f);
            yield return (object) 0.2f;
        }

        private IEnumerator LaughAndAirQuotes()
        {
            yield return (object) 0.6f;
            this.granny.LaughSfx.FirstPlay = true;
            this.granny.Sprite.Play("laugh");
            yield return (object) 2f;
            this.granny.Sprite.Play("airQuotes");
        }

        private IEnumerator Laugh()
        {
            this.granny.LaughSfx.FirstPlay = false;
            yield return (object) null;
            this.granny.Sprite.Play("laugh");
        }

        private IEnumerator StopLaughing()
        {
            this.granny.Sprite.Play("idle");
            yield break;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator OminousZoom()
        {
                Vector2 screenSpaceFocusPoint = new Vector2(210f, 100f);
                this.zoomCoroutine = new Coroutine(this.Level.ZoomAcross(screenSpaceFocusPoint, 4f, 3f), true);
                base.Add(this.zoomCoroutine);
                this.granny.Sprite.Play("idle", false, false);
                yield return 0.2f;
                yield break;
        }

        private IEnumerator PanToMaddy()
        {
            CS00_Granny cs00Granny = this;
            while (cs00Granny.zoomCoroutine != null && cs00Granny.zoomCoroutine.Active)
                yield return (object) null;
            yield return (object) 0.2f;
            yield return (object) cs00Granny.Level.ZoomAcross(new Vector2(210f, 90f), 2f, 0.5f);
            yield return (object) 0.2f;
        }

        public override void OnEnd(Level level)
        {
            this.granny.Hahaha.Enabled = true;
            this.granny.Sprite.Play("laugh");
            this.granny.Sprite.Scale.X = 1f;
            this.player.Position.X = this.endPlayerPosition.X;
            this.player.Facing = Facings.Left;
            this.player.StateMachine.State = 0;
            level.Session.SetFlag("granny");
            level.ResetZoom();
        }
    }
}
