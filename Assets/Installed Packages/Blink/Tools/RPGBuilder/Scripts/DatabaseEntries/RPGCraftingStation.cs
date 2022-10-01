using System.Collections.Generic;
using UnityEngine;

public class RPGCraftingStation : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public Sprite icon;
    

    public float maxDistance;

    [System.Serializable]
    public class CraftSkillsDATA
    {
        [SkillID] public int craftSkillID = -1;
    }
    [RPGDataList] public List<CraftSkillsDATA> craftSkills = new List<CraftSkillsDATA>();

    public void UpdateEntryData(RPGCraftingStation newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        maxDistance = newEntryData.maxDistance;
        craftSkills = newEntryData.craftSkills;
    }
}