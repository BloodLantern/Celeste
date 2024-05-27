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
                for (int index = 0; index < Modes.Length; ++index)
                    totalStrawberries += Modes[index].TotalStrawberries;
                return totalStrawberries;
            }
        }

        public int TotalDeaths
        {
            get
            {
                int totalDeaths = 0;
                for (int index = 0; index < Modes.Length; ++index)
                    totalDeaths += Modes[index].Deaths;
                return totalDeaths;
            }
        }

        public long TotalTimePlayed
        {
            get
            {
                long totalTimePlayed = 0;
                for (int index = 0; index < Modes.Length; ++index)
                    totalTimePlayed += Modes[index].TimePlayed;
                return totalTimePlayed;
            }
        }

        public int BestTotalDeaths
        {
            get
            {
                int bestTotalDeaths = 0;
                for (int index = 0; index < Modes.Length; ++index)
                    bestTotalDeaths += Modes[index].BestDeaths;
                return bestTotalDeaths;
            }
        }

        public int BestTotalDashes
        {
            get
            {
                int bestTotalDashes = 0;
                for (int index = 0; index < Modes.Length; ++index)
                    bestTotalDashes += Modes[index].BestDashes;
                return bestTotalDashes;
            }
        }

        public long BestTotalTime
        {
            get
            {
                long bestTotalTime = 0;
                for (int index = 0; index < Modes.Length; ++index)
                    bestTotalTime += Modes[index].BestTime;
                return bestTotalTime;
            }
        }

        public AreaStats(int id)
        {
            ID = id;
            Modes = new AreaModeStats[Enum.GetValues(typeof (AreaMode)).Length];
            for (int index = 0; index < Modes.Length; ++index)
                Modes[index] = new AreaModeStats();
        }

        private AreaStats()
        {
            int length = Enum.GetValues(typeof (AreaMode)).Length;
            Modes = new AreaModeStats[length];
            for (int index = 0; index < length; ++index)
                Modes[index] = new AreaModeStats();
        }

        public AreaStats Clone()
        {
            AreaStats areaStats = new AreaStats
            {
                ID = ID,
                Cassette = Cassette
            };
            for (int index = 0; index < Modes.Length; ++index)
                areaStats.Modes[index] = Modes[index].Clone();
            return areaStats;
        }

        public void CleanCheckpoints()
        {
            foreach (AreaMode index in Enum.GetValues(typeof (AreaMode)))
            {
                if ((AreaMode) AreaData.Get(ID).Mode.Length > index)
                {
                    AreaModeStats mode = Modes[(int) index];
                    ModeProperties modeProperties = AreaData.Get(ID).Mode[(int) index];
                    HashSet<string> stringSet = new HashSet<string>(mode.Checkpoints);
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
