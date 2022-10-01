using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGLootTable : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    

    [Serializable]
    public class LOOT_ITEMS
    {
        [ItemID] public int itemID = -1;
        public int min = 1;
        public int max = 1;
        public float dropRate = 100f;
    }

    [RPGDataList] public List<LOOT_ITEMS> lootItems = new List<LOOT_ITEMS>();

    public bool LimitDroppedItems;
    public int maxDroppedItems = 1;
    
    public List<RequirementsData.RequirementGroup> Requirements = new List<RequirementsData.RequirementGroup>();
    public bool UseRequirementsTemplate;
    public RequirementsTemplate RequirementsTemplate;

    public void UpdateEntryData(RPGLootTable newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        lootItems = newEntryData.lootItems;
        maxDroppedItems = newEntryData.maxDroppedItems;
        LimitDroppedItems = newEntryData.LimitDroppedItems;
        
        Requirements = newEntryData.Requirements;
        UseRequirementsTemplate = newEntryData.UseRequirementsTemplate;
        RequirementsTemplate = newEntryData.RequirementsTemplate;
    }
}