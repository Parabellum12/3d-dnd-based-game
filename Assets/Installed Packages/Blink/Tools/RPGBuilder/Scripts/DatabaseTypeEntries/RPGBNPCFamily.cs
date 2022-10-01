using UnityEngine;

public class RPGBNPCFamily : RPGBuilderDatabaseEntry
{
    public Sprite icon;
    public Color color = Color.white;
    
    public void UpdateEntryData(RPGBNPCFamily newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        icon = newEntryData.icon;
        color = newEntryData.color;
    }
}
