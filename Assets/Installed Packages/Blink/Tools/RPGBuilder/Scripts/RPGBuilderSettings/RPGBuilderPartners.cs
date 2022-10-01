using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGBuilderPartners : RPGBuilderDatabaseEntry
{
    [Serializable]
    public class Partner
    {
        public string name;
        public string description;
        public string link;
        public string email;
        public Sprite image;
    }
    public List<Partner> partners = new List<Partner>();
    
    public void UpdateEntryData(RPGBuilderPartners newEntryData)
    {

    }
}
