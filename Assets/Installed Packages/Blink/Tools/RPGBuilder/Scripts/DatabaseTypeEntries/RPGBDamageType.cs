using UnityEngine;

public class RPGBDamageType : RPGBuilderDatabaseEntry
{
    public Color color = Color.white;
    public void UpdateEntryData(RPGBDamageType newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        color = newEntryData.color;
    }
}
