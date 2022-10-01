using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBWeaponType : RPGBuilderDatabaseEntry
{
    
    public void UpdateEntryData(RPGBWeaponType newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
