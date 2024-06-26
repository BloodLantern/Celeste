﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class CS02_Mirror : CutsceneEntity
    {
        private Player player;
        private DreamMirror mirror;
        private float playerEndX;
        private int direction = 1;
        private SoundSource sfx;

        public CS02_Mirror(Player player, DreamMirror mirror)
        {
            this.player = player;
            this.mirror = mirror;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS02_Mirror cs02Mirror = this;
            cs02Mirror.Add(cs02Mirror.sfx = new SoundSource());
            cs02Mirror.sfx.Position = cs02Mirror.mirror.Center;
            cs02Mirror.sfx.Play("event:/music/lvl2/dreamblock_sting_pt1");
            cs02Mirror.direction = Math.Sign(cs02Mirror.player.X - cs02Mirror.mirror.X);
            cs02Mirror.player.StateMachine.State = 11;
            cs02Mirror.playerEndX = 8 * cs02Mirror.direction;
            yield return 1f;
            cs02Mirror.player.Facing = (Facings) (-cs02Mirror.direction);
            yield return 0.4f;
            yield return cs02Mirror.player.DummyRunTo(cs02Mirror.mirror.X + cs02Mirror.playerEndX);
            yield return 0.5f;
            yield return level.ZoomTo(cs02Mirror.mirror.Position - level.Camera.Position - Vector2.UnitY * 24f, 2f, 1f);
            yield return 0.5f;
            yield return cs02Mirror.mirror.BreakRoutine(cs02Mirror.direction);
            cs02Mirror.player.DummyAutoAnimate = false;
            cs02Mirror.player.Sprite.Play("lookUp");
            Vector2 from = level.Camera.Position;
            Vector2 to = level.Camera.Position + new Vector2(0.0f, -80f);
            for (float ease = 0.0f; ease < 1.0; ease += Engine.DeltaTime * 1.2f)
            {
                level.Camera.Position = from + (to - from) * Ease.CubeInOut(ease);
                yield return null;
            }
            cs02Mirror.Add(new Coroutine(cs02Mirror.ZoomBack()));
            List<Entity>.Enumerator enumerator = cs02Mirror.Scene.Tracker.GetEntities<DreamBlock>().GetEnumerator();
            try
            {
                if (enumerator.MoveNext())
                    yield return ((DreamBlock) enumerator.Current).Activate();
            }
            finally
            {
                enumerator.Dispose();
            }
            enumerator = new List<Entity>.Enumerator();
            from = new Vector2();
            to = new Vector2();
            yield return 0.5f;
            cs02Mirror.EndCutscene(level);
        }

        private IEnumerator ZoomBack()
        {
            CS02_Mirror cs02Mirror = this;
            yield return 1.2f;
            yield return cs02Mirror.Level.ZoomBack(3f);
        }

        public override void OnEnd(Level level)
        {
            mirror.Broken(WasSkipped);
            if (WasSkipped)
                SceneAs<Level>().ParticlesFG.Clear();
            Player entity1 = Scene.Tracker.GetEntity<Player>();
            if (entity1 != null)
            {
                entity1.StateMachine.State = 0;
                entity1.DummyAutoAnimate = true;
                entity1.Speed = Vector2.Zero;
                entity1.X = mirror.X + playerEndX;
                entity1.Facing = direction == 0 ? Facings.Right : (Facings) (-direction);
            }
            foreach (DreamBlock entity2 in Scene.Tracker.GetEntities<DreamBlock>())
                entity2.ActivateNoRoutine();
            level.ResetZoom();
            level.Session.Inventory.DreamDash = true;
            level.Session.Audio.Music.Event = "event:/music/lvl2/mirror";
            level.Session.Audio.Apply();
        }
    }
}
