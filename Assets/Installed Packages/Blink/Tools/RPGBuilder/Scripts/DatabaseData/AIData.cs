using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    public class AIData
    {
        public enum SpawnerType
        {
            Endless,
            Limited,
            Manual
        }
        
        [Serializable]
        public class ThreatTableData
        {
            public int threatAmount;
        }
        
        [Serializable]
        public class ActiveState
        {
            public AIState State;
            public AIStateTemplate StateTemplate;
            public Coroutine StateLoop;
        }
        
        [Serializable]
        public class AIPhase
        {
            public AIPhaseTemplate PhaseTemplate;
            public NPCPresetTemplate Preset;
        }
        
        [Serializable]
        public class AIActiveAbility
        {
            public RPGAbility ability;
            public int abilityRank;
            public float nextUse;
            public int usedAmount;
            public float DistanceRequired;
            public bool AllyAbility;
        }
        
        [Serializable]
        public class AIPhaseAction
        {
            public ActivationType Activation;
            public RPGCombatDATA.TARGET_TYPE RequirementsTarget = RPGCombatDATA.TARGET_TYPE.Caster;
            public RequirementsTemplate RequirementsTemplate;
            
            public RPGCombatDATA.TARGET_TYPE ActionsTarget;
            public GameActionsTemplate GameActionsTemplate;
        }
        
        [Serializable]
        public class AIPhaseAbility
        {
            public int abilityID = -1;
            public int abilityRank;

            public float chance = 100;
            public RequirementsTemplate RequirementsTemplate;
            public bool optional;

            public bool LimitedUse;
            public int MaxUseAmount = 1;
        }
        
        [Serializable]
        public class AIPhasePotentialBehavior
        {
            public AIBehaviorTemplate BehaviorTemplate;
            public float chance = 100;
        }
        
        [Serializable]
        public class AIPhasePotentialAbilities
        {
            public AIPhaseAbilitiesTemplate AbilitiesTemplate;
            public float chance = 100;
        }

        public enum ActivationType
        {
            Enter = 0,
            Exit = 1
        }
        
        
        public enum AggroLinkType
        {
            NPC = 0,
            Family = 1
        }
        
        public enum NPCColliderType
        {
            Capsule,
            Sphere,
            Box
        }
        
        [Serializable]
        public class NPCMerchantTable
        {
            public int MerchantTableID = -1;
            public RequirementsTemplate RequirementsTemplate;
        }
    }
}