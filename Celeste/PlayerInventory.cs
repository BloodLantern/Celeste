// Decompiled with JetBrains decompiler
// Type: Celeste.PlayerInventory
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace Celeste
{
    [Serializable]
    public struct PlayerInventory
    {
        public static readonly PlayerInventory Prologue = new PlayerInventory(0, false);
        public static readonly PlayerInventory Default = new PlayerInventory();
        public static readonly PlayerInventory OldSite = new PlayerInventory(dreamDash: false);
        public static readonly PlayerInventory CH6End = new PlayerInventory(2);
        public static readonly PlayerInventory TheSummit = new PlayerInventory(2, backpack: false);
        public static readonly PlayerInventory Core = new PlayerInventory(2, noRefills: true);
        public static readonly PlayerInventory Farewell = new PlayerInventory(backpack: false);
        public int Dashes;
        public bool DreamDash;
        public bool Backpack;
        public bool NoRefills;

        public PlayerInventory(int dashes = 1, bool dreamDash = true, bool backpack = true, bool noRefills = false)
        {
            this.Dashes = dashes;
            this.DreamDash = dreamDash;
            this.Backpack = backpack;
            this.NoRefills = noRefills;
        }
    }
}
