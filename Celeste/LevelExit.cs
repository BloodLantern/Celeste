using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace Celeste
{
    public class LevelExit : Scene
    {
        private readonly LevelExit.Mode mode;
        private readonly Session session;
        private float timer;
        private XmlElement completeXml;
        private Atlas completeAtlas;
        private bool completeLoaded;
        private HiresSnow snow;
        private OverworldLoader overworldLoader;
        public string GoldenStrawberryEntryLevel;
        private const float MinTimeForCompleteScreen = 3.3f;

        public LevelExit(LevelExit.Mode mode, Session session, HiresSnow snow = null)
        {
            Add(new HudRenderer());
            this.session = session;
            this.mode = mode;
            this.snow = snow;
        }

        public override void Begin()
        {
            base.Begin();
            if (mode != LevelExit.Mode.GoldenBerryRestart)
                SaveLoadIcon.Show(this);
            bool flag = snow == null;
            if (flag)
                snow = new HiresSnow();
            if (mode == LevelExit.Mode.Completed)
            {
                snow.Direction = new Vector2(0.0f, 16f);
                if (flag)
                    snow.Reset();
                RunThread.Start(new Action(LoadCompleteThread), "COMPLETE_LEVEL");
                if (session.Area.Mode != AreaMode.Normal)
                    Audio.SetMusic("event:/music/menu/complete_bside");
                else if (session.Area.ID == 7)
                    Audio.SetMusic("event:/music/menu/complete_summit");
                else
                    Audio.SetMusic("event:/music/menu/complete_area");
                Audio.SetAmbience(null);
            }
            if (mode == LevelExit.Mode.GiveUp)
                overworldLoader = new OverworldLoader(Overworld.StartMode.AreaQuit, snow);
            else if (mode == LevelExit.Mode.SaveAndQuit)
                overworldLoader = new OverworldLoader(Overworld.StartMode.MainMenu, snow);
            else if (mode == LevelExit.Mode.CompletedInterlude)
                overworldLoader = new OverworldLoader(Overworld.StartMode.AreaComplete, snow);
            Entity entity;
            Add(entity = new Entity());
            entity.Add(new Coroutine(Routine()));
            if (mode is not LevelExit.Mode.Restart and not LevelExit.Mode.GoldenBerryRestart)
            {
                Add(snow);
                if (flag)
                {
                    FadeWipe fadeWipe = new(this, true);
                }
            }
            //Stats.Store();
            RendererList.UpdateLists();
        }

        private void LoadCompleteThread()
        {
            completeXml = AreaData.Get(session).CompleteScreenXml;
            if (completeXml != null && completeXml.HasAttr("atlas"))
                completeAtlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", completeXml.Attr("atlas")), Atlas.AtlasDataFormat.PackerNoAtlas);
            completeLoaded = true;
        }

        private IEnumerator Routine()
        {
            if (mode != LevelExit.Mode.GoldenBerryRestart)
            {
                UserIO.SaveHandler(true, true);
                while (UserIO.Saving)
                    yield return null;
                if (mode == LevelExit.Mode.Completed)
                {
                    while (!completeLoaded)
                        yield return null;
                }
                while (SaveLoadIcon.OnScreen)
                    yield return null;
            }
            if (mode == LevelExit.Mode.Completed)
            {
                while (timer < 3.2999999523162842)
                    yield return null;
                Audio.SetMusicParam("end", 1f);
                Engine.Scene = new AreaComplete(session, completeXml, completeAtlas, snow);
            }
            else if (mode is Mode.GiveUp or Mode.SaveAndQuit or Mode.CompletedInterlude)
                Engine.Scene = overworldLoader;
            else if (mode is Mode.Restart or Mode.GoldenBerryRestart)
            {
                Session session;
                if (mode == Mode.GoldenBerryRestart)
                {
                    if ((this.session.Audio.Music.Event == "event:/music/lvl7/main" || this.session.Audio.Music.Event == "event:/music/lvl7/final_ascent") && this.session.Audio.Music.Progress > 0)
                        Audio.SetMusic(null);
                    session = this.session.Restart(GoldenStrawberryEntryLevel);
                }
                else
                    session = this.session.Restart();
                LevelLoader levelLoader = new(session);
                if (mode == Mode.GoldenBerryRestart)
                    levelLoader.PlayerIntroTypeOverride = new Player.IntroTypes?(Player.IntroTypes.Respawn);
                Engine.Scene = levelLoader;
            }
        }

        public override void Update()
        {
            timer += Engine.DeltaTime;
            base.Update();
        }

        public enum Mode
        {
            SaveAndQuit,
            GiveUp,
            Restart,
            GoldenBerryRestart,
            Completed,
            CompletedInterlude,
        }
    }
}
