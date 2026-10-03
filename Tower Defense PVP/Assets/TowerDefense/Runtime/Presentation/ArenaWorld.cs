using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Presentation
{
    public sealed class ArenaWorld : MonoBehaviour
    {
        public ContentCatalog Catalog;
        private static readonly Color Stone = new Color(0.43f, 0.49f, 0.55f);
        private void Awake()
        {
            if (transform.childCount == 0) Build();
            if (GetComponent<CombatWorld>() == null) gameObject.AddComponent<CombatWorld>().Catalog = Catalog;
        }

        public void Build()
        {
            var map = Catalog.TestMap;
            WorldGeometry.Shape(transform, "Ground", PrimitiveType.Cube, new Vector3(0, -0.6f, 0),
                new Vector3(map.HalfSize.x * 2, 1.2f, map.HalfSize.y * 2), new Color(0.24f, 0.38f, 0.28f), Catalog.BaseMaterial, true);
            foreach (var lane in map.Lanes)
            {
                var laneRoot = new GameObject(lane.Id).transform; laneRoot.SetParent(transform, false);
                var midpoint = (lane.Entry + lane.Castle) / 2;
                float length = Vector3.Distance(lane.Entry, lane.Castle);
                var path = WorldGeometry.Shape(laneRoot, "Straight path", PrimitiveType.Cube, midpoint + Vector3.up * 0.02f,
                    new Vector3(3.2f, 0.05f, length), new Color(0.65f, 0.6f, 0.45f), Catalog.BaseMaterial);
                path.transform.rotation = Quaternion.LookRotation(lane.Castle - lane.Entry);
                WorldGeometry.Shape(laneRoot, "Entry marker", PrimitiveType.Cylinder, lane.Entry + Vector3.up * 0.09f,
                    new Vector3(3, 0.06f, 3), lane.SideColor, Catalog.BaseMaterial);
                Castle(laneRoot, lane.Castle, lane.SideColor);
            }
            for (int i = 0; i < map.Plots.Length; i++)
            {
                Vector3 p = map.Plots[i];
                WorldGeometry.Shape(transform, "Build plot " + i, PrimitiveType.Cube, p + Vector3.up * 0.06f,
                    new Vector3(3.8f, 0.1f, 3.8f), new Color(0.43f, 0.45f, 0.32f), Catalog.BaseMaterial);
                for (int edge = -1; edge <= 1; edge += 2)
                {
                    WorldGeometry.Shape(transform, "Plot border", PrimitiveType.Cube, p + new Vector3(edge * 1.9f, 0.13f, 0), new Vector3(0.1f, 0.1f, 3.8f), new Color(0.86f, 0.75f, 0.42f), Catalog.BaseMaterial);
                    WorldGeometry.Shape(transform, "Plot border", PrimitiveType.Cube, p + new Vector3(0, 0.13f, edge * 1.9f), new Vector3(3.8f, 0.1f, 0.1f), new Color(0.86f, 0.75f, 0.42f), Catalog.BaseMaterial);
                }
            }
            for (int i = 0; i < map.ResourceNodes.Length; i++)
            {
                var p = map.ResourceNodes[i];
                WorldGeometry.Shape(transform, "Resource rock " + i, PrimitiveType.Sphere, p + Vector3.up * 0.7f, new Vector3(1.7f, 1.4f, 1.6f), Stone, Catalog.BaseMaterial, true);
                WorldGeometry.Shape(transform, "Ore marker " + i, PrimitiveType.Cube, p + Vector3.up * 1.3f, Vector3.one * 0.42f, new Color(0.25f, 0.72f, 0.8f), Catalog.BaseMaterial);
            }
            WorldGeometry.Shape(transform, "Center crossing", PrimitiveType.Cube, new Vector3(0, 0.02f, 0), new Vector3(18, 0.04f, 3.5f), new Color(0.32f, 0.42f, 0.36f), Catalog.BaseMaterial);
        }

        private void Castle(Transform parent, Vector3 position, Color color)
        {
            WorldGeometry.Shape(parent, "Castle keep", PrimitiveType.Cube, position + Vector3.up * 2, new Vector3(3.8f, 4, 3.8f), Stone, Catalog.BaseMaterial, true);
            WorldGeometry.Shape(parent, "Castle banner", PrimitiveType.Cube, position + new Vector3(0, 2.8f, -1.94f), new Vector3(1.2f, 1.8f, 0.1f), color, Catalog.BaseMaterial);
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                var p = position + new Vector3(x * 2.4f, 2.5f, z * 2.4f);
                WorldGeometry.Shape(parent, "Castle turret", PrimitiveType.Cylinder, p, new Vector3(1.5f, 2.5f, 1.5f), Stone, Catalog.BaseMaterial, true);
                WorldGeometry.Shape(parent, "Turret cap", PrimitiveType.Cylinder, p + Vector3.up * 2.7f, new Vector3(1.7f, 0.25f, 1.7f), color, Catalog.BaseMaterial);
            }
        }
    }
}
