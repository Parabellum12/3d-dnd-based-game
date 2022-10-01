using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBWeaponHandSlot : RPGBuilderDatabaseEntry
{
    
    public void UpdateEntryData(RPGBWeaponHandSlot newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
    }
}
