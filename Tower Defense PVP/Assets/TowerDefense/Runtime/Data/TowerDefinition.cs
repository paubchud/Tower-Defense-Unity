using UnityEngine;

namespace TowerDefense.Data
{
    [CreateAssetMenu(menuName = "Tower Defense/Tower")]
    public sealed class TowerDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Color Tint = Color.white;
    }
}
