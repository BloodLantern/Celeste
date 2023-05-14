// Decompiled with JetBrains decompiler
// Type: Celeste.CS05_SaveTheo
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class CS05_SaveTheo : CutsceneEntity
    {
        public const string Flag = "foundTheoInCrystal";
        private Player player;
        private TheoCrystal theo;
        private Vector2 playerEndPosition;
        private bool wasDashAssistOn;

        public CS05_SaveTheo(Player player)
            : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            theo = level.Tracker.GetEntity<TheoCrystal>();
            playerEndPosition = theo.Position + new Vector2(-24f, 0.0f);
            wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS05_SaveTheo cs05SaveTheo = this;
            cs05SaveTheo.player.StateMachine.State = 11;
            cs05SaveTheo.player.StateMachine.Locked = true;
            cs05SaveTheo.player.ForceCameraUpdate = true;
            _ = level.Session.Audio.Music.Layer(6, 0.0f);
            level.Session.Audio.Apply();
            yield return cs05SaveTheo.player.DummyWalkTo(cs05SaveTheo.theo.X - 18f);
            cs05SaveTheo.player.Facing = Facings.Right;
            yield return Textbox.Say("ch5_found_theo", new Func<IEnumerator>(cs05SaveTheo.TryToBreakCrystal));
            yield return 0.25f;
            yield return cs05SaveTheo.Level.ZoomBack(0.5f);
            cs05SaveTheo.EndCutscene(level);
        }

        private IEnumerator TryToBreakCrystal()
        {
            CS05_SaveTheo cs05SaveTheo = this;
            cs05SaveTheo.Scene.Entities.FindFirst<TheoCrystalPedestal>().Collidable = true;
            yield return cs05SaveTheo.player.DummyWalkTo(cs05SaveTheo.theo.X);
            yield return 0.1f;
            yield return cs05SaveTheo.Level.ZoomTo(new Vector2(160f, 90f), 2f, 0.5f);
            cs05SaveTheo.player.DummyAutoAnimate = false;
            cs05SaveTheo.player.Sprite.Play("lookUp");
            yield return 1f;
            cs05SaveTheo.wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
            SaveData.Instance.Assists.DashAssist = false;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            MInput.Disabled = true;
            cs05SaveTheo.player.OverrideDashDirection = new Vector2?(new Vector2(0.0f, -1f));
            cs05SaveTheo.player.StateMachine.Locked = false;
            cs05SaveTheo.player.StateMachine.State = cs05SaveTheo.player.StartDash();
            cs05SaveTheo.player.Dashes = 0;
            yield return 0.1f;
            while (!cs05SaveTheo.player.OnGround() || cs05SaveTheo.player.Speed.Y < 0.0)
            {
                cs05SaveTheo.player.Dashes = 0;
                Input.MoveY.Value = -1;
                Input.MoveX.Value = 0;
                yield return null;
            }
            cs05SaveTheo.player.OverrideDashDirection = new Vector2?();
            cs05SaveTheo.player.StateMachine.State = 11;
            cs05SaveTheo.player.StateMachine.Locked = true;
            MInput.Disabled = false;
            cs05SaveTheo.player.DummyAutoAnimate = true;
            yield return cs05SaveTheo.player.DummyWalkToExact((int)cs05SaveTheo.playerEndPosition.X, true);
            yield return 1.5f;
        }

        public override void OnEnd(Level level)
        {
            SaveData.Instance.Assists.DashAssist = wasDashAssistOn;
            player.Position = playerEndPosition;
            while (!player.OnGround())
            {
                _ = player.MoveV(1f);
            }

            level.Camera.Position = player.CameraTarget;
            level.Session.SetFlag("foundTheoInCrystal");
            level.ResetZoom();
            _ = level.Session.Audio.Music.Layer(6, 1f);
            level.Session.Audio.Apply();
            List<Follower> followerList = new(player.Leader.Followers);
            player.RemoveSelf();
            level.Add(player = new Player(player.Position, player.DefaultSpriteMode));
            foreach (Follower follower in followerList)
            {
                player.Leader.Followers.Add(follower);
                follower.Leader = player.Leader;
            }
            player.Facing = Facings.Right;
            player.IntroType = Player.IntroTypes.None;
            TheoCrystalPedestal first = Scene.Entities.FindFirst<TheoCrystalPedestal>();
            first.Collidable = false;
            first.DroppedTheo = true;
            theo.Depth = 100;
            theo.OnPedestal = false;
            theo.Speed = Vector2.Zero;
            while (!theo.OnGround())
            {
                _ = theo.MoveV(1f);
            }
        }
    }
}
