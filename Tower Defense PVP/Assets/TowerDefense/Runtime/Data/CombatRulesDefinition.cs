using System;
using UnityEngine;

namespace TowerDefense.Data
{
    [Serializable]
    public struct AttackEnergyCost
    {
        public string EnergyId;
        public float Amount;
    }

    [CreateAssetMenu(menuName = "Tower Defense/Combat Rules")]
    public sealed class CombatRulesDefinition : ScriptableObject
    {
        public string TroopName = "Raider";
        public float TroopHealth = 45;
        public float HealthPerUpgrade = 45;
        public float TroopSpeed = 3;
        public float TroopAttackRange = 1.6f;
        public float TroopDamage = 8;
        public float TroopAttackInterval = 1;
        public float CastleDamage = 12;
        public float CastleDamagePerUpgrade = 6;
        public int KillGold = 10;
        public float SendInterval = 1.5f;
        public int SendXP = 5;
        public int UpgradeXP = 25;
        public int MaximumUpgrade = 1;
        public int MaximumTroopsPerLane = 16;
        public float CastleHealth = 300;
        public float RespawnSeconds = 7;
        public bool AllowGhostSending;

        public void Validate()
        {
            foreach (float value in new[] { TroopHealth, TroopSpeed, TroopAttackRange, TroopDamage,
                TroopAttackInterval, CastleDamage, SendInterval, CastleHealth, RespawnSeconds })
                if (!float.IsFinite(value) || value <= 0) throw new ArgumentException("Combat rules require positive finite values.");
            if (!float.IsFinite(HealthPerUpgrade) || HealthPerUpgrade < 0 || !float.IsFinite(CastleDamagePerUpgrade) || CastleDamagePerUpgrade < 0 ||
                KillGold < 0 || KillGold > 100000 || SendXP < 1 || SendXP > 100000 || UpgradeXP < 1 || UpgradeXP > 100000 ||
                MaximumUpgrade < 1 || MaximumUpgrade > 5 || MaximumTroopsPerLane < 1 || MaximumTroopsPerLane > 32)
                throw new ArgumentException("Combat limits/rewards are invalid.");
            if (!float.IsFinite(HealthAt(MaximumUpgrade)) || !float.IsFinite(CastleDamageAt(MaximumUpgrade)))
                throw new ArgumentException("Upgraded combat values overflow.");
        }

        public float HealthAt(int level) => TroopHealth + HealthPerUpgrade * level;
        public float CastleDamageAt(int level) => CastleDamage + CastleDamagePerUpgrade * level;
    }
}
