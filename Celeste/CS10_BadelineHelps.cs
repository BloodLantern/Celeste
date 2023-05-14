// Decompiled with JetBrains decompiler
// Type: Celeste.CS10_BadelineHelps
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CS10_BadelineHelps : CutsceneEntity
    {
        public const string Flag = "badeline_helps";
        private readonly Player player;
        private BadelineDummy badeline;
        private EventInstance entrySfx;

        public CS10_BadelineHelps(Player player)
            : base()
        {
            Depth = -8500;
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS10_BadelineHelps cs10BadelineHelps = this;
            Vector2 spawn = level.GetSpawnPoint(cs10BadelineHelps.player.Position);
            cs10BadelineHelps.player.Dashes = 2;
            cs10BadelineHelps.player.StateMachine.State = 11;
            cs10BadelineHelps.player.DummyGravity = false;
            cs10BadelineHelps.entrySfx = Audio.Play("event:/new_content/char/madeline/screenentry_stubborn", cs10BadelineHelps.player.Position);
            yield return cs10BadelineHelps.player.MoonLanding(spawn);
            yield return level.ZoomTo(new Vector2(spawn.X - level.Camera.X, 134f), 2f, 0.5f);
            yield return 1f;
            yield return cs10BadelineHelps.BadelineAppears();
            yield return 0.3f;
            yield return Textbox.Say("CH9_HELPING_HAND", new Func<IEnumerator>(cs10BadelineHelps.MadelineFacesAway), new Func<IEnumerator>(cs10BadelineHelps.MadelineFacesBadeline), new Func<IEnumerator>(cs10BadelineHelps.MadelineStepsForwards));
            if (cs10BadelineHelps.badeline != null)
            {
                yield return cs10BadelineHelps.BadelineVanishes();
            }

            yield return level.ZoomBack(0.5f);
            cs10BadelineHelps.EndCutscene(level);
        }

        private IEnumerator BadelineAppears()
        {
            CS10_BadelineHelps cs10BadelineHelps = this;
            cs10BadelineHelps.StartMusic();
            _ = Audio.Play("event:/char/badeline/maddy_split", cs10BadelineHelps.player.Position);
            cs10BadelineHelps.Level.Add(cs10BadelineHelps.badeline = new BadelineDummy(cs10BadelineHelps.player.Center));
            _ = cs10BadelineHelps.Level.Displacement.AddBurst(cs10BadelineHelps.badeline.Center, 0.5f, 8f, 32f, 0.5f);
            cs10BadelineHelps.player.Dashes = 1;
            cs10BadelineHelps.badeline.Sprite.Scale.X = -1f;
            yield return cs10BadelineHelps.badeline.FloatTo(cs10BadelineHelps.player.Center + new Vector2(18f, -10f), new int?(-1), false);
            yield return 0.2f;
            cs10BadelineHelps.player.Facing = Facings.Right;
            yield return null;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator MadelineFacesAway()
        {
            Level.NextColorGrade("feelingdown", 0.1f);
            yield return player.DummyWalkTo(player.X - 16f, false, 1f, false);
            yield break;
        }


        private IEnumerator MadelineFacesBadeline()
        {
            player.Facing = Facings.Right;
            yield return 0.2f;
        }

        private IEnumerator MadelineStepsForwards()
        {
            CS10_BadelineHelps cs10BadelineHelps = this;
            Vector2 spawnPoint = cs10BadelineHelps.Level.GetSpawnPoint(cs10BadelineHelps.player.Position);
            cs10BadelineHelps.Add(new Coroutine(cs10BadelineHelps.player.DummyWalkToExact((int)spawnPoint.X)));
            yield return 0.1f;
            yield return cs10BadelineHelps.badeline.FloatTo(cs10BadelineHelps.badeline.Position + new Vector2(20f, 0.0f), faceDirection: false);
        }

        private IEnumerator BadelineVanishes()
        {
            yield return 0.2f;
            badeline.Vanish();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            badeline = null;
            yield return 0.2f;
        }

        private void StartMusic()
        {
            if (!(Level.Session.Audio.Music.Event != "event:/new_content/music/lvl10/cassette_rooms"))
            {
                return;
            }

            int num = 0;
            CassetteBlockManager entity = Level.Tracker.GetEntity<CassetteBlockManager>();
            if (entity != null)
            {
                num = entity.GetSixteenthNote();
            }

            Level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/cassette_rooms";
            _ = Level.Session.Audio.Music.Param("sixteenth_note", num);
            Level.Session.Audio.Apply(true);
            _ = Level.Session.Audio.Music.Param("sixteenth_note", 7f);
        }

        public override void OnEnd(Level level)
        {
            Level.Session.Inventory.Dashes = 1;
            this.player.Dashes = 1;
            this.player.Depth = 0;
            this.player.Dashes = 1;
            this.player.Speed = Vector2.Zero;
            this.player.Position = level.GetSpawnPoint(this.player.Position);
            Player player = this.player;
            player.Position -= Vector2.UnitY * 12f;
            _ = this.player.MoveVExact(100);
            this.player.Active = true;
            this.player.Visible = true;
            this.player.StateMachine.State = 0;
            badeline?.RemoveSelf();
            level.ResetZoom();
            level.Session.SetFlag("badeline_helps");
            if (!WasSkipped)
            {
                return;
            }

            Audio.Stop(entrySfx);
            StartMusic();
            level.SnapColorGrade("feelingdown");
        }
    }
}
