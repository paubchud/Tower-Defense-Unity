using TowerDefense.Data;

namespace TowerDefense.Core
{
    public sealed class StarterInventory
    {
        private readonly ItemDefinition[] slots;
        public int Count => slots.Length;
        public ItemDefinition Armor { get; }
        public ItemDefinition this[int slot] => slot >= 0 && slot < slots.Length ? slots[slot] : null;

        public StarterInventory(HeroClassDefinition hero)
        {
            // The hotbar points at this player's owned starter entries; no copies on selection.
            slots = new[] { hero.StartingWeapon, hero.StartingTool, null };
            Armor = hero.StartingArmor;
        }

        public bool ValidSlot(int slot) => slot >= 0 && slot < slots.Length;
        public int Cycle(int slot, int direction) => ((slot + direction) % Count + Count) % Count;
    }
}
