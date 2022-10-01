using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBGender : RPGBuilderDatabaseEntry
{
    public Sprite background;
    public Color color = Color.white;
    
    public void UpdateEntryData(RPGBGender newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        background = newEntryData.background;
        color = newEntryData.color;
    }
}
