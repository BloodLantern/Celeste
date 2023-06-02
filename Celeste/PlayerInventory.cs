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
