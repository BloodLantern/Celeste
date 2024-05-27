using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Celeste
{
    [Serializable]
    public class Session
    {
        public AreaKey Area;
        public Vector2? RespawnPoint;
        public AudioState Audio = new();
        public PlayerInventory Inventory;
        public HashSet<string> Flags = new();
        public HashSet<string> LevelFlags = new();
        public HashSet<EntityID> Strawberries = new();
        public HashSet<EntityID> DoNotLoad = new();
        public HashSet<EntityID> Keys = new();
        public List<Counter> Counters = new();
        public bool[] SummitGems = new bool[6];
        public AreaStats OldStats;
        public bool UnlockedCSide;
        public string FurthestSeenLevel;
        public bool BeatBestTime;
        [XmlAttribute]
        public string Level;
        [XmlAttribute]
        public long Time;
        [XmlAttribute]
        public bool StartedFromBeginning;
        [XmlAttribute]
        public int Deaths;
        [XmlAttribute]
        public int Dashes;
        [XmlAttribute]
        public int DashesAtLevelStart;
        [XmlAttribute]
        public int DeathsInCurrentLevel;
        [XmlAttribute]
        public bool InArea;
        [XmlAttribute]
        public string StartCheckpoint;
        [XmlAttribute]
        public bool FirstLevel = true;
        [XmlAttribute]
        public bool Cassette;
        [XmlAttribute]
        public bool HeartGem;
        [XmlAttribute]
        public bool Dreaming;
        [XmlAttribute]
        public string ColorGrade;
        [XmlAttribute]
        public float LightingAlphaAdd;
        [XmlAttribute]
        public float BloomBaseAdd;
        [XmlAttribute]
        public float DarkRoomAlpha = 0.75f;
        [XmlAttribute]
        public CoreModes CoreMode;
        [XmlAttribute]
        public bool GrabbedGolden;
        [XmlAttribute]
        public bool HitCheckpoint;
        [XmlIgnore]
        [NonSerialized]
        public bool JustStarted;

        public MapData MapData => AreaData.Areas[Area.ID].Mode[(int) Area.Mode].MapData;

        private Session()
        {
            JustStarted = true;
            InArea = true;
        }

        public Session(AreaKey area, string checkpoint = null, AreaStats oldStats = null)
            : this()
        {
            Area = area;
            StartCheckpoint = checkpoint;
            ColorGrade = MapData.Data.ColorGrade;
            Dreaming = AreaData.Areas[area.ID].Dreaming;
            Inventory = AreaData.GetCheckpointInventory(area, checkpoint);
            CoreMode = AreaData.Areas[area.ID].CoreMode;
            FirstLevel = true;
            Audio = MapData.ModeData.AudioState.Clone();
            if (StartCheckpoint == null)
            {
                Level = MapData.StartLevel().Name;
                StartedFromBeginning = true;
            }
            else
            {
                Level = StartCheckpoint;
                StartedFromBeginning = false;
                Dreaming = AreaData.GetCheckpointDreaming(area, checkpoint);
                CoreMode = AreaData.GetCheckpointCoreMode(area, checkpoint);
                AudioState checkpointAudioState = AreaData.GetCheckpointAudioState(area, checkpoint);
                if (checkpointAudioState != null)
                {
                    if (checkpointAudioState.Music != null)
                        Audio.Music = checkpointAudioState.Music.Clone();
                    if (checkpointAudioState.Ambience != null)
                        Audio.Ambience = checkpointAudioState.Ambience.Clone();
                }
                string checkpointColorGrading = AreaData.GetCheckpointColorGrading(area, checkpoint);
                if (checkpointColorGrading != null)
                    ColorGrade = checkpointColorGrading;
                CheckpointData checkpoint1 = AreaData.GetCheckpoint(area, checkpoint);
                if (checkpoint1 != null && checkpoint1.Flags != null)
                {
                    foreach (string flag in checkpoint1.Flags)
                        SetFlag(flag);
                }
            }
            if (oldStats != null)
                OldStats = oldStats;
            else
                OldStats = SaveData.Instance.Areas[Area.ID].Clone();
        }

        public LevelData LevelData => MapData.Get(Level);

        public bool FullClear
        {
            get
            {
                if (Area.Mode != AreaMode.Normal || !Cassette || !HeartGem || Strawberries.Count < MapData.DetectedStrawberries)
                    return false;
                return Area.ID != 7 || HasAllSummitGems;
            }
        }

        public bool ShouldAdvance => Area.Mode == AreaMode.Normal && !OldStats.Modes[0].Completed && Area.ID < SaveData.Instance.MaxArea;

        public Session Restart(string intoLevel = null)
        {
            Session session = new(Area, StartCheckpoint, OldStats)
            {
                UnlockedCSide = UnlockedCSide
            };
            if (intoLevel != null)
            {
                session.Level = intoLevel;
                if (intoLevel != MapData.StartLevel().Name)
                    session.StartedFromBeginning = false;
            }
            return session;
        }

        public void UpdateLevelStartDashes() => DashesAtLevelStart = Dashes;

        public bool HasAllSummitGems
        {
            get
            {
                for (int index = 0; index < SummitGems.Length; ++index)
                {
                    if (!SummitGems[index])
                        return false;
                }
                return true;
            }
        }

        public Vector2 GetSpawnPoint(Vector2 from) => LevelData.Spawns.ClosestTo(from);

        public bool GetFlag(string flag) => Flags.Contains(flag);

        public void SetFlag(string flag, bool setTo = true)
        {
            if (setTo)
                Flags.Add(flag);
            else
                Flags.Remove(flag);
        }

        public int GetCounter(string counter)
        {
            for (int index = 0; index < Counters.Count; ++index)
            {
                if (Counters[index].Key.Equals(counter))
                    return Counters[index].Value;
            }
            return 0;
        }

        public void SetCounter(string counter, int value)
        {
            for (int index = 0; index < Counters.Count; ++index)
            {
                if (Counters[index].Key.Equals(counter))
                {
                    Counters[index].Value = value;
                    return;
                }
            }
            Counters.Add(new Counter
            {
                Key = counter,
                Value = value
            });
        }

        public void IncrementCounter(string counter)
        {
            for (int index = 0; index < Counters.Count; ++index)
            {
                if (Counters[index].Key.Equals(counter))
                {
                    ++Counters[index].Value;
                    return;
                }
            }
            Counters.Add(new Counter
            {
                Key = counter,
                Value = 1
            });
        }

        public bool GetLevelFlag(string level) => LevelFlags.Contains(level);

        [Serializable]
        public class Counter
        {
            [XmlAttribute("key")]
            public string Key;
            [XmlAttribute("value")]
            public int Value;
        }

        public enum CoreModes
        {
            None,
            Hot,
            Cold,
        }
    }
}
