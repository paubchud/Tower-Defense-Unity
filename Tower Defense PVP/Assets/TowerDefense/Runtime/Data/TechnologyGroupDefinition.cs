using UnityEngine;

namespace TowerDefense.Data
{
    [CreateAssetMenu(menuName = "Tower Defense/Technology Group")]
    public sealed class TechnologyGroupDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public TowerDefinition[] Towers;

        public bool ContainsTower(string id)
        {
            if (Towers == null) return false;
            foreach (var tower in Towers)
                if (tower != null && tower.Id == id) return true;
            return false;
        }
    }
}
