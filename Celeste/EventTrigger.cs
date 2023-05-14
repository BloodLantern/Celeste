// Decompiled with JetBrains decompiler
// Type: Celeste.EventTrigger
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
    public class EventTrigger : Trigger
    {
        public string Event;
        public bool OnSpawnHack;
        private bool triggered;
        private EventInstance snapshot;

        public EventTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Event = data.Attr("event");
            OnSpawnHack = data.Bool("onSpawn");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (OnSpawnHack)
            {
                Player player = CollideFirst<Player>();
                if (player != null)
                {
                    OnEnter(player);
                }
            }
            if (!(Event == "ch9_badeline_helps"))
            {
                return;
            }

            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || (double)entity.Left <= (double)Right)
            {
                return;
            }

            RemoveSelf();
        }

        public float Time { get; private set; }

        public override void OnEnter(Player player)
        {
            if (triggered)
            {
                return;
            }

            triggered = true;
            Level level = Scene as Level;
            switch (Event)
            {
                case "cancel_ch5_see_theo":
                    level.Session.SetFlag("it_ch5_see_theo");
                    level.Session.SetFlag("it_ch5_see_theo_b");
                    level.Session.SetFlag("ignore_darkness_" + level.Session.Level);
                    Add(new Coroutine(Brighten()));
                    break;
                case "ch5_found_theo":
                    if (level.Session.GetFlag("foundTheoInCrystal"))
                    {
                        break;
                    }

                    Scene.Add(new CS05_SaveTheo(player));
                    break;
                case "ch5_mirror_reflection":
                    if (level.Session.GetFlag("reflection"))
                    {
                        break;
                    }

                    Scene.Add(new CS05_Reflection1(player));
                    break;
                case "ch5_see_theo":
                    if ((Scene as Level).Session.GetFlag("seeTheoInCrystal"))
                    {
                        break;
                    }

                    Scene.Add(new CS05_SeeTheo(player, 0));
                    break;
                case "ch6_boss_intro":
                    if (level.Session.GetFlag("boss_intro"))
                    {
                        break;
                    }

                    level.Add(new CS06_BossIntro(Center.X, player, level.Entities.FindFirst<FinalBoss>()));
                    break;
                case "ch6_reflect":
                    if (level.Session.GetFlag("reflection"))
                    {
                        break;
                    }

                    Scene.Add(new CS06_Reflection(player, Center.X - 5f));
                    break;
                case "ch7_summit":
                    Scene.Add(new CS07_Ending(player, new Vector2(Center.X, Bottom)));
                    break;
                case "ch8_door":
                    Scene.Add(new CS08_EnterDoor(player, Left));
                    break;
                case "ch9_badeline_helps":
                    if (level.Session.GetFlag("badeline_helps"))
                    {
                        break;
                    }

                    Scene.Add(new CS10_BadelineHelps(player));
                    break;
                case "ch9_ding_ding_ding":
                    _ = Audio.Play("event:/new_content/game/10_farewell/pico8_flag", Center);
                    Decal decal1 = null;
                    foreach (Decal decal2 in Scene.Entities.FindAll<Decal>())
                    {
                        if (decal2.Name.ToLower() == "decals/10-farewell/finalflag")
                        {
                            decal1 = decal2;
                            break;
                        }
                    }
                    decal1?.FinalFlagTrigger();
                    break;
                case "ch9_end_golden":
                    ScreenWipe.WipeColor = Color.White;
                    new FadeWipe(level, false, () => level.OnEndOfFrame += () =>
                    {
                        level.TeleportTo(player, "end-granny", Player.IntroTypes.Transition);
                        player.Speed = Vector2.Zero;
                    }).Duration = 1f;
                    break;
                case "ch9_ending":
                    Scene.Add(new CS10_Ending(player));
                    break;
                case "ch9_farewell":
                    Scene.Add(new CS10_Farewell(player));
                    break;
                case "ch9_final_room":
                    Session session = (Scene as Level).Session;
                    switch (session.GetCounter("final_room_deaths"))
                    {
                        case 0:
                            Scene.Add(new CS10_FinalRoom(player, true));
                            break;
                        case 50:
                            Scene.Add(new CS10_FinalRoom(player, false));
                            break;
                    }
                    session.IncrementCounter("final_room_deaths");
                    break;
                case "ch9_golden_snapshot":
                    snapshot = Audio.CreateSnapshot("snapshot:/game_10_golden_room_flavour");
                    (Scene as Level).SnapColorGrade("golden");
                    break;
                case "ch9_goto_the_future":
                case "ch9_goto_the_past":
                    level.OnEndOfFrame += () =>
                    {
                        Vector2 vector2_1 = new(level.LevelOffset.X + level.Bounds.Width - player.X, player.Y - level.LevelOffset.Y);
                        Vector2 levelOffset = level.LevelOffset;
                        Vector2 vector2_2 = player.Position - level.LevelOffset;
                        Vector2 vector2_3 = level.Camera.Position - level.LevelOffset;
                        Facings facing = player.Facing;
                        level.Remove(player);
                        level.UnloadLevel();
                        level.Session.Dreaming = true;
                        level.Session.Level = Event == "ch9_goto_the_future" ? "intro-01-future" : "intro-00-past";
                        level.Session.RespawnPoint = new Vector2?(level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top)));
                        level.Session.FirstLevel = false;
                        level.LoadLevel(Player.IntroTypes.Transition);
                        level.Camera.Position = level.LevelOffset + vector2_3;
                        level.Session.Inventory.Dashes = 1;
                        player.Dashes = Math.Min(player.Dashes, 1);
                        level.Add(player);
                        player.Position = level.LevelOffset + vector2_2;
                        player.Facing = facing;
                        player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                        level.Wipe?.Cancel();
                        level.Flash(Color.White);
                        level.Shake();
                        level.Add(new LightningStrike(new Vector2(player.X + 60f, level.Bounds.Bottom - 180), 10, 200f));
                        level.Add(new LightningStrike(new Vector2(player.X + 220f, level.Bounds.Bottom - 180), 40, 200f, 0.25f));
                        _ = Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
                    };
                    break;
                case "ch9_hub_intro":
                    if (level.Session.GetFlag("hub_intro"))
                    {
                        break;
                    }

                    Scene.Add(new CS10_HubIntro(Scene, player));
                    break;
                case "ch9_hub_transition_out":
                    Add(new Coroutine(Ch9HubTransitionBackgroundToBright(player)));
                    break;
                case "ch9_moon_intro":
                    if (!level.Session.GetFlag("moon_intro") && player.StateMachine.State == 13)
                    {
                        Scene.Add(new CS10_MoonIntro(player));
                        break;
                    }
                    level.Entities.FindFirst<BirdNPC>()?.RemoveSelf();
                    level.Session.Inventory.Dashes = 1;
                    player.Dashes = 1;
                    break;
                case "end_city":
                    Scene.Add(new CS01_Ending(player));
                    break;
                case "end_oldsite_awake":
                    Scene.Add(new CS02_Ending(player));
                    break;
                case "end_oldsite_dream":
                    Scene.Add(new CS02_DreamingPhonecall(player));
                    break;
                default:
                    throw new Exception("Event '" + Event + "' does not exist!");
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.ReleaseSnapshot(snapshot);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.ReleaseSnapshot(snapshot);
        }

        private IEnumerator Brighten()
        {
            Level level = Scene as Level;
            float darkness = AreaData.Get(level).DarknessAlpha;
            while (level.Lighting.Alpha != (double)darkness)
            {
                level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, darkness, Engine.DeltaTime * 4f);
                yield return null;
            }
        }

        private IEnumerator Ch9HubTransitionBackgroundToBright(Player player)
        {
            EventTrigger eventTrigger = this;
            Level level = eventTrigger.Scene as Level;
            float start = eventTrigger.Bottom;
            float end = eventTrigger.Top;
            while (true)
            {
                float num = Calc.ClampedMap(player.Y, start, end);
                foreach (Backdrop backdrop in level.Background.GetEach<Backdrop>("bright"))
                {
                    backdrop.ForceVisible = true;
                    backdrop.FadeAlphaMultiplier = num;
                }
                yield return null;
            }
        }
    }
}
