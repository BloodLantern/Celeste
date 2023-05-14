// Decompiled with JetBrains decompiler
// Type: Celeste.CS10_Ending
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste
{
    public class CS10_Ending : CutsceneEntity
    {
        private const int FPS = 12;
        private const float DELAY = 0.0833333358f;
        private Atlas Atlas;
        private List<MTexture> Frames;
        private int frame;
        private float fade = 1f;
        private float zoom = 1f;
        private float computerFade;
        private Coroutine talkingLoop;
        private Vector2 center = Celeste.TargetCenter;
        private Coroutine cutscene;
        private Color fadeColor = Color.White;
        private Monocle.Image attachment;
        private Monocle.Image cursor;
        private Monocle.Image ok;
        private Monocle.Image picture;
        private const float PictureIdleScale = 0.9f;
        private float speedrunTimerEase;
        private string speedrunTimerChapterString;
        private string speedrunTimerFileString;
        private readonly string chapterSpeedrunText = Dialog.Get("OPTIONS_SPEEDRUN_CHAPTER") + ":";
        private readonly string version = Celeste.Instance.Version.ToString();
        private bool showTimer;
        private EventInstance endAmbience;
        private EventInstance cinIntro;
        private bool counting;
        private float timer;

        public CS10_Ending(Player player)
            : base(false, true)
        {
            Tag = (int)Tags.HUD;
            player.StateMachine.State = 11;
            player.DummyAutoAnimate = false;
            player.Sprite.Rate = 0.0f;
            RemoveOnSkipped = false;
            Add(new LevelEndingHook(() => Audio.Stop(cinIntro)));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            level.TimerStopped = true;
            level.TimerHidden = true;
            level.SaveQuitDisabled = true;
            level.PauseLock = true;
            level.AllowHudHide = false;
        }

        public override void OnBegin(Level level)
        {
            _ = Audio.SetAmbience(null);
            level.AutoSave();
            speedrunTimerChapterString = TimeSpan.FromTicks(level.Session.Time).ShortGameplayFormat();
            speedrunTimerFileString = Dialog.FileTime(SaveData.Instance.Time);
            SpeedrunTimerDisplay.CalculateBaseSizes();
            Add(cutscene = new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS10_Ending cs10Ending = this;
            level.Wipe?.Cancel();
            while (level.IsAutoSaving())
            {
                yield return null;
            }

            yield return 1f;
            cs10Ending.Atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Farewell"), Atlas.AtlasDataFormat.PackerNoAtlas);
            cs10Ending.Frames = cs10Ending.Atlas.GetAtlasSubtextures("");
            cs10Ending.Add(cs10Ending.attachment = new Monocle.Image(cs10Ending.Atlas["21-window"]));
            cs10Ending.Add(cs10Ending.picture = new Monocle.Image(cs10Ending.Atlas["21-picture"]));
            cs10Ending.Add(cs10Ending.ok = new Monocle.Image(cs10Ending.Atlas["21-button"]));
            cs10Ending.Add(cs10Ending.cursor = new Monocle.Image(cs10Ending.Atlas["21-cursor"]));
            cs10Ending.attachment.Visible = false;
            cs10Ending.picture.Visible = false;
            cs10Ending.ok.Visible = false;
            cs10Ending.cursor.Visible = false;
            level.PauseLock = false;
            yield return 2f;
            cs10Ending.cinIntro = Audio.Play("event:/new_content/music/lvl10/cinematic/end_intro");
            _ = Audio.SetAmbience(null);
            cs10Ending.counting = true;
            cs10Ending.Add(new Coroutine(cs10Ending.Fade(1f, 0.0f, 4f)));
            cs10Ending.Add(new Coroutine(cs10Ending.Zoom(1.38f, 1.2f, 4f)));
            yield return cs10Ending.Loop("0", 2f);
            Input.Rumble(RumbleStrength.Climb, RumbleLength.TwoSeconds);
            yield return cs10Ending.Loop("0,1,1,0,0,1,1,0*8", 2f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            _ = Audio.SetMusic("event:/new_content/music/lvl10/cinematic/end", allowFadeOut: false);
            cs10Ending.endAmbience = Audio.Play("event:/new_content/env/10_endscene");
            cs10Ending.Add(new Coroutine(cs10Ending.Zoom(1.2f, 1.05f, 0.06f, Ease.CubeOut)));
            yield return cs10Ending.Play("2-7");
            yield return cs10Ending.Loop("7", 1.5f);
            yield return cs10Ending.Play("8-10,10*20");
            List<int> frameData = cs10Ending.GetFrameData("10-13,13*16,14*28,14-17,14*24");
            float duration = (frameData.Count + 3) * 0.0833333358f;
            cs10Ending.fadeColor = Color.Black;
            cs10Ending.Add(new Coroutine(cs10Ending.Zoom(1.05f, 1f, duration, Ease.Linear)));
            cs10Ending.Add(new Coroutine(cs10Ending.Fade(0.0f, 1f, duration * 0.1f, duration * 0.85f)));
            cs10Ending.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => Audio.Play("event:/new_content/game/10_farewell/endscene_dial_theo"), duration, true));
            yield return cs10Ending.Play(frameData);
            cs10Ending.frame = 18;
            cs10Ending.fade = 1f;
            yield return 0.5f;
            yield return cs10Ending.Fade(1f, 0.0f, 1.2f);
            cs10Ending.Add(cs10Ending.talkingLoop = new Coroutine(cs10Ending.Loop("18*24,19,19,18*6,20,20")));
            yield return 1f;
            yield return Textbox.Say("CH9_END_CINEMATIC", new Func<IEnumerator>(cs10Ending.ShowPicture));
            Audio.SetMusicParam("end", 1f);
            _ = Audio.Play("event:/new_content/game/10_farewell/endscene_photo_zoom");
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 4f)
            {
                Audio.SetParameter(cs10Ending.endAmbience, "fade", 1f - p);
                cs10Ending.computerFade = p;
                cs10Ending.picture.Scale = Vector2.One * (float)(0.89999997615814209 + (0.10000002384185791 * (double)p));
                yield return null;
            }
            cs10Ending.EndCutscene(level, false);
        }

        private IEnumerator ShowPicture()
        {
            CS10_Ending cs10Ending = this;
            cs10Ending.center = new Vector2(1230f, 312f);
            cs10Ending.Add(new Coroutine(cs10Ending.Fade(0.0f, 1f, 0.25f)));
            cs10Ending.Add(new Coroutine(cs10Ending.Zoom(1f, 1.1f, 0.25f)));
            yield return 0.25f;
            cs10Ending.talkingLoop?.RemoveSelf();
            cs10Ending.talkingLoop = null;
            yield return null;
            cs10Ending.frame = 21;
            cs10Ending.cursor.Visible = true;
            cs10Ending.center = Celeste.TargetCenter;
            cs10Ending.Add(new Coroutine(cs10Ending.Fade(1f, 0.0f, 0.25f)));
            cs10Ending.Add(new Coroutine(cs10Ending.Zoom(1.1f, 1f, 0.25f)));
            yield return 0.25f;
            _ = Audio.Play("event:/new_content/game/10_farewell/endscene_attachment_notify");
            cs10Ending.attachment.Origin = Celeste.TargetCenter;
            cs10Ending.attachment.Position = Celeste.TargetCenter;
            cs10Ending.attachment.Visible = true;
            cs10Ending.attachment.Scale = Vector2.Zero;
            float p;
            for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 0.3f)
            {
                cs10Ending.attachment.Scale.Y = (float)(0.25 + (0.75 * (double)Ease.BigBackOut(p)));
                cs10Ending.attachment.Scale.X = (float)(1.5 - (0.5 * (double)Ease.BigBackOut(p)));
                yield return null;
            }
            yield return 0.25f;
            cs10Ending.ok.Position = new Vector2(1208f, 620f);
            cs10Ending.ok.Origin = cs10Ending.ok.Position;
            cs10Ending.ok.Visible = true;
            for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 0.25f)
            {
                cs10Ending.ok.Scale.Y = (float)(0.25 + (0.75 * (double)Ease.BigBackOut(p)));
                cs10Ending.ok.Scale.X = (float)(1.5 - (0.5 * (double)Ease.BigBackOut(p)));
                yield return null;
            }
            yield return 0.8f;
            Vector2 from = cs10Ending.cursor.Position;
            Vector2 to = from + new Vector2(-160f, -190f);
            for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 0.5f)
            {
                cs10Ending.cursor.Position = from + ((to - from) * Ease.CubeInOut(p));
                yield return null;
            }
            yield return 0.2f;
            from = new Vector2();
            to = new Vector2();
            _ = Audio.Play("event:/new_content/game/10_farewell/endscene_attachment_click");
            for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 0.25f)
            {
                cs10Ending.ok.Scale.Y = 1f - Ease.BigBackIn(p);
                cs10Ending.ok.Scale.X = 1f - Ease.BigBackIn(p);
                yield return null;
            }
            cs10Ending.ok.Visible = false;
            yield return 0.1f;
            cs10Ending.picture.Origin = Celeste.TargetCenter;
            cs10Ending.picture.Position = Celeste.TargetCenter;
            cs10Ending.picture.Visible = true;
            for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 0.4f)
            {
                cs10Ending.picture.Scale.Y = (float)((0.89999997615814209 + (0.10000000149011612 * (double)Ease.BigBackOut(p))) * 0.89999997615814209);
                cs10Ending.picture.Scale.X = (float)((1.1000000238418579 - (0.10000000149011612 * (double)Ease.BigBackOut(p))) * 0.89999997615814209);
                cs10Ending.picture.Position = Celeste.TargetCenter + (Vector2.UnitY * 120f * (1f - Ease.CubeOut(p)));
                cs10Ending.picture.Color = Color.White * p;
                yield return null;
            }
            cs10Ending.picture.Position = Celeste.TargetCenter;
            cs10Ending.attachment.Visible = false;
            to = cs10Ending.cursor.Position;
            from = new Vector2(120f, 30f);
            for (p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 0.5f)
            {
                cs10Ending.cursor.Position = to + ((from - to) * Ease.CubeInOut(p));
                yield return null;
            }
            cs10Ending.cursor.Visible = false;
            _ = new Vector2();
            _ = new Vector2();
            yield return 2f;
        }

        public override void OnEnd(Level level)
        {
            ScreenWipe.WipeColor = Color.Black;
            if (Audio.CurrentMusicEventInstance == null)
            {
                _ = Audio.SetMusic("event:/new_content/music/lvl10/cinematic/end");
            }

            Audio.SetMusicParam("end", 1f);
            frame = 21;
            zoom = 1f;
            fade = 0.0f;
            fadeColor = Color.Black;
            center = Celeste.TargetCenter;
            picture.Scale = Vector2.One;
            picture.Visible = true;
            picture.Position = Celeste.TargetCenter;
            picture.Origin = Celeste.TargetCenter;
            computerFade = 1f;
            attachment.Visible = false;
            cursor.Visible = false;
            ok.Visible = false;
            Audio.Stop(cinIntro);
            cinIntro = null;
            Audio.Stop(endAmbience);
            endAmbience = null;
            List<Coroutine> coroutineList = new();
            foreach (Coroutine coroutine in Components.GetAll<Coroutine>())
            {
                coroutineList.Add(coroutine);
            }

            foreach (Component component in coroutineList)
            {
                component.RemoveSelf();
            }

            Scene.Entities.FindFirst<Textbox>()?.RemoveSelf();
            Level.InCutscene = true;
            Level.PauseLock = true;
            Level.TimerHidden = true;
            Add(new Coroutine(EndingRoutine()));
        }

        private IEnumerator EndingRoutine()
        {
            CS10_Ending cs10Ending = this;
            cs10Ending.Level.InCutscene = true;
            cs10Ending.Level.PauseLock = true;
            cs10Ending.Level.TimerHidden = true;
            yield return 0.5f;
            if (Settings.Instance.SpeedrunClock != SpeedrunType.Off)
            {
                cs10Ending.showTimer = true;
            }

            while (!Input.MenuConfirm.Pressed)
            {
                yield return null;
            }

            _ = Audio.Play("event:/new_content/game/10_farewell/endscene_final_input");
            cs10Ending.showTimer = false;
            cs10Ending.Add(new Coroutine(cs10Ending.Zoom(1f, 0.75f, 5f, Ease.CubeIn)));
            cs10Ending.Add(new Coroutine(cs10Ending.Fade(0.0f, 1f, 5f)));
            yield return 4f;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 3f)
            {
                Audio.SetMusicParam("fade", 1f - p);
                yield return null;
            }
            _ = Audio.SetMusic(null);
            yield return 1f;
            cs10Ending.Atlas?.Dispose();
            cs10Ending.Atlas = null;
            _ = cs10Ending.Level.CompleteArea(false, true, true);
        }

        public override void Update()
        {
            if (counting)
            {
                timer += Engine.DeltaTime;
            }

            speedrunTimerEase = Calc.Approach(speedrunTimerEase, showTimer ? 1f : 0.0f, Engine.DeltaTime * 4f);
            base.Update();
        }

        public override void Render()
        {
            Draw.Rect(-100f, -100f, 2120f, 1280f, Color.Black);
            if (Atlas != null && Frames != null && frame < Frames.Count)
            {
                MTexture frame = Frames[this.frame];
                MTexture linkedTexture = Atlas.GetLinkedTexture(frame.AtlasPath);
                linkedTexture?.DrawJustified(center, new Vector2(center.X / linkedTexture.Width, center.Y / linkedTexture.Height), Color.White, zoom);
                frame.DrawJustified(center, new Vector2(center.X / frame.Width, center.Y / frame.Height), Color.White, zoom);
                if (computerFade > 0.0)
                {
                    Draw.Rect(0.0f, 0.0f, 1920f, 1080f, Color.Black * computerFade);
                }

                base.Render();
                AreaComplete.Info(speedrunTimerEase, speedrunTimerChapterString, speedrunTimerFileString, chapterSpeedrunText, version);
            }
            Draw.Rect(0.0f, 0.0f, 1920f, 1080f, fadeColor * fade);
            if (!(Scene as Level).Paused)
            {
                return;
            }

            Draw.Rect(0.0f, 0.0f, 1920f, 1080f, Color.Black * 0.5f);
        }

        private List<int> GetFrameData(string data)
        {
            List<int> frameData = new();
            string[] strArray1 = data.Split(',');
            for (int index1 = 0; index1 < strArray1.Length; ++index1)
            {
                if (strArray1[index1].Contains<char>('*'))
                {
                    string[] strArray2 = strArray1[index1].Split('*');
                    int num1 = int.Parse(strArray2[0]);
                    int num2 = int.Parse(strArray2[1]);
                    for (int index2 = 0; index2 < num2; ++index2)
                    {
                        frameData.Add(num1);
                    }
                }
                else if (strArray1[index1].Contains<char>('-'))
                {
                    string[] strArray3 = strArray1[index1].Split('-');
                    int num3 = int.Parse(strArray3[0]);
                    int num4 = int.Parse(strArray3[1]);
                    for (int index3 = num3; index3 <= num4; ++index3)
                    {
                        frameData.Add(index3);
                    }
                }
                else
                {
                    frameData.Add(int.Parse(strArray1[index1]));
                }
            }
            return frameData;
        }

        private IEnumerator Zoom(float from, float to, float duration, Ease.Easer ease = null)
        {
            ease ??= Ease.Linear;
            zoom = from;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / duration)
            {
                zoom = from + ((to - from) * ease(p));
                if (picture != null)
                {
                    picture.Scale = Vector2.One * zoom;
                }

                yield return null;
            }
            zoom = to;
        }

        private IEnumerator Play(string data)
        {
            return Play(GetFrameData(data));
        }

        private IEnumerator Play(List<int> frames)
        {
            for (int i = 0; i < frames.Count; ++i)
            {
                frame = frames[i];
                yield return 0.0833333358f;
            }
        }

        private IEnumerator Loop(string data, float duration = -1f)
        {
            List<int> frames = GetFrameData(data);
            float time = 0.0f;
            while ((double)time < (double)duration || (double)duration < 0.0)
            {
                frame = frames[(int)((double)time / 0.0833333358168602) % frames.Count];
                time += Engine.DeltaTime;
                yield return null;
            }
        }

        private IEnumerator Fade(float from, float to, float duration, float delay = 0.0f)
        {
            fade = from;
            yield return delay;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / duration)
            {
                fade = from + ((to - from) * p);
                yield return null;
            }
            fade = to;
        }
    }
}
