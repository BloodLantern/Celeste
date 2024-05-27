using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Xml;

namespace Celeste
{
    public class AreaComplete : Scene
    {
        public Session Session;
        private bool finishedSlide;
        private bool canConfirm = true;
        private HiresSnow snow;
        private float speedrunTimerDelay = 1.1f;
        private float speedrunTimerEase;
        private string speedrunTimerChapterString;
        private string speedrunTimerFileString;
        private string chapterSpeedrunText = Dialog.Get("OPTIONS_SPEEDRUN_CHAPTER") + ":";
        private AreaCompleteTitle title;
        private CompleteRenderer complete;
        private string version;

        public AreaComplete(Session session, XmlElement xml, Atlas atlas, HiresSnow snow)
        {
            Session = session;
            version = Celeste.Instance.Version.ToString();
            if (session.Area.ID != 7)
            {
                string text = Dialog.Clean("areacomplete_" + session.Area.Mode + (session.FullClear ? "_fullclear" : (object) ""));
                Vector2 origin = new Vector2(960f, 200f);
                float scale = Math.Min(1600f / ActiveFont.Measure(text).X, 3f);
                title = new AreaCompleteTitle(origin, text, scale);
            }
            Add(complete = new CompleteRenderer(xml, atlas, 1f, () => finishedSlide = true));
            if (title != null)
                complete.RenderUI = v => title.DrawLineUI();
            complete.RenderPostUI = RenderUI;
            speedrunTimerChapterString = TimeSpan.FromTicks(Session.Time).ShortGameplayFormat();
            speedrunTimerFileString = Dialog.FileTime(SaveData.Instance.Time);
            SpeedrunTimerDisplay.CalculateBaseSizes();
            Add(this.snow = snow);
            RendererList.UpdateLists();
            AreaKey area = session.Area;
            if (area.Mode != AreaMode.Normal)
                return;
            if (area.ID == 1)
                Achievements.Register(Achievement.CH1);
            else if (area.ID == 2)
                Achievements.Register(Achievement.CH2);
            else if (area.ID == 3)
                Achievements.Register(Achievement.CH3);
            else if (area.ID == 4)
                Achievements.Register(Achievement.CH4);
            else if (area.ID == 5)
                Achievements.Register(Achievement.CH5);
            else if (area.ID == 6)
            {
                Achievements.Register(Achievement.CH6);
            }
            else
            {
                if (area.ID != 7)
                    return;
                Achievements.Register(Achievement.CH7);
            }
        }

        public override void End()
        {
            base.End();
            complete.Dispose();
        }

        public override void Update()
        {
            base.Update();
            if (Input.MenuConfirm.Pressed && finishedSlide && canConfirm)
            {
                canConfirm = false;
                if (Session.Area.ID == 7 && Session.Area.Mode == AreaMode.Normal)
                {
                    FadeWipe fadeWipe = new FadeWipe(this, false, () =>
                    {
                        Session.RespawnPoint = new Vector2?();
                        Session.FirstLevel = false;
                        Session.Level = "credits-summit";
                        Session.Audio.Music.Event = "event:/music/lvl8/main";
                        Session.Audio.Apply();
                        Engine.Scene = new LevelLoader(Session)
                        {
                            PlayerIntroTypeOverride = Player.IntroTypes.None,
                            Level = {
                                new CS07_Credits()
                            }
                        };
                    });
                }
                else
                {
                    HiresSnow outSnow = new HiresSnow
                    {
                        Alpha = 0.0f
                    };
                    outSnow.AttachAlphaTo = new FadeWipe(this, false, () => Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, outSnow));
                    Add(outSnow);
                }
            }
            snow.Alpha = Calc.Approach(snow.Alpha, 0.0f, Engine.DeltaTime * 0.5f);
            snow.Direction.Y = Calc.Approach(snow.Direction.Y, 1f, Engine.DeltaTime * 24f);
            speedrunTimerDelay -= Engine.DeltaTime;
            if (speedrunTimerDelay <= 0.0)
                speedrunTimerEase = Calc.Approach(speedrunTimerEase, 1f, Engine.DeltaTime * 2f);
            if (title != null)
                title.Update();
            if (Celeste.PlayMode != Celeste.PlayModes.Debug)
                return;
            if (MInput.Keyboard.Pressed(Keys.F2))
            {
                Celeste.ReloadAssets(false, true, false);
                Engine.Scene = new LevelExit(LevelExit.Mode.Completed, Session);
            }
            else
            {
                if (!MInput.Keyboard.Pressed(Keys.F3))
                    return;
                Celeste.ReloadAssets(false, true, true);
                Engine.Scene = new LevelExit(LevelExit.Mode.Completed, Session);
            }
        }

        private void RenderUI()
        {
            Entities.Render();
            AreaComplete.Info(speedrunTimerEase, speedrunTimerChapterString, speedrunTimerFileString, chapterSpeedrunText, version);
            if (!complete.HasUI || title == null)
                return;
            title.Render();
        }

        public static void Info(
            float ease,
            string speedrunTimerChapterString,
            string speedrunTimerFileString,
            string chapterSpeedrunText,
            string versionText)
        {
            if (ease <= 0.0 || Settings.Instance.SpeedrunClock == SpeedrunType.Off)
                return;
            Vector2 position = new Vector2((float) (80.0 - 300.0 * (1.0 - Ease.CubeOut(ease))), 1000f);
            if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
            {
                SpeedrunTimerDisplay.DrawTime(position, speedrunTimerChapterString);
            }
            else
            {
                position.Y -= 16f;
                SpeedrunTimerDisplay.DrawTime(position, speedrunTimerFileString);
                ActiveFont.DrawOutline(chapterSpeedrunText, position + new Vector2(0.0f, 40f), new Vector2(0.0f, 1f), Vector2.One * 0.6f, Color.White, 2f, Color.Black);
                SpeedrunTimerDisplay.DrawTime(position + new Vector2((float) (ActiveFont.Measure(chapterSpeedrunText).X * 0.60000002384185791 + 8.0), 40f), speedrunTimerChapterString, 0.6f);
            }
            AreaComplete.VersionNumberAndVariants(versionText, ease, 1f);
        }

        public static void VersionNumberAndVariants(string version, float ease, float alpha)
        {
            Vector2 position1 = new Vector2((float) (1820.0 + 300.0 * (1.0 - Ease.CubeOut(ease))), 1020f);
            if (SaveData.Instance.AssistMode || SaveData.Instance.VariantMode)
            {
                MTexture mtexture = GFX.Gui[SaveData.Instance.AssistMode ? "cs_assistmode" : "cs_variantmode"];
                position1.Y -= 32f;
                Vector2 position2 = position1 + new Vector2(0.0f, -8f);
                Vector2 justify = new Vector2(0.5f, 1f);
                Color white = Color.White;
                mtexture.DrawJustified(position2, justify, white, 0.6f);
                ActiveFont.DrawOutline(version, position1, new Vector2(0.5f, 0.0f), Vector2.One * 0.5f, Color.White, 2f, Color.Black);
            }
            else
                ActiveFont.DrawOutline(version, position1, new Vector2(0.5f, 0.5f), Vector2.One * 0.5f, Color.White, 2f, Color.Black);
        }
    }
}
