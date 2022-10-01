using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBGemSocketType : RPGBuilderDatabaseEntry
{
    public void UpdateEntryData(RPGBGemSocketType newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
