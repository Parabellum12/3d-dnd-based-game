using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBStatCategory : RPGBuilderDatabaseEntry
{
    public void UpdateEntryData(RPGBStatCategory newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
