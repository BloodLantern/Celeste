﻿using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS05_Entrance : CutsceneEntity
    {
        public const string Flag = "entrance";
        private NPC theo;
        private Player player;
        private Vector2 playerMoveTo;

        public CS05_Entrance(NPC theo)
        {
            this.theo = theo;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS05_Entrance cs05Entrance = this;
            cs05Entrance.player = level.Tracker.GetEntity<Player>();
            cs05Entrance.player.StateMachine.State = 11;
            cs05Entrance.player.StateMachine.Locked = true;
            cs05Entrance.player.X = cs05Entrance.theo.X - 32f;
            cs05Entrance.playerMoveTo = new Vector2(cs05Entrance.theo.X - 32f, cs05Entrance.player.Y);
            cs05Entrance.player.Facing = Facings.Left;
            SpotlightWipe.FocusPoint = cs05Entrance.theo.TopCenter - Vector2.UnitX * 16f - level.Camera.Position;
            yield return 2f;
            cs05Entrance.player.Facing = Facings.Right;
            yield return 0.3f;
            yield return cs05Entrance.theo.MoveTo(new Vector2(cs05Entrance.theo.X + 48f, cs05Entrance.theo.Y));
            yield return Textbox.Say("ch5_entrance", cs05Entrance.MaddyTurnsRight, cs05Entrance.TheoTurns, cs05Entrance.TheoLeaves);
            cs05Entrance.EndCutscene(level);
        }

        private IEnumerator MaddyTurnsRight()
        {
            player.Facing = Facings.Right;
            yield break;
        }

        private IEnumerator TheoTurns()
        {
            theo.Sprite.Scale.X *= -1f;
            yield break;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator TheoLeaves()
        {
                yield return theo.MoveTo(new Vector2(Level.Bounds.Right + 32, theo.Y));
        }

                public override void OnEnd(Level level)
        {
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = 0;
                player.ForceCameraUpdate = false;
                player.Position = playerMoveTo;
                player.Facing = Facings.Right;
            }
            Scene.Remove(theo);
            level.Session.SetFlag("entrance");
        }
    }
}
