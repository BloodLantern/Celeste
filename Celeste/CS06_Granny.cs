// Decompiled with JetBrains decompiler
// Type: Celeste.CS06_Granny
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS06_Granny : CutsceneEntity
    {
        public const string FlagPrefix = "granny_";
        private readonly NPC06_Granny granny;
        private readonly Player player;
        private float startX;
        private readonly int index;
        private bool firstLaugh;

        public CS06_Granny(NPC06_Granny granny, Player player, int index)
            : base()
        {
            this.granny = granny;
            this.player = player;
            this.index = index;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS06_Granny cs06Granny = this;
            cs06Granny.player.StateMachine.State = 11;
            cs06Granny.player.StateMachine.Locked = true;
            cs06Granny.player.ForceCameraUpdate = true;
            if (cs06Granny.index == 0)
            {
                yield return cs06Granny.player.DummyWalkTo(cs06Granny.granny.X - 40f);
                cs06Granny.startX = cs06Granny.player.X;
                cs06Granny.player.Facing = Facings.Right;
                cs06Granny.firstLaugh = true;
                yield return Textbox.Say("ch6_oldlady", new Func<IEnumerator>(cs06Granny.ZoomIn), new Func<IEnumerator>(cs06Granny.Laughs), new Func<IEnumerator>(cs06Granny.StopLaughing), new Func<IEnumerator>(cs06Granny.MaddyWalksRight), new Func<IEnumerator>(cs06Granny.MaddyWalksLeft), new Func<IEnumerator>(cs06Granny.WaitABit), new Func<IEnumerator>(cs06Granny.MaddyTurnsRight));
            }
            else if (cs06Granny.index == 1)
            {
                yield return cs06Granny.ZoomIn();
                yield return cs06Granny.player.DummyWalkTo(cs06Granny.granny.X - 20f);
                cs06Granny.player.Facing = Facings.Right;
                yield return Textbox.Say("ch6_oldlady_b");
            }
            else if (cs06Granny.index == 2)
            {
                yield return cs06Granny.ZoomIn();
                yield return cs06Granny.player.DummyWalkTo(cs06Granny.granny.X - 20f);
                cs06Granny.player.Facing = Facings.Right;
                yield return Textbox.Say("ch6_oldlady_c");
            }
            yield return cs06Granny.Level.ZoomBack(0.5f);
            cs06Granny.EndCutscene(level);
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator ZoomIn()
        {
            Vector2 screenSpaceFocusPoint = Vector2.Lerp(granny.Position, player.Position, 0.5f) - Level.Camera.Position + new Vector2(0f, -20f);
            yield return Level.ZoomTo(screenSpaceFocusPoint, 2f, 0.5f);
            yield break;
        }

        private IEnumerator Laughs()
        {
            if (firstLaugh)
            {
                firstLaugh = false;
                yield return 0.5f;
            }
            granny.Sprite.Play("laugh");
            yield return 1f;
        }

        private IEnumerator StopLaughing()
        {
            granny.Sprite.Play("idle");
            yield return 0.25f;
        }

        private IEnumerator MaddyWalksLeft()
        {
            yield return 0.1f;
            player.Facing = Facings.Left;
            yield return player.DummyWalkToExact((int)player.X - 8);
            yield return 0.1f;
        }

        private IEnumerator MaddyWalksRight()
        {
            yield return 0.1f;
            player.Facing = Facings.Right;
            yield return player.DummyWalkToExact((int)player.X + 8);
            yield return 0.1f;
        }

        private IEnumerator WaitABit()
        {
            yield return 0.8f;
        }

        private IEnumerator MaddyTurnsRight()
        {
            yield return 0.1f;
            player.Facing = Facings.Right;
            yield return 0.1f;
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            player.ForceCameraUpdate = false;
            granny.Sprite.Play("idle");
            level.Session.SetFlag("granny_" + index);
        }
    }
}
