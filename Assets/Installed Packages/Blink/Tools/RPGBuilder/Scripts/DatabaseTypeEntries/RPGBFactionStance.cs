using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBFactionStance : RPGBuilderDatabaseEntry
{
    public Color color = Color.white;
    
    public void UpdateEntryData(RPGBFactionStance newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        color = newEntryData.color;
    }
}
