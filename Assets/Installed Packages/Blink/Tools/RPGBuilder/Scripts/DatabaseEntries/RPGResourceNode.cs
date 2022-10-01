using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGResourceNode : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public Sprite icon;
    
    
    public bool learnedByDefault;
    [SkillID] public int skillRequiredID = -1;

    [Serializable]
    public class RPGResourceNodeRankData
    {
        public bool ShowedInEditor;
        public int unlockCost;

        [LootTableID] public int lootTableID = -1;

        public int skillLevelRequired;

        public int Experience;

        public float distanceMax;

        public float gatherTime;
        public float respawnTime;
    }
    public List<RPGResourceNodeRankData> ranks = new List<RPGResourceNodeRankData>();

    public void CopyEntryData(RPGResourceNodeRankData original, RPGResourceNodeRankData copied)
    {
        original.unlockCost = copied.unlockCost;
        original.lootTableID = copied.lootTableID;
        original.skillLevelRequired = copied.skillLevelRequired;
        original.Experience = copied.Experience;
        original.distanceMax = copied.distanceMax;
        original.gatherTime = copied.gatherTime;
        original.respawnTime = copied.respawnTime;
    }
    
    public void UpdateEntryData(RPGResourceNode newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        learnedByDefault = newEntryData.learnedByDefault;
        ranks = newEntryData.ranks;
        skillRequiredID = newEntryData.skillRequiredID;
    }
}