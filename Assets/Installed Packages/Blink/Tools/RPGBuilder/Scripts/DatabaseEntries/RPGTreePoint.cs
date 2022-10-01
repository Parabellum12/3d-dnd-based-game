using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGTreePoint : RPGBuilderDatabaseEntry
{
    [HideInInspector] public Sprite icon;
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector]  public string _displayName;
    [HideInInspector] public string description;

    
    public int startAmount;
    public int maxPoints;

    public enum TreePointGainRequirementTypes
    {
        characterLevelUp,
        skillLevelUp,
        npcKilled,
        itemGained,
        weaponTemplateLevelUp
    }

    [Serializable]
    public class GainRequirements
    {
        public TreePointGainRequirementTypes gainType;
        public int amountGained;

        [ClassID] public int classRequiredID = -1;
        [SkillID] public int skillRequiredID = -1;
        [ItemID] public int itemRequiredID = -1;
        public int itemRequiredCount;
        [NPCID] public int npcRequiredID = -1;
        [WeaponTemplateID] public int weaponTemplateRequiredID = -1;
    }

    [RPGDataList] public List<GainRequirements> gainPointRequirements = new List<GainRequirements>();

    public void UpdateEntryData(RPGTreePoint newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        startAmount = newEntryData.startAmount;
        maxPoints = newEntryData.maxPoints;
        gainPointRequirements = newEntryData.gainPointRequirements;
    }
}