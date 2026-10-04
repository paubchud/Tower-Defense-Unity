using System;
using Unity.Collections;
using Unity.Netcode;

namespace TowerDefense.Networking
{
    public struct TroopSnapshot : INetworkSerializable, IEquatable<TroopSnapshot>
    {
        public uint Id;
        public int Level;
        public float Health;
        public double SpawnAt;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        { serializer.SerializeValue(ref Id); serializer.SerializeValue(ref Level); serializer.SerializeValue(ref Health); serializer.SerializeValue(ref SpawnAt); }
        public bool Equals(TroopSnapshot other) => Id == other.Id && Level == other.Level && Health == other.Health && SpawnAt == other.SpawnAt;
    }

    public struct EnergySnapshot : INetworkSerializable, IEquatable<EnergySnapshot>
    {
        public FixedString64Bytes Id;
        public float Current, Capacity;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        { serializer.SerializeValue(ref Id); serializer.SerializeValue(ref Current); serializer.SerializeValue(ref Capacity); }
        public bool Equals(EnergySnapshot other) => Id == other.Id && Current == other.Current && Capacity == other.Capacity;
    }

    public struct CombatEconomy : INetworkSerializable, IEquatable<CombatEconomy>
    {
        public int Gold, XP, TroopLevel, Stone, HeroLevel, HarvestNode;
        public double HarvestEnds;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Gold); serializer.SerializeValue(ref XP); serializer.SerializeValue(ref TroopLevel);
            serializer.SerializeValue(ref Stone); serializer.SerializeValue(ref HeroLevel);
            serializer.SerializeValue(ref HarvestNode); serializer.SerializeValue(ref HarvestEnds);
        }
        public bool Equals(CombatEconomy other) => Gold == other.Gold && XP == other.XP && TroopLevel == other.TroopLevel &&
            Stone == other.Stone && HeroLevel == other.HeroLevel && HarvestNode == other.HarvestNode && HarvestEnds == other.HarvestEnds;
    }

    public struct PlotSnapshot : INetworkSerializable, IEquatable<PlotSnapshot>
    {
        public int Id, TowerIndex;
        public bool Owned;
        public double AttackAt;
        public UnityEngine.Vector3 AttackEnd;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id); serializer.SerializeValue(ref TowerIndex); serializer.SerializeValue(ref Owned);
            serializer.SerializeValue(ref AttackAt); serializer.SerializeValue(ref AttackEnd);
        }
        public bool Equals(PlotSnapshot other) => Id == other.Id && TowerIndex == other.TowerIndex && Owned == other.Owned &&
            AttackAt == other.AttackAt && AttackEnd == other.AttackEnd;
    }

    public struct NodeSnapshot : INetworkSerializable, IEquatable<NodeSnapshot>
    {
        public int Id, Remaining;
        public double RecoverAt;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        { serializer.SerializeValue(ref Id); serializer.SerializeValue(ref Remaining); serializer.SerializeValue(ref RecoverAt); }
        public bool Equals(NodeSnapshot other) => Id == other.Id && Remaining == other.Remaining && RecoverAt == other.RecoverAt;
    }
}
