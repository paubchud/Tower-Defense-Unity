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
        public int Gold, XP, TroopLevel;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        { serializer.SerializeValue(ref Gold); serializer.SerializeValue(ref XP); serializer.SerializeValue(ref TroopLevel); }
        public bool Equals(CombatEconomy other) => Gold == other.Gold && XP == other.XP && TroopLevel == other.TroopLevel;
    }
}
