using System.Collections.Generic;
using BLINK.RPGBuilder.AI;

namespace BLINK.RPGBuilder.Templates
{
    public class AIPhaseTemplate : RPGBuilderDatabaseEntry
    {
        public float TransitionDuration = 1;
        public RequirementsTemplate EnterPhaseRequirementsTemplate;
        public List<AIData.AIPhasePotentialBehavior> PotentialBehaviors = new List<AIData.AIPhasePotentialBehavior>();
        public AIPhaseActionsTemplate ActionsTemplate;

        public AnimationTemplate SpawnedAnimationTemplate;
        
        public List<VisualEffectEntry> VisualEffectEntries = new List<VisualEffectEntry>();
        public List<AnimationEntry> AnimationEntries = new List<AnimationEntry>();
        public List<SoundEntry> SoundEntries = new List<SoundEntry>();
        
        public void UpdateEntryData(AIPhaseTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            TransitionDuration = newEntryData.TransitionDuration;
            EnterPhaseRequirementsTemplate = newEntryData.EnterPhaseRequirementsTemplate;
            PotentialBehaviors = newEntryData.PotentialBehaviors;
            ActionsTemplate = newEntryData.ActionsTemplate;
            VisualEffectEntries = newEntryData.VisualEffectEntries;
            AnimationEntries = newEntryData.AnimationEntries;
            SoundEntries = newEntryData.SoundEntries;
            SpawnedAnimationTemplate = newEntryData.SpawnedAnimationTemplate;
        }
    }
}
