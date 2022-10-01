using System;
using BLINK.RPGBuilder.World;

namespace BLINK.RPGBuilder.Data
{
    public static class EconomyData
    {
        public enum EquipFunction
        {
            Armor = 0,
            Weapon = 1,
        }

        public enum ItemTypeFunction
        {
            None = 0,
            Currency = 1,
            Gem = 2,
            Enchantment = 3,
        }
        
        [Serializable]
        public class EquippedArmor
        {
            public RPGBArmorSlot ArmorSlot;
            public RPGItem item;
            public int temporaryItemDataID = -1;
        }
        [Serializable]
        public class EquippedWeapon
        {
            public RPGItem item;
            public int temporaryItemDataID = -1;
        }
        
        [Serializable]
        public class EquippedItemData
        {
            public RPGItem item;
            public int itemDataID = -1;
        }
        
        [Serializable]
        public class WorldDroppedItemEntry
        {
            public RPGItem item;
            public int count;
            public WorldDroppedItem worldDroppedItemREF;
            public int itemDataID = -1;
        }
        
    }

}
