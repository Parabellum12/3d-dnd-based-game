using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGCombo : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string displayName;
    [HideInInspector] public string fileName;
    [HideInInspector] public string description;

    [AbilityID] public int initialAbilityID = -1;
    public bool StartCancelOtherCombos;

    public enum KeyType {
        StartAbilityKey,
        OverrideKey,
        ActionKey
    }
    
    [System.Serializable]
    public class ComboEntry
    {
        [AbilityID] public int abilityID = -1;
        public bool abMustBeKnown;
        public bool mustHit;
        public float expireTime = 3, readyTime = 0;
        public KeyType keyType;
        public KeyCode overrideKey;
        public string actionKeyName;
        public List<RequirementsData.RequirementGroup> Requirements = new List<RequirementsData.RequirementGroup>();
        public bool UseRequirementsTemplate;
        public RequirementsTemplate RequirementsTemplate;
    }

    [RPGDataList] public List<ComboEntry> combos = new List<ComboEntry>();
    
    
    public void UpdateEntryData(RPGCombo newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        initialAbilityID = newEntryData.initialAbilityID;
        StartCancelOtherCombos = newEntryData.StartCancelOtherCombos;
        combos = newEntryData.combos;
    }
}
