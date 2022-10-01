using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class RPGSpellbook : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public string description;
    [HideInInspector] public Sprite icon;
    
    public enum SpellbookNodeType
    {
        ability,
        bonus
    }

    [Serializable]
    public class Node_DATA
    {
        public SpellbookNodeType nodeType;
        public string nodeName;
        [AbilityID] public int abilityID = -1;
        [BonusID] public int bonusID = -1;

        public int unlockLevel = 1;
    }
    [RPGDataList] public List<Node_DATA> nodeList = new List<Node_DATA>();
    
    public class SpellBookData
    {
        public RPGSpellbook spellbook;
        [WeaponTemplateID] public int weaponTemplateID = -1;
    }
    
    public enum spellbookSourceType
    {
        _class,
        _weapon
    }
    public spellbookSourceType sourceType;
    
    public void UpdateEntryData(RPGSpellbook newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        nodeList = newEntryData.nodeList;
        sourceType = newEntryData.sourceType;
    }
}
