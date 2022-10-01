using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGCraftingRecipe : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public Sprite icon;
    
    
    public bool learnedByDefault;
    [SkillID] public int craftingSkillID = -1;
    [CraftingStationID] public int craftingStationID = -1;

    [Serializable]
    public class CraftedItemsDATA
    {
        public float chance = 100f;
        public int count = 1;
        [ItemID] public int craftedItemID = -1;
    }
    
    
    [Serializable]
    public class ComponentsRequired
    {
        public int count = 1;
        [ItemID] public int componentItemID = -1;
    }
    
    
    [Serializable]
    public class RPGCraftingRecipeRankData
    {
        public bool ShowedInEditor;
        public int unlockCost;
        public int Experience;
        public float craftTime;

        [RPGDataList] public List<CraftedItemsDATA> allCraftedItems = new List<CraftedItemsDATA>();
        [RPGDataList] public List<ComponentsRequired> allComponents = new List<ComponentsRequired>();
    }
    [RPGDataList] public List<RPGCraftingRecipeRankData> ranks = new List<RPGCraftingRecipeRankData>();

    public void CopyEntryData(RPGCraftingRecipeRankData original, RPGCraftingRecipeRankData copied)
    {
        original.Experience = copied.Experience;
        original.craftTime = copied.craftTime;
        original.unlockCost = copied.unlockCost;

        original.allCraftedItems = new List<CraftedItemsDATA>();
        for (var index = 0; index < copied.allCraftedItems.Count; index++)
        {
            CraftedItemsDATA newRef = new CraftedItemsDATA();
            newRef.chance = copied.allCraftedItems[index].chance;
            newRef.count = copied.allCraftedItems[index].count;
            newRef.craftedItemID = copied.allCraftedItems[index].craftedItemID;
            original.allCraftedItems.Add(newRef);
        }

        original.allComponents = new List<ComponentsRequired>();
        for (var index = 0; index < copied.allComponents.Count; index++)
        {
            ComponentsRequired newRef = new ComponentsRequired();
            newRef.count = copied.allComponents[index].count;
            newRef.componentItemID = copied.allComponents[index].componentItemID;
            original.allComponents.Add(newRef);
        }
    }

    public void UpdateEntryData(RPGCraftingRecipe newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        ranks = newEntryData.ranks;
        learnedByDefault = newEntryData.learnedByDefault;
        craftingSkillID = newEntryData.craftingSkillID;
        craftingStationID = newEntryData.craftingStationID;
    }
}