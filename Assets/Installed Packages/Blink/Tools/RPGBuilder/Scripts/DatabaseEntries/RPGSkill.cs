using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using UnityEngine.Serialization;

public class RPGSkill : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public string description;
    [HideInInspector] public Sprite icon;
    
    
    [FormerlySerializedAs("automaticlyAdded")] public bool automaticallyAdded;
    public int MaxLevel;
    [LevelsID] public int levelTemplateID = -1;

    [Serializable]
    public class TalentTreesDATA
    {
        [TalentTreeID] public int talentTreeID = -1;
    }

    [RPGDataList] public List<TalentTreesDATA> talentTrees = new List<TalentTreesDATA>();

    [Serializable]
    public class STATS_DATA
    {
        public string statName;
        [StatID] public int statID = -1;
        public float amount;
        public bool isPercent;
        public float bonusPerLevel;
    }

    [RPGDataList] public List<STATS_DATA> stats = new List<STATS_DATA>();
    [RPGDataList] public List<CombatData.CustomStatValues> CustomStats = new List<CombatData.CustomStatValues>();
    public bool UseStatListTemplate;
    public StatListTemplate StatListTemplate;
    
    [RPGDataList] public List<RPGItemDATA.StartingItemsDATA> startItems = new List<RPGItemDATA.StartingItemsDATA>();
    [RPGDataList] public List<RPGCombatDATA.ActionAbilityDATA> actionAbilities = new List<RPGCombatDATA.ActionAbilityDATA>();
    
    [RPGDataList] public List<CharacterEntries.AllocatedStatEntry> allocatedStatsEntriesGame = new List<CharacterEntries.AllocatedStatEntry>();
    public void UpdateEntryData(RPGSkill newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        MaxLevel = newEntryData.MaxLevel;
        levelTemplateID = newEntryData.levelTemplateID;
        automaticallyAdded = newEntryData.automaticallyAdded;
        talentTrees = newEntryData.talentTrees;
        startItems = newEntryData.startItems;
        actionAbilities = newEntryData.actionAbilities;
        CustomStats = newEntryData.CustomStats;
        UseStatListTemplate = newEntryData.UseStatListTemplate;
        StatListTemplate = newEntryData.StatListTemplate;
        allocatedStatsEntriesGame = newEntryData.allocatedStatsEntriesGame;
    }
}