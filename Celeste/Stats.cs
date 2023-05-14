// Decompiled with JetBrains decompiler
// Type: Celeste.Stats
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

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
            Stats.ready = SteamUserStats.RequestCurrentStats();
            _ = SteamUserStats.RequestGlobalStats(0);
        }

        public static bool Has()
        {
            return Stats.ready;
        }

        public static void Increment(Stat stat, int increment = 1)
        {
            if (!Stats.ready)
            {
                return;
            }

            if (!Stats.statToString.TryGetValue(stat, out string str))
            {
                Stats.statToString.Add(stat, str = stat.ToString());
            }

            if (!SteamUserStats.GetStat(str, out int num))
            {
                return;
            }

            _ = SteamUserStats.SetStat(str, num + increment);
        }

        public static int Local(Stat stat)
        {
            int num = 0;
            if (Stats.ready)
            {
                if (!Stats.statToString.TryGetValue(stat, out string str))
                {
                    Stats.statToString.Add(stat, str = stat.ToString());
                }

                _ = SteamUserStats.GetStat(str, out num);
            }
            return num;
        }

        public static long Global(Stat stat)
        {
            long num = 0;
            if (Stats.ready)
            {
                if (!Stats.statToString.TryGetValue(stat, out string str))
                {
                    Stats.statToString.Add(stat, str = stat.ToString());
                }

                _ = SteamUserStats.GetGlobalStat(str, out num);
            }
            return num;
        }

        public static void Store()
        {
            if (!Stats.ready)
            {
                return;
            }

            _ = SteamUserStats.StoreStats();
        }

        public static string Name(Stat stat)
        {
            return Dialog.Clean("STAT_" + stat.ToString());
        }
    }
}
