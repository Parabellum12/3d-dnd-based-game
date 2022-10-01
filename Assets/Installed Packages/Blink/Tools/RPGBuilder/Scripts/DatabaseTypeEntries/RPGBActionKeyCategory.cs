using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBActionKeyCategory : RPGBuilderDatabaseEntry
{
    
    public void UpdateEntryData(RPGBActionKeyCategory newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
