﻿using Celeste.Editor;
using Celeste.Pico8;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Celeste
{
    /// <summary>
    /// Class containing commands to be used in the in-game console. These functions are called using reflection so don't worry about VS 'warnings'.
    /// </summary>
    public static class Commands
    {
        [Command("global_stats", "logs global steam stats")]
        private static void CmdGlobalStats()
        {
            foreach (Stat stat in Enum.GetValues(typeof (Stat)))
                Engine.Commands.Log((stat.ToString() + ": " + Stats.Global(stat)));
        }

        [Command("export_dialog", "export dialog files to binary format")]
        private static void CmdExportDialog()
        {
            foreach (string enumerateFile in Directory.EnumerateFiles(Path.Combine("Content", "Dialog"), "*.txt"))
            {
                if (enumerateFile.EndsWith(".txt"))
                {
                    Language language = Language.FromTxt(enumerateFile);
                    language.Export(enumerateFile + ".export");
                    language.Dispose();
                }
            }
        }

        [Command("give_golden", "gives you a golden strawb")]
        private static void CmdGiveGolden()
        {
            if (Engine.Scene is not Level scene)
                return;
            Player entity = scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            EntityData data = new();
            data.Position = entity.Position + new Vector2(0.0f, -16f);
            data.ID = Calc.Random.Next();
            data.Name = "goldenBerry";
            EntityID gid = new(scene.Session.Level, data.ID);
            Strawberry strawberry = new(data, Vector2.Zero, gid);
            scene.Add(strawberry);
        }

        [Command("unlock_doors", "unlock all lockblocks")]
        private static void CmdUnlockDoors()
        {
            foreach (Entity entity in (Engine.Scene as Level).Entities.FindAll<LockBlock>())
                entity.RemoveSelf();
        }

        [Command("ltng", "disable lightning")]
        private static void CmdLightning(bool disabled = true) => (Engine.Scene as Level).Session.SetFlag("disable_lightning", disabled);

        [Command("bounce", "bounces the player!")]
        private static void CmdBounce()
        {
            Player entity = Engine.Scene.Tracker.GetEntity<Player>();
            entity?.Bounce(entity.Bottom);
        }

        [Command("sound_instances", "gets active sound count")]
        private static void CmdSounds()
        {
            int num = 0;
            foreach (KeyValuePair<string, EventDescription> eventDescription in Audio.cachedEventDescriptions)
            {
                int instanceCount = (int)eventDescription.Value.getInstanceCount(out int count);
                if (count > 0)
                {
                    int path2 = (int)eventDescription.Value.getPath(out string path1);
                    Engine.Commands.Log((path1 + ": " + count));
                    Console.WriteLine(path1 + ": " + count);
                }
                num += count;
            }
            Engine.Commands.Log(("total: " + num));
            Console.WriteLine("total: " + num);
        }

        [Command("lighting", "checks lightiing values")]
        private static void CmdLighting()
        {
            if (Engine.Scene is not Level scene)
                return;
            Engine.Commands.Log(("base(" + scene.BaseLightingAlpha + "), session add(" + scene.Session.LightingAlphaAdd + "), current (" + scene.Lighting.Alpha + ")"));
        }

        [Command("detailed_levels", "counts detailed levels")]
        private static void CmdDetailedLevels(int area = -1, int mode = 0)
        {
            if (area == -1)
            {
                int num1 = 0;
                int num2 = 0;
                foreach (AreaData area1 in AreaData.Areas)
                {
                    for (int index = 0; index < area1.Mode.Length; ++index)
                    {
                        ModeProperties modeProperties = area1.Mode[index];
                        if (modeProperties != null)
                        {
                            foreach (LevelData level in modeProperties.MapData.Levels)
                            {
                                if (!level.Dummy)
                                {
                                    ++num1;
                                    if (level.BgDecals.Count + level.FgDecals.Count >= 2)
                                        ++num2;
                                }
                            }
                        }
                    }
                }
                Engine.Commands.Log((num2.ToString() + " / " + num1));
            }
            else
            {
                int num3 = 0;
                int num4 = 0;
                List<string> values = new();
                foreach (LevelData level in AreaData.GetMode(area, (AreaMode) mode).MapData.Levels)
                {
                    if (!level.Dummy)
                    {
                        ++num3;
                        if (level.BgDecals.Count + level.FgDecals.Count >= 2)
                            ++num4;
                        else
                            values.Add(level.Name);
                    }
                }
                Engine.Commands.Log(string.Join(", ", values), Color.Red);
                Engine.Commands.Log((num4.ToString() + " / " + num3));
            }
        }

        [Command("hearts", "gives a certain number of hearts (default all)")]
        private static void CmdHearts(int amount = 24)
        {
            int num = 0;
            for (int index1 = 0; index1 < 3; ++index1)
            {
                for (int index2 = 0; index2 < SaveData.Instance.Areas.Count; ++index2)
                {
                    AreaModeStats mode = SaveData.Instance.Areas[index2].Modes[index1];
                    if (mode != null)
                    {
                        if (num < amount)
                        {
                            ++num;
                            mode.HeartGem = true;
                        }
                        else
                            mode.HeartGem = false;
                    }
                }
            }
            Calc.Log(SaveData.Instance.TotalHeartGems);
        }

        [Command("logsession", "log session to output")]
        private static void CmdLogSession()
        {
            Session session = (Engine.Scene as Level).Session;
            XmlSerializer xmlSerializer = new(typeof (Session));
            StringWriter stringWriter1 = new();
            StringWriter stringWriter2 = stringWriter1;
            Session o = session;
            xmlSerializer.Serialize(stringWriter2, o);
            Console.WriteLine(stringWriter1.ToString());
        }

        [Command("postcard", "views a postcard")]
        private static void CmdPostcard(string id, int area = 1) => Engine.Scene = new PreviewPostcard(new Postcard(Dialog.Get("POSTCARD_" + id), area));

        [Command("postcard_cside", "views a postcard")]
        private static void CmdPostcardCside() => Engine.Scene = new PreviewPostcard(new Postcard(Dialog.Get("POSTCARD_CSIDES"), "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out"));

        [Command("postcard_variants", "views a postcard")]
        private static void CmdPostcardVariants() => Engine.Scene = new PreviewPostcard(new Postcard(Dialog.Get("POSTCARD_VARIANTS"), "event:/new_content/ui/postcard_variants_in", "event:/new_content/ui/postcard_variants_out"));

        [Command("check_all_languages", "compares all langauges to english")]
        private static void CmdCheckLangauges(bool compareContent = false)
        {
            Engine.Commands.Log("---------------------");
            bool flag = true;
            foreach (KeyValuePair<string, Language> language in Dialog.Languages)
                flag &= CmdCheckLangauge(language.Key, compareContent);
            Engine.Commands.Log("---------------------");
            Engine.Commands.Log(("RESULT: " + flag.ToString()), flag ? Color.LawnGreen : Color.Red);
        }

        [Command("check_language", "compares all langauges to english")]
        private static bool CmdCheckLangauge(string id, bool compareContent = false)
        {
            bool flag1 = true;
            Language language1 = Dialog.Languages[id];
            Language language2 = Dialog.Languages["english"];
            int num = !(language1.FontFace != language2.FontFace) ? 0 : (Settings.Instance == null ? 1 : (language1.FontFace != Dialog.Languages[Settings.Instance.Language].FontFace ? 1 : 0));
            if (num != 0)
                Fonts.Load(language1.FontFace);
            bool flag2 = Dialog.CheckLanguageFontCharacters(id);
            bool flag3 = Dialog.CompareLanguages("english", id, compareContent);
            if (num != 0)
                Fonts.Unload(language1.FontFace);
            Engine.Commands.Log((id + " [FONT: " + flag2.ToString() + ", MATCH: " + flag3.ToString() + "]"), flag2 & flag3 ? Color.White : Color.Red);
            return flag1 & flag2 & flag3;
        }

        [Command("characters", "gets all the characters of each text file (writes to console")]
        private static void CmdTextCharacters() => Dialog.CheckCharacters();

        [Command("berries_order", "checks strawbs order")]
        private static void CmdBerriesOrder()
        {
            foreach (AreaData area in AreaData.Areas)
            {
                for (int index1 = 0; index1 < area.Mode.Length; ++index1)
                {
                    if (area.Mode[index1] != null)
                    {
                        HashSet<string> stringSet = new();
                        EntityData[,] entityDataArray = new EntityData[10, 25];
                        foreach (EntityData strawberry in area.Mode[index1].MapData.Strawberries)
                        {
                            int index2 = strawberry.Int("checkpointID");
                            int index3 = strawberry.Int("order");
                            string str = index2.ToString() + ":" + index3;
                            if (stringSet.Contains(str))
                                Engine.Commands.Log(("Conflicting Berry: Area[" + area.ID + "] Mode[" + index1 + "] Checkpoint[" + index2 + "] Order[" + index3 + "]"), Color.Red);
                            else
                                stringSet.Add(str);
                            entityDataArray[index2, index3] = strawberry;
                        }
                        for (int index4 = 0; index4 < entityDataArray.GetLength(0); ++index4)
                        {
                            for (int index5 = 1; index5 < entityDataArray.GetLength(1); ++index5)
                            {
                                if (entityDataArray[index4, index5] != null && entityDataArray[index4, index5 - 1] == null)
                                    Engine.Commands.Log(("Missing Berry Order #" + (index5 - 1) + ": Area[" + area.ID + "] Mode[" + index1 + "] Checkpoint[" + index4 + "]"), Color.Red);
                            }
                        }
                    }
                }
            }
        }

        [Command("ow_reflection_fall", "tests reflection overworld fall cutscene")]
        private static void CmdOWReflectionFall() => Engine.Scene = new OverworldReflectionsFall(null, () => Engine.Scene = new OverworldLoader(Overworld.StartMode.Titlescreen));

        [Command("core", "set the core mode of the level")]
        private static void CmdCore(int mode = 0, bool session = false)
        {
            (Engine.Scene as Level).CoreMode = (Session.CoreModes) mode;
            if (!session)
                return;
            (Engine.Scene as Level).Session.CoreMode = (Session.CoreModes) mode;
        }

        [Command("audio", "checks audio state of session")]
        private static void CmdAudio()
        {
            if (Engine.Scene is not Level)
                return;
            AudioState audio = (Engine.Scene as Level).Session.Audio;
            Engine.Commands.Log(("MUSIC: " + audio.Music.Event), Color.Green);
            foreach (MEP parameter in audio.Music.Parameters)
                Engine.Commands.Log(("    " + parameter.Key + " = " + parameter.Value));
            Engine.Commands.Log(("AMBIENCE: " + audio.Ambience.Event), Color.Green);
            foreach (MEP parameter in audio.Ambience.Parameters)
                Engine.Commands.Log(("    " + parameter.Key + " = " + parameter.Value));
        }

        [Command("heartgem", "give heart gem")]
        private static void CmdHeartGem(int area, int mode, bool gem = true) => SaveData.Instance.Areas[area].Modes[mode].HeartGem = gem;

        [Command("summitgem", "gives summit gem")]
        private static void CmdSummitGem(string gem)
        {
            if (gem == "all")
            {
                for (int index = 0; index < 6; ++index)
                    (Engine.Scene as Level).Session.SummitGems[index] = true;
            }
            else
                (Engine.Scene as Level).Session.SummitGems[int.Parse(gem)] = true;
        }

        [Command("screenpadding", "sets level screenpadding")]
        private static void CmdScreenPadding(int value)
        {
            if (Engine.Scene is not Level scene)
                return;
            scene.ScreenPadding = value;
        }

        [Command("textures", "counts textures in memory")]
        private static void CmdTextures()
        {
            Engine.Commands.Log(VirtualContent.Count);
            VirtualContent.BySize();
        }

        [Command("givekey", "creates a key on the player")]
        private static void CmdGiveKey()
        {
            Player entity = Engine.Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            Level scene = Engine.Scene as Level;
            Key key = new(entity, new EntityID("unknown", 1073741823 + Calc.Random.Next(10000)));
            scene.Add(key);
            scene.Session.Keys.Add(key.ID);
        }

        [Command("ref_fall", "test the reflection fall sequence")]
        private static void CmdRefFall()
        {
            SaveData.InitializeDebugMode();
            Session session = new(new AreaKey(6));
            session.Level = "04";
            LevelLoader levelLoader = new(session, new Vector2?(session.GetSpawnPoint(new Vector2(session.LevelData.Bounds.Center.X, session.LevelData.Bounds.Top))));
            levelLoader.PlayerIntroTypeOverride = new Player.IntroTypes?(Player.IntroTypes.Fall);
            levelLoader.Level.Add(new BackgroundFadeIn(Color.Black, 2f, 30f));
            Engine.Scene = levelLoader;
        }

        [Command("lines", "Counts Dialog Lines")]
        private static void CmdLines(string language)
        {
            if (string.IsNullOrEmpty(language))
                language = Dialog.Language.Id;
            if (Dialog.Languages.ContainsKey(language))
                Engine.Commands.Log((language + ": " + Dialog.Languages[language].Lines + " lines, " + Dialog.Languages[language].Words + " words"));
            else
                Engine.Commands.Log(("language '" + language + "' doesn't exist"));
        }

        [Command("leaf", "play the leaf minigame")]
        private static void CmdLeaf() => Engine.Scene = new TestBreathingGame();

        [Command("wipes", "plays screen wipes for kert")]
        private static void CmdWipes() => Engine.Scene = new TestWipes();

        [Command("pico", "plays pico-8 game, optional room skip (x/y)")]
        private static void CmdPico(int roomX = 0, int roomY = 0) => Engine.Scene = new Emulator(null, roomX, roomY);

        [Command("colorgrading", "sets color grading enabled (true/false)")]
        private static void CmdColorGrading(bool enabled) => ColorGrade.Enabled = enabled;

        [Command("portraits", "portrait debugger")]
        private static void CmdPortraits() => Engine.Scene = new PreviewPortrait();

        [Command("dialog", "dialog debugger")]
        private static void CmdDialog() => Engine.Scene = new PreviewDialog();

        [Command("titlescreen", "go to the titlescreen")]
        private static void CmdTitlescreen() => Engine.Scene = new OverworldLoader(Overworld.StartMode.Titlescreen);

        [Command("time", "set the time speed")]
        private static void CmdTime(float rate = 1f) => Engine.TimeRate = rate;

        [Command("load", "test a level")]
        private static void CmdLoad(int id = 0, string level = null)
        {
            SaveData.InitializeDebugMode();
            SaveData.Instance.LastArea = new AreaKey(id);
            Session session = new(new AreaKey(id));
            if (level != null && session.MapData.Get(level) != null)
            {
                session.Level = level;
                session.FirstLevel = false;
            }
            Engine.Scene = new LevelLoader(session);
        }

        [Command("hard", "test a hard level")]
        private static void CmdHard(int id = 0, string level = null)
        {
            SaveData.InitializeDebugMode();
            SaveData.Instance.LastArea = new AreaKey(id, AreaMode.BSide);
            Session session = new(new AreaKey(id, AreaMode.BSide));
            if (level != null)
            {
                session.Level = level;
                session.FirstLevel = false;
            }
            Engine.Scene = new LevelLoader(session);
        }

        [Command("music_progress", "set music progress value")]
        private static void CmdMusProgress(int progress) => Audio.SetMusicParam(nameof (progress), progress);

        [Command("rmx2", "test a RMX2 level")]
        private static void CmdRMX2(int id = 0, string level = null)
        {
            SaveData.InitializeDebugMode();
            SaveData.Instance.LastArea = new AreaKey(id, AreaMode.CSide);
            Session session = new(new AreaKey(id, AreaMode.CSide));
            if (level != null)
            {
                session.Level = level;
                session.FirstLevel = false;
            }
            Engine.Scene = new LevelLoader(session);
        }

        [Command("complete", "test the complete screen for an area")]
        private static void CmdComplete(int index = 1, int mode = 0, int deaths = 0, int strawberries = 0, bool gem = false)
        {
            if (SaveData.Instance == null)
            {
                SaveData.InitializeDebugMode();
                SaveData.Instance.CurrentSession = new Session(AreaKey.Default);
            }
            AreaKey area = new(index, (AreaMode) mode);
            int entityID = 0;
            Session session = new(area);
            while (session.Strawberries.Count < strawberries)
            {
                ++entityID;
                session.Strawberries.Add(new EntityID("null", entityID));
            }
            session.Deaths = deaths;
            session.Cassette = gem;
            session.Time = 100000L + Calc.Random.Next();
            Engine.Scene = new LevelExit(LevelExit.Mode.Completed, session);
        }

        [Command("ow_complete", "test the completion sequence on the overworld after a level")]
        private static void CmdOWComplete(
            int index = 1,
            int mode = 0,
            int deaths = 0,
            int strawberries = -1,
            bool cassette = true,
            bool heartGem = true,
            float beatBestTimeBy = 1.7921f,
            float beatBestFullClearTimeBy = 1.7921f)
        {
            if (SaveData.Instance == null)
            {
                SaveData.InitializeDebugMode();
                SaveData.Instance.CurrentSession = new Session(AreaKey.Default);
            }
            AreaKey area1 = new(index, (AreaMode) mode);
            Session session = new(area1);
            AreaStats area2 = SaveData.Instance.Areas[index];
            AreaModeStats mode1 = area2.Modes[mode];
            TimeSpan timeSpan = TimeSpan.FromTicks(mode1.BestTime);
            double totalSeconds1 = timeSpan.TotalSeconds;
            timeSpan = TimeSpan.FromTicks(mode1.BestFullClearTime);
            double totalSeconds2 = timeSpan.TotalSeconds;
            SaveData.Instance.RegisterCompletion(session);
            SaveData.Instance.CurrentSession = session;
            SaveData.Instance.CurrentSession.OldStats = new AreaStats(index);
            SaveData.Instance.LastArea = area1;
            if (mode == 0)
            {
                area2.Modes[0].TotalStrawberries = strawberries != -1 ? Math.Max(area2.TotalStrawberries, strawberries) : AreaData.Areas[index].Mode[0].TotalStrawberries;
                if (cassette)
                    area2.Cassette = true;
            }
            mode1.Deaths = Math.Max(deaths, mode1.Deaths);
            if (heartGem)
                mode1.HeartGem = true;
            if (totalSeconds1 <= 0.0)
            {
                AreaModeStats areaModeStats = mode1;
                timeSpan = TimeSpan.FromMinutes(5.0);
                long ticks = timeSpan.Ticks;
                areaModeStats.BestTime = ticks;
            }
            else if ((double) beatBestTimeBy > 0.0)
            {
                AreaModeStats areaModeStats = mode1;
                timeSpan = TimeSpan.FromSeconds(totalSeconds1 - (double) beatBestTimeBy);
                long ticks = timeSpan.Ticks;
                areaModeStats.BestTime = ticks;
            }
            if ((double) beatBestFullClearTimeBy > 0.0)
            {
                if (totalSeconds2 <= 0.0)
                {
                    AreaModeStats areaModeStats = mode1;
                    timeSpan = TimeSpan.FromMinutes(5.0);
                    long ticks = timeSpan.Ticks;
                    areaModeStats.BestFullClearTime = ticks;
                }
                else
                {
                    AreaModeStats areaModeStats = mode1;
                    timeSpan = TimeSpan.FromSeconds(totalSeconds2 - (double) beatBestFullClearTimeBy);
                    long ticks = timeSpan.Ticks;
                    areaModeStats.BestFullClearTime = ticks;
                }
            }
            Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete);
        }

        [Command("mapedit", "edit a map")]
        private static void CmdMapEdit(int index = -1, int mode = 0)
        {
            Engine.Scene = new MapEditor(index != -1 ? new AreaKey(index, (AreaMode)mode) : (Engine.Scene is not Level ? AreaKey.Default : (Engine.Scene as Level).Session.Area));
            Engine.Commands.Open = false;
        }

        [Command("dflag", "Set a savedata flag")]
        private static void CmdDFlag(string flag, bool setTo = true)
        {
            if (setTo)
                SaveData.Instance.SetFlag(flag);
            else
                SaveData.Instance.Flags.Remove(flag);
        }

        [Command("meet", "Sets flags as though you met Theo")]
        private static void CmdMeet(bool met = true, bool knowsName = true)
        {
            if (met)
                SaveData.Instance.SetFlag("MetTheo");
            else
                SaveData.Instance.Flags.Remove("MetTheo");
            if (knowsName)
                SaveData.Instance.SetFlag("TheoKnowsName");
            else
                SaveData.Instance.Flags.Remove("TheoKnowsName");
        }

        [Command("flag", "set a session flag")]
        private static void CmdFlag(string flag, bool setTo = true) => SaveData.Instance.CurrentSession.SetFlag(flag, setTo);

        [Command("level_flag", "set a session load flag")]
        private static void CmdLevelFlag(string flag) => SaveData.Instance.CurrentSession.LevelFlags.Add(flag);

        [Command("e", "edit a map")]
        private static void CmdE(int index = -1, int mode = 0) => Commands.CmdMapEdit(index, mode);

        [Command("overworld", "go to the overworld")]
        private static void CmdOverworld()
        {
            if (SaveData.Instance == null)
            {
                SaveData.InitializeDebugMode();
                SaveData.Instance.CurrentSession = new Session(AreaKey.Default);
            }
            Engine.Scene = new OverworldLoader(Overworld.StartMode.Titlescreen);
        }

        [Command("music", "play a music track")]
        private static void CmdMusic(string song) => Audio.SetMusic(SFX.EventnameByHandle(song));

        [Command("sd_clearflags", "clears all flags from the save file")]
        private static void CmdClearSave() => SaveData.Instance.Flags.Clear();

        [Command("music_vol", "set the music volume")]
        private static void CmdMusicVol(int num)
        {
            Settings.Instance.MusicVolume = num;
            Settings.Instance.ApplyVolumes();
        }

        [Command("sfx_vol", "set the sfx volume")]
        private static void CmdSFXVol(int num)
        {
            Settings.Instance.SFXVolume = num;
            Settings.Instance.ApplyVolumes();
        }

        [Command("p_dreamdash", "enable dream dashing")]
        private static void CmdDreamDash(bool set = true) => (Engine.Scene as Level).Session.Inventory.DreamDash = set;

        [Command("p_twodashes", "enable two dashes")]
        private static void CmdTwoDashes(bool set = true) => (Engine.Scene as Level).Session.Inventory.Dashes = set ? 2 : 1;

        [Command("berries", "check how many strawberries are in the given chapter, or the entire game")]
        private static void CmdStrawberries(int chapterID = -1)
        {
            if (chapterID == -1)
            {
                int total = 0;
                int[] areaTotals = new int[AreaData.Areas.Count];
                for (int id = 0; id < AreaData.Areas.Count; ++id)
                {
                    new MapData(new AreaKey(id)).GetStrawberries(out areaTotals[id]);
                    total += areaTotals[id];
                }
                Engine.Commands.Log("Grand Total Strawberries: " + total, Color.Yellow);

                for (int i = 0; i < areaTotals.Length; ++i)
                {
                    // Shows the text in lime if the strawberry count is the same as the total it should have
                    // Shows it in gray if it has 0 berries
                    // Shows in red if its strawberry count is different from the total it should have
                    Color color = areaTotals[i] == AreaData.Areas[i].Mode[0].TotalStrawberries ? (areaTotals[i] != 0 ? Color.Lime : Color.Gray) : Color.Red;
                    Engine.Commands.Log("Chapter " + i + ": " + areaTotals[i], color);
                }
            }
            else
            {
                AreaData area = AreaData.Areas[chapterID];
                int totalStrawberries = area.Mode[0].TotalStrawberries;
                int[] checkpointStrawberries = new int[area.Mode[0].Checkpoints.Length + 1];
                checkpointStrawberries[0] = area.Mode[0].StartStrawberries;
                for (int index = 1; index < checkpointStrawberries.Length; ++index)
                    checkpointStrawberries[index] = area.Mode[0].Checkpoints[index - 1].Strawberries;

                int[] strawberries = new MapData(new AreaKey(chapterID)).GetStrawberries(out int total);
                Engine.Commands.Log("Chapter " + chapterID + " Strawberries");
                Engine.Commands.Log("Total: " + total, totalStrawberries == total ? Color.Lime : Color.Red);
                for (int index = 0; index < checkpointStrawberries.Length; ++index)
                {
                    Color color = strawberries[index] == checkpointStrawberries[index] ? (strawberries[index] != 0 ? Color.Lime : Color.Gray) : Color.Red;
                    Engine.Commands.Log("CP" + index + ": " + strawberries[index], color);
                }
            }
        }

        [Command("say", "initiate a dialog message")]
        private static void CmdSay(string id) => Engine.Scene.Add(new Textbox(id, new Func<IEnumerator>[0]));

        [Command("level_count", "print out total level count!")]
        private static void CmdTotalLevels(int areaID = -1, int mode = 0)
        {
            if (areaID >= 0)
            {
                Engine.Commands.Log(Commands.GetLevelsInArea(new AreaKey(areaID, (AreaMode) mode)));
            }
            else
            {
                int num = 0;
                foreach (AreaData area in AreaData.Areas)
                {
                    for (int mode1 = 0; mode1 < area.Mode.Length; ++mode1)
                        num += Commands.GetLevelsInArea(new AreaKey(area.ID, (AreaMode) mode1));
                }
                Engine.Commands.Log(num);
            }
        }

        [Command("input_gui", "override input gui")]
        private static void CmdInputGui(string prefix) => Input.OverrideInputPrefix = prefix;

        private static int GetLevelsInArea(AreaKey key)
        {
            ModeProperties modeProperties = AreaData.Get(key).Mode[(int) key.Mode];
            return modeProperties != null ? modeProperties.MapData.LevelCount : 0;
        }

        [Command("assist", "toggle assist mode for current savefile")]
        private static void CmdAssist()
        {
            SaveData.Instance.AssistMode = !SaveData.Instance.AssistMode;
            SaveData.Instance.VariantMode = false;
        }

        [Command("variants", "toggle varaint mode for current savefile")]
        private static void CmdVariants()
        {
            SaveData.Instance.VariantMode = !SaveData.Instance.VariantMode;
            SaveData.Instance.AssistMode = false;
        }

        [Command("cheat", "toggle cheat mode for the current savefile")]
        private static void CmdCheat() => SaveData.Instance.CheatMode = !SaveData.Instance.CheatMode;

        [Command("capture", "capture the last ~200 frames of player movement to a file")]
        private static void CmdCapture(string filename)
        {
            Player entity = Engine.Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            PlaybackData.Export(entity.ChaserStates, filename + ".bin");
        }

        [Command("playback", "play back the file name")]
        private static void CmdPlayback(string filename)
        {
            filename += ".bin";
            if (File.Exists(filename))
                Engine.Scene = new PreviewRecording(filename);
            else
                Engine.Commands.Log("FILE NOT FOUND");
        }

        [Command("fonts", "check loaded fonts")]
        private static void CmdFonts() => Fonts.Log();

        [Command("rename", "renames a level")]
        private static void CmdRename(string current, string newName)
        {
            if (Engine.Scene is not MapEditor scene)
                Engine.Commands.Log("Must be in the Map Editor");
            else
                scene.Rename(current, newName);
        }

        [Command("blackhole_strength", "value 0 - 3")]
        private static void CmdBlackHoleStrength(int strength)
        {
            strength = Calc.Clamp(strength, 0, 3);
            if (Engine.Scene is not Level scene)
                return;
            scene.Background.Get<BlackholeBG>()?.NextStrength(scene, (BlackholeBG.Strengths) strength);
        }
    }
}
