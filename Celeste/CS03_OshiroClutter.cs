// Decompiled with JetBrains decompiler
// Type: Celeste.CS03_OshiroClutter
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
    public class CS03_OshiroClutter : CutsceneEntity
    {
        private readonly int index;
        private readonly Player player;
        private readonly NPC03_Oshiro_Cluttter oshiro;
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
            doors = Scene.Entities.FindAll<ClutterDoor>();
            doors.Sort((a, b) => (int)((double)a.Y - (double)b.Y));
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS03_OshiroClutter cs03OshiroClutter = this;
            cs03OshiroClutter.player.StateMachine.State = 11;
            cs03OshiroClutter.player.StateMachine.Locked = true;
            if ((cs03OshiroClutter.index is 1 or 2 ? -1 : 1) == -1)
            {
                yield return cs03OshiroClutter.player.DummyWalkToExact((int)cs03OshiroClutter.oshiro.X - 24);
                cs03OshiroClutter.player.Facing = Facings.Right;
                cs03OshiroClutter.oshiro.Sprite.Scale.X = -1f;
            }
            else
            {
                cs03OshiroClutter.Add(new Coroutine(cs03OshiroClutter.oshiro.PaceRight()));
                yield return cs03OshiroClutter.player.DummyWalkToExact((int)cs03OshiroClutter.oshiro.HomePosition.X + 24);
                cs03OshiroClutter.player.Facing = Facings.Left;
                cs03OshiroClutter.oshiro.Sprite.Scale.X = 1f;
            }
            if (cs03OshiroClutter.index < 3)
            {
                yield return cs03OshiroClutter.Level.ZoomTo(cs03OshiroClutter.oshiro.ZoomPoint, 2f, 0.5f);
                yield return Textbox.Say("CH3_OSHIRO_CLUTTER" + cs03OshiroClutter.index, new Func<IEnumerator>(cs03OshiroClutter.Collapse), new Func<IEnumerator>(cs03OshiroClutter.oshiro.PaceLeft), new Func<IEnumerator>(cs03OshiroClutter.oshiro.PaceRight));
                yield return cs03OshiroClutter.Level.ZoomBack(0.5f);
                level.Session.SetFlag("oshiro_clutter_door_open");
                if (cs03OshiroClutter.index == 0)
                {
                    cs03OshiroClutter.SetMusic();
                }

                foreach (ClutterDoor door in cs03OshiroClutter.doors)
                {
                    if (!door.IsLocked(level.Session))
                    {
                        yield return door.UnlockRoutine();
                    }
                }
            }
            else
            {
                yield return CutsceneEntity.CameraTo(new Vector2(cs03OshiroClutter.Level.Bounds.X, cs03OshiroClutter.Level.Bounds.Y), 0.5f);
                yield return cs03OshiroClutter.Level.ZoomTo(new Vector2(90f, 60f), 2f, 0.5f);
                yield return Textbox.Say("CH3_OSHIRO_CLUTTER_ENDING");
                yield return cs03OshiroClutter.oshiro.MoveTo(new Vector2(cs03OshiroClutter.oshiro.X, level.Bounds.Top - 32));
                cs03OshiroClutter.oshiro.Add(new SoundSource("event:/char/oshiro/move_05_09b_exit"));
                yield return cs03OshiroClutter.Level.ZoomBack(0.5f);
            }
            cs03OshiroClutter.EndCutscene(level);
        }

        private IEnumerator Collapse()
        {
            _ = Audio.Play("event:/char/oshiro/chat_collapse", oshiro.Position);
            oshiro.Sprite.Play("fall");
            yield return 0.5f;
        }

        private void SetMusic()
        {
            Level scene = Scene as Level;
            scene.Session.Audio.Music.Event = "event:/music/lvl3/clean";
            scene.Session.Audio.Music.Progress = 1;
            scene.Session.Audio.Apply();
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            if (oshiro.Sprite.CurrentAnimationID == "side")
            {
                (oshiro.Sprite as OshiroSprite).Pop("idle", true);
            }

            if (index < 3)
            {
                level.Session.SetFlag("oshiro_clutter_door_open");
                level.Session.SetFlag("oshiro_clutter_" + index);
                if (index == 0 && WasSkipped)
                {
                    SetMusic();
                }

                foreach (ClutterDoor door in doors)
                {
                    if (!door.IsLocked(level.Session))
                    {
                        door.InstantUnlock();
                    }
                }
                if (!WasSkipped || index != 0)
                {
                    return;
                }

                oshiro.Sprite.Play("idle_ground");
            }
            else
            {
                level.Session.SetFlag("oshiro_clutter_finished");
                Scene.Remove(oshiro);
            }
        }
    }
}
