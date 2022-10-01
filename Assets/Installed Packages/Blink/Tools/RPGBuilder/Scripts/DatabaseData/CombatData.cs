using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

namespace BLINK.RPGBuilder.Combat
{
    public static class CombatData
    {
        public enum EntityAlignment
        {
            Ally = 0,
            Neutral = 1,
            Enemy = 2
        }
        
        #region STATS
        [Serializable]
        public class CombatEntityStat
        {
            public string name;
            public RPGStat stat;
            public float currentMinValue;
            public float currentMaxValue;
            public float currentValue;
            public float nextCombatShift, nextRestShift;
            public float valueFromItem;
            public float valueFromBonus;
            public float valueFromEffect;
            public float valueFromShapeshifting;
            public float valueFromMount;
            public float valueFromLinkedStats;
        }
        
        [Serializable]
        public class VitalityStatEntry
        {
            public string StatName;
            public int StatID;
            public float value;
        }
        
        [Serializable]
        public class CustomStatValues
        {
            [StatID] public int statID = -1;
            public bool overrideMinValue;
            public float minValue;
            public bool overrideMaxValue;
            public float maxValue;
            public bool overrideStartPercentage;
            public float startPercentage = 100f;
            public float addedValue;
            public float valuePerLevel;
            public bool Percent;
            public float chance = 100;
            [RPGDataList] public List<RPGStat.VitalityActions> vitalityActions = new List<RPGStat.VitalityActions>();
        }

        #endregion

        #region STATES

        [Serializable]
        public class CombatEntityStateEffect
        {
            public string stateName;
            public CombatEntity casterEntity;
            public RPGEffect stateEffect;
            public int stateEffectID;
            public RPGEffect.RPGEffectRankData effectRank;
            public int effectRankIndex;
            public RPGAbility.RPGAbilityRankData abilityRank;
            public Sprite stateIcon;
            public int maxPulses;
            public int curPulses;
            public float nextPulse;
            public float pulseInterval;
            public float stateMaxDuration;
            public float stateCurDuration;
            public int curStack;
            public int maxStack;
            public GameObject stateEffectGO;
        }

        #endregion
        
        #region ABILITIES
        [Serializable]
        public class ActiveToggledAbilities
        {
            public RPGAbility ability;
            public RPGAbility.RPGAbilityRankData rank;
            public float nextTrigger;
        }
        
        public class AutoAttackData
        {
            public int CurrentAutoAttackAbilityID = -1;
            public RPGItem WeaponItem;
        }
        
        [Serializable]
        public class AbilityAction
        {
            public RPGCombatDATA.TARGET_TYPE RequirementsTarget = RPGCombatDATA.TARGET_TYPE.Caster;
            public RequirementsTemplate RequirementsTemplate;
            
            public RPGCombatDATA.TARGET_TYPE ActionsTarget;
            public GameActionsTemplate GameActionsTemplate;
        }

        #region COMBOS

        [Serializable]
        public class ActiveCombo
        {
            public RPGCombo combo;
            public int initialAbilityID;
            public int comboIndex;
            public float curTime, expireTime;
            public float curLoadTime, readyTime;
            public ComboSlot UISlotREF;
            public KeyCode keyRequired = KeyCode.None;
        }
        

        #endregion

        #endregion
        
        #region NPCs

        [Serializable]
        public class PersistentNPCEntry
        {
            public string NPCName;
            public int ID;
            public Vector3 position;
            public Vector3 rotation;

            public List<VitalityStatEntry> VitalityStats = new List<VitalityStatEntry>();
        }

        [Serializable]
        public class NPCPickData
        {
            public RPGNpc npc;
            public bool Persistent;
        }

        #endregion
        
        #region PETS

        public enum PetMovementActionTypes
        {
            Follow,
            Stay
        }

        public enum PetCombatActionTypes
        {
            Defend,
            Aggro
        }

        #endregion

        #region ACTIVE BLOCKING

        [Serializable]
        public class ActiveBlockingState
        {
            public float curBlockChargeTime = 0;
            public float targetBlockChargeTime = 0;
            public float blockDurationLeft = 0;
            public float curBlockPowerFlat = 0;
            public float nextBlockStatDrain;
            public int curBlockHitCount = 0;
            public float curBlockPowerModifier = 0;
            public int curBlockedDamageLeft = 0;
            public bool blockIsDoneCharging = false;
            public float cachedBlockMaxDuration = 0;
            public RPGEffect effect;
            public RPGEffect.RPGEffectRankData effectRank;
        }

        #endregion
    }
}
