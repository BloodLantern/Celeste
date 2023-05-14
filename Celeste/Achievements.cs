// Decompiled with JetBrains decompiler
// Type: Celeste.Achievements
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Steamworks;

namespace Celeste
{
    public static class Achievements
    {
        public static string ID(Achievement achievement)
        {
            return achievement.ToString();
        }

        public static bool Has(Achievement achievement)
        {
            return SteamUserStats.GetAchievement(ID(achievement), out bool flag) & flag;
        }

        public static void Register(Achievement achievement)
        {
            if (Has(achievement))
            {
                return;
            }

            _ = SteamUserStats.SetAchievement(ID(achievement));
            Stats.Store();
        }
    }
}
