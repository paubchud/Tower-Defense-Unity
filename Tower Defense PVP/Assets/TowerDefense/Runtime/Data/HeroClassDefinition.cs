using UnityEngine;

namespace TowerDefense.Data
{
    [CreateAssetMenu(menuName = "Tower Defense/Hero Class")]
    public sealed class HeroClassDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public string Difficulty = "Beginner";
        public string UnlockPrerequisite;
        public TechnologyGroupDefinition TechnologyGroup;
        public Color BodyColor = Color.white;
        public Color AccentColor = Color.yellow;
        public float MovementSpeed = 6;
        public float MaxHealth = 100;
        public float AttackDamage = 26;
        public float AttackRange = 2.8f;
        public float AttackInterval = 0.6f;
        public AttackEnergyCost[] AttackCosts;
        public ItemDefinition StartingWeapon;
        public ItemDefinition StartingTool;
        public ItemDefinition StartingArmor;
        public StartingEnergy[] Energies;
    }
}
