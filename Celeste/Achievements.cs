using Steamworks;

namespace Celeste
{
    public static class Achievements
    {
        public static string ID(Achievement achievement) => achievement.ToString();

        public static bool Has(Achievement achievement)
        {
            return SteamUserStats.GetAchievement(ID(achievement), out bool flag) & flag;
        }

        public static void Register(Achievement achievement)
        {
#if ENABLE_STEAM
            if (Has(achievement))
                return;
            SteamUserStats.SetAchievement(ID(achievement));
            Stats.Store();
#endif
        }
    }
}
