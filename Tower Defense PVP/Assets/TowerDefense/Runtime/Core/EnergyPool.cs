using System;

namespace TowerDefense.Core
{
    // Per-player runtime state. Never mutate a shared resource definition.
    public sealed class EnergyPool
    {
        public string Id { get; }
        public float Capacity { get; }
        public float Current { get; private set; }
        public float RecoveryPerSecond { get; }

        public EnergyPool(string id, float capacity, float initial, float recoveryPerSecond)
        {
            if (string.IsNullOrWhiteSpace(id) || !Finite(capacity) || capacity < 0 ||
                !Finite(initial) || !Finite(recoveryPerSecond) || recoveryPerSecond < 0)
                throw new ArgumentException("Invalid energy pool definition.");
            Id = id;
            Capacity = capacity;
            Current = Math.Clamp(initial, 0, capacity);
            RecoveryPerSecond = recoveryPerSecond;
        }

        public bool TrySpend(float amount)
        {
            if (!Finite(amount) || amount < 0 || amount > Current) return false;
            Current -= amount;
            return true;
        }

        public void Recover(float seconds)
        {
            if (!Finite(seconds) || seconds < 0) return;
            Current = Math.Min(Capacity, Current + RecoveryPerSecond * seconds);
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
