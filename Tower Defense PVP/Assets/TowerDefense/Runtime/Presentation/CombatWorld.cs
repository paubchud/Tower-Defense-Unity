using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Data;
using TowerDefense.Networking;
using UnityEngine;

namespace TowerDefense.Presentation
{
    // Cosmetic geometry only: the host rules choose hits, rewards and lane progress.
    public sealed class CombatWorld : MonoBehaviour
    {
        public ContentCatalog Catalog;
        private sealed class UnitView { public Transform Root, Bar; public ulong Key; }
        private sealed class TowerView { public Transform Root; public LineRenderer Beam; public string Class; public uint Round; }
        private readonly Dictionary<ulong, UnitView> units = new Dictionary<ulong, UnitView>();
        private readonly Stack<UnitView> pool = new Stack<UnitView>();
        private readonly HashSet<ulong> visible = new HashSet<ulong>();
        private readonly List<ulong> removed = new List<ulong>();
        private readonly TowerView[] towers = new TowerView[2];

        private void Update()
        {
            var session = PrototypeSession.Instance;
            if (session == null || Catalog == null || Catalog.CombatRules == null) return;
            visible.Clear();
            foreach (var hero in session.Heroes)
            {
                if (hero == null || !hero.IsSpawned || !hero.Running.Value || hero.Definition == null) continue;
                int side = hero.Side.Value;
                var lane = Catalog.TestMap.Lanes[side];
                ShowTower(hero, lane);
                for (int i = 0; i < hero.Incoming.Count; i++)
                {
                    var state = hero.Incoming[i];
                    ulong key = ((ulong)hero.Round.Value << 32) | state.Id;
                    visible.Add(key);
                    if (!units.TryGetValue(key, out var unit))
                    {
                        unit = pool.Count > 0 ? pool.Pop() : CreateUnit();
                        unit.Key = key; unit.Root.gameObject.SetActive(true); units.Add(key, unit);
                        // Material is shared, never allocated per hit or per frame.
                        foreach (var renderer in unit.Root.GetComponentsInChildren<Renderer>())
                            if (renderer.transform != unit.Bar)
                                renderer.sharedMaterial = WorldGeometry.MaterialFor(lane.SideColor, Catalog.BaseMaterial);
                    }
                    float progress = (float)System.Math.Max(0, hero.CombatNow - state.SpawnAt) * Catalog.CombatRules.TroopSpeed / Vector3.Distance(lane.Entry, lane.Castle);
                    unit.Root.position = lane.Evaluate(progress);
                    unit.Root.rotation = Quaternion.LookRotation(lane.Castle - lane.Entry);
                    float fraction = Mathf.Clamp01(state.Health / Catalog.CombatRules.HealthAt(state.Level));
                    unit.Bar.localScale = new Vector3(1.2f * fraction, 0.1f, 0.14f);
                    unit.Bar.localPosition = new Vector3((fraction - 1) * 0.6f, 1.9f, 0);
                }
            }
            removed.Clear();
            foreach (var pair in units) if (!visible.Contains(pair.Key)) removed.Add(pair.Key);
            foreach (ulong key in removed)
            { var unit = units[key]; unit.Root.gameObject.SetActive(false); pool.Push(unit); units.Remove(key); }
            for (int side = 0; side < 2; side++)
            {
                bool active = false;
                foreach (var hero in session.Heroes) if (hero != null && hero.IsSpawned && hero.Running.Value && hero.Side.Value == side) active = true;
                if (!active && towers[side] != null) towers[side].Root.gameObject.SetActive(false);
            }
        }

        private UnitView CreateUnit()
        {
            var root = new GameObject("Raider (pooled cosmetic)").transform; root.SetParent(transform, false);
            WorldGeometry.Shape(root, "Body", PrimitiveType.Capsule, Vector3.up * 0.75f, new Vector3(0.8f, 0.75f, 0.8f), Color.gray, Catalog.BaseMaterial);
            WorldGeometry.Shape(root, "Head", PrimitiveType.Sphere, Vector3.up * 1.45f, Vector3.one * 0.5f, Color.gray, Catalog.BaseMaterial);
            var bar = WorldGeometry.Shape(root, "Health", PrimitiveType.Cube, Vector3.up * 1.9f, new Vector3(1.2f, 0.1f, 0.14f), Color.green, Catalog.BaseMaterial).transform;
            return new UnitView { Root = root, Bar = bar };
        }

        private void ShowTower(NetworkHero hero, LaneDefinition lane)
        {
            int side = hero.Side.Value; var view = towers[side];
            if (view == null || view.Class != hero.Definition.Id || view.Round != hero.Round.Value)
            {
                if (view != null) Destroy(view.Root.gameObject);
                var tower = hero.Definition.TechnologyGroup.Towers[0];
                var root = new GameObject(tower.DisplayName).transform; root.SetParent(transform, false);
                Vector3 right = Vector3.Cross(Vector3.up, (lane.Castle - lane.Entry).normalized);
                root.position = lane.Evaluate(0.65f) + right * (side == 0 ? -3.5f : 3.5f);
                WorldGeometry.Shape(root, "Pedestal", PrimitiveType.Cylinder, Vector3.up, new Vector3(1.7f, 1, 1.7f), tower.Tint, Catalog.BaseMaterial);
                WorldGeometry.Shape(root, "Focus", hero.Definition.TechnologyGroup.Id == "mystic" ? PrimitiveType.Sphere : PrimitiveType.Cube,
                    Vector3.up * 2.4f, Vector3.one * 1.1f, tower.Tint, Catalog.BaseMaterial);
                var beam = root.gameObject.AddComponent<LineRenderer>(); beam.positionCount = 2; beam.startWidth = beam.endWidth = 0.1f;
                beam.sharedMaterial = WorldGeometry.MaterialFor(tower.Tint, Catalog.BaseMaterial);
                towers[side] = view = new TowerView { Root = root, Beam = beam, Class = hero.Definition.Id, Round = hero.Round.Value };
            }
            view.Root.gameObject.SetActive(true);
            view.Beam.enabled = hero.Phase.Value == CombatPhase.Playing && hero.CombatNow - hero.TowerAttackAt.Value < 0.15;
            if (view.Beam.enabled)
            { view.Beam.SetPosition(0, view.Root.position + Vector3.up * 2.4f); view.Beam.SetPosition(1, hero.TowerAttackEnd.Value + Vector3.up); }
        }
    }
}
