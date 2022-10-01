using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGLevelsTemplate : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    
    
    public int levels;
    public int baseXPValue;
    public float increaseAmount;
    public GameObject levelUpPrefab;

    [Serializable]
    public class LEVELS_DATA
    {
        public string levelName;
        public int level;
        public int XPRequired;
    }

    [RPGDataList] public List<LEVELS_DATA> allLevels = new List<LEVELS_DATA>();

    public void UpdateEntryData(RPGLevelsTemplate newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        levels = newEntryData.levels;
        baseXPValue = newEntryData.baseXPValue;
        increaseAmount = newEntryData.increaseAmount;
        allLevels = newEntryData.allLevels;
        levelUpPrefab = newEntryData.levelUpPrefab;
    }
}