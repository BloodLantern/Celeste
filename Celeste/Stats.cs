using Steamworks;
using System.Collections.Generic;

namespace Celeste
{
    public static class Stats
    {
        private static readonly Dictionary<Stat, string> statToString = new();
        private static bool ready;

        public static void MakeRequest()
        {
            ready = SteamUserStats.RequestCurrentStats();
            SteamUserStats.RequestGlobalStats(0);
        }

        public static bool Has() => ready;

        public static void Increment(Stat stat, int increment = 1)
        {
            if (!ready)
                return;
            if (!statToString.TryGetValue(stat, out string str))
                statToString.Add(stat, str = stat.ToString());
            if (!SteamUserStats.GetStat(str, out int num))
                return;
            SteamUserStats.SetStat(str, num + increment);
        }

        public static int Local(Stat stat)
        {
            int num = 0;
            if (ready)
            {
                if (!statToString.TryGetValue(stat, out string str))
                    statToString.Add(stat, str = stat.ToString());
                SteamUserStats.GetStat(str, out num);
            }
            return num;
        }

        public static long Global(Stat stat)
        {
            long num = 0;
            if (ready)
            {
                if (!statToString.TryGetValue(stat, out string str))
                    statToString.Add(stat, str = stat.ToString());
                SteamUserStats.GetGlobalStat(str, out num);
            }
            return num;
        }

        public static void Store()
        {
            if (!ready)
                return;
            SteamUserStats.StoreStats();
        }

        public static string Name(Stat stat) => Dialog.Clean("STAT_" + stat.ToString());
    }
}
