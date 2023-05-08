// Decompiled with JetBrains decompiler
// Type: Celeste.CheckpointData
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Collections.Generic;

namespace Celeste
{
    public class CheckpointData
    {
        public string Level;
        public string Name;
        public bool Dreaming;
        public int Strawberries;
        public string ColorGrade;
        public PlayerInventory? Inventory;
        public AudioState AudioState;
        public HashSet<string> Flags;
        public Session.CoreModes? CoreMode;

        public CheckpointData(
            string level,
            string name,
            PlayerInventory? inventory = null,
            bool dreaming = false,
            AudioState audioState = null)
        {
            Level = level;
            Name = name;
            Inventory = inventory;
            Dreaming = dreaming;
            AudioState = audioState;
            CoreMode = new Session.CoreModes?();
            ColorGrade = null;
        }
    }
}
