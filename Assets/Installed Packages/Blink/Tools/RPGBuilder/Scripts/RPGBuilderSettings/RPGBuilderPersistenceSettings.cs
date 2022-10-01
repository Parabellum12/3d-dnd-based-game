using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGBuilderPersistenceSettings : RPGBuilderDatabaseEntry
{
    [Serializable]
    public class PersistentPrefab
    {
        public string name;
        public GameObject prefab;
        public int ID = -1;
    }

    public List<PersistentPrefab> prefabList = new List<PersistentPrefab>();
    
    public void UpdateEntryData(RPGBuilderPersistenceSettings newEntryData)
    {
        prefabList = newEntryData.prefabList;
    }
}
