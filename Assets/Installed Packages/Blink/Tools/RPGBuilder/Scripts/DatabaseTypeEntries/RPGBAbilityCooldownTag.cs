using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBAbilityCooldownTag : RPGBuilderDatabaseEntry
{
    
    public void UpdateEntryData(RPGBAbilityCooldownTag newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
