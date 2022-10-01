using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBuilderEconomySettings : RPGBuilderDatabaseEntry
{
    public List<string> itemRarityList = new List<string>();
    public List<Sprite> itemRarityImagesList = new List<Sprite>();
    public List<Color> itemRarityColorsList = new List<Color>();
    public List<string> itemTypeList = new List<string>();
    public List<string> weaponTypeList = new List<string>();
    public List<string> armorTypeList = new List<string>();
    public List<string> armorSlotsList = new List<string>();
    public List<string> weaponSlotsList = new List<string>();
    public List<string> slotTypeList = new List<string>();
    public List<string> socketTypeList = new List<string>();
    
    public List<RPGItemDATA.WeaponAnimatorOverride> weaponAnimatorOverrides;
    
    public int InventorySlots;


    [System.Serializable]
    public class StartingItemsDATA
    {
        public int itemID = -1;
        public int count = 1;
        public bool equipped;
    }

    
    [System.Serializable]
    public class RandomItemData
    {
        public List<RandomizedStat> randomStats = new List<RandomizedStat>();
        public int randomItemID = -1;
    }
    
    [System.Serializable]
    public class RandomizedStat
    {
        public int statID = -1;
        public float statValue;
    }
    
    [System.Serializable]
    public class RandomizedStatData
    {
        public int statID = -1;
        public float minValue, maxValue = 1f;
        public bool isPercent;
        public bool isInt;
        public float chance = 100f;
    }

    public GameObject LootBagPrefab;
    
    public void UpdateEntryData(RPGBuilderEconomySettings newEntryData)
    {
        itemTypeList = newEntryData.itemTypeList;
        weaponTypeList = newEntryData.weaponTypeList;
        armorTypeList = newEntryData.armorTypeList;
        itemRarityList = newEntryData.itemRarityList;
        armorSlotsList = newEntryData.armorSlotsList;
        weaponSlotsList = newEntryData.weaponSlotsList;
        slotTypeList = newEntryData.slotTypeList;
        InventorySlots = newEntryData.InventorySlots;
        itemRarityImagesList = newEntryData.itemRarityImagesList;
        itemRarityColorsList = newEntryData.itemRarityColorsList;
        socketTypeList = newEntryData.socketTypeList;
        weaponAnimatorOverrides = newEntryData.weaponAnimatorOverrides;
        LootBagPrefab = newEntryData.LootBagPrefab;
    }
}
