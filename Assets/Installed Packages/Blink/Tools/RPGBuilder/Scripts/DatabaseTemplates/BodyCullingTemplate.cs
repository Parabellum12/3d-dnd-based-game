using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Templates
{
    public class BodyCullingTemplate : RPGBuilderDatabaseEntry
    {
        [RPGDataList] public List<HiddenBodyPart> HiddenBodyParts = new List<HiddenBodyPart>();
        
        public void UpdateEntryData(BodyCullingTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            HiddenBodyParts = newEntryData.HiddenBodyParts;
        }
    }
}
