using UnityEngine;

public class RPGBItemRarity : RPGBuilderDatabaseEntry
{
    public Sprite background;
    public Color color = Color.white;
    
    public void UpdateEntryData(RPGBItemRarity newEntryData)
    {
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        background = newEntryData.background;
        color = newEntryData.color;
    }
}
