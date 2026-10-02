using UnityEngine;

namespace TowerDefense.Networking
{
    [CreateAssetMenu(menuName = "Tower Defense/Steam Settings")]
    public sealed class SteamSettings : ScriptableObject
    {
        [Tooltip("Your game's Steamworks App ID. Zero disables Steam online play until configured.")]
        public uint AppId;
    }

    public static class SteamLaunchPolicy
    {
        public static uint ResolveAppId(uint configuredAppId, bool developmentBuild, bool privateRequested)
        {
            if (privateRequested)
            {
                if (!developmentBuild) throw new SteamOnlineException("Private Steam testing is available only in development builds.");
                return 480;
            }
            if (configuredAppId == 0) throw new SteamOnlineException("Steam is not configured yet. Enable Private Steam Test for friends-only development play, or configure your own App ID. LAN testing still works.");
            if (configuredAppId == 480) throw new SteamOnlineException("App ID 480 requires explicit private test mode. Configure your game's own Steamworks App ID for normal play.");
            return configuredAppId;
        }
    }
}
