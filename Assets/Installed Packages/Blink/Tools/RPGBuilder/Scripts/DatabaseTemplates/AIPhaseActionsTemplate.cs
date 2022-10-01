using System.Collections.Generic;
using BLINK.RPGBuilder.AI;

namespace BLINK.RPGBuilder.Templates
{
    public class AIPhaseActionsTemplate : RPGBuilderDatabaseEntry
    {
        public List<AIData.AIPhaseAction> PhaseActions = new List<AIData.AIPhaseAction>();
        
        public void UpdateEntryData(AIPhaseActionsTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            PhaseActions = newEntryData.PhaseActions;
        }
    }
}
