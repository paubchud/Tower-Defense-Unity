using System;
using System.Collections.Generic;
using NUnit.Framework;
using TowerDefense.Core;
using TowerDefense.Data;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Tests
{
    public sealed class FoundationTests
    {
        private ContentCatalog catalog;
        [SetUp]
        public void LoadCatalog()
        {
            catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/TowerDefense/Content/PrototypeCatalog.asset");
            Assert.That(catalog, Is.Not.Null, "Generate the prototype assets before running tests.");
        }

        [Test]
        public void ContentHasUniqueClassesAndValidStarterReferences()
        {
            var ids = new HashSet<string>();
            foreach (var hero in catalog.Classes)
            {
                Assert.That(ids.Add(hero.Id), Is.True);
                Assert.That(hero.TechnologyGroup, Is.Not.Null);
                Assert.That(hero.StartingWeapon, Is.Not.Null);
                Assert.That(hero.MovementSpeed, Is.GreaterThan(0));
                foreach (var energy in hero.Energies) Assert.That(energy.Definition, Is.Not.Null);
            }
        }

        [Test]
        public void TechnologyCatalogsAreSharedByReferenceAndRejectOtherGroups()
        {
            var warrior = catalog.FindClass("warrior");
            var wizard = catalog.FindClass("wizard");
            var anotherClass = ScriptableObject.CreateInstance<HeroClassDefinition>();
            try
            {
                anotherClass.TechnologyGroup = warrior.TechnologyGroup;
                Assert.That(anotherClass.TechnologyGroup.Towers, Is.SameAs(warrior.TechnologyGroup.Towers));
                Assert.That(warrior.TechnologyGroup.ContainsTower(warrior.TechnologyGroup.Towers[0].Id), Is.True);
                Assert.That(warrior.TechnologyGroup.ContainsTower(wizard.TechnologyGroup.Towers[0].Id), Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(anotherClass); }
        }

        [Test]
        public void StarterInventoriesHaveIndependentContainersAndValidWrapping()
        {
            var warrior = catalog.FindClass("warrior");
            var first = new StarterInventory(warrior);
            var second = new StarterInventory(warrior);
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first[0], Is.SameAs(warrior.StartingWeapon));
            Assert.That(first[first.Cycle(0, -1)], Is.Null);
            Assert.That(first.Cycle(2, 1), Is.EqualTo(0));
            Assert.That(first.ValidSlot(-1), Is.False);
            Assert.That(first.ValidSlot(first.Count), Is.False);
        }

        [Test]
        public void EnergyRejectsOverspendingAndInvalidCostsWithoutChangingBalance()
        {
            var pool = new EnergyPool("mana", 100, 30, 5);
            Assert.That(pool.TrySpend(20), Is.True);
            Assert.That(pool.TrySpend(20), Is.False);
            Assert.That(pool.TrySpend(float.NaN), Is.False);
            Assert.That(pool.TrySpend(float.PositiveInfinity), Is.False);
            Assert.That(pool.TrySpend(-10), Is.False);
            Assert.That(pool.Current, Is.EqualTo(10));
        }

        [Test]
        public void EnergyRecoveryIsBoundedAndIsolatedPerPlayer()
        {
            var first = new EnergyPool("stamina", 100, 50, 10);
            var second = new EnergyPool("stamina", 100, 50, 10);
            first.Recover(10);
            first.Recover(-1);
            first.Recover(float.NaN);
            Assert.That(first.Current, Is.EqualTo(100));
            Assert.That(second.Current, Is.EqualTo(50));
            Assert.Throws<ArgumentException>(() => new EnergyPool("mana", float.PositiveInfinity, 0, 1));
        }

        [Test]
        public void BothLanePathsAndSpawnsFitTheAuthoredArena()
        {
            var map = catalog.TestMap;
            Assert.That(map.Lanes.Length, Is.EqualTo(2));
            foreach (var lane in map.Lanes)
            {
                Assert.That(Vector3.Distance(lane.Entry, lane.Castle), Is.GreaterThan(10));
                Assert.That(lane.Evaluate(-1), Is.EqualTo(lane.Entry));
                Assert.That(lane.Evaluate(2), Is.EqualTo(lane.Castle));
                Assert.That(map.ClampHero(lane.HeroSpawn), Is.EqualTo(lane.HeroSpawn));
            }
        }
    }
}
