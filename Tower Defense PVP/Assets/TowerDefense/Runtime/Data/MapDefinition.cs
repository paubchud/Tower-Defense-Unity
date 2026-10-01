using UnityEngine;

namespace TowerDefense.Data
{
    [System.Serializable]
    public struct LaneDefinition
    {
        public string Id;
        public Vector3 Entry;
        public Vector3 Castle;
        public Vector3 HeroSpawn;
        public Color SideColor;

        public Vector3 Evaluate(float progress) => Vector3.Lerp(Entry, Castle, Mathf.Clamp01(progress));
    }

    [CreateAssetMenu(menuName = "Tower Defense/Map")]
    public sealed class MapDefinition : ScriptableObject
    {
        public string Id;
        public Vector2 HalfSize = new Vector2(24, 25);
        public LaneDefinition[] Lanes;
        public Vector3[] Plots;
        public Vector3[] ResourceNodes;

        public Vector3 ClampHero(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, -HalfSize.x + 1, HalfSize.x - 1);
            position.z = Mathf.Clamp(position.z, -HalfSize.y + 1, HalfSize.y - 1);
            return position;
        }
    }
}
