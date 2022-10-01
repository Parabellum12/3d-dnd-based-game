using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBArmorType : RPGBuilderDatabaseEntry
{
    
    public void UpdateEntryData(RPGBArmorType newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
