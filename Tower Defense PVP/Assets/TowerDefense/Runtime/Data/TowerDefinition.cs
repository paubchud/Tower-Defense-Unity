using UnityEngine;

namespace TowerDefense.Data
{
    [CreateAssetMenu(menuName = "Tower Defense/Tower")]
    public sealed class TowerDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Color Tint = Color.white;
        public float Damage = 14;
        public float Range = 7;
        public float AttackInterval = 0.9f;
    }
}
