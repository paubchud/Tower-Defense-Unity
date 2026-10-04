using System;
using System.Collections.Generic;
using TowerDefense.Data;
using UnityEngine;

namespace TowerDefense.Core
{
    public enum HeroLife : byte { Alive, Ghost }
    public enum CombatPhase : byte { Lobby, Playing, Finished }

    // Stable per-match identity and owned rules state, independent of a transport/Netcode ID.
    public sealed class CombatPlayer
    {
        public readonly string Id;
        public readonly int Side;
        public readonly HeroClassDefinition Definition;
        public readonly Dictionary<string, EnergyPool> Energies = new Dictionary<string, EnergyPool>();
        public Vector3 Position { get; internal set; }
        public int SelectedSlot { get; internal set; }
        public float Health { get; internal set; }
        public HeroLife Life { get; internal set; }
        public double RespawnAt { get; internal set; }
        public int Gold { get; internal set; }
        public int XP { get; internal set; }
        public int TroopLevel { get; internal set; }
        public int Stone { get; internal set; }
        public int HeroLevel { get; internal set; }
        public float HealthBonus { get; internal set; }
        public float MaximumHealth => Definition.MaxHealth + HealthBonus;
        public int HarvestNode { get; internal set; } = -1;
        public double HarvestEnds { get; internal set; }
        public uint LastSequence { get; internal set; }
        internal double NextSend, NextAttack, NextTower;
        public bool GrantsVision => Life == HeroLife.Alive;
        public bool CanCollect => Life == HeroLife.Alive;
        public Vector3 TowerPosition { get; internal set; }
        public double LastHeroAttack { get; internal set; } = -100;
        public Vector3 HeroAttackEnd { get; internal set; }
        public double LastTowerAttack { get; internal set; } = -100;
        public Vector3 TowerAttackEnd { get; internal set; }

        internal CombatPlayer(string id, int side, HeroClassDefinition definition, LaneDefinition lane)
        {
            Id = id; Side = side; Definition = definition; Position = lane.HeroSpawn;
            Health = definition.MaxHealth; Life = HeroLife.Alive;
            Vector3 right = Vector3.Cross(Vector3.up, (lane.Castle - lane.Entry).normalized);
            TowerPosition = lane.Evaluate(0.65f) + right * (side == 0 ? -3.5f : 3.5f);
            RestoreEnergy();
        }

        internal void RestoreEnergy()
        {
            Energies.Clear();
            foreach (var start in Definition.Energies ?? Array.Empty<StartingEnergy>())
                Energies.Add(start.Definition.Id, new EnergyPool(start.Definition.Id, start.Capacity, start.Initial, start.RecoveryPerSecond));
        }
    }

    public sealed class CombatTroop
    {
        public uint Id { get; internal set; }
        public int Lane { get; internal set; }
        public int Level { get; internal set; }
        public double SpawnAt { get; internal set; }
        public float Health { get; internal set; }
        internal double NextAttack;
    }

    public readonly struct CombatTransaction
    {
        public readonly string PlayerId;
        public readonly uint Sequence;
        public readonly string Action;
        public readonly int XPDelta, GoldDelta, StoneDelta;
        public readonly double Time;
        public CombatTransaction(string player, uint sequence, string action, int xp, int gold, double time, int stone = 0)
        { PlayerId = player; Sequence = sequence; Action = action; XPDelta = xp; GoldDelta = gold; StoneDelta = stone; Time = time; }
    }

    // No networking, GameObjects or client balances here. Only the authority mutates these rules.
    public sealed class CombatMatch
    {
        public readonly string Id = Guid.NewGuid().ToString("N");
        public readonly uint Generation;
        public readonly CombatRulesDefinition Rules;
        public readonly MapDefinition Map;
        public readonly CombatPlayer[] Players;
        public readonly float[] Castles;
        public readonly EconomyMatch Economy;
        private readonly List<CombatTroop> troops = new List<CombatTroop>(32);
        private readonly Queue<CombatTransaction> journal = new Queue<CombatTransaction>(128);
        public IReadOnlyList<CombatTroop> Troops => troops;
        public IEnumerable<CombatTransaction> Journal => journal;
        public CombatPhase Phase { get; private set; } = CombatPhase.Playing;
        public int Winner { get; private set; } = -1;
        public double Time { get; private set; }
        private uint nextTroop;

        public CombatMatch(CombatRulesDefinition rules, MapDefinition map, HeroClassDefinition[] classes, uint generation, double start, EconomyRulesDefinition economy = null)
        {
            if (rules == null || map == null || map.Lanes == null || map.Lanes.Length != 2 || classes == null || classes.Length != 2 ||
                generation == 0 || !double.IsFinite(start)) throw new ArgumentException("Combat requires a validated duel roster/map/time.");
            rules.Validate();
            foreach (var definition in classes) ValidateClass(definition);
            foreach (var lane in map.Lanes)
                if (!Finite(lane.Entry) || !Finite(lane.Castle) || !Finite(lane.HeroSpawn) || Vector3.Distance(lane.Entry, lane.Castle) < 1)
                    throw new ArgumentException("Invalid combat lane.");
            Rules = rules; Map = map; Generation = generation; Time = start;
            Players = new[] { new CombatPlayer(Id + "-0", 0, classes[0], map.Lanes[0]), new CombatPlayer(Id + "-1", 1, classes[1], map.Lanes[1]) };
            Castles = new[] { rules.CastleHealth, rules.CastleHealth };
            if (economy != null) Economy = new EconomyMatch(this, economy);
        }

        private static void ValidateClass(HeroClassDefinition hero)
        {
            if (hero == null || hero.StartingWeapon == null || hero.TechnologyGroup == null || hero.TechnologyGroup.Towers == null ||
                hero.TechnologyGroup.Towers.Length == 0 || !float.IsFinite(hero.MaxHealth) || hero.MaxHealth <= 0 ||
                !float.IsFinite(hero.AttackDamage) || hero.AttackDamage <= 0 || !float.IsFinite(hero.AttackRange) || hero.AttackRange <= 0 ||
                !float.IsFinite(hero.AttackInterval) || hero.AttackInterval <= 0)
                throw new ArgumentException("Invalid hero combat definition.");
            var ids = new HashSet<string>();
            foreach (var energy in hero.Energies ?? Array.Empty<StartingEnergy>())
            {
                if (energy.Definition == null || !ids.Add(energy.Definition.Id)) throw new ArgumentException("Duplicate/missing energy definition.");
                _ = new EnergyPool(energy.Definition.Id, energy.Capacity, energy.Initial, energy.RecoveryPerSecond);
            }
            var costs = new HashSet<string>();
            foreach (var cost in hero.AttackCosts ?? Array.Empty<AttackEnergyCost>())
                if (!ids.Contains(cost.EnergyId) || !costs.Add(cost.EnergyId) || !float.IsFinite(cost.Amount) || cost.Amount < 0)
                    throw new ArgumentException("Invalid attack energy cost.");
            var tower = hero.TechnologyGroup.Towers[0];
            if (tower == null || !float.IsFinite(tower.Damage) || tower.Damage <= 0 || !float.IsFinite(tower.Range) || tower.Range <= 0 ||
                !float.IsFinite(tower.AttackInterval) || tower.AttackInterval <= 0) throw new ArgumentException("Invalid starter tower.");
        }

        public void UpdateHero(int side, Vector3 position, int selectedSlot)
        {
            if (side < 0 || side >= Players.Length || !Finite(position)) return;
            Players[side].Position = Map.ClampHero(position);
            Players[side].SelectedSlot = selectedSlot;
        }

        internal bool Admit(string playerId, uint generation, uint sequence, out CombatPlayer player, out string reason)
        {
            player = Array.Find(Players, p => p.Id == playerId);
            reason = "The match is not active.";
            if (Phase != CombatPhase.Playing || generation != Generation || player == null) return false;
            reason = "Duplicate or out-of-window command.";
            if (sequence == 0 || sequence <= player.LastSequence || (ulong)sequence > (ulong)player.LastSequence + 64) return false;
            // Consume rejected gameplay requests too; replay cannot later turn one into an accepted action.
            player.LastSequence = sequence;
            return true;
        }

        public bool TrySend(string playerId, uint generation, uint sequence, out string reason)
        {
            if (!Admit(playerId, generation, sequence, out var player, out reason)) return false;
            if (player.Life == HeroLife.Ghost && !Rules.AllowGhostSending) { reason = "Ghosts cannot send troops in this prototype."; return false; }
            if (Time < player.NextSend) { reason = "Troop send is cooling down."; return false; }
            int lane = 1 - player.Side;
            int count = 0; foreach (var troop in troops) if (troop.Lane == lane) count++;
            if (count >= Rules.MaximumTroopsPerLane) { reason = "Your troop limit is reached."; return false; }
            if (player.XP > int.MaxValue - Rules.SendXP || nextTroop == uint.MaxValue) { reason = "Match capacity reached."; return false; }
            troops.Add(new CombatTroop { Id = ++nextTroop, Lane = lane, Level = player.TroopLevel,
                SpawnAt = Time, Health = Rules.HealthAt(player.TroopLevel), NextAttack = Time });
            player.XP += Rules.SendXP; player.NextSend = Time + Rules.SendInterval;
            Record(player, sequence, "send", Rules.SendXP, 0);
            reason = "Raider sent. +" + Rules.SendXP + " XP"; return true;
        }

        public bool TryUpgrade(string playerId, uint generation, uint sequence, out string reason)
        {
            if (!Admit(playerId, generation, sequence, out var player, out reason)) return false;
            if (player.TroopLevel >= Rules.MaximumUpgrade) { reason = "Troops are fully upgraded."; return false; }
            if (player.XP < Rules.UpgradeXP) { reason = "Not enough match XP."; return false; }
            player.XP -= Rules.UpgradeXP; player.TroopLevel++;
            Record(player, sequence, "upgrade", -Rules.UpgradeXP, 0);
            reason = "Future sends upgraded. Existing troops are unchanged."; return true;
        }

        public bool TryAttack(string playerId, uint generation, uint sequence, Vector2 aim, out string reason)
        {
            if (!Admit(playerId, generation, sequence, out var player, out reason)) return false;
            if (!float.IsFinite(aim.x) || !float.IsFinite(aim.y) || !float.IsFinite(aim.sqrMagnitude) || aim.sqrMagnitude < 0.01f || aim.sqrMagnitude > 1000000)
            { reason = "Invalid aim."; return false; }
            if (player.Life != HeroLife.Alive) { reason = "Ghosts cannot attack."; return false; }
            if (player.SelectedSlot != 0 || player.Definition.StartingWeapon == null) { reason = "Select your starter weapon to attack."; return false; }
            if (Time < player.NextAttack) { reason = "Attack is cooling down."; return false; }
            foreach (var cost in player.Definition.AttackCosts ?? Array.Empty<AttackEnergyCost>())
                if (!player.Energies.TryGetValue(cost.EnergyId, out var pool) || pool.Current < cost.Amount)
                { reason = "Not enough energy."; return false; }
            foreach (var cost in player.Definition.AttackCosts ?? Array.Empty<AttackEnergyCost>()) player.Energies[cost.EnergyId].TrySpend(cost.Amount);
            player.NextAttack = Time + player.Definition.AttackInterval;
            Vector3 direction = new Vector3(aim.x, 0, aim.y).normalized;
            CombatTroop target = null; float nearest = float.MaxValue;
            foreach (var troop in troops)
            {
                if (troop.Lane != player.Side) continue;
                Vector3 delta = Position(troop) - player.Position; delta.y = 0;
                float distance = delta.magnitude;
                // Sword cone or a narrow staff ray; all target choice/range checks run on the authority.
                float along = Vector3.Dot(delta, direction);
                bool aimed = player.Definition.StartingWeapon.Kind == ItemKind.MeleeWeapon
                    ? distance < 0.1f || along / distance >= 0.55f
                    : along >= 0 && (delta - direction * along).sqrMagnitude <= 0.8f * 0.8f;
                if (distance <= player.Definition.AttackRange && aimed && distance < nearest) { target = troop; nearest = distance; }
            }
            player.LastHeroAttack = Time;
            player.HeroAttackEnd = target != null ? Position(target) : player.Position + direction * player.Definition.AttackRange;
            if (target != null) DamageTroop(target, player, player.Definition.AttackDamage);
            Record(player, sequence, "attack", 0, 0);
            reason = target == null ? "Attack missed." : "Hit!"; return true;
        }

        public Vector3 Position(CombatTroop troop)
        {
            var lane = Map.Lanes[troop.Lane];
            float length = Vector3.Distance(lane.Entry, lane.Castle);
            return lane.Evaluate((float)Math.Max(0, Time - troop.SpawnAt) * Rules.TroopSpeed / length);
        }

        public void Step(float seconds)
        {
            if (Phase != CombatPhase.Playing || !float.IsFinite(seconds) || seconds <= 0 || seconds > 0.25f) return;
            Time += seconds;
            foreach (var player in Players)
            {
                if (player.Life == HeroLife.Ghost && Time >= player.RespawnAt)
                {
                    player.Life = HeroLife.Alive; player.Health = player.MaximumHealth; player.RespawnAt = 0;
                    player.Position = Map.Lanes[player.Side].HeroSpawn; player.RestoreEnergy();
                }
                if (player.Life == HeroLife.Alive) foreach (var pool in player.Energies.Values) pool.Recover(seconds);
                if (Time < player.NextTower) continue;
                var tower = player.Definition.TechnologyGroup.Towers[0];
                CombatTroop target = null; double oldest = double.MaxValue;
                foreach (var troop in troops)
                    if (troop.Lane == player.Side && Vector3.Distance(Position(troop), player.TowerPosition) <= tower.Range && troop.SpawnAt < oldest)
                    { target = troop; oldest = troop.SpawnAt; }
                if (target == null) continue;
                player.NextTower = Time + tower.AttackInterval; player.LastTowerAttack = Time; player.TowerAttackEnd = Position(target);
                DamageTroop(target, player, tower.Damage);
            }
            Economy?.FireTowers();
            for (int i = troops.Count - 1; i >= 0; i--)
            {
                var troop = troops[i];
                var lane = Map.Lanes[troop.Lane];
                if ((Time - troop.SpawnAt) * Rules.TroopSpeed >= Vector3.Distance(lane.Entry, lane.Castle))
                {
                    Castles[troop.Lane] = Mathf.Max(0, Castles[troop.Lane] - Rules.CastleDamageAt(troop.Level));
                    troops.RemoveAt(i); // Arrival never gives kill gold.
                    continue;
                }
                var defender = Players[troop.Lane];
                Vector3 delta = Position(troop) - defender.Position; delta.y = 0;
                if (defender.Life == HeroLife.Alive && Time >= troop.NextAttack && delta.sqrMagnitude <= Rules.TroopAttackRange * Rules.TroopAttackRange)
                {
                    troop.NextAttack = Time + Rules.TroopAttackInterval;
                    defender.Health = Mathf.Max(0, defender.Health - Rules.TroopDamage);
                    if (defender.Health <= 0)
                    { defender.Life = HeroLife.Ghost; defender.RespawnAt = Time + Rules.RespawnSeconds; Record(defender, 0, "death", 0, 0); }
                }
            }
            if (Castles[0] <= 0 || Castles[1] <= 0)
            {
                Phase = CombatPhase.Finished;
                Winner = Castles[0] <= 0 && Castles[1] <= 0 ? -1 : Castles[0] <= 0 ? 1 : 0;
                // Finish the tick before resolving simultaneous castle arrivals. No further commands/timers/rewards.
            }
            // After damage/results: death on the harvest-completion tick must never grant stone.
            Economy?.Step();
        }

        internal void DamageTroop(CombatTroop troop, CombatPlayer defender, float damage)
        {
            troop.Health = Mathf.Max(0, troop.Health - damage);
            if (troop.Health > 0 || !troops.Remove(troop)) return;
            int reward = Math.Min(Rules.KillGold, int.MaxValue - defender.Gold);
            defender.Gold += reward; Record(defender, 0, "kill", 0, reward);
        }

        internal void Record(CombatPlayer player, uint sequence, string action, int xp, int gold, int stone = 0)
        {
            if (journal.Count >= 128) journal.Dequeue();
            journal.Enqueue(new CombatTransaction(player.Id, sequence, action, xp, gold, Time, stone));
        }

        private static bool Finite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }
}
