using System;
using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Core
{
    public enum EconomyAction : byte { BuyPlot, BuildTower, SellTower, SellPlot, LevelHero, Harvest }
    public sealed class PlotState
    {
        public readonly int Id, Side;
        public bool Owned { get; internal set; }
        public int TowerIndex { get; internal set; } = -1;
        public double LastAttack { get; internal set; } = -100;
        public Vector3 AttackEnd { get; internal set; }
        internal double NextAttack;
        internal PlotState(int id, int side) { Id = id; Side = side; }
    }
    public sealed class NodeState
    {
        public readonly int Id, Side;
        public int Remaining { get; internal set; }
        public double RecoverAt { get; internal set; }
        internal NodeState(int id, int side, int remaining) { Id = id; Side = side; Remaining = remaining; }
    }
    // All economy state belongs to this match, not shared content assets or client UI.
    public sealed class EconomyMatch
    {
        public readonly EconomyRulesDefinition Rules;
        public readonly PlotState[] Plots;
        public readonly NodeState[] Nodes;
        private readonly CombatMatch match;

        internal EconomyMatch(CombatMatch combat, EconomyRulesDefinition rules)
        {
            match = combat; Rules = rules; rules.Validate();
            ValidateSites(match.Map.ResourceNodes, match.Map.NodeSides, false);
            ValidateSites(match.Map.Plots, match.Map.PlotSides, true);
            foreach (var player in match.Players)
            {
                foreach (var tower in player.Definition.TechnologyGroup.Towers)
                    if (tower == null || string.IsNullOrWhiteSpace(tower.Id) || !float.IsFinite(tower.Damage) || tower.Damage <= 0 ||
                        !float.IsFinite(tower.Range) || tower.Range <= 0 || !float.IsFinite(tower.AttackInterval) || tower.AttackInterval <= 0)
                        throw new ArgumentException("Invalid group tower catalog.");
                if (!float.IsFinite(player.Definition.MaxHealth + rules.HeroHealthBonus)) throw new ArgumentException("Hero health overflow.");
                player.Gold = rules.StartingGold;
            }
            Plots = new PlotState[match.Map.Plots.Length];
            for (int i = 0; i < Plots.Length; i++) Plots[i] = new PlotState(i, match.Map.PlotSides[i]);
            Nodes = new NodeState[match.Map.ResourceNodes.Length];
            for (int i = 0; i < Nodes.Length; i++) Nodes[i] = new NodeState(i, match.Map.NodeSides[i], rules.NodeCapacity);
        }

        private void ValidateSites(Vector3[] sites, int[] sides, bool plots)
        {
            if (sites == null || sides == null || sites.Length != sides.Length || sites.Length == 0 || sites.Length > 32)
                throw new ArgumentException("Author site positions and side ownership together (1..32).");
            for (int i = 0; i < sites.Length; i++)
            {
                var site = sites[i];
                if (sides[i] < 0 || sides[i] >= match.Players.Length || !float.IsFinite(site.x) || !float.IsFinite(site.y) ||
                    !float.IsFinite(site.z) || Math.Abs(site.x) > match.Map.HalfSize.x - 2 || Math.Abs(site.z) > match.Map.HalfSize.y - 2)
                    throw new ArgumentException("Invalid economy site/side.");
                for (int j = 0; j < i; j++) if (FlatDistance(site, sites[j]) < 4) throw new ArgumentException("Overlapping economy sites.");
                if (!plots) continue;
                foreach (var lane in match.Map.Lanes)
                {
                    Vector3 path = lane.Castle - lane.Entry;
                    var closest = lane.Entry + path * Mathf.Clamp01(Vector3.Dot(site - lane.Entry, path) / path.sqrMagnitude);
                    if (FlatDistance(site, closest) < Rules.LaneClearance) throw new ArgumentException("Plot obstructs a lane.");
                }
                foreach (var node in match.Map.ResourceNodes)
                    if (FlatDistance(site, node) < 4) throw new ArgumentException("Plot overlaps a resource node.");
            }
        }

        public bool TryAction(string id, uint generation, uint sequence, EconomyAction action, int target, int towerIndex, out string reason)
        {
            if (!match.Admit(id, generation, sequence, out var player, out reason)) return false;
            if (action == EconomyAction.Harvest) return StartHarvest(player, target, sequence, out reason);
            if (action == EconomyAction.LevelHero)
            {
                if (player.HeroLevel != 0) { reason = "Hero growth is already purchased."; return false; }
                if (player.Gold < Rules.HeroLevelGold) { reason = "Not enough gold."; return false; }
                player.Gold -= Rules.HeroLevelGold; player.HeroLevel = 1; player.HealthBonus = Rules.HeroHealthBonus;
                // No heal/resurrection exploit: extra maximum HP is filled on the next respawn only.
                match.Record(player, sequence, "hero-level", 0, -Rules.HeroLevelGold);
                reason = "Maximum HP increased; no instant healing. Resets next match."; return true;
            }
            if (target < 0 || target >= Plots.Length || Plots[target].Side != player.Side)
            { reason = "Choose a plot on your own side."; return false; }
            var plot = Plots[target];
            switch (action)
            {
                case EconomyAction.BuyPlot:
                    if (plot.Owned) { reason = "Plot already owned."; return false; }
                    if (player.Gold < Rules.PlotGold) { reason = "Not enough gold."; return false; }
                    player.Gold -= Rules.PlotGold; plot.Owned = true;
                    match.Record(player, sequence, "buy-plot", 0, -Rules.PlotGold);
                    reason = "Land purchased. Build your group tower with stone."; return true;
                case EconomyAction.BuildTower:
                    if (!plot.Owned || plot.TowerIndex >= 0) { reason = "Buy empty land first."; return false; }
                    if (towerIndex < 0 || towerIndex >= player.Definition.TechnologyGroup.Towers.Length)
                    { reason = "Choose a tower from your technology group."; return false; }
                    if (player.Stone < Rules.TowerStone) { reason = "Not enough stone."; return false; }
                    player.Stone -= Rules.TowerStone; plot.TowerIndex = towerIndex; plot.NextAttack = match.Time; plot.LastAttack = -100;
                    match.Record(player, sequence, "build-tower", 0, 0, -Rules.TowerStone);
                    reason = "Tower built."; return true;
                case EconomyAction.SellTower:
                    if (!plot.Owned || plot.TowerIndex < 0) { reason = "No owned tower here."; return false; }
                    if (player.Stone > Rules.StoneCapacity - Rules.TowerRefundStone)
                    { reason = "Make room for the stone refund first."; return false; }
                    player.Stone += Rules.TowerRefundStone; plot.TowerIndex = -1; plot.LastAttack = -100;
                    match.Record(player, sequence, "sell-tower", 0, 0, Rules.TowerRefundStone);
                    reason = "Tower sold for a partial stone refund."; return true;
                case EconomyAction.SellPlot:
                    if (!plot.Owned || plot.TowerIndex >= 0) { reason = "Sell the tower before selling owned land."; return false; }
                    if (player.Gold > int.MaxValue - Rules.PlotRefundGold) { reason = "Gold capacity reached."; return false; }
                    player.Gold += Rules.PlotRefundGold; plot.Owned = false;
                    match.Record(player, sequence, "sell-plot", 0, Rules.PlotRefundGold);
                    reason = "Land sold for a partial gold refund."; return true;
                default: reason = "Unknown economy action."; return false;
            }
        }

        private bool StartHarvest(CombatPlayer player, int target, uint sequence, out string reason)
        {
            if (target < 0 || target >= Nodes.Length || Nodes[target].Side != player.Side)
            { reason = "Harvest only your own resource nodes."; return false; }
            var node = Nodes[target];
            if (!CanHarvest(player, node)) { reason = "Alive, pickaxe selected, and within harvest range required."; return false; }
            if (player.HarvestNode >= 0) { reason = "Mining is already in progress."; return false; }
            int yield = Math.Min(Rules.HarvestYield, node.Remaining);
            if (yield == 0) { reason = "Node depleted; wait for recovery."; return false; }
            if (player.Stone > Rules.StoneCapacity - yield) { reason = "Stone inventory is full."; return false; }
            player.HarvestNode = target; player.HarvestEnds = match.Time + Rules.HarvestSeconds;
            match.Record(player, sequence, "harvest-start", 0, 0);
            reason = "Mining started. Stay nearby with your pickaxe selected."; return true;
        }

        private bool CanHarvest(CombatPlayer player, NodeState node) => player.CanCollect && player.SelectedSlot == 1 &&
            player.Definition.StartingTool != null && player.Definition.StartingTool.Kind == ItemKind.HarvestTool &&
            FlatDistance(player.Position, match.Map.ResourceNodes[node.Id]) <= Rules.HarvestRange;

        internal void Step()
        {
            if (match.Phase != CombatPhase.Playing)
            { foreach (var player in match.Players) Cancel(player); return; }
            foreach (var node in Nodes)
                if (node.Remaining == 0 && match.Time >= node.RecoverAt) { node.Remaining = Rules.NodeCapacity; node.RecoverAt = 0; }
            foreach (var player in match.Players)
            {
                if (player.HarvestNode < 0) continue;
                var node = Nodes[player.HarvestNode];
                if (!CanHarvest(player, node)) { Cancel(player); continue; }
                if (match.Time < player.HarvestEnds) continue;
                int yield = Math.Min(Rules.HarvestYield, node.Remaining);
                if (yield > 0 && player.Stone <= Rules.StoneCapacity - yield)
                {
                    node.Remaining -= yield; player.Stone += yield;
                    if (node.Remaining == 0) node.RecoverAt = match.Time + Rules.NodeRecoverySeconds;
                    match.Record(player, 0, "harvest-complete", 0, 0, yield);
                }
                Cancel(player);
            }
        }
        internal void FireTowers()
        {
            foreach (var plot in Plots)
            {
                if (plot.TowerIndex < 0 || match.Time < plot.NextAttack) continue;
                var owner = match.Players[plot.Side];
                var tower = owner.Definition.TechnologyGroup.Towers[plot.TowerIndex];
                CombatTroop target = null;
                foreach (var troop in match.Troops)
                    if (troop.Lane == plot.Side && FlatDistance(match.Position(troop), match.Map.Plots[plot.Id]) <= tower.Range &&
                        (target == null || troop.SpawnAt < target.SpawnAt)) target = troop;
                if (target == null) continue;
                plot.NextAttack = match.Time + tower.AttackInterval; plot.LastAttack = match.Time; plot.AttackEnd = match.Position(target);
                match.DamageTroop(target, owner, tower.Damage);
            }
        }
        private static void Cancel(CombatPlayer player) { player.HarvestNode = -1; player.HarvestEnds = 0; }
        private static float FlatDistance(Vector3 a, Vector3 b) { a.y = b.y = 0; return Vector3.Distance(a, b); }
    }
}
