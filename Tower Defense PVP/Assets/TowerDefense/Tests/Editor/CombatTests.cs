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
    public sealed class CombatTests
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private CombatRulesDefinition rules;
        private ContentCatalog catalog;
        private HeroClassDefinition[] classes;
        private CombatMatch match;
        private readonly uint[] sequence = new uint[2];
        private T Clone<T>(T value) where T : UnityEngine.Object { var clone = UnityEngine.Object.Instantiate(value); owned.Add(clone); return clone; }

        [SetUp] public void Setup()
        {
            catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/TowerDefense/Content/PrototypeCatalog.asset");
            Assert.That(catalog.CombatRules, Is.Not.Null);
            rules = Clone(catalog.CombatRules);
            classes = catalog.Classes.Select(Clone).ToArray();
            foreach (var hero in classes)
            {
                hero.TechnologyGroup = Clone(hero.TechnologyGroup);
                var tower = Clone(hero.TechnologyGroup.Towers[0]); tower.Range = 0.01f;
                hero.TechnologyGroup.Towers = new[] { tower }; // Isolate tests; never modify shared catalog assets.
            }
            sequence[0] = sequence[1] = 0;
            Restart();
        }
        [TearDown] public void Cleanup() { foreach (var obj in owned) UnityEngine.Object.DestroyImmediate(obj); owned.Clear(); }
        private void Restart(uint generation = 1) => match = new CombatMatch(rules, catalog.TestMap, classes, generation, 100);
        private bool Send(int side) => match.TrySend(match.Players[side].Id, match.Generation, ++sequence[side], out _);
        private bool Attack(int side, Vector2 aim) => match.TryAttack(match.Players[side].Id, match.Generation, ++sequence[side], aim, out _);
        private bool Upgrade(int side) => match.TryUpgrade(match.Players[side].Id, match.Generation, ++sequence[side], out _);
        private void Advance(float seconds) { for (int i = 0; i < Mathf.CeilToInt(seconds / 0.05f); i++) match.Step(0.05f); }

        [Test] public void ShippedContentHasDistinctWeaponCostsAndGroupTowers()
        {
            var shipped = new CombatMatch(catalog.CombatRules, catalog.TestMap, catalog.Classes, 1, 0);
            Assert.That(shipped.Players[0].Energies.ContainsKey("stamina"), Is.True);
            Assert.That(shipped.Players[1].Energies.ContainsKey("mana"), Is.True);
            Assert.That(classes[1].AttackRange, Is.GreaterThan(classes[0].AttackRange));
            Assert.That(classes[0].TechnologyGroup.Id, Is.Not.EqualTo(classes[1].TechnologyGroup.Id));
        }
        [Test] public void SendsSpawnOnEnemyLaneAndAwardXPOnlyOnce()
        {
            Assert.That(Send(0), Is.True);
            Assert.That(match.Troops.Single().Lane, Is.EqualTo(1));
            Assert.That(match.Players[0].XP, Is.EqualTo(5));
            Assert.That(match.TrySend(match.Players[0].Id, 1, 1, out _), Is.False);
            Assert.That(match.Players[0].XP, Is.EqualTo(5));
        }
        [Test] public void InvalidIdentityGenerationAndSequenceCannotGrantXP()
        {
            Assert.That(match.TrySend("unknown", 1, 1, out _), Is.False);
            Assert.That(match.TrySend(match.Players[0].Id, 2, 1, out _), Is.False);
            Assert.That(match.TrySend(match.Players[0].Id, 1, 0, out _), Is.False);
            Assert.That(match.TrySend(match.Players[0].Id, 1, 65, out _), Is.False);
            Assert.That(match.Players[0].LastSequence, Is.Zero);
            Assert.That(match.Troops.Count, Is.Zero);
        }
        [Test] public void CooldownRejectionCannotBeReplayedLater()
        {
            Send(0); Assert.That(Send(0), Is.False); Advance(2);
            Assert.That(match.TrySend(match.Players[0].Id, 1, 2, out _), Is.False);
            Assert.That(Send(0), Is.True);
        }
        [Test] public void LaneLimitIsBoundedAndDoesNotRewardRejectedSend()
        {
            rules.MaximumTroopsPerLane = 2;
            Send(0); Advance(1.6f); Send(0); Advance(1.6f);
            Assert.That(Send(0), Is.False);
            Assert.That(match.Troops.Count, Is.EqualTo(2));
            Assert.That(match.Players[0].XP, Is.EqualTo(10));
        }
        [Test] public void UpgradeIsAtomicAndAffectsOnlyFutureSends()
        {
            Assert.That(Upgrade(0), Is.False);
            for (int i = 0; i < 5; i++) { Assert.That(Send(0), Is.True); Advance(1.6f); }
            float before = match.Troops[0].Health;
            Assert.That(Upgrade(0), Is.True);
            Assert.That(match.Players[0].XP, Is.Zero);
            Assert.That(match.Troops[0].Health, Is.EqualTo(before));
            Assert.That(Upgrade(0), Is.False);
            Assert.That(Send(0), Is.True);
            Assert.That(match.Troops.Last().Health, Is.EqualTo(rules.HealthAt(1)));
        }
        [Test] public void MeleeDamageAndKillRewardAreAppliedExactlyOnce()
        {
            Send(1); var troop = match.Troops[0];
            match.UpdateHero(0, match.Position(troop) + Vector3.back, 0);
            Assert.That(Attack(0, Vector2.up), Is.True);
            Assert.That(troop.Health, Is.EqualTo(19));
            Assert.That(Attack(0, Vector2.up), Is.False);
            Advance(0.65f); match.UpdateHero(0, match.Position(troop) + Vector3.back, 0);
            Assert.That(Attack(0, Vector2.up), Is.True);
            Assert.That(match.Troops.Count, Is.Zero);
            Assert.That(match.Players[0].Gold, Is.EqualTo(10));
            Advance(1); Attack(0, Vector2.up);
            Assert.That(match.Players[0].Gold, Is.EqualTo(10));
        }
        [Test] public void AttackValidatesAimSlotRangeAndEnergyOnAuthority()
        {
            Send(1); var troop = match.Troops[0]; float energy = match.Players[0].Energies["stamina"].Current;
            Assert.That(Attack(0, new Vector2(float.NaN, 0)), Is.False);
            Assert.That(Attack(0, new Vector2(float.MaxValue, 0)), Is.False);
            match.UpdateHero(0, match.Position(troop), 1);
            Assert.That(Attack(0, Vector2.up), Is.False);
            Assert.That(match.Players[0].Energies["stamina"].Current, Is.EqualTo(energy));
            match.UpdateHero(0, catalog.TestMap.Lanes[0].HeroSpawn, 0);
            Assert.That(Attack(0, Vector2.up), Is.True); // Valid miss still consumes a swing.
            Assert.That(troop.Health, Is.EqualTo(rules.TroopHealth));
            Assert.That(match.Players[0].Energies["stamina"].Current, Is.EqualTo(92));
        }
        [Test] public void StaffUsesNarrowRayAndMana()
        {
            Send(0); var troop = match.Troops[0];
            match.UpdateHero(1, match.Position(troop) + Vector3.back * 6, 0);
            Attack(1, Vector2.up);
            Assert.That(troop.Health, Is.EqualTo(27));
            Assert.That(match.Players[1].Energies["mana"].Current, Is.EqualTo(88));
        }
        [Test] public void InsufficientEnergyDoesNotPartiallySpendMultiplePools()
        {
            classes[0].Energies = new[] { classes[0].Energies[0], new StartingEnergy { Definition = classes[1].Energies[0].Definition, Capacity = 100, Initial = 5 } };
            classes[0].AttackCosts = new[] { new AttackEnergyCost { EnergyId = "stamina", Amount = 8 }, new AttackEnergyCost { EnergyId = "mana", Amount = 6 } };
            Restart();
            Assert.That(Attack(0, Vector2.up), Is.False);
            Assert.That(match.Players[0].Energies["stamina"].Current, Is.EqualTo(100));
            Assert.That(match.Players[0].Energies["mana"].Current, Is.EqualTo(5));
            Assert.That(match.Players[0].LastHeroAttack, Is.EqualTo(-100));
        }
        [Test] public void TowersKillForTheirDefenderNotTheSender()
        {
            classes[0].TechnologyGroup.Towers[0].Range = 100;
            classes[0].TechnologyGroup.Towers[0].Damage = 100;
            Send(1); Advance(0.1f);
            Assert.That(match.Troops.Count, Is.Zero);
            Assert.That(match.Players[0].Gold, Is.EqualTo(10));
            Assert.That(match.Players[1].Gold, Is.Zero);
        }
        [Test] public void GhostCannotAttackCollectOrRevealAndRespawnsOnceWithEconomyIntact()
        {
            rules.TroopDamage = 200;
            for (int i = 0; i < 5; i++) { Send(0); Advance(1.6f); }
            Send(1); var troop = match.Troops.Last();
            match.UpdateHero(0, match.Position(troop), 0); Advance(0.05f);
            var hero = match.Players[0]; Assert.That(hero.Life, Is.EqualTo(HeroLife.Ghost));
            Assert.That(hero.GrantsVision || hero.CanCollect, Is.False);
            Assert.That(Attack(0, Vector2.up), Is.False);
            Assert.That(Send(0), Is.False);
            Assert.That(Upgrade(0), Is.True); // Management survives death.
            Advance(6); Assert.That(hero.Life, Is.EqualTo(HeroLife.Ghost));
            Advance(1.1f); Assert.That(hero.Life, Is.EqualTo(HeroLife.Alive));
            Assert.That(hero.Health, Is.EqualTo(100)); Assert.That(hero.TroopLevel, Is.EqualTo(1));
            Assert.That(hero.Position, Is.EqualTo(catalog.TestMap.Lanes[0].HeroSpawn));
            Assert.That(hero.RespawnAt, Is.Zero);
        }
        [Test] public void GhostSendingCanBeExplicitlyEnabledInRules()
        {
            rules.AllowGhostSending = true; rules.TroopDamage = 200;
            Send(1); match.UpdateHero(0, match.Position(match.Troops[0]), 0); Advance(0.05f);
            Assert.That(match.Players[0].Life, Is.EqualTo(HeroLife.Ghost));
            Assert.That(Send(0), Is.True);
        }
        [Test] public void CastleArrivalFinishesMatchWithoutKillGoldOrPostResultCommands()
        {
            rules.CastleHealth = 12; Restart(); Send(0); Advance(12);
            Assert.That(match.Phase, Is.EqualTo(CombatPhase.Finished));
            Assert.That(match.Winner, Is.EqualTo(0));
            Assert.That(match.Players[1].Gold, Is.Zero);
            Assert.That(Send(0) || Attack(0, Vector2.up) || Upgrade(0), Is.False);
            double time = match.Time; Advance(10); Assert.That(match.Time, Is.EqualTo(time));
        }
        [Test] public void SimultaneousCastleArrivalsProduceDraw()
        {
            rules.CastleHealth = 12; Restart(); Send(0); Send(1); Advance(12);
            Assert.That(match.Phase, Is.EqualTo(CombatPhase.Finished)); Assert.That(match.Winner, Is.EqualTo(-1));
        }
        [Test] public void NewRoundHasFreshIdentityAndRejectsOldCommands()
        {
            Send(0); string oldId = match.Players[0].Id; Restart(2);
            Assert.That(match.TrySend(oldId, 1, 2, out _), Is.False);
            Assert.That(match.Players[0].XP, Is.Zero); Assert.That(match.Troops.Count, Is.Zero);
            Assert.That(match.Players[0].Id, Is.Not.EqualTo(oldId));
        }
        [Test] public void InvalidContentAndSimulationDeltaAreRejected()
        {
            double time = match.Time; match.Step(float.NaN); match.Step(-1); match.Step(1);
            Assert.That(match.Time, Is.EqualTo(time));
            rules.TroopSpeed = float.PositiveInfinity;
            Assert.Throws<ArgumentException>(() => Restart());
        }
        [Test] public void JournalIsBoundedAndCapturesAcceptedTransactions()
        {
            classes[0].AttackCosts = Array.Empty<AttackEnergyCost>(); Restart();
            for (int i = 0; i < 150; i++) { Attack(0, Vector2.up); Advance(0.65f); }
            Assert.That(match.Journal.Count(), Is.EqualTo(128));
            Assert.That(match.Journal.Last().Action, Is.EqualTo("attack"));
        }
    }
}
