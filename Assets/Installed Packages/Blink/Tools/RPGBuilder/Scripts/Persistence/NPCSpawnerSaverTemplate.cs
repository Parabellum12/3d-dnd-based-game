using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [Serializable]
    public class NPCSpawnerSaverTemplate : ObjectSaverTemplate
    {
        public int spawnedCount;
        public List<CombatData.PersistentNPCEntry> persistentNPCs = new List<CombatData.PersistentNPCEntry>();
    }
}
