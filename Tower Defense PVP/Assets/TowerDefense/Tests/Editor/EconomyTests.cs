using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TowerDefense.Core;
using TowerDefense.Data;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Tests
{
    public sealed class EconomyTests
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private ContentCatalog catalog;
        private EconomyRulesDefinition rules;
        private CombatRulesDefinition combat;
        private MapDefinition map;
        private HeroClassDefinition[] classes;
        private CombatMatch match;
        private uint[] seq;
        private T Clone<T>(T value) where T : UnityEngine.Object { var copy = UnityEngine.Object.Instantiate(value); owned.Add(copy); return copy; }
        [SetUp] public void Setup()
        {
            catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/TowerDefense/Content/PrototypeCatalog.asset");
            rules = Clone(catalog.EconomyRules); combat = Clone(catalog.CombatRules); map = Clone(catalog.TestMap);
            classes = catalog.Classes.Select(Clone).ToArray();
            foreach (var hero in classes)
            {
                hero.TechnologyGroup = Clone(hero.TechnologyGroup);
                var tower = Clone(hero.TechnologyGroup.Towers[0]); tower.Range = 0.01f;
                hero.TechnologyGroup.Towers = new[] { tower };
            }
            Restart();
        }
        private void Restart() { seq = new uint[2]; match = new CombatMatch(combat, map, classes, 1, 0, rules); }
        [TearDown] public void Cleanup() { foreach (var obj in owned) UnityEngine.Object.DestroyImmediate(obj); owned.Clear(); }
        private bool Do(EconomyAction action, int target = 0, int side = 0, int tower = 0) =>
            match.Economy.TryAction(match.Players[side].Id, 1, ++seq[side], action, target, tower, out _);
        private void MineAt(int node, int side = 0)
        { match.UpdateHero(side, map.ResourceNodes[node], 1); Assert.That(Do(EconomyAction.Harvest, node, side), Is.True); }
        private void Advance(float seconds)
        { for (int i = 0; i < Mathf.CeilToInt(seconds / 0.05f); i++) match.Step(0.05f); }

        [Test] public void ShippedSitesAndDefaultsAreValidAndEachSideHasTwoSites()
        {
            var game = new CombatMatch(catalog.CombatRules, catalog.TestMap, catalog.Classes, 1, 0, catalog.EconomyRules);
            foreach (var p in game.Players) { Assert.That(p.Gold, Is.EqualTo(50)); Assert.That(p.Stone, Is.Zero); }
            Assert.That(game.Economy.Plots.Count(p => p.Side == 0), Is.EqualTo(2));
            Assert.That(game.Economy.Nodes.Count(n => n.Side == 1), Is.EqualTo(2));
        }
        [Test] public void CommandsShareCombatSequenceAndRejectOldRoundWrongIdentityAndUnknownActions()
        {
            var id = match.Players[0].Id;
            Assert.That(match.Economy.TryAction("spoof", 1, 1, EconomyAction.BuyPlot, 0, 0, out _), Is.False);
            Assert.That(match.Economy.TryAction(id, 2, 1, EconomyAction.BuyPlot, 0, 0, out _), Is.False);
            Assert.That(match.Economy.TryAction(id, 1, 65, EconomyAction.BuyPlot, 0, 0, out _), Is.False);
            Assert.That(Do(EconomyAction.BuyPlot), Is.True);
            Assert.That(match.TrySend(id, 1, 1, out _), Is.False);
            Assert.That(Do((EconomyAction)255), Is.False);
            Assert.That(match.Players[0].Gold, Is.EqualTo(25));
        }
        [Test] public void PlotPurchaseIsAtomicOwnedAndCannotOverspend()
        {
            Assert.That(Do(EconomyAction.BuyPlot, 2), Is.False);
            Assert.That(Do(EconomyAction.BuyPlot, -1), Is.False);
            Assert.That(Do(EconomyAction.BuyPlot), Is.True);
            Assert.That(Do(EconomyAction.BuyPlot), Is.False);
            Assert.That(Do(EconomyAction.BuyPlot, 1), Is.True);
            Assert.That(Do(EconomyAction.LevelHero), Is.False);
            Assert.That(match.Players[0].Gold, Is.Zero);
            Assert.That(match.Economy.Plots.Count(p => p.Owned), Is.EqualTo(2));
        }
        [Test] public void HarvestNeedsLivingOwnerToolRangeAndCapacity()
        {
            Assert.That(Do(EconomyAction.Harvest, 2), Is.False);
            Assert.That(Do(EconomyAction.Harvest), Is.False);
            match.UpdateHero(0, map.ResourceNodes[0], 0);
            Assert.That(Do(EconomyAction.Harvest), Is.False);
            match.UpdateHero(0, map.ResourceNodes[0], 1); match.Players[0].Stone = rules.StoneCapacity;
            Assert.That(Do(EconomyAction.Harvest), Is.False);
            Assert.That(match.Economy.Nodes[0].Remaining, Is.EqualTo(50));
        }
        [Test] public void HarvestAwardsOnlyAfterDurationAndOnce()
        {
            MineAt(0); Assert.That(Do(EconomyAction.Harvest), Is.False);
            Advance(1); Assert.That(match.Players[0].Stone, Is.Zero);
            Advance(1.1f); Assert.That(match.Players[0].Stone, Is.EqualTo(5));
            Advance(1); Assert.That(match.Players[0].Stone, Is.EqualTo(5));
            Assert.That(match.Players[0].HarvestNode, Is.EqualTo(-1));
            Assert.That(match.Economy.Nodes[0].Remaining, Is.EqualTo(45));
            Assert.That(match.Journal.Where(e => e.Action == "harvest-complete").Sum(e => e.StoneDelta), Is.EqualTo(5));
        }
        [Test] public void LeavingRangeOrChangingToolCancelsWithoutPartialYield()
        {
            MineAt(0); Advance(1); match.UpdateHero(0, Vector3.zero, 1); Advance(1.1f);
            Assert.That(match.Players[0].HarvestNode, Is.EqualTo(-1)); Assert.That(match.Players[0].Stone, Is.Zero);
            MineAt(0); match.UpdateHero(0, map.ResourceNodes[0], 0); Advance(2.1f);
            Assert.That(match.Players[0].Stone, Is.Zero); Assert.That(match.Economy.Nodes[0].Remaining, Is.EqualTo(50));
        }
        [Test] public void SimultaneousOwnedHarvestsRemainIsolated()
        {
            MineAt(0); MineAt(2, 1); Advance(2.1f);
            Assert.That(match.Players.Select(p => p.Stone), Is.EqualTo(new[] { 5, 5 }));
            Assert.That(match.Economy.Nodes.Select(n => n.Remaining), Is.EqualTo(new[] { 45, 50, 45, 50 }));
        }
        [Test] public void DeathOnCompletionTickCancelsBeforeCollection()
        {
            map.ResourceNodes[0] = map.Lanes[0].Evaluate(0.2f); Restart(); MineAt(0);
            Assert.That(match.TrySend(match.Players[1].Id, 1, ++seq[1], out _), Is.True);
            Advance(1.9f);
            var troop = match.Troops.Single(); match.Players[0].Health = 1;
            troop.NextAttack = 0; // The fixture forces the damaging hit on the completion tick, not the previous tick.
            troop.SpawnAt = match.Time + 0.15 - Vector3.Distance(map.Lanes[0].Entry, map.ResourceNodes[0]) / combat.TroopSpeed;
            match.Step(0.15f);
            Assert.That(match.Players[0].Life, Is.EqualTo(HeroLife.Ghost));
            Assert.That(match.Players[0].Stone, Is.Zero); Assert.That(match.Players[0].HarvestNode, Is.EqualTo(-1));
            Assert.That(match.Economy.Nodes[0].Remaining, Is.EqualTo(50));
        }
        [Test] public void FullInventoryAtCompletionDoesNotConsumeNode()
        {
            MineAt(0); match.Players[0].Stone = rules.StoneCapacity; Advance(2.1f);
            Assert.That(match.Economy.Nodes[0].Remaining, Is.EqualTo(50));
            Assert.That(match.Players[0].Stone, Is.EqualTo(rules.StoneCapacity));
        }
        [Test] public void DepletedNodesRecoverOnAuthorityDeadline()
        {
            rules.NodeCapacity = rules.HarvestYield; rules.NodeRecoverySeconds = 3; Restart();
            MineAt(0); Advance(2.1f); Assert.That(match.Economy.Nodes[0].Remaining, Is.Zero);
            Assert.That(Do(EconomyAction.Harvest), Is.False);
            Advance(2.8f); Assert.That(match.Economy.Nodes[0].Remaining, Is.Zero);
            Advance(0.3f); Assert.That(match.Economy.Nodes[0].Remaining, Is.EqualTo(5));
        }
        [Test] public void BuildValidatesOwnedSlotCatalogAndAllMaterialsBeforeMutation()
        {
            match.Players[0].Stone = 40;
            Assert.That(Do(EconomyAction.BuildTower), Is.False); Do(EconomyAction.BuyPlot);
            Assert.That(Do(EconomyAction.BuildTower, tower: 1), Is.False);
            Assert.That(Do(EconomyAction.BuildTower, tower: -1), Is.False);
            Assert.That(match.Players[0].Stone, Is.EqualTo(40));
            Assert.That(Do(EconomyAction.BuildTower), Is.True); Assert.That(Do(EconomyAction.BuildTower), Is.False);
            Assert.That(match.Players[0].Stone, Is.EqualTo(20));
            Assert.That(classes[0].TechnologyGroup.Towers[match.Economy.Plots[0].TowerIndex].Id, Is.EqualTo("primate-arrow"));
            Assert.That(Do(EconomyAction.BuildTower, 2, 1), Is.False);
        }
        [Test] public void InsufficientBuildFundsCannotOccupyPlot()
        {
            Do(EconomyAction.BuyPlot); match.Players[0].Stone = 19;
            Assert.That(Do(EconomyAction.BuildTower), Is.False);
            Assert.That(match.Economy.Plots[0].TowerIndex, Is.EqualTo(-1)); Assert.That(match.Players[0].Stone, Is.EqualTo(19));
        }
        [Test] public void BuiltTowerFiresAndAwardsOneKillToOwnerEvenWhileGhost()
        {
            var p = match.Players[0]; p.NextTower = double.MaxValue; p.Stone = 20;
            var tower = classes[0].TechnologyGroup.Towers[0]; tower.Range = 100; tower.Damage = 100;
            Do(EconomyAction.BuyPlot); Do(EconomyAction.BuildTower);
            p.Life = HeroLife.Ghost; p.RespawnAt = 100;
            match.TrySend(match.Players[1].Id, 1, ++seq[1], out _); Advance(0.1f);
            Assert.That(match.Troops.Count, Is.Zero); Assert.That(p.Gold, Is.EqualTo(35));
            Advance(1); Assert.That(p.Gold, Is.EqualTo(35));
        }
        [Test] public void GhostCanManageButCannotMineOrReveal()
        {
            var p = match.Players[0]; p.Life = HeroLife.Ghost; p.RespawnAt = 100; p.Gold = 100; p.Stone = 20;
            match.UpdateHero(0, map.ResourceNodes[0], 1);
            Assert.That(Do(EconomyAction.Harvest), Is.False); Assert.That(p.GrantsVision, Is.False);
            Assert.That(Do(EconomyAction.BuyPlot), Is.True); Assert.That(Do(EconomyAction.BuildTower), Is.True);
            Assert.That(Do(EconomyAction.SellTower), Is.True); Assert.That(Do(EconomyAction.SellPlot), Is.True);
            Assert.That(Do(EconomyAction.LevelHero), Is.True); Assert.That(p.Life, Is.EqualTo(HeroLife.Ghost));
            p.RespawnAt = match.Time + 0.1; Advance(0.15f); Assert.That(p.Health, Is.EqualTo(125));
        }
        [Test] public void RefundsArePartialIdempotentAndCannotSellOccupiedLand()
        {
            match.Players[0].Stone = 20; Do(EconomyAction.BuyPlot); Do(EconomyAction.BuildTower);
            Assert.That(Do(EconomyAction.SellPlot), Is.False);
            Assert.That(Do(EconomyAction.SellTower), Is.True); Assert.That(Do(EconomyAction.SellTower), Is.False);
            Assert.That(match.Players[0].Stone, Is.EqualTo(10));
            Assert.That(Do(EconomyAction.SellPlot), Is.True); Assert.That(Do(EconomyAction.SellPlot), Is.False);
            Assert.That(match.Players[0].Gold, Is.EqualTo(37));
        }
        [Test] public void RefundCapacityFailuresKeepAssetsAndBalances()
        {
            var p = match.Players[0]; p.Stone = 20; Do(EconomyAction.BuyPlot); Do(EconomyAction.BuildTower);
            p.Stone = rules.StoneCapacity; Assert.That(Do(EconomyAction.SellTower), Is.False);
            Assert.That(match.Economy.Plots[0].TowerIndex, Is.Zero); p.Stone = 0; Do(EconomyAction.SellTower);
            p.Gold = int.MaxValue; Assert.That(Do(EconomyAction.SellPlot), Is.False);
            Assert.That(match.Economy.Plots[0].Owned, Is.True); Assert.That(p.Gold, Is.EqualTo(int.MaxValue));
        }
        [Test] public void HeroGrowthIsOncePerMatchAndDoesNotHealOrPermanentlyChangeClass()
        {
            var p = match.Players[0]; p.Health = 10;
            Assert.That(Do(EconomyAction.LevelHero), Is.True); Assert.That(Do(EconomyAction.LevelHero), Is.False);
            Assert.That(p.Health, Is.EqualTo(10)); Assert.That(p.MaximumHealth, Is.EqualTo(125));
            Assert.That(p.Definition.MaxHealth, Is.EqualTo(100)); Assert.That(p.Gold, Is.EqualTo(20));
            Restart(); Assert.That(match.Players[0].MaximumHealth, Is.EqualTo(100));
            Assert.That(match.Players[0].Gold, Is.EqualTo(50)); Assert.That(match.Economy.Plots.All(s => !s.Owned), Is.True);
        }
        [Test] public void MatchEndCancelsHarvestAndRejectsPurchases()
        {
            MineAt(0); match.Castles[0] = 0; Advance(0.1f);
            Assert.That(match.Players[0].HarvestNode, Is.EqualTo(-1)); Assert.That(match.Players[0].Stone, Is.Zero);
            Assert.That(Do(EconomyAction.BuyPlot), Is.False);
        }
        [Test] public void InvalidSiteOwnershipOverlapAndLaneObstructionAreRejected()
        {
            map.PlotSides = new[] { 0 }; Assert.Throws<ArgumentException>(() => Restart());
            map.PlotSides = new[] { 0, 0, 1, 1 }; map.Plots[0] = map.Lanes[0].Entry;
            Assert.Throws<ArgumentException>(() => Restart());
            map.Plots[0] = map.Plots[1]; Assert.Throws<ArgumentException>(() => Restart());
        }
        [Test] public void InvalidEconomyCostsAndRefundsAreRejected()
        {
            rules.PlotRefundGold = 26; Assert.Throws<ArgumentException>(() => rules.Validate()); rules.PlotRefundGold = 12;
            rules.TowerRefundStone = 21; Assert.Throws<ArgumentException>(() => rules.Validate()); rules.TowerRefundStone = 10;
            rules.HarvestSeconds = float.NaN; Assert.Throws<ArgumentException>(() => rules.Validate()); rules.HarvestSeconds = 2;
            rules.StoneCapacity = int.MaxValue; Assert.Throws<ArgumentException>(() => rules.Validate());
        }
    }
}
