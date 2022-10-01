using System.Collections.Generic;
using BLINK.RPGBuilder.AI;

namespace BLINK.RPGBuilder.Templates
{
    public class AIPhaseAbilitiesTemplate : RPGBuilderDatabaseEntry
    {
        public bool CheckMaxAbilities;
        public int MaxAbilities = 1;

        public List<AIData.AIPhaseAbility> Abilities = new List<AIData.AIPhaseAbility>();
        
        public void UpdateEntryData(AIPhaseAbilitiesTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            CheckMaxAbilities = newEntryData.CheckMaxAbilities;
            MaxAbilities = newEntryData.MaxAbilities;
            Abilities = newEntryData.Abilities;
        }
    }
}
