using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class CS03_OshiroClutter : CutsceneEntity
    {
        private int index;
        private Player player;
        private NPC03_Oshiro_Cluttter oshiro;
        private List<ClutterDoor> doors;

        public CS03_OshiroClutter(Player player, NPC03_Oshiro_Cluttter oshiro, int index)
            : base()
        {
            this.player = player;
            this.oshiro = oshiro;
            this.index = index;
        }

        public override void OnBegin(Level level)
        {
            this.doors = this.Scene.Entities.FindAll<ClutterDoor>();
            this.doors.Sort((Comparison<ClutterDoor>) ((a, b) => (int) ((double) a.Y - (double) b.Y)));
            this.Add((Component) new Coroutine(this.Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS03_OshiroClutter cs03OshiroClutter = this;
            cs03OshiroClutter.player.StateMachine.State = 11;
            cs03OshiroClutter.player.StateMachine.Locked = true;
            if ((cs03OshiroClutter.index == 1 || cs03OshiroClutter.index == 2 ? -1 : 1) == -1)
            {
                yield return (object) cs03OshiroClutter.player.DummyWalkToExact((int) cs03OshiroClutter.oshiro.X - 24);
                cs03OshiroClutter.player.Facing = Facings.Right;
                cs03OshiroClutter.oshiro.Sprite.Scale.X = -1f;
            }
            else
            {
                cs03OshiroClutter.Add((Component) new Coroutine(cs03OshiroClutter.oshiro.PaceRight()));
                yield return (object) cs03OshiroClutter.player.DummyWalkToExact((int) cs03OshiroClutter.oshiro.HomePosition.X + 24);
                cs03OshiroClutter.player.Facing = Facings.Left;
                cs03OshiroClutter.oshiro.Sprite.Scale.X = 1f;
            }
            if (cs03OshiroClutter.index < 3)
            {
                yield return (object) cs03OshiroClutter.Level.ZoomTo(cs03OshiroClutter.oshiro.ZoomPoint, 2f, 0.5f);
                yield return (object) Textbox.Say("CH3_OSHIRO_CLUTTER" + (object) cs03OshiroClutter.index, new Func<IEnumerator>(cs03OshiroClutter.Collapse), new Func<IEnumerator>(cs03OshiroClutter.oshiro.PaceLeft), new Func<IEnumerator>(cs03OshiroClutter.oshiro.PaceRight));
                yield return (object) cs03OshiroClutter.Level.ZoomBack(0.5f);
                level.Session.SetFlag("oshiro_clutter_door_open");
                if (cs03OshiroClutter.index == 0)
                    cs03OshiroClutter.SetMusic();
                foreach (ClutterDoor door in cs03OshiroClutter.doors)
                {
                    if (!door.IsLocked(level.Session))
                        yield return (object) door.UnlockRoutine();
                }
            }
            else
            {
                yield return (object) CutsceneEntity.CameraTo(new Vector2((float) cs03OshiroClutter.Level.Bounds.X, (float) cs03OshiroClutter.Level.Bounds.Y), 0.5f);
                yield return (object) cs03OshiroClutter.Level.ZoomTo(new Vector2(90f, 60f), 2f, 0.5f);
                yield return (object) Textbox.Say("CH3_OSHIRO_CLUTTER_ENDING");
                yield return (object) cs03OshiroClutter.oshiro.MoveTo(new Vector2(cs03OshiroClutter.oshiro.X, (float) (level.Bounds.Top - 32)));
                cs03OshiroClutter.oshiro.Add((Component) new SoundSource("event:/char/oshiro/move_05_09b_exit"));
                yield return (object) cs03OshiroClutter.Level.ZoomBack(0.5f);
            }
            cs03OshiroClutter.EndCutscene(level);
        }

        private IEnumerator Collapse()
        {
            Audio.Play("event:/char/oshiro/chat_collapse", this.oshiro.Position);
            this.oshiro.Sprite.Play("fall");
            yield return (object) 0.5f;
        }

        private void SetMusic()
        {
            Level scene = this.Scene as Level;
            scene.Session.Audio.Music.Event = "event:/music/lvl3/clean";
            scene.Session.Audio.Music.Progress = 1;
            scene.Session.Audio.Apply();
        }

        public override void OnEnd(Level level)
        {
            this.player.StateMachine.Locked = false;
            this.player.StateMachine.State = 0;
            if (this.oshiro.Sprite.CurrentAnimationID == "side")
                (this.oshiro.Sprite as OshiroSprite).Pop("idle", true);
            if (this.index < 3)
            {
                level.Session.SetFlag("oshiro_clutter_door_open");
                level.Session.SetFlag("oshiro_clutter_" + (object) this.index);
                if (this.index == 0 && this.WasSkipped)
                    this.SetMusic();
                foreach (ClutterDoor door in this.doors)
                {
                    if (!door.IsLocked(level.Session))
                        door.InstantUnlock();
                }
                if (!this.WasSkipped || this.index != 0)
                    return;
                this.oshiro.Sprite.Play("idle_ground");
            }
            else
            {
                level.Session.SetFlag("oshiro_clutter_finished");
                this.Scene.Remove((Entity) this.oshiro);
            }
        }
    }
}
