using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Combat
{
    public static class CombatUtilities
    {
        #region GET STAT INFO
        
        public static float GetCurrentStatValue(CombatEntity entity, int statID)
        {
            if (entity.GetStats().ContainsKey(statID)) return entity.GetStats()[statID].currentValue;
            return 0;
        }
        public static void SetCurrentStatValue(CombatEntity entity, int statID, float newValue)
        {
            entity.GetStats()[statID].currentValue = newValue;
        }
        
        public static float GetMaxStatValue(CombatEntity entity, int statID)
        {
            if (entity.GetStats().ContainsKey(statID)) return entity.GetStats()[statID].currentMaxValue;
            return 0;
        }
        public static float SetMaxStatValue(CombatEntity entity, int statID, float newValue)
        {
            if (entity.GetStats().ContainsKey(statID)) return entity.GetStats()[statID].currentMaxValue = newValue;
            return 0;
        }
        
        public static float GetMinStatValue(CombatEntity entity, int statID)
        {
            if (entity.GetStats().ContainsKey(statID)) return entity.GetStats()[statID].currentMinValue;
            return 0;
        }
        public static float SetMinStatValue(CombatEntity entity, int statID, float newValue)
        {
            if (entity.GetStats().ContainsKey(statID)) return entity.GetStats()[statID].currentMinValue = newValue;
            return 0;
        }

        public static float GetCurrentStatTypeValue(CombatEntity entity, RPGStat.STAT_TYPE statType)
        {
            float total = 0;
            foreach (var stat in entity.GetStats())
            {
                foreach (var bonus in stat.Value.stat.statBonuses)
                {
                    if (bonus.statType != statType) continue;
                    total += stat.Value.currentValue * bonus.modifyValue;
                }
            }

            return total;
        }
        
        public static float GetCurrentStatValue(CombatEntity entity, int statID, string statType)
        {
            foreach (var t in entity.GetStats())
                if (t.Value.stat.ID == statID)
                    foreach (var t1 in t.Value.stat.statBonuses)
                        if (t1.statType.ToString() == statType)
                            return t.Value.currentValue * t1.modifyValue;
            return 0;
        }
        
        public static int GetStatIndexFromID(CombatEntity entity, int statID)
        {
            for (var i = 0; i < entity.GetStats().Count; i++)
                if (entity.GetStats()[i].stat.ID == statID)
                    return i;
            return -1;
        }
        
        public static float GetTotalOfStatType(CombatEntity entity, RPGStat.STAT_TYPE statType)
        {
            float total = 0;
            foreach (var t in entity.GetStats())
            {
                total += t.Value.stat.statBonuses.Where(t1 => t1.statType == statType).Sum(t1 => t.Value.currentValue * t1.modifyValue);
            }

            return total;
        }
        
        public static float GetTotalOfStatFunction(CombatEntity entity, RPGStat.STAT_TYPE statType, RPGBDamageType damageType)
        {
            float total = 0;
            foreach (var t in entity.GetStats())
            {
                float sum = t.Value.stat.statBonuses.Where(t1 => t1.statType == statType && t1.CustomDamageType == damageType).Sum(t1 => t.Value.currentValue * t1.modifyValue);
                total += sum;
            }

            return total;
        }
        
        public static void UpdateCurrentStatValue(CombatEntity entity, int statID, float amount)
        {
            entity.GetStats()[statID].currentValue += amount;
            StatCalculator.ClampStat(entity.GetStats()[statID].stat, entity);

            if (entity.GetStats()[statID].stat.isVitalityStat)
            {
                CombatManager.Instance.HandleVitalityStatActions(entity, entity.GetStats()[statID].stat);
            }

            CombatEvents.Instance.OnStatValueChanged(entity, entity.GetStats()[statID].stat, entity.GetStats()[statID].currentValue, entity.GetStats()[statID].currentMaxValue);
            CombatEvents.Instance.OnStatsChanged(entity);
        }
        
        public static float GetTotalShiftAmount(CombatEntity entity, RPGStat shiftedStat)
        {
            float total = 0;
            foreach (var t in entity.GetStats())
            {
                foreach (var t1 in t.Value.stat.statBonuses)
                {
                    switch (t1.statType)
                    {
                        case RPGStat.STAT_TYPE.VITALITY_REGEN:
                            if (t1.statID == shiftedStat.ID)
                            {
                                total += t.Value.currentValue * t1.modifyValue;
                            }

                            break;
                    }
                }
            }

            return total;
        }
        
        public static bool StatIsOverridenByNPC(RPGNpc npc, int statID)
        {
            foreach (var t in npc.CustomStats)
                if (t.statID == statID)
                    return true;

            return false;
        }
        
        public static int GetOverridenNPCStatIndex(RPGNpc npc, int statID)
        {
            for (var i = 0; i < npc.CustomStats.Count; i++)
                if (npc.CustomStats[i].statID == statID)
                    return i;
            return -1;
        }
        
        public static bool StatHasDeathCondition(RPGStat stat)
        {
            foreach (var vitalityAction in stat.vitalityActions)
            {
                if(vitalityAction.GameActionsTemplate == null) continue;
                foreach (var gameAction in vitalityAction.GameActionsTemplate.GameActions)
                {
                    if (gameAction.type != GameActionsData.GameActionType.Death) continue;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region GET EFFECTS INFO
        public static bool IsEffectActiveOnTarget(RPGEffect effectRequired, CombatEntity target)
        {
            return target.GetStates().Any(effect => effect.stateEffect == effectRequired);
        }
        public static bool IsEffectActiveOnTarget(CombatEntity target, int effectID)
        {
            return target.GetStates().Any(effect => effect.stateEffect.ID == effectID);
        }
        public static bool IsEffectTypeActiveOnTarget(CombatEntity target, RPGEffect.EFFECT_TYPE type)
        {
            return target.GetStates().Any(effect => effect.stateEffect.effectType == type);
        }
        public static bool IsEffectTagActiveOnTarget(CombatEntity target, RPGBEffectTag tag)
        {
            return target.GetStates().Any(effect => effect.stateEffect.EffectTag == tag);
        }
        
        public static int GetEffectStacks(CombatEntity target, int effectID)
        {
            foreach (var state in target.GetStates())
            {
                if(state.stateEffectID != effectID) continue;
                return state.curStack;
            }

            return 0;
        }

        #endregion

        #region GET ABILITIES INFO
        public static bool AbilityHasTag(RPGAbility.RPGAbilityRankData abilityRank, RPGAbility.ABILITY_TAGS tag)
        {
            foreach (var t in abilityRank.tagsData)
            {
                if (t.tag == tag) return true;
            }

            return false;
        }

        public static bool IsAbilityKnown(int abilityID)
        {
            foreach (var ability in Character.Instance.CharacterData.Abilities)
            {
                if (ability.ID != abilityID) continue;
                return ability.known;
            }

            return false;
        }

        #endregion
    }
}
