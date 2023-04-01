// Decompiled with JetBrains decompiler
// Type: Celeste.AreaModeStats
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Celeste
{
    [Serializable]
    public class AreaModeStats
    {
        [XmlAttribute]
        public int TotalStrawberries;
        [XmlAttribute]
        public bool Completed;
        [XmlAttribute]
        public bool SingleRunCompleted;
        [XmlAttribute]
        public bool FullClear;
        [XmlAttribute]
        public int Deaths;
        [XmlAttribute]
        public long TimePlayed;
        [XmlAttribute]
        public long BestTime;
        [XmlAttribute]
        public long BestFullClearTime;
        [XmlAttribute]
        public int BestDashes;
        [XmlAttribute]
        public int BestDeaths;
        [XmlAttribute]
        public bool HeartGem;
        public HashSet<EntityID> Strawberries = new();
        public HashSet<string> Checkpoints = new();

        public AreaModeStats Clone()
        {
            return new AreaModeStats()
            {
                TotalStrawberries = TotalStrawberries,
                Strawberries = new HashSet<EntityID>(Strawberries),
                Completed = Completed,
                SingleRunCompleted = SingleRunCompleted,
                FullClear = FullClear,
                Deaths = Deaths,
                TimePlayed = TimePlayed,
                BestTime = BestTime,
                BestFullClearTime = BestFullClearTime,
                BestDashes = BestDashes,
                BestDeaths = BestDeaths,
                HeartGem = HeartGem,
                Checkpoints = new HashSet<string>(Checkpoints)
            };
        }
    }
}
