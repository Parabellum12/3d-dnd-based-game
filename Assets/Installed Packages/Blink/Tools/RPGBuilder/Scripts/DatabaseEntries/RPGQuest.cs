using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGQuest : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string displayName;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string description;
    
    
    public string ObjectiveText;
    public string ProgressText;

    public bool repeatable;
    public bool canBeTurnedInWithoutNPC;

    public List<RequirementsData.RequirementGroup> Requirements = new List<RequirementsData.RequirementGroup>();
    public bool UseRequirementsTemplate;
    public RequirementsTemplate RequirementsTemplate;

    [Serializable]
    public class QuestItemsGivenDATA
    {
        [ItemID] public int itemID = -1;
        public int count;
    }

    [RPGDataList] public List<QuestItemsGivenDATA> itemsGiven = new List<QuestItemsGivenDATA>();

    public enum QuestObjectiveType
    {
        task
    }

    [Serializable]
    public class QuestObjectiveDATA
    {
        public QuestObjectiveType objectiveType;
        [TaskID] public int taskID = -1;
        public float timeLimit;
    }

    [RPGDataList] public List<QuestObjectiveDATA> objectives = new List<QuestObjectiveDATA>();

    public enum QuestRewardType
    {
        item,
        currency,
        treePoint,
        Experience,
        FactionPoint,
        weaponTemplateEXP
    }

    [Serializable]
    public class QuestRewardDATA
    {
        public QuestRewardType rewardType;
        [ItemID] public int itemID = -1;
        [CurrencyID] public int currencyID = -1;
        [PointID] public int treePointID = -1;
        [FactionID] public int factionID = -1;
        [WeaponTemplateID] public int weaponTemplateID = -1;
        public int count;
        public int Experience;
    }

    [RPGDataList] public List<QuestRewardDATA> rewardsGiven = new List<QuestRewardDATA>();
    [RPGDataList] public List<QuestRewardDATA> rewardsToPick = new List<QuestRewardDATA>();

    public void UpdateEntryData(RPGQuest newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        ObjectiveText = newEntryData.ObjectiveText;
        ProgressText = newEntryData.ProgressText;
        repeatable = newEntryData.repeatable;
        Requirements = newEntryData.Requirements;
        itemsGiven = newEntryData.itemsGiven;
        objectives = newEntryData.objectives;
        rewardsGiven = newEntryData.rewardsGiven;
        rewardsToPick = newEntryData.rewardsToPick;
        canBeTurnedInWithoutNPC = newEntryData.canBeTurnedInWithoutNPC;
        UseRequirementsTemplate = newEntryData.UseRequirementsTemplate;
        RequirementsTemplate = newEntryData.RequirementsTemplate;
    }
}