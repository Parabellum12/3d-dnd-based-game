using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBWeaponSlot : RPGBuilderDatabaseEntry
{
    
    public void UpdateEntryData(RPGBWeaponSlot newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
