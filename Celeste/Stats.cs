using Steamworks;
using System.Collections.Generic;

namespace Celeste
{
    public static class Stats
    {
        private static Dictionary<Stat, string> statToString = new Dictionary<Stat, string>();
        private static bool ready;

        public static void MakeRequest()
        {
            Stats.ready = SteamUserStats.RequestCurrentStats();
            SteamUserStats.RequestGlobalStats(0);
        }

        public static bool Has() => Stats.ready;

        public static void Increment(Stat stat, int increment = 1)
        {
            if (!Stats.ready)
                return;
            string str = (string) null;
            if (!Stats.statToString.TryGetValue(stat, out str))
                Stats.statToString.Add(stat, str = stat.ToString());
            int num;
            if (!SteamUserStats.GetStat(str, out num))
                return;
            SteamUserStats.SetStat(str, num + increment);
        }

        public static int Local(Stat stat)
        {
            int num = 0;
            if (Stats.ready)
            {
                string str = (string) null;
                if (!Stats.statToString.TryGetValue(stat, out str))
                    Stats.statToString.Add(stat, str = stat.ToString());
                SteamUserStats.GetStat(str, out num);
            }
            return num;
        }

        public static long Global(Stat stat)
        {
            long num = 0;
            if (Stats.ready)
            {
                string str = (string) null;
                if (!Stats.statToString.TryGetValue(stat, out str))
                    Stats.statToString.Add(stat, str = stat.ToString());
                SteamUserStats.GetGlobalStat(str, out num);
            }
            return num;
        }

        public static void Store()
        {
            if (!Stats.ready)
                return;
            SteamUserStats.StoreStats();
        }

        public static string Name(Stat stat) => Dialog.Clean("STAT_" + stat.ToString());
    }
}
