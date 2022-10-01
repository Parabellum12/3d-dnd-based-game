using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGClass : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public string description;
    [HideInInspector] public Sprite icon;
    

    [AbilityID] public int autoAttackAbilityID = -1;

    [Serializable]
    public class CLASS_STATS_DATA
    {
        public string statName;
        [StatID] public int statID = -1;
        public float amount;
        public bool isPercent;
        public float bonusPerLevel;
    }

    [RPGDataList] public List<CLASS_STATS_DATA> stats = new List<CLASS_STATS_DATA>();
    [RPGDataList] public List<CombatData.CustomStatValues> CustomStats = new List<CombatData.CustomStatValues>();
    public bool UseStatListTemplate;
    public StatListTemplate StatListTemplate;

    [LevelsID] public int levelTemplateID = -1;

    [Serializable]
    public class TalentTreesDATA
    {
        [TalentTreeID] public int talentTreeID = -1;
    }

    [RPGDataList] public List<TalentTreesDATA> talentTrees = new List<TalentTreesDATA>();

    [Serializable]
    public class SpellbookDATA
    {
        [SpellbookID] public int spellbookID = -1;
    }

    [RPGDataList] public List<SpellbookDATA> spellbooks = new List<SpellbookDATA>();
    
    [RPGDataList] public List<RPGItemDATA.StartingItemsDATA> startItems = new List<RPGItemDATA.StartingItemsDATA>();

    [RPGDataList] public List<RPGCombatDATA.ActionAbilityDATA> actionAbilities = new List<RPGCombatDATA.ActionAbilityDATA>();
    
    public int allocationStatPoints;
    [RPGDataList] public List<CharacterEntries.AllocatedStatEntry> allocatedStatsEntries = new List<CharacterEntries.AllocatedStatEntry>();
    
    [RPGDataList] public List<CharacterEntries.AllocatedStatEntry> allocatedStatsEntriesGame = new List<CharacterEntries.AllocatedStatEntry>();
    public void UpdateEntryData(RPGClass newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        CustomStats = newEntryData.CustomStats;
        UseStatListTemplate = newEntryData.UseStatListTemplate;
        StatListTemplate = newEntryData.StatListTemplate;
        levelTemplateID = newEntryData.levelTemplateID;
        talentTrees = newEntryData.talentTrees;
        autoAttackAbilityID = newEntryData.autoAttackAbilityID;
        startItems = newEntryData.startItems;
        actionAbilities = newEntryData.actionAbilities;
        spellbooks = newEntryData.spellbooks;
        allocationStatPoints = newEntryData.allocationStatPoints;
        allocatedStatsEntries = newEntryData.allocatedStatsEntries;
        allocatedStatsEntriesGame = newEntryData.allocatedStatsEntriesGame;
    }
}