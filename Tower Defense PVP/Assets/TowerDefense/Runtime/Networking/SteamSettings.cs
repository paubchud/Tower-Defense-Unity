using UnityEngine;

namespace TowerDefense.Networking
{
    [CreateAssetMenu(menuName = "Tower Defense/Steam Settings")]
    public sealed class SteamSettings : ScriptableObject
    {
        [Tooltip("Your game's Steamworks App ID. Zero disables Steam online play until configured.")]
        public uint AppId;
    }
}
