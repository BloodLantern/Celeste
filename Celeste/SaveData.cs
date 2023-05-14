// Decompiled with JetBrains decompiler
// Type: Celeste.SaveData
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Celeste
{
    [Serializable]
    public class SaveData
    {
        public const int MaxStrawberries = 175;
        public const int MaxGoldenStrawberries = 25;
        public const int MaxStrawberriesDLC = 202;
        public const int MaxHeartGems = 24;
        public const int MaxCassettes = 8;
        public const int MaxCompletions = 8;
        public static SaveData Instance;
        public string Version;
        public string Name = "Madeline";
        public long Time;
        public DateTime LastSave;
        public bool CheatMode;
        public bool AssistMode;
        public bool VariantMode;
        public Assists Assists = Assists.Default;
        public string TheoSisterName;
        public int UnlockedAreas;
        public int TotalDeaths;
        public int TotalStrawberries;
        public int TotalGoldenStrawberries;
        public int TotalJumps;
        public int TotalWallJumps;
        public int TotalDashes;
        public HashSet<string> Flags = new();
        public List<string> Poem = new();
        public bool[] SummitGems;
        public bool RevealedChapter9;
        public AreaKey LastArea;
        public Session CurrentSession;
        public List<AreaStats> Areas = new();
        [XmlIgnore]
        [NonSerialized]
        public int FileSlot;
        [XmlIgnore]
        [NonSerialized]
        public bool DoNotSave;
        [XmlIgnore]
        [NonSerialized]
        public bool DebugMode;

        public static void Start(SaveData data, int slot)
        {
            SaveData.Instance = data;
            SaveData.Instance.FileSlot = slot;
            SaveData.Instance.AfterInitialize();
        }

        public static string GetFilename(int slot)
        {
            return slot == 4 ? "debug" : slot.ToString();
        }

        public static string GetFilename()
        {
            return SaveData.GetFilename(SaveData.Instance.FileSlot);
        }

        public static void InitializeDebugMode(bool loadExisting = true)
        {
            SaveData data = null;
            if (loadExisting && UserIO.Open(UserIO.Mode.Read))
            {
                data = UserIO.Load<SaveData>(SaveData.GetFilename(4));
                UserIO.Close();
            }
            data ??= new SaveData();
            data.DebugMode = true;
            data.CurrentSession = null;
            SaveData.Start(data, 4);
        }

        public static bool TryDelete(int slot)
        {
            return UserIO.Delete(SaveData.GetFilename(slot));
        }

        public void AfterInitialize()
        {
            while (Areas.Count < AreaData.Areas.Count)
            {
                Areas.Add(new AreaStats(Areas.Count));
            }

            while (Areas.Count > AreaData.Areas.Count)
            {
                Areas.RemoveAt(Areas.Count - 1);
            }

            int num = -1;
            for (int index = 0; index < Areas.Count; ++index)
            {
                if (Areas[index].Modes[0].Completed || (Areas[index].Modes.Length > 1 && Areas[index].Modes[1].Completed))
                {
                    num = index;
                }
            }
            if (UnlockedAreas < num + 1 && MaxArea >= num + 1)
            {
                UnlockedAreas = num + 1;
            }

            if (DebugMode)
            {
                CurrentSession = null;
                RevealedChapter9 = true;
                UnlockedAreas = MaxArea;
            }
            if (CheatMode)
            {
                UnlockedAreas = MaxArea;
            }

            if (string.IsNullOrEmpty(TheoSisterName))
            {
                TheoSisterName = Dialog.Clean("THEO_SISTER_NAME");
                if (Name.IndexOf(TheoSisterName, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    TheoSisterName = Dialog.Clean("THEO_SISTER_ALT_NAME");
                }
            }
            AssistModeChecks();
            foreach (AreaStats area in Areas)
            {
                area.CleanCheckpoints();
            }

            if (Version == null || !(new System.Version(Version) < new System.Version(1, 2, 1, 1)))
            {
                return;
            }

            for (int index1 = 0; index1 < Areas.Count; ++index1)
            {
                if (Areas[index1] != null)
                {
                    for (int index2 = 0; index2 < Areas[index1].Modes.Length; ++index2)
                    {
                        if (Areas[index1].Modes[index2] != null)
                        {
                            if (Areas[index1].Modes[index2].BestTime > 0L)
                            {
                                Areas[index1].Modes[index2].SingleRunCompleted = true;
                            }

                            Areas[index1].Modes[index2].BestTime = 0L;
                            Areas[index1].Modes[index2].BestFullClearTime = 0L;
                        }
                    }
                }
            }
        }

        public void AssistModeChecks()
        {
            if (!VariantMode && !AssistMode)
            {
                Assists = new Assists();
            }
            else if (!VariantMode)
            {
                Assists.EnfornceAssistMode();
            }

            if (Assists.GameSpeed is < 5 or > 20)
            {
                Assists.GameSpeed = 10;
            }

            Input.MoveX.Inverted = Input.Aim.InvertedX = Input.Feather.InvertedX = Assists.MirrorMode;
        }

        public static void NoFileAssistChecks()
        {
            Input.MoveX.Inverted = Input.Aim.InvertedX = Input.Feather.InvertedX = false;
        }

        public void BeforeSave()
        {
            SaveData.Instance.Version = Celeste.Instance.Version.ToString();
        }

        public void StartSession(Session session)
        {
            LastArea = session.Area;
            CurrentSession = session;
            if (!DebugMode)
            {
                return;
            }

            AreaModeStats mode1 = Areas[session.Area.ID].Modes[(int)session.Area.Mode];
            AreaModeStats mode2 = session.OldStats.Modes[(int)session.Area.Mode];
            SaveData.Instance.TotalStrawberries -= mode1.TotalStrawberries;
            mode1.Strawberries.Clear();
            mode1.TotalStrawberries = 0;
            mode2.Strawberries.Clear();
            mode2.TotalStrawberries = 0;
        }

        public void AddDeath(AreaKey area)
        {
            ++TotalDeaths;
            ++Areas[area.ID].Modes[(int)area.Mode].Deaths;
#if STEAM
            Stats.Increment(Stat.DEATHS);
#endif
            StatsForStadia.Increment(StadiaStat.DEATHS);
        }

        public void AddStrawberry(AreaKey area, EntityID strawberry, bool golden)
        {
            AreaModeStats mode = Areas[area.ID].Modes[(int)area.Mode];
            if (!mode.Strawberries.Contains(strawberry))
            {
                mode.Strawberries.Add(strawberry);
                ++mode.TotalStrawberries;
                ++TotalStrawberries;
                if (golden)
                {
                    ++TotalGoldenStrawberries;
                }
#if STEAM
                if (this.TotalStrawberries >= 30)
                    Achievements.Register(Achievement.STRB1);
                if (this.TotalStrawberries >= 80)
                    Achievements.Register(Achievement.STRB2);
                if (this.TotalStrawberries >= 175)
                    Achievements.Register(Achievement.STRB3);
#endif
                StatsForStadia.SetIfLarger(StadiaStat.BERRIES, TotalStrawberries);
            }
#if STEAM
            Stats.Increment(golden ? Stat.GOLDBERRIES : Stat.BERRIES);
#endif
        }

        public void AddStrawberry(EntityID strawberry, bool golden)
        {
            AddStrawberry(CurrentSession.Area, strawberry, golden);
        }

        public bool CheckStrawberry(AreaKey area, EntityID strawberry)
        {
            return Areas[area.ID].Modes[(int)area.Mode].Strawberries.Contains(strawberry);
        }

        public bool CheckStrawberry(EntityID strawberry)
        {
            return CheckStrawberry(CurrentSession.Area, strawberry);
        }

        public void AddTime(AreaKey area, long time)
        {
            Time += time;
            Areas[area.ID].Modes[(int)area.Mode].TimePlayed += time;
        }

        public void RegisterHeartGem(AreaKey area)
        {
            Areas[area.ID].Modes[(int)area.Mode].HeartGem = true;
#if STEAM
            if (area.Mode == AreaMode.Normal)
            {
                if (area.ID == 1)
                    Achievements.Register(Achievement.HEART1);
                else if (area.ID == 2)
                    Achievements.Register(Achievement.HEART2);
                else if (area.ID == 3)
                    Achievements.Register(Achievement.HEART3);
                else if (area.ID == 4)
                    Achievements.Register(Achievement.HEART4);
                else if (area.ID == 5)
                    Achievements.Register(Achievement.HEART5);
                else if (area.ID == 6)
                    Achievements.Register(Achievement.HEART6);
                else if (area.ID == 7)
                    Achievements.Register(Achievement.HEART7);
                else if (area.ID == 9)
                    Achievements.Register(Achievement.HEART8);
            }
            else if (area.Mode == AreaMode.BSide)
            {
                if (area.ID == 1)
                    Achievements.Register(Achievement.BSIDE1);
                else if (area.ID == 2)
                    Achievements.Register(Achievement.BSIDE2);
                else if (area.ID == 3)
                    Achievements.Register(Achievement.BSIDE3);
                else if (area.ID == 4)
                    Achievements.Register(Achievement.BSIDE4);
                else if (area.ID == 5)
                    Achievements.Register(Achievement.BSIDE5);
                else if (area.ID == 6)
                    Achievements.Register(Achievement.BSIDE6);
                else if (area.ID == 7)
                    Achievements.Register(Achievement.BSIDE7);
                else if (area.ID == 9)
                    Achievements.Register(Achievement.BSIDE8);
            }
#endif
            StatsForStadia.SetIfLarger(StadiaStat.HEARTS, TotalHeartGems);
        }

        public void RegisterCassette(AreaKey area)
        {
            Areas[area.ID].Cassette = true;
#if STEAM
            Achievements.Register(Achievement.CASS);
#endif
        }

        public bool RegisterPoemEntry(string id)
        {
            id = id.ToLower();
            if (Poem.Contains(id))
            {
                return false;
            }

            Poem.Add(id);
            return true;
        }

        public void RegisterSummitGem(int id)
        {
            SummitGems ??= new bool[6];
            SummitGems[id] = true;
        }

        public void RegisterCompletion(Session session)
        {
            AreaKey area = session.Area;
            AreaModeStats mode = Areas[area.ID].Modes[(int)area.Mode];
            if (session.GrabbedGolden)
            {
                mode.BestDeaths = 0;
            }

            if (session.StartedFromBeginning)
            {
                mode.SingleRunCompleted = true;
                if (mode.BestTime <= 0L || session.Deaths < mode.BestDeaths)
                {
                    mode.BestDeaths = session.Deaths;
                }

                if (mode.BestTime <= 0L || session.Dashes < mode.BestDashes)
                {
                    mode.BestDashes = session.Dashes;
                }

                if (mode.BestTime <= 0L || session.Time < mode.BestTime)
                {
                    if (mode.BestTime > 0L)
                    {
                        session.BeatBestTime = true;
                    }

                    mode.BestTime = session.Time;
                }
                if (area.Mode == AreaMode.Normal && session.FullClear)
                {
                    mode.FullClear = true;
                    if (session.StartedFromBeginning && (mode.BestFullClearTime <= 0L || session.Time < mode.BestFullClearTime))
                    {
                        mode.BestFullClearTime = session.Time;
                    }
                }
            }
            if (area.ID + 1 > UnlockedAreas && area.ID < MaxArea)
            {
                UnlockedAreas = area.ID + 1;
            }

            mode.Completed = true;
            session.InArea = false;
        }

        public bool SetCheckpoint(AreaKey area, string level)
        {
            AreaModeStats mode = Areas[area.ID].Modes[(int)area.Mode];
            if (mode.Checkpoints.Contains(level))
            {
                return false;
            }

            _ = mode.Checkpoints.Add(level);
            return true;
        }

        public bool HasCheckpoint(AreaKey area, string level)
        {
            return Areas[area.ID].Modes[(int)area.Mode].Checkpoints.Contains(level);
        }

        public bool FoundAnyCheckpoints(AreaKey area)
        {
            if (Celeste.PlayMode == Celeste.PlayModes.Event)
            {
                return false;
            }

            if (!DebugMode && !CheatMode)
            {
                return Areas[area.ID].Modes[(int)area.Mode].Checkpoints.Count > 0;
            }

            ModeProperties modeProperties = AreaData.Areas[area.ID].Mode[(int)area.Mode];
            return modeProperties != null && modeProperties.Checkpoints != null && modeProperties.Checkpoints.Length != 0;
        }

        public HashSet<string> GetCheckpoints(AreaKey area)
        {
            if (Celeste.PlayMode == Celeste.PlayModes.Event)
            {
                return new HashSet<string>();
            }

            if (!DebugMode && !CheatMode)
            {
                return Areas[area.ID].Modes[(int)area.Mode].Checkpoints;
            }

            HashSet<string> checkpoints = new();
            ModeProperties modeProperties = AreaData.Areas[area.ID].Mode[(int)area.Mode];
            if (modeProperties.Checkpoints != null)
            {
                foreach (CheckpointData checkpoint in modeProperties.Checkpoints)
                {
                    _ = checkpoints.Add(checkpoint.Level);
                }
            }
            return checkpoints;
        }

        public bool HasFlag(string flag)
        {
            return Flags.Contains(flag);
        }

        public void SetFlag(string flag)
        {
            if (HasFlag(flag))
            {
                return;
            }

            _ = Flags.Add(flag);
        }

        public int UnlockedModes
        {
            get
            {
                if (DebugMode || CheatMode || TotalHeartGems >= 16)
                {
                    return 3;
                }

                for (int index = 1; index <= MaxArea; ++index)
                {
                    if (Areas[index].Cassette)
                    {
                        return 2;
                    }
                }
                return 1;
            }
        }

        public int MaxArea => Celeste.PlayMode == Celeste.PlayModes.Event ? 2 : AreaData.Areas.Count - 1;

        public int MaxAssistArea => AreaData.Areas.Count - 1;

        public int TotalHeartGems
        {
            get
            {
                int totalHeartGems = 0;
                foreach (AreaStats area in Areas)
                {
                    for (int index = 0; index < area.Modes.Length; ++index)
                    {
                        if (area.Modes[index] != null && area.Modes[index].HeartGem)
                        {
                            ++totalHeartGems;
                        }
                    }
                }
                return totalHeartGems;
            }
        }

        public int TotalCassettes
        {
            get
            {
                int totalCassettes = 0;
                for (int index = 0; index <= MaxArea; ++index)
                {
                    if (!AreaData.Get(index).Interlude && Areas[index].Cassette)
                    {
                        ++totalCassettes;
                    }
                }
                return totalCassettes;
            }
        }

        public int TotalCompletions
        {
            get
            {
                int totalCompletions = 0;
                for (int index = 0; index <= MaxArea; ++index)
                {
                    if (!AreaData.Get(index).Interlude && Areas[index].Modes[0].Completed)
                    {
                        ++totalCompletions;
                    }
                }
                return totalCompletions;
            }
        }

        public bool HasAllFullClears
        {
            get
            {
                for (int index = 0; index <= MaxArea; ++index)
                {
                    if (AreaData.Get(index).CanFullClear && !Areas[index].Modes[0].FullClear)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public int CompletionPercent
        {
            get
            {
                float num1 = 0.0f;
                float num2 = TotalHeartGems < 24 ? num1 + (float)(TotalHeartGems / 24.0 * 24.0) : num1 + 24f;
                float num3 = TotalStrawberries < 175 ? num2 + (float)(TotalStrawberries / 175.0 * 55.0) : num2 + 55f;
                float num4 = TotalCassettes < 8 ? num3 + (float)(TotalCassettes / 8.0 * 7.0) : num3 + 7f;
                float completionPercent = TotalCompletions < 8 ? num4 + (float)(TotalCompletions / 8.0 * 14.0) : num4 + 14f;
                if ((double)completionPercent < 0.0)
                {
                    completionPercent = 0.0f;
                }
                else if ((double)completionPercent > 100.0)
                {
                    completionPercent = 100f;
                }

                return (int)completionPercent;
            }
        }
    }
}
