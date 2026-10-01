using UnityEngine;

namespace TowerDefense.Data
{
    [CreateAssetMenu(menuName = "Tower Defense/Energy Type")]
    public sealed class EnergyDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Color Tint = Color.cyan;
    }

    [System.Serializable]
    public struct StartingEnergy
    {
        public EnergyDefinition Definition;
        public float Capacity;
        public float Initial;
        public float RecoveryPerSecond;
    }
}
