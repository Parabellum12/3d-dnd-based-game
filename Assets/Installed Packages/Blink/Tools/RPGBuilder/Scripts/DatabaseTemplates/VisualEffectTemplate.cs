using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Templates
{
    public class VisualEffectTemplate : RPGBuilderDatabaseEntry
    {
        public List<GameObject> Prefabs = new List<GameObject>();
        
        public List<SoundTemplate> SoundTemplates = new List<SoundTemplate>();
        public bool ParentSoundToPrefab;
        public bool IsDestroyedOnDeath = true;
        public bool IsDestroyedOnStun = true;
        public bool IsDestroyedOnStealth;
        public bool IsDestroyedOnStealthEnd;
        
        public void UpdateEntryData(VisualEffectTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            Prefabs = newEntryData.Prefabs;
            SoundTemplates = newEntryData.SoundTemplates;
            ParentSoundToPrefab = newEntryData.ParentSoundToPrefab;
            IsDestroyedOnDeath = newEntryData.IsDestroyedOnDeath;
            IsDestroyedOnStun = newEntryData.IsDestroyedOnStun;
            IsDestroyedOnStealth = newEntryData.IsDestroyedOnStealth;
            IsDestroyedOnStealthEnd = newEntryData.IsDestroyedOnStealthEnd;
        }
    }
}
