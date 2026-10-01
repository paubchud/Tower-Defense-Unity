using UnityEngine;

namespace TowerDefense.Data
{
    public enum ItemKind { MeleeWeapon, RangedWeapon, HarvestTool, Armor }

    [CreateAssetMenu(menuName = "Tower Defense/Item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public ItemKind Kind;
        public Color Tint = Color.white;
    }
}
