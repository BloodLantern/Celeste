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
                for (int i = 0; i < Modes.Length; ++i)
                {
                    totalStrawberries += Modes[i].TotalStrawberries;
                }

                return totalStrawberries;
            }
        }

        public int TotalDeaths
        {
            get
            {
                int totalDeaths = 0;
                for (int i = 0; i < Modes.Length; ++i)
                {
                    totalDeaths += Modes[i].Deaths;
                }

                return totalDeaths;
            }
        }

        public long TotalTimePlayed
        {
            get
            {
                long totalTimePlayed = 0;
                for (int i = 0; i < Modes.Length; ++i)
                {
                    totalTimePlayed += Modes[i].TimePlayed;
                }

                return totalTimePlayed;
            }
        }

        public int BestTotalDeaths
        {
            get
            {
                int bestTotalDeaths = 0;
                for (int i = 0; i < Modes.Length; ++i)
                {
                    bestTotalDeaths += Modes[i].BestDeaths;
                }

                return bestTotalDeaths;
            }
        }

        public int BestTotalDashes
        {
            get
            {
                int bestTotalDashes = 0;
                for (int i = 0; i < Modes.Length; ++i)
                {
                    bestTotalDashes += Modes[i].BestDashes;
                }

                return bestTotalDashes;
            }
        }

        public long BestTotalTime
        {
            get
            {
                long bestTotalTime = 0;
                for (int i = 0; i < Modes.Length; ++i)
                {
                    bestTotalTime += Modes[i].BestTime;
                }

                return bestTotalTime;
            }
        }

        public AreaStats(int id)
        {
            ID = id;
            Modes = new AreaModeStats[Enum.GetValues(typeof(AreaMode)).Length];
            for (int i = 0; i < Modes.Length; ++i)
            {
                Modes[i] = new AreaModeStats();
            }
        }

        private AreaStats()
        {
            int length = Enum.GetValues(typeof(AreaMode)).Length;
            Modes = new AreaModeStats[length];
            for (int i = 0; i < length; ++i)
            {
                Modes[i] = new AreaModeStats();
            }
        }

        public AreaStats Clone()
        {
            AreaStats areaStats = new()
            {
                ID = ID,
                Cassette = Cassette
            };
            for (int i = 0; i < Modes.Length; ++i)
            {
                areaStats.Modes[i] = Modes[i].Clone();
            }

            return areaStats;
        }

        public void CleanCheckpoints()
        {
            foreach (AreaMode index in Enum.GetValues(typeof(AreaMode)))
                if ((AreaMode) AreaData.Get(ID).Mode.Length > index)
                {
                    AreaModeStats mode = Modes[(int) index];
                    ModeProperties modeProperties = AreaData.Get(ID).Mode[(int) index];
                    HashSet<string> stringSet = new(mode.Checkpoints);
                    mode.Checkpoints.Clear();
                    if (modeProperties != null && modeProperties.Checkpoints != null)
                        foreach (CheckpointData checkpoint in modeProperties.Checkpoints)
                            if (stringSet.Contains(checkpoint.Level))
                                _ = mode.Checkpoints.Add(checkpoint.Level);
                }
        }
    }
}
