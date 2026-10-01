using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Presentation
{
    public static class HeroModel
    {
        public static Transform Create(Transform owner, HeroClassDefinition definition, Material template)
        {
            var root = new GameObject("Class model").transform;
            root.SetParent(owner, false);
            WorldGeometry.Shape(root, "Body", PrimitiveType.Capsule, new Vector3(0, 0.95f, 0),
                new Vector3(0.65f, 0.65f, 0.55f), definition.BodyColor, template);
            WorldGeometry.Shape(root, "Head", PrimitiveType.Sphere, new Vector3(0, 1.85f, 0),
                Vector3.one * 0.46f, new Color(0.8f, 0.65f, 0.48f), template);
            WorldGeometry.Shape(root, "Boot L", PrimitiveType.Cube, new Vector3(-0.2f, 0.17f, 0.08f),
                new Vector3(0.24f, 0.34f, 0.42f), new Color(0.14f, 0.17f, 0.2f), template);
            WorldGeometry.Shape(root, "Boot R", PrimitiveType.Cube, new Vector3(0.2f, 0.17f, 0.08f),
                new Vector3(0.24f, 0.34f, 0.42f), new Color(0.14f, 0.17f, 0.2f), template);
            if (definition.StartingArmor != null)
                WorldGeometry.Shape(root, "Starter clothing", PrimitiveType.Cube, new Vector3(0, 1.25f, 0),
                    new Vector3(0.78f, 0.48f, 0.55f), definition.StartingArmor.Tint, template);
            WorldGeometry.Shape(root, "Class marker", PrimitiveType.Cylinder, new Vector3(0, 0.04f, 0),
                new Vector3(1.25f, 0.02f, 1.25f), definition.AccentColor, template);
            return root;
        }

        public static void SetHeldItem(Transform model, ItemDefinition item, Material template)
        {
            var old = model.Find("Held item");
            if (old != null) { old.gameObject.SetActive(false); Object.Destroy(old.gameObject); }
            if (item == null) return;
            var root = new GameObject("Held item").transform;
            root.SetParent(model, false);
            root.localPosition = new Vector3(0.48f, 0.9f, 0.2f);
            WorldGeometry.Shape(root, "Handle", PrimitiveType.Cube, new Vector3(0, 0.25f, 0),
                new Vector3(0.1f, 0.65f, 0.1f), new Color(0.32f, 0.2f, 0.12f), template);
            switch (item.Kind)
            {
                case ItemKind.MeleeWeapon:
                    WorldGeometry.Shape(root, "Blade", PrimitiveType.Cube, new Vector3(0, 0.8f, 0), new Vector3(0.16f, 0.8f, 0.09f), item.Tint, template);
                    WorldGeometry.Shape(root, "Guard", PrimitiveType.Cube, new Vector3(0, 0.4f, 0), new Vector3(0.45f, 0.08f, 0.12f), item.Tint, template);
                    break;
                case ItemKind.RangedWeapon:
                    WorldGeometry.Shape(root, "Focus", PrimitiveType.Sphere, new Vector3(0, 0.65f, 0), Vector3.one * 0.3f, item.Tint, template);
                    break;
                case ItemKind.HarvestTool:
                    WorldGeometry.Shape(root, "Tool head", PrimitiveType.Cube, new Vector3(0, 0.6f, 0), new Vector3(0.65f, 0.14f, 0.14f), item.Tint, template);
                    break;
            }
        }
    }
}
