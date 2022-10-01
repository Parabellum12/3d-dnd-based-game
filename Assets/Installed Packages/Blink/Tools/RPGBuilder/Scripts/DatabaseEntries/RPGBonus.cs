using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGBonus : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string displayName;
    [HideInInspector] public string _fileName;
    [HideInInspector] public Sprite icon;
    
    
    public bool learnedByDefault;

    [Serializable]
    public class RPGBonusRankDATA
    {
        public bool ShowedInEditor;
        public int unlockCost;

        [RPGDataList] public List<RequirementsData.RequirementGroup> Requirements = new List<RequirementsData.RequirementGroup>();
        public bool UseRequirementsTemplate;
        public RequirementsTemplate RequirementsTemplate;

        [RPGDataList] public List<RPGEffect.STAT_EFFECTS_DATA> statEffectsData = new List<RPGEffect.STAT_EFFECTS_DATA>();
    }
    [RPGDataList] public List<RPGBonusRankDATA> ranks = new List<RPGBonusRankDATA>();

    public void CopyEntryData(RPGBonusRankDATA original, RPGBonusRankDATA copied)
    {
        original.unlockCost = copied.unlockCost;
        
        original.statEffectsData = new List<RPGEffect.STAT_EFFECTS_DATA>();
        for (var index = 0; index < copied.statEffectsData.Count; index++)
        {
            RPGEffect.STAT_EFFECTS_DATA newRef = new RPGEffect.STAT_EFFECTS_DATA();
            newRef.isPercent = copied.statEffectsData[index].isPercent;
            newRef.statEffectModification = copied.statEffectsData[index].statEffectModification;
            newRef.statID = copied.statEffectsData[index].statID;
            original.statEffectsData.Add(newRef);
        }
    }
    
    public void UpdateEntryData(RPGBonus newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        ranks = newEntryData.ranks;
        learnedByDefault = newEntryData.learnedByDefault;
    }
}