using TowerDefense.Data;
using TowerDefense.Networking;
using UnityEngine;

namespace TowerDefense.Presentation
{
    // Bounded cosmetic site views. Private node reserves are read only from the owning hero.
    public sealed class EconomyWorld : MonoBehaviour
    {
        public ContentCatalog Catalog;
        private sealed class SiteView
        {
            public Transform Tower;
            public LineRenderer Beam;
            public TextMesh Label;
            public string TowerId;
            public uint Round;
        }
        private SiteView[] plots, nodes;
        private float nextLabels;
        private void Start()
        {
            plots = new SiteView[Catalog.TestMap.Plots.Length]; nodes = new SiteView[Catalog.TestMap.ResourceNodes.Length];
            for (int i = 0; i < plots.Length; i++) plots[i] = Label("Plot " + (i + 1), Catalog.TestMap.Plots[i] + Vector3.up * 3.4f);
            for (int i = 0; i < nodes.Length; i++) nodes[i] = Label("Stone " + (i + 1), Catalog.TestMap.ResourceNodes[i] + Vector3.up * 2.6f);
        }
        private SiteView Label(string name, Vector3 position)
        {
            var obj = new GameObject(name, typeof(TextMesh)); obj.transform.SetParent(transform, false); obj.transform.position = position;
            var label = obj.GetComponent<TextMesh>(); label.text = name; label.fontSize = 40; label.characterSize = 0.055f;
            label.anchor = TextAnchor.MiddleCenter; label.color = Color.white;
            return new SiteView { Label = label };
        }
        private void Update()
        {
            var session = PrototypeSession.Instance;
            if (session == null || plots == null) return;
            // Keep world labels out of management text, including camera-rendered QA captures.
            bool showLabels = !session.Controls.BlockGameplay;
            foreach (var view in plots) view.Label.gameObject.SetActive(showLabels);
            foreach (var view in nodes) view.Label.gameObject.SetActive(showLabels);
            bool refresh = Time.unscaledTime >= nextLabels;
            if (refresh) nextLabels = Time.unscaledTime + 0.25f;
            foreach (var view in plots)
            {
                if (view.Tower != null) view.Tower.gameObject.SetActive(false);
                if (refresh) view.Label.text = view.Label.gameObject.name;
            }
            if (refresh) foreach (var view in nodes) view.Label.text = view.Label.gameObject.name;
            foreach (var hero in session.Heroes)
            {
                if (!hero.IsSpawned || !hero.Running.Value || hero.Definition == null) continue;
                foreach (var state in hero.Plots)
                {
                    var view = plots[state.Id];
                    if (refresh) view.Label.text = $"Plot {state.Id + 1}\n" + (!state.Owned ? "For sale" : state.TowerIndex < 0 ? "Owned / empty" : "Built");
                    if (state.TowerIndex < 0) continue;
                    var tower = hero.Definition.TechnologyGroup.Towers[state.TowerIndex];
                    if (view.Tower == null || view.TowerId != tower.Id || view.Round != hero.Round.Value)
                    {
                        if (view.Tower != null) Destroy(view.Tower.gameObject);
                        view.Tower = new GameObject(tower.DisplayName + " (built)").transform; view.Tower.SetParent(transform, false);
                        view.Tower.position = Catalog.TestMap.Plots[state.Id]; view.TowerId = tower.Id; view.Round = hero.Round.Value;
                        WorldGeometry.Shape(view.Tower, "Base", PrimitiveType.Cylinder, Vector3.up, new Vector3(1.7f, 1, 1.7f), tower.Tint, Catalog.BaseMaterial);
                        WorldGeometry.Shape(view.Tower, "Focus", PrimitiveType.Sphere, Vector3.up * 2.4f, Vector3.one, tower.Tint, Catalog.BaseMaterial);
                        view.Beam = view.Tower.gameObject.AddComponent<LineRenderer>(); view.Beam.positionCount = 2;
                        view.Beam.startWidth = view.Beam.endWidth = 0.1f; view.Beam.sharedMaterial = WorldGeometry.MaterialFor(tower.Tint, Catalog.BaseMaterial);
                    }
                    view.Tower.gameObject.SetActive(true);
                    view.Beam.enabled = hero.Phase.Value == Core.CombatPhase.Playing && hero.CombatNow - state.AttackAt < 0.15;
                    if (view.Beam.enabled) { view.Beam.SetPosition(0, view.Tower.position + Vector3.up * 2.4f); view.Beam.SetPosition(1, state.AttackEnd + Vector3.up); }
                }
                if (refresh && hero.IsOwner) foreach (var state in hero.Nodes)
                    nodes[state.Id].Label.text = state.Remaining > 0 ? $"Stone {state.Id + 1}: {state.Remaining}\nPickaxe + E" :
                        $"Depleted: {System.Math.Max(0, state.RecoverAt - hero.CombatNow):0}s";
            }
            if (Camera.main == null) return;
            foreach (var view in plots) view.Label.transform.rotation = Camera.main.transform.rotation;
            foreach (var view in nodes) view.Label.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
