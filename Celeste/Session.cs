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
        public AudioState Audio = new AudioState();
        public PlayerInventory Inventory;
        public HashSet<string> Flags = new HashSet<string>();
        public HashSet<string> LevelFlags = new HashSet<string>();
        public HashSet<EntityID> Strawberries = new HashSet<EntityID>();
        public HashSet<EntityID> DoNotLoad = new HashSet<EntityID>();
        public HashSet<EntityID> Keys = new HashSet<EntityID>();
        public List<Session.Counter> Counters = new List<Session.Counter>();
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
        public Session.CoreModes CoreMode;
        [XmlAttribute]
        public bool GrabbedGolden;
        [XmlAttribute]
        public bool HitCheckpoint;
        [XmlIgnore]
        [NonSerialized]
        public bool JustStarted;

        public MapData MapData => AreaData.Areas[this.Area.ID].Mode[(int) this.Area.Mode].MapData;

        private Session()
        {
            this.JustStarted = true;
            this.InArea = true;
        }

        public Session(AreaKey area, string checkpoint = null, AreaStats oldStats = null)
            : this()
        {
            this.Area = area;
            this.StartCheckpoint = checkpoint;
            this.ColorGrade = this.MapData.Data.ColorGrade;
            this.Dreaming = AreaData.Areas[area.ID].Dreaming;
            this.Inventory = AreaData.GetCheckpointInventory(area, checkpoint);
            this.CoreMode = AreaData.Areas[area.ID].CoreMode;
            this.FirstLevel = true;
            this.Audio = this.MapData.ModeData.AudioState.Clone();
            if (this.StartCheckpoint == null)
            {
                this.Level = this.MapData.StartLevel().Name;
                this.StartedFromBeginning = true;
            }
            else
            {
                this.Level = this.StartCheckpoint;
                this.StartedFromBeginning = false;
                this.Dreaming = AreaData.GetCheckpointDreaming(area, checkpoint);
                this.CoreMode = AreaData.GetCheckpointCoreMode(area, checkpoint);
                AudioState checkpointAudioState = AreaData.GetCheckpointAudioState(area, checkpoint);
                if (checkpointAudioState != null)
                {
                    if (checkpointAudioState.Music != null)
                        this.Audio.Music = checkpointAudioState.Music.Clone();
                    if (checkpointAudioState.Ambience != null)
                        this.Audio.Ambience = checkpointAudioState.Ambience.Clone();
                }
                string checkpointColorGrading = AreaData.GetCheckpointColorGrading(area, checkpoint);
                if (checkpointColorGrading != null)
                    this.ColorGrade = checkpointColorGrading;
                CheckpointData checkpoint1 = AreaData.GetCheckpoint(area, checkpoint);
                if (checkpoint1 != null && checkpoint1.Flags != null)
                {
                    foreach (string flag in checkpoint1.Flags)
                        this.SetFlag(flag);
                }
            }
            if (oldStats != null)
                this.OldStats = oldStats;
            else
                this.OldStats = SaveData.Instance.Areas[this.Area.ID].Clone();
        }

        public LevelData LevelData => this.MapData.Get(this.Level);

        public bool FullClear
        {
            get
            {
                if (this.Area.Mode != AreaMode.Normal || !this.Cassette || !this.HeartGem || this.Strawberries.Count < this.MapData.DetectedStrawberries)
                    return false;
                return this.Area.ID != 7 || this.HasAllSummitGems;
            }
        }

        public bool ShouldAdvance => this.Area.Mode == AreaMode.Normal && !this.OldStats.Modes[0].Completed && this.Area.ID < SaveData.Instance.MaxArea;

        public Session Restart(string intoLevel = null)
        {
            Session session = new Session(this.Area, this.StartCheckpoint, this.OldStats)
            {
                UnlockedCSide = this.UnlockedCSide
            };
            if (intoLevel != null)
            {
                session.Level = intoLevel;
                if (intoLevel != this.MapData.StartLevel().Name)
                    session.StartedFromBeginning = false;
            }
            return session;
        }

        public void UpdateLevelStartDashes() => this.DashesAtLevelStart = this.Dashes;

        public bool HasAllSummitGems
        {
            get
            {
                for (int index = 0; index < this.SummitGems.Length; ++index)
                {
                    if (!this.SummitGems[index])
                        return false;
                }
                return true;
            }
        }

        public Vector2 GetSpawnPoint(Vector2 from) => this.LevelData.Spawns.ClosestTo(from);

        public bool GetFlag(string flag) => this.Flags.Contains(flag);

        public void SetFlag(string flag, bool setTo = true)
        {
            if (setTo)
                this.Flags.Add(flag);
            else
                this.Flags.Remove(flag);
        }

        public int GetCounter(string counter)
        {
            for (int index = 0; index < this.Counters.Count; ++index)
            {
                if (this.Counters[index].Key.Equals(counter))
                    return this.Counters[index].Value;
            }
            return 0;
        }

        public void SetCounter(string counter, int value)
        {
            for (int index = 0; index < this.Counters.Count; ++index)
            {
                if (this.Counters[index].Key.Equals(counter))
                {
                    this.Counters[index].Value = value;
                    return;
                }
            }
            this.Counters.Add(new Session.Counter()
            {
                Key = counter,
                Value = value
            });
        }

        public void IncrementCounter(string counter)
        {
            for (int index = 0; index < this.Counters.Count; ++index)
            {
                if (this.Counters[index].Key.Equals(counter))
                {
                    ++this.Counters[index].Value;
                    return;
                }
            }
            this.Counters.Add(new Session.Counter()
            {
                Key = counter,
                Value = 1
            });
        }

        public bool GetLevelFlag(string level) => this.LevelFlags.Contains(level);

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
