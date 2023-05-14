// Decompiled with JetBrains decompiler
// Type: Celeste.CS05_Reflection1
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS05_Reflection1 : CutsceneEntity
    {
        public const string Flag = "reflection";
        private readonly Player player;

        public CS05_Reflection1(Player player)
            : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS05_Reflection1 cs05Reflection1 = this;
            cs05Reflection1.player.StateMachine.State = 11;
            cs05Reflection1.player.StateMachine.Locked = true;
            cs05Reflection1.player.ForceCameraUpdate = true;
            TempleMirror first = cs05Reflection1.Scene.Entities.FindFirst<TempleMirror>();
            yield return cs05Reflection1.player.DummyWalkTo(first.Center.X + 8f);
            yield return 0.2f;
            cs05Reflection1.player.Facing = Facings.Left;
            yield return 0.3f;
            yield return !cs05Reflection1.player.Dead
                ? Textbox.Say("ch5_reflection", new Func<IEnumerator>(cs05Reflection1.MadelineFallsToKnees), new Func<IEnumerator>(cs05Reflection1.MadelineStopsPanicking), new Func<IEnumerator>(cs05Reflection1.MadelineGetsUp))
                : 100f;
            yield return cs05Reflection1.Level.ZoomBack(0.5f);
            cs05Reflection1.EndCutscene(level);
        }

        private IEnumerator MadelineFallsToKnees()
        {
            CS05_Reflection1 cs05Reflection1 = this;
            yield return 0.2f;
            cs05Reflection1.player.DummyAutoAnimate = false;
            cs05Reflection1.player.Sprite.Play("tired");
            yield return 0.2f;
            yield return cs05Reflection1.Level.ZoomTo(new Vector2(90f, 116f), 2f, 0.5f);
            yield return 0.2f;
        }

        private IEnumerator MadelineStopsPanicking()
        {
            yield return 0.8f;
            player.Sprite.Play("tiredStill");
            yield return 0.4f;
        }

        private IEnumerator MadelineGetsUp()
        {
            player.DummyAutoAnimate = true;
            player.Sprite.Play("idle");
            yield break;
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            player.ForceCameraUpdate = false;
            player.FlipInReflection = false;
            level.Session.SetFlag("reflection");
        }
    }
}
