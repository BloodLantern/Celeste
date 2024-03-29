﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS08_EnterDoor : CutsceneEntity
    {
        private Player player;
        private float targetX;

        public CS08_EnterDoor(Player player, float targetX)
            : base()
        {
            this.player = player;
            this.targetX = targetX;
        }

        public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

        // ISSUE: reference to a compiler-generated field
        private IEnumerator Cutscene(Level level)
        {
                this.player.StateMachine.State = 11;
                base.Add(new Coroutine(this.player.DummyWalkToExact((int)this.targetX, false, 0.7f, false), true));
                base.Add(new Coroutine(level.ZoomTo(new Vector2(this.targetX - level.Camera.X, 90f), 2f, 2f), true));
                yield return new FadeWipe(level, false, null)
                {
                        Duration = 2f
                }.Wait();
                base.EndCutscene(level, true);
                yield break;
        }

        public override void OnEnd(Level level) => level.OnEndOfFrame += (Action) (() =>
        {
            level.Remove((Entity) this.player);
            level.UnloadLevel();
            level.Session.Level = "inside";
            Session session = level.Session;
            Level level1 = level;
            Rectangle bounds = level.Bounds;
            double left = (double) bounds.Left;
            bounds = level.Bounds;
            double top = (double) bounds.Top;
            Vector2 from = new Vector2((float) left, (float) top);
            Vector2? nullable = new Vector2?(level1.GetSpawnPoint(from));
            session.RespawnPoint = nullable;
            level.LoadLevel(Player.IntroTypes.None);
            level.Add((Entity) new CS08_Ending());
        });
    }
}
