using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Presentation
{
    public static class WorldGeometry
    {
        private static readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();

        public static Material MaterialFor(Color color, Material template)
        {
            if (materials.TryGetValue(color, out var existing) && existing != null) return existing;
            var material = new Material(template) { color = color, name = "Prototype " + ColorUtility.ToHtmlStringRGB(color) };
            materials[color] = material;
            return material;
        }

        public static GameObject Shape(Transform parent, string name, PrimitiveType primitive,
            Vector3 position, Vector3 scale, Color color, Material template, bool solid = false)
        {
            var obj = GameObject.CreatePrimitive(primitive);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = MaterialFor(color, template);
            var collider = obj.GetComponent<Collider>();
            if (!solid && collider != null)
            {
                collider.enabled = false;
                if (Application.isPlaying) UnityEngine.Object.Destroy(collider);
                else UnityEngine.Object.DestroyImmediate(collider);
            }
            return obj;
        }

        public static void ClearMaterials()
        {
            foreach (var material in materials.Values)
                if (material != null) UnityEngine.Object.Destroy(material);
            materials.Clear();
        }
    }
}
