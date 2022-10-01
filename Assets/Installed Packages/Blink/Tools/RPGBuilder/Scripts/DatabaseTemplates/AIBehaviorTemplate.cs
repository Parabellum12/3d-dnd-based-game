using System.Collections.Generic;
using BLINK.RPGBuilder.AI;

namespace BLINK.RPGBuilder.Templates
{
    public class AIBehaviorTemplate : RPGBuilderDatabaseEntry
    {
        public AIStateIdle DefaultState;
        public AIStateIdleTemplate DefaultStateTemplate;
        public AIStateIdle DefaultPetState;
        public AIStateIdleTemplate DefaultPetStateTemplate;
        
        public AIStateChase ChaseState;
        public AIStateChaseTemplate ChaseTemplate;
        
        public AIStateCombatIdle CombatIdleState;
        public AIStateCombatIdleTemplate CombatIdleTemplate;
        
        public AIStateCombat CombatState;
        public AIStateCombatTemplate CombatTemplate;
        
        public AIStateWalkBackward WalkBackwardState;
        public AIStateWalkBackwardTemplate WalkBackwardTemplate;
        
        public AIStateFlee FleeState;
        public AIStateFleeTemplate FleeTemplate;
        public RequirementsTemplate FleeRequirementsTemplate;
        public float FleeCheckInterval = 3;
        
        public List<AIData.AIPhasePotentialAbilities> PotentialAbilities = new List<AIData.AIPhasePotentialAbilities>();

        public float MaxDistanceFromTarget = 3;
        public float MinDistanceFromTarget = 2;

        public float LookAtTargetSpeed = 50;
        public float CheckTargetInterval = 0.5f;

        public bool ResetTargetAfterDistanceFromSpawner;
        public float SpawnerDistanceMax = 30;
        public bool ResetTargetAfterDistanceFromSpawnpoint;
        public float SpawnPointDistanceMax = 30;
        
        
        public string StrafeParameterName = "Strafing";
        public string CombatParameterName = "InCombatState";

        public bool CanAggroAlly, CanAggroNeutral, CanAggroEnemy = true;
        public float PhaseCheckInterval = 3;
        
        public void UpdateEntryData(AIBehaviorTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            CanAggroAlly = newEntryData.CanAggroAlly;
            CanAggroNeutral = newEntryData.CanAggroNeutral;
            CanAggroEnemy = newEntryData.CanAggroEnemy;
            PhaseCheckInterval = newEntryData.PhaseCheckInterval;
            
            PotentialAbilities = newEntryData.PotentialAbilities;
            
            DefaultState = newEntryData.DefaultState;
            DefaultStateTemplate = newEntryData.DefaultStateTemplate;
            DefaultPetState = newEntryData.DefaultPetState;
            DefaultPetStateTemplate = newEntryData.DefaultPetStateTemplate;
            ChaseState = newEntryData.ChaseState;
            ChaseTemplate = newEntryData.ChaseTemplate;
            CombatIdleState = newEntryData.CombatIdleState;
            CombatIdleTemplate = newEntryData.CombatIdleTemplate;
            CombatState = newEntryData.CombatState;
            CombatTemplate = newEntryData.CombatTemplate;
            WalkBackwardState = newEntryData.WalkBackwardState;
            WalkBackwardTemplate = newEntryData.WalkBackwardTemplate;
            FleeState = newEntryData.FleeState;
            FleeTemplate = newEntryData.FleeTemplate;
            FleeCheckInterval = newEntryData.FleeCheckInterval;
            FleeRequirementsTemplate = newEntryData.FleeRequirementsTemplate;
            
            ResetTargetAfterDistanceFromSpawner = newEntryData.ResetTargetAfterDistanceFromSpawner;
            SpawnerDistanceMax = newEntryData.SpawnerDistanceMax;
            ResetTargetAfterDistanceFromSpawnpoint = newEntryData.ResetTargetAfterDistanceFromSpawnpoint;
            SpawnPointDistanceMax = newEntryData.SpawnPointDistanceMax;
            
            MaxDistanceFromTarget = newEntryData.MaxDistanceFromTarget;
            MinDistanceFromTarget = newEntryData.MinDistanceFromTarget;
            LookAtTargetSpeed = newEntryData.LookAtTargetSpeed;
            CheckTargetInterval = newEntryData.CheckTargetInterval;
            StrafeParameterName = newEntryData.StrafeParameterName;
            CombatParameterName = newEntryData.CombatParameterName;
        }
    }
}
