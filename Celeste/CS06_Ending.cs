﻿// Decompiled with JetBrains decompiler
// Type: Celeste.CS06_Ending
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS06_Ending : CutsceneEntity
    {
        private readonly Player player;
        private BadelineDummy badeline;
        private readonly NPC granny;
        private NPC theo;

        public CS06_Ending(Player player, NPC granny)
            : base(false, true)
        {
            this.player = player;
            this.granny = granny;
        }

        public override void OnBegin(Level level)
        {
            level.RegisterAreaComplete();
            theo = Scene.Entities.FindFirst<NPC06_Theo_Ending>();
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS06_Ending cs06Ending = this;
            cs06Ending.player.StateMachine.State = 11;
            cs06Ending.player.StateMachine.Locked = true;
            yield return 1f;
            cs06Ending.player.Dashes = 1;
            level.Session.Inventory.Dashes = 1;
            level.Add(cs06Ending.badeline = new BadelineDummy(cs06Ending.player.Center));
            cs06Ending.badeline.Appear(level, true);
            cs06Ending.badeline.FloatSpeed = 80f;
            cs06Ending.badeline.Sprite.Scale.X = -1f;
            _ = Audio.Play("event:/char/badeline/maddy_split", cs06Ending.player.Center);
            yield return cs06Ending.badeline.FloatTo(cs06Ending.player.Position + new Vector2(24f, -20f), new int?(-1), false);
            yield return level.ZoomTo(new Vector2(160f, 120f), 2f, 1f);
            yield return Textbox.Say("ch6_ending", new Func<IEnumerator>(cs06Ending.GrannyEnter), new Func<IEnumerator>(cs06Ending.TheoEnter), new Func<IEnumerator>(cs06Ending.MaddyTurnsRight), new Func<IEnumerator>(cs06Ending.BadelineTurnsRight), new Func<IEnumerator>(cs06Ending.BadelineTurnsLeft), new Func<IEnumerator>(cs06Ending.WaitAbit), new Func<IEnumerator>(cs06Ending.TurnToLeft), new Func<IEnumerator>(cs06Ending.TheoRaiseFist), new Func<IEnumerator>(cs06Ending.TheoStopTired));
            _ = Audio.Play("event:/char/madeline/backpack_drop", cs06Ending.player.Position);
            cs06Ending.player.DummyAutoAnimate = false;
            cs06Ending.player.Sprite.Play("bagdown");
            cs06Ending.EndCutscene(level);
        }

        private IEnumerator GrannyEnter()
        {
            CS06_Ending cs06Ending = this;
            yield return 0.25f;
            cs06Ending.badeline.Sprite.Scale.X = 1f;
            yield return 0.1f;
            cs06Ending.granny.Visible = true;
            cs06Ending.Add(new Coroutine(cs06Ending.badeline.FloatTo(new Vector2(cs06Ending.badeline.X - 10f, cs06Ending.badeline.Y), new int?(1), false)));
            yield return cs06Ending.granny.MoveTo(cs06Ending.player.Position + new Vector2(40f, 0.0f));
        }

        private IEnumerator TheoEnter()
        {
            CS06_Ending cs06Ending = this;
            cs06Ending.player.Facing = Facings.Left;
            cs06Ending.badeline.Sprite.Scale.X = -1f;
            yield return 0.25f;
            yield return CutsceneEntity.CameraTo(new Vector2(cs06Ending.Level.Camera.X - 40f, cs06Ending.Level.Camera.Y), 1f);
            cs06Ending.theo.Visible = true;
            cs06Ending.Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(cs06Ending.Level.Camera.X + 40f, cs06Ending.Level.Camera.Y), 2f, delay: 1f)));
            cs06Ending.Add(new Coroutine(cs06Ending.badeline.FloatTo(new Vector2(cs06Ending.badeline.X + 6f, cs06Ending.badeline.Y + 4f), new int?(-1), false)));
            yield return cs06Ending.theo.MoveTo(cs06Ending.player.Position + new Vector2(-32f, 0.0f));
            cs06Ending.theo.Sprite.Play("tired");
        }

        private IEnumerator MaddyTurnsRight()
        {
            yield return 0.1f;
            player.Facing = Facings.Right;
            yield return 0.1f;
            yield return badeline.FloatTo(badeline.Position + new Vector2(-2f, 10f), new int?(-1), false);
            yield return 0.1f;
        }

        private IEnumerator BadelineTurnsRight()
        {
            yield return 0.1f;
            badeline.Sprite.Scale.X = 1f;
            yield return 0.1f;
        }

        private IEnumerator BadelineTurnsLeft()
        {
            yield return 0.1f;
            badeline.Sprite.Scale.X = -1f;
            yield return 0.1f;
        }

        private IEnumerator WaitAbit()
        {
            yield return 0.4f;
        }

        private IEnumerator TurnToLeft()
        {
            yield return 0.1f;
            player.Facing = Facings.Left;
            yield return 0.05f;
            badeline.Sprite.Scale.X = -1f;
            yield return 0.1f;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator TheoRaiseFist()
        {
            theo.Sprite.Play("yolo", false, false);
            base.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
            {
                theo.Sprite.Play("yoloEnd", false, false);
            }, 0.8f, true));
            yield return null;
            yield break;
        }

        private IEnumerator TheoStopTired()
        {
            theo.Sprite.Play("idle");
            yield return null;
        }

        public override void OnEnd(Level level)
        {
            _ = level.CompleteArea();
            SpotlightWipe.FocusPoint += new Vector2(0.0f, -20f);
        }
    }
}
