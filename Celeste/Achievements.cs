using Steamworks;

namespace Celeste
{
    public static class Achievements
    {
        public static string ID(Achievement achievement) => achievement.ToString();

        public static bool Has(Achievement achievement)
        {
            bool flag;
            return SteamUserStats.GetAchievement(Achievements.ID(achievement), out flag) & flag;
        }

        public static void Register(Achievement achievement)
        {
            if (Achievements.Has(achievement))
                return;
            SteamUserStats.SetAchievement(Achievements.ID(achievement));
            Stats.Store();
        }
    }
}
