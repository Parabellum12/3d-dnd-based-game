using UnityEngine;

public class RPGBHealingType : RPGBuilderDatabaseEntry
{
    public Color color = Color.white;
    public void UpdateEntryData(RPGBHealingType newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        color = newEntryData.color;
    }
}
