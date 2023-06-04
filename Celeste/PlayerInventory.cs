using System;

namespace Celeste
{
    [Serializable]
    public struct PlayerInventory
    {
        public static readonly PlayerInventory Prologue = new(0, false);
        public static readonly PlayerInventory Default = new(1); // Not using at least 1 parameter will call the default constructor which will initialize everything to 0
        public static readonly PlayerInventory OldSite = new(dreamDash: false);
        public static readonly PlayerInventory CH6End = new(2);
        public static readonly PlayerInventory TheSummit = new(2, backpack: false);
        public static readonly PlayerInventory Core = new(2, noRefills: true);
        public static readonly PlayerInventory Farewell = new(backpack: false);

        public int Dashes;
        public bool DreamDash;
        public bool Backpack;
        public bool NoRefills;

        private PlayerInventory(int dashes = 1, bool dreamDash = true, bool backpack = true, bool noRefills = false)
        {
            Dashes = dashes;
            DreamDash = dreamDash;
            Backpack = backpack;
            NoRefills = noRefills;
        }
    }
}
