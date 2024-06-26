﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS05_SeeTheo : CutsceneEntity
    {
        private const float NewDarknessAlpha = 0.3f;
        public const string Flag = "seeTheoInCrystal";
        private int index;
        private Player player;
        private TheoCrystal theo;

        public CS05_SeeTheo(Player player, int index)
        {
            this.player = player;
            this.index = index;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS05_SeeTheo cs05SeeTheo = this;
            while (cs05SeeTheo.player.Scene == null || !cs05SeeTheo.player.OnGround())
                yield return null;
            cs05SeeTheo.player.StateMachine.State = 11;
            cs05SeeTheo.player.StateMachine.Locked = true;
            yield return 0.25f;
            cs05SeeTheo.theo = cs05SeeTheo.Scene.Tracker.GetEntity<TheoCrystal>();
            if (cs05SeeTheo.theo != null && Math.Sign(cs05SeeTheo.player.X - cs05SeeTheo.theo.X) != 0)
                cs05SeeTheo.player.Facing = (Facings) Math.Sign(cs05SeeTheo.theo.X - cs05SeeTheo.player.X);
            yield return 0.25f;
            if (cs05SeeTheo.index == 0)
                yield return Textbox.Say("ch5_see_theo", cs05SeeTheo.ZoomIn, cs05SeeTheo.MadelineTurnsAround, cs05SeeTheo.WaitABit, cs05SeeTheo.MadelineTurnsBackAndBrighten);
            else if (cs05SeeTheo.index == 1)
                yield return Textbox.Say("ch5_see_theo_b");
            yield return cs05SeeTheo.Level.ZoomBack(0.5f);
            cs05SeeTheo.EndCutscene(level);
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator ZoomIn()
                {
                yield return Level.ZoomTo(Vector2.Lerp(player.Position, theo.Position, 0.5f) - Level.Camera.Position + new Vector2(0f, -20f), 2f, 0.5f);
                }

                private IEnumerator MadelineTurnsAround()
        {
            yield return 0.3f;
            player.Facing = Facings.Left;
            yield return 0.1f;
        }

        private IEnumerator WaitABit()
        {
            yield return 1f;
        }

        private IEnumerator MadelineTurnsBackAndBrighten()
        {
            CS05_SeeTheo cs05SeeTheo = this;
            yield return 0.1f;
            Coroutine coroutine = new Coroutine(cs05SeeTheo.Brighten());
            cs05SeeTheo.Add(coroutine);
            yield return 0.2f;
            cs05SeeTheo.player.Facing = Facings.Right;
            yield return 0.1f;
            while (coroutine.Active)
                yield return null;
        }

        private IEnumerator Brighten()
        {
            CS05_SeeTheo cs05SeeTheo = this;
            yield return cs05SeeTheo.Level.ZoomBack(0.5f);
            yield return 0.3f;
            cs05SeeTheo.Level.Session.DarkRoomAlpha = 0.3f;
            float darkness = cs05SeeTheo.Level.Session.DarkRoomAlpha;
            while (cs05SeeTheo.Level.Lighting.Alpha != (double) darkness)
            {
                cs05SeeTheo.Level.Lighting.Alpha = Calc.Approach(cs05SeeTheo.Level.Lighting.Alpha, darkness, Engine.DeltaTime * 0.5f);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            player.ForceCameraUpdate = false;
            player.DummyAutoAnimate = true;
            level.Session.DarkRoomAlpha = 0.3f;
            level.Lighting.Alpha = level.Session.DarkRoomAlpha;
            level.Session.SetFlag("seeTheoInCrystal");
        }
    }
}
