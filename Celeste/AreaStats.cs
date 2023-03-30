// Decompiled with JetBrains decompiler
// Type: Celeste.AreaStats
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Celeste
{
    [Serializable]
    public class AreaStats
    {
        [XmlAttribute]
        public int ID;
        [XmlAttribute]
        public bool Cassette;
        public AreaModeStats[] Modes;

        public int TotalStrawberries
        {
            get
            {
                int totalStrawberries = 0;
                for (int index = 0; index < this.Modes.Length; ++index)
                    totalStrawberries += this.Modes[index].TotalStrawberries;
                return totalStrawberries;
            }
        }

        public int TotalDeaths
        {
            get
            {
                int totalDeaths = 0;
                for (int index = 0; index < this.Modes.Length; ++index)
                    totalDeaths += this.Modes[index].Deaths;
                return totalDeaths;
            }
        }

        public long TotalTimePlayed
        {
            get
            {
                long totalTimePlayed = 0;
                for (int index = 0; index < this.Modes.Length; ++index)
                    totalTimePlayed += this.Modes[index].TimePlayed;
                return totalTimePlayed;
            }
        }

        public int BestTotalDeaths
        {
            get
            {
                int bestTotalDeaths = 0;
                for (int index = 0; index < this.Modes.Length; ++index)
                    bestTotalDeaths += this.Modes[index].BestDeaths;
                return bestTotalDeaths;
            }
        }

        public int BestTotalDashes
        {
            get
            {
                int bestTotalDashes = 0;
                for (int index = 0; index < this.Modes.Length; ++index)
                    bestTotalDashes += this.Modes[index].BestDashes;
                return bestTotalDashes;
            }
        }

        public long BestTotalTime
        {
            get
            {
                long bestTotalTime = 0;
                for (int index = 0; index < this.Modes.Length; ++index)
                    bestTotalTime += this.Modes[index].BestTime;
                return bestTotalTime;
            }
        }

        public AreaStats(int id)
        {
            this.ID = id;
            this.Modes = new AreaModeStats[Enum.GetValues(typeof (AreaMode)).Length];
            for (int index = 0; index < this.Modes.Length; ++index)
                this.Modes[index] = new AreaModeStats();
        }

        private AreaStats()
        {
            int length = Enum.GetValues(typeof (AreaMode)).Length;
            this.Modes = new AreaModeStats[length];
            for (int index = 0; index < length; ++index)
                this.Modes[index] = new AreaModeStats();
        }

        public AreaStats Clone()
        {
            AreaStats areaStats = new AreaStats()
            {
                ID = this.ID,
                Cassette = this.Cassette
            };
            for (int index = 0; index < this.Modes.Length; ++index)
                areaStats.Modes[index] = this.Modes[index].Clone();
            return areaStats;
        }

        public void CleanCheckpoints()
        {
            foreach (AreaMode index in Enum.GetValues(typeof (AreaMode)))
            {
                if ((AreaMode) AreaData.Get(this.ID).Mode.Length > index)
                {
                    AreaModeStats mode = this.Modes[(int) index];
                    ModeProperties modeProperties = AreaData.Get(this.ID).Mode[(int) index];
                    HashSet<string> stringSet = new HashSet<string>((IEnumerable<string>) mode.Checkpoints);
                    mode.Checkpoints.Clear();
                    if (modeProperties != null && modeProperties.Checkpoints != null)
                    {
                        foreach (CheckpointData checkpoint in modeProperties.Checkpoints)
                        {
                            if (stringSet.Contains(checkpoint.Level))
                                mode.Checkpoints.Add(checkpoint.Level);
                        }
                    }
                }
            }
        }
    }
}
