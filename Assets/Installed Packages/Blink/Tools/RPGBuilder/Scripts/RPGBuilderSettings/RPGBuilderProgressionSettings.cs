using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBuilderProgressionSettings : RPGBuilderDatabaseEntry
{
    public int TalentTreeNodesPerTier = 6;
    
    public GameObject characterLevelUpPrefab, skillLevelUpPrefab, weaponTemplateLevelUpPrefab;

    public void UpdateEntryData(RPGBuilderProgressionSettings newEntryData)
    {
        TalentTreeNodesPerTier = newEntryData.TalentTreeNodesPerTier;
        characterLevelUpPrefab = newEntryData.characterLevelUpPrefab;
        skillLevelUpPrefab = newEntryData.skillLevelUpPrefab;
        weaponTemplateLevelUpPrefab = newEntryData.weaponTemplateLevelUpPrefab;
    }
}
