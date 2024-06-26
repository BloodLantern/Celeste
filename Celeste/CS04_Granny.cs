﻿using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS04_Granny : CutsceneEntity
    {
        public const string Flag = "granny_1";
        private NPC04_Granny granny;
        private Player player;

        public CS04_Granny(NPC04_Granny granny, Player player)
        {
            this.granny = granny;
            this.player = player;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS04_Granny cs04Granny = this;
            cs04Granny.player.StateMachine.State = 11;
            cs04Granny.player.StateMachine.Locked = true;
            cs04Granny.player.ForceCameraUpdate = true;
            yield return cs04Granny.player.DummyWalkTo(cs04Granny.granny.X - 30f);
            cs04Granny.player.Facing = Facings.Right;
            yield return Textbox.Say("CH4_GRANNY_1", cs04Granny.Laughs, cs04Granny.StopLaughing, cs04Granny.WaitABeat, cs04Granny.ZoomIn, cs04Granny.MaddyTurnsAround, cs04Granny.MaddyApproaches, cs04Granny.MaddyWalksPastGranny);
            yield return cs04Granny.Level.ZoomBack(0.5f);
            cs04Granny.EndCutscene(level);
        }

        private IEnumerator Laughs()
        {
            granny.Sprite.Play("laugh");
            yield return 1f;
        }

        private IEnumerator StopLaughing()
        {
            granny.Sprite.Play("idle");
            yield return 0.25f;
        }

        private IEnumerator WaitABeat()
        {
            yield return 1.2f;
        }

        private IEnumerator ZoomIn()
        {
                // ISSUE: reference to a compiler-generated field
                yield return Level.ZoomTo(new Vector2(123f, 116f), 2f, 0.5f);
        }

        private IEnumerator MaddyTurnsAround()
        {
            yield return 0.2f;
            player.Facing = Facings.Left;
            yield return 0.1f;
        }

        private IEnumerator MaddyApproaches()
        {
            yield return player.DummyWalkTo(granny.X - 20f);
        }

        private IEnumerator MaddyWalksPastGranny()
        {
            yield return player.DummyWalkToExact((int) granny.X + 30);
        }

        public override void OnEnd(Level level)
        {
            player.X = granny.X + 30f;
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            player.ForceCameraUpdate = false;
            if (WasSkipped)
                level.Camera.Position = player.CameraTarget;
            granny.Sprite.Play("laugh");
            level.Session.SetFlag("granny_1");
        }
    }
}
