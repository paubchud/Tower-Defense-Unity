using System;
using UnityEngine;

namespace TowerDefense.Data
{
    [CreateAssetMenu(menuName = "Tower Defense/Economy Rules")]
    public sealed class EconomyRulesDefinition : ScriptableObject
    {
        public int StartingGold = 50, PlotGold = 25, PlotRefundGold = 12;
        public int StoneCapacity = 100, NodeCapacity = 50, HarvestYield = 5;
        public float HarvestSeconds = 2, HarvestRange = 3, NodeRecoverySeconds = 20;
        public int TowerStone = 20, TowerRefundStone = 10;
        public int HeroLevelGold = 30;
        public float HeroHealthBonus = 25;
        public float LaneClearance = 3;

        public void Validate()
        {
            if (StartingGold < 0 || StartingGold > 100000 || PlotGold < 1 || PlotGold > 100000 ||
                PlotRefundGold < 0 || PlotRefundGold > PlotGold || StoneCapacity < 1 || StoneCapacity > 100000 ||
                NodeCapacity < 1 || NodeCapacity > 100000 || HarvestYield < 1 || HarvestYield > NodeCapacity ||
                HarvestYield > StoneCapacity || TowerStone < 1 || TowerStone > StoneCapacity ||
                TowerRefundStone < 0 || TowerRefundStone > TowerStone || HeroLevelGold < 1 || HeroLevelGold > 100000 ||
                !Positive(HarvestSeconds) || !Positive(HarvestRange) || !Positive(NodeRecoverySeconds) ||
                !Positive(HeroHealthBonus) || !Positive(LaneClearance))
                throw new ArgumentException("Invalid prototype economy rules.");
        }
        private static bool Positive(float value) => float.IsFinite(value) && value > 0 && value <= 100000;
    }
}
