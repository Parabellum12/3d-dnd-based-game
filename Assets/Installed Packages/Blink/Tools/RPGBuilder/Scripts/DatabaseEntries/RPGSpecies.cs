using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGSpecies : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string displayName;
    [HideInInspector] public string fileName;
    [HideInInspector] public string description;
    [HideInInspector] public Sprite icon;
    
    
    [Serializable]
    public class SPECIES_STAT_DATA
    {
        [StatID] public int statID = -1;
        public float value;
        
        public List<RPGStat.VitalityActions> vitalityActions = new List<RPGStat.VitalityActions>();
    }

    [RPGDataList] public List<SPECIES_STAT_DATA> stats = new List<SPECIES_STAT_DATA>();
    [RPGDataList] public List<CombatData.CustomStatValues> CustomStats = new List<CombatData.CustomStatValues>();
    public bool UseStatListTemplate;
    public StatListTemplate StatListTemplate;
    
    [Serializable]
    public class SPECIES_TRAIT
    {
        [HideInInspector] public string statFunction;
        public RPGBDamageType damageType;
        public float modifier = 100;
    }

    public List<SPECIES_TRAIT> traits = new List<SPECIES_TRAIT>();
    
    public void UpdateEntryData(RPGSpecies newEntryData)
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
        traits = newEntryData.traits;
    }
}
