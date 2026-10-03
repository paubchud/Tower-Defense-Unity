using UnityEngine;

namespace TowerDefense.Data
{
    [CreateAssetMenu(menuName = "Tower Defense/Content Catalog")]
    public sealed class ContentCatalog : ScriptableObject
    {
        public HeroClassDefinition[] Classes;
        public MapDefinition TestMap;
        public Material BaseMaterial;
        public CombatRulesDefinition CombatRules;

        public HeroClassDefinition FindClass(string id)
        {
            if (Classes == null) return null;
            foreach (var hero in Classes)
                if (hero != null && hero.Id == id) return hero;
            return null;
        }
    }
}
