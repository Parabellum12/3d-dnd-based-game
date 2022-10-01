using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBArmorSlot : RPGBuilderDatabaseEntry
{
    
    public void UpdateEntryData(RPGBArmorSlot newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
