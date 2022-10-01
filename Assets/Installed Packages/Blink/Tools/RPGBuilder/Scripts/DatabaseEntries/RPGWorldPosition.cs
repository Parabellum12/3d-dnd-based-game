using UnityEngine;

public class RPGWorldPosition : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;

    
    public Vector3 position;
    public bool useRotation;
    public Vector3 rotation;

    public void UpdateEntryData(RPGWorldPosition newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        position = newEntryData.position;
        useRotation = newEntryData.useRotation;
        rotation = newEntryData.rotation;
    }
}