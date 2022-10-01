using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBEffectTag : RPGBuilderDatabaseEntry
{
    
    public void UpdateEntryData(RPGBEffectTag newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
