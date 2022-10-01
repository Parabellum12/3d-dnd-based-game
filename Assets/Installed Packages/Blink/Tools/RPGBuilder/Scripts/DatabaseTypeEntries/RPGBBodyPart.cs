using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBBodyPart : RPGBuilderDatabaseEntry
{
    public void UpdateEntryData(RPGBBodyPart newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
    }
}
