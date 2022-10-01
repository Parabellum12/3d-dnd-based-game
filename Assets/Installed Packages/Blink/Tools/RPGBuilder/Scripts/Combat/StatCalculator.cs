using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Logic
{
    public static class StatCalculator
    {

        private class TemporaryStatsDATA
        {
            public RPGStat stat;
            public float value;
        }
        
        private static List<TemporaryStatsDATA> tempStatList = new List<TemporaryStatsDATA>();

        public enum TemporaryStatSourceType
        {
            none,
            item,
            effect,
            bonus,
            shapeshifting,
            mount
        }
        
        public class TemporaryActiveGearSetsDATA
        {
            public RPGGearSet gearSet;
            public int activeTierIndex;
        }

        public class VitalityStatState
        {
            public int statID;
            public float percent;
        }
        
        public class CustomStatsGroup
        {
            public List<CombatData.CustomStatValues> stats = new List<CombatData.CustomStatValues>();
            public int level;
        }
        
        private static void TriggerStatEvents(CombatEntity combatEntity, int statID)
        {
            CombatEvents.Instance.OnStatValueChanged(combatEntity, combatEntity.GetStats()[statID].stat, 
                combatEntity.GetStats()[statID].currentValue, combatEntity.GetStats()[statID].currentMaxValue);
        }
        
        public static void TriggerMovementSpeedChange(CombatEntity combatEntity)
        {
            CombatEvents.Instance.OnMovementSpeedChanged(combatEntity, CombatUtilities.GetTotalOfStatType(combatEntity, RPGStat.STAT_TYPE.MOVEMENT_SPEED));
        }
        
        public static void ResetPlayerStatsAfterRespawn()
        {
            foreach (var playerStat in GameState.playerEntity.GetStats().Where(t => t.Value.stat.isVitalityStat))
            {
                playerStat.Value.currentValue = (int)(playerStat.Value.currentMaxValue * (playerStat.Value.stat.startPercentage/100f));
                TriggerStatEvents(GameState.playerEntity, playerStat.Value.stat.ID);
            }
        }

        public static void UpdateLevelUpStats()
        {
            int statsAffectingMovementSpeed = 0;
            
            var thisRace = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
            foreach (var raceStat in thisRace.CustomStats)
            {
                HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[raceStat.statID], GameState.playerEntity.GetStats()[raceStat.statID], raceStat.valuePerLevel, raceStat.Percent, TemporaryStatSourceType.none);
                TriggerStatEvents(GameState.playerEntity, raceStat.statID);
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(GameState.playerEntity.GetStats()[raceStat.statID].stat)) statsAffectingMovementSpeed++;
            }

            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                var thisClass = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                foreach (var classStat in thisClass.CustomStats)
                {
                    HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[classStat.statID], GameState.playerEntity.GetStats()[classStat.statID], classStat.valuePerLevel, classStat.Percent, TemporaryStatSourceType.none);
                    TriggerStatEvents(GameState.playerEntity, classStat.statID);
                    if (RPGBuilderUtilities.StatAffectsMoveSpeed(GameState.playerEntity.GetStats()[classStat.statID].stat)) statsAffectingMovementSpeed++;
                }
            }

            if (thisRace.speciesID != -1)
            {
                foreach (var speciesStat in GameDatabase.Instance.GetSpecies()[thisRace.speciesID].CustomStats)
                {
                    HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[speciesStat.statID], GameState.playerEntity.GetStats()[speciesStat.statID], speciesStat.valuePerLevel, speciesStat.Percent, TemporaryStatSourceType.none);
                    TriggerStatEvents(GameState.playerEntity, speciesStat.statID);
                    if (RPGBuilderUtilities.StatAffectsMoveSpeed(GameState.playerEntity.GetStats()[speciesStat.statID].stat)) statsAffectingMovementSpeed++;
                }
            }
            
            ResetLinkedStats(GameState.playerEntity, true);
            ProcessLinkedStats(GameState.playerEntity, true);
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(GameState.playerEntity);
            CombatEvents.Instance.OnStatsChanged(GameState.playerEntity);
        }

        
        public static void UpdateWeaponTemplateLevelUpStats(int weaponTemplateID)
        {
            var wpTemplate = GameDatabase.Instance.GetWeaponTemplates()[weaponTemplateID];
            int statsAffectingMovementSpeed = 0;
            int statChanged = 0;
            
            foreach (var weaponTemplateStat in wpTemplate.CustomStats.Where(weaponTemplateStat => weaponTemplateStat.valuePerLevel != 0))
            {
                HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[weaponTemplateStat.statID], GameState.playerEntity.GetStats()[weaponTemplateStat.statID], weaponTemplateStat.valuePerLevel, weaponTemplateStat.Percent, TemporaryStatSourceType.none);
                
                TriggerStatEvents(GameState.playerEntity, weaponTemplateStat.statID);
                
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(GameState.playerEntity.GetStats()[weaponTemplateStat.statID].stat)) statsAffectingMovementSpeed++;
                statChanged++;
            }

            if (statChanged > 0)
            {
                ResetLinkedStats(GameState.playerEntity, true);
                ProcessLinkedStats(GameState.playerEntity, true);
                if (statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(GameState.playerEntity);
                CombatEvents.Instance.OnStatsChanged(GameState.playerEntity);
            }
        }
        
        
        public static void UpdateSkillLevelUpStats(int skillID)
        {
            var thisSkill = GameDatabase.Instance.GetSkills()[skillID];
            int statsAffectingMovementSpeed = 0;
            int statChanged = 0;
            
            foreach (var skillStat in thisSkill.CustomStats.Where(skillStat => skillStat.valuePerLevel != 0))
            {
                HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[skillStat.statID], GameState.playerEntity.GetStats()[skillStat.statID], skillStat.valuePerLevel, skillStat.Percent, TemporaryStatSourceType.none);
                
                TriggerStatEvents(GameState.playerEntity, skillStat.statID);
                
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(GameState.playerEntity.GetStats()[skillStat.statID].stat)) statsAffectingMovementSpeed++;
                statChanged++;
            }

            if (statChanged > 0)
            {
                ResetLinkedStats(GameState.playerEntity, true);
                ProcessLinkedStats(GameState.playerEntity, true);
                if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(GameState.playerEntity);
                CombatEvents.Instance.OnStatsChanged(GameState.playerEntity);
            }
        }
        
        public static void UpdateStatAllocation(int statID, int amount)
        {
            HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[statID], GameState.playerEntity.GetStats()[statID], amount, false, TemporaryStatSourceType.none);
            
            TriggerStatEvents(GameState.playerEntity, statID);
            CombatEvents.Instance.OnStatsChanged(GameState.playerEntity);
            
            ResetLinkedStats(GameState.playerEntity, true);
            ProcessLinkedStats(GameState.playerEntity, true);
            if (RPGBuilderUtilities.StatAffectsMoveSpeed(GameState.playerEntity.GetStats()[statID].stat)) TriggerMovementSpeedChange(GameState.playerEntity);
        }

        private static List<CustomStatsGroup> GetCustomStats()
        {
            List<CustomStatsGroup> CustomStatsGroups = new List<CustomStatsGroup>();
            RPGRace raceEntry = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
            CustomStatsGroup raceStats = new CustomStatsGroup {level = Character.Instance.CharacterData.Level};
            foreach (var customStat in raceEntry.CustomStats)
            {
                raceStats.stats.Add(customStat);
            }
            CustomStatsGroups.Add(raceStats);
            
            foreach (var skill in Character.Instance.CharacterData.Skills)
            {
                RPGSkill skillEntry = GameDatabase.Instance.GetSkills()[skill.skillID];
                CustomStatsGroup skillStats = new CustomStatsGroup {level = RPGBuilderUtilities.getSkillLevel(skill.skillID)};
                foreach (var customStat in skillEntry.CustomStats)
                {
                    skillStats.stats.Add(customStat);
                }
                CustomStatsGroups.Add(skillStats);
            }
            
            foreach (var weaponTemplate in Character.Instance.CharacterData.WeaponTemplates)
            {
                RPGWeaponTemplate weaponTemplateEntry = GameDatabase.Instance.GetWeaponTemplates()[weaponTemplate.weaponTemplateID];
                CustomStatsGroup weaponTemplateStats = new CustomStatsGroup {level = RPGBuilderUtilities.getWeaponTemplateLevel(weaponTemplate.weaponTemplateID)};
                foreach (var customStat in weaponTemplateEntry.CustomStats)
                {
                    weaponTemplateStats.stats.Add(customStat);
                }
                CustomStatsGroups.Add(weaponTemplateStats);
            }
            
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                RPGClass classEntry = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                CustomStatsGroup classStats = new CustomStatsGroup {level = Character.Instance.CharacterData.Level};
                foreach (var customStat in classEntry.CustomStats)
                {
                    classStats.stats.Add(customStat);
                }
                CustomStatsGroups.Add(classStats);
            }

            if (raceEntry.speciesID != -1)
            {
                CustomStatsGroup speciesStats = new CustomStatsGroup {level = Character.Instance.CharacterData.Level};
                foreach (var customStat in GameDatabase.Instance.GetSpecies()[raceEntry.speciesID].CustomStats)
                {
                    speciesStats.stats.Add(customStat);
                }
                CustomStatsGroups.Add(speciesStats);
            }
            
            return CustomStatsGroups;
        }
        
        public static void InitCharacterStats()
        {
            List<VitalityStatState> vitalityStatStates = GetVitalityStatsStates(GameState.playerEntity);
            foreach (var StatsGroup in GetCustomStats())
            {
                foreach (var stat in StatsGroup.stats)
                {
                    float amount = stat.addedValue;
                    if (stat.valuePerLevel != 0)
                    {
                        amount += StatsGroup.level * stat.valuePerLevel;
                    }

                    if (stat.overrideMinValue)
                    {
                        GameState.playerEntity.GetStats()[stat.statID].currentMinValue = stat.minValue;
                    }
                    if (stat.overrideMaxValue)
                    {
                        GameState.playerEntity.GetStats()[stat.statID].currentMaxValue = stat.maxValue;
                    }
                    
                    HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[stat.statID], GameState.playerEntity.GetStats()[stat.statID], amount, stat.Percent, TemporaryStatSourceType.none);
                }
            }
            
            foreach (var allocatedStat in Character.Instance.CharacterData.AllocatedStats)
            {
                HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[allocatedStat.statID], GameState.playerEntity.GetStats()[allocatedStat.statID], allocatedStat.value+allocatedStat.valueGame, false, TemporaryStatSourceType.none);
            }
            
            foreach (var playerStat in GameState.playerEntity.GetStats())
            {
                CombatData.CombatEntityStat temp = GameModifierManager.Instance.GetStatValueAfterGameModifier("Combat+Stat+Settings", playerStat.Value, -1, true);
                
                if (playerStat.Value.stat.isVitalityStat)
                {
                    playerStat.Value.currentValue = temp.currentMaxValue * (playerStat.Value.stat.startPercentage/100f);
                }
                else
                {
                    playerStat.Value.currentValue = temp.currentValue;
                }
                
                if (playerStat.Value.stat.minCheck)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MinOverride, playerStat.Value.stat.ID);
                    playerStat.Value.currentValue = statOverride != -1 ? statOverride : temp.currentValue;
                }
                if (playerStat.Value.stat.maxCheck)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, playerStat.Value.stat.ID);
                    playerStat.Value.currentMaxValue = statOverride != -1 ? statOverride : temp.currentMaxValue;
                }
                
                HandleStat(GameState.playerEntity, playerStat.Value.stat, playerStat.Value, 0, false, TemporaryStatSourceType.none);
            }
            
            ResetLinkedStats(GameState.playerEntity, false);
            ProcessLinkedStats(GameState.playerEntity, false);
            TriggerMovementSpeedChange(GameState.playerEntity);

            ApplyVitalityStatsStates(vitalityStatStates, GameState.playerEntity);
            
            foreach (var stat in GameState.playerEntity.GetStats().Values)
            {
                TriggerStatEvents(GameState.playerEntity, stat.stat.ID);
            }
            CombatEvents.Instance.OnStatsChanged(GameState.playerEntity);
        }

        public static void ApplyVitalityStatsStates(List<VitalityStatState> vitalityStatStates, CombatEntity entity)
        {
            foreach (var vitState in vitalityStatStates)
            {
                RPGStat statREF = GameDatabase.Instance.GetStats()[vitState.statID];
                float newValue = CombatUtilities.GetMaxStatValue(entity, statREF.ID);
                newValue = (newValue / 100f) * vitState.percent;
                if (statREF.ID == GameDatabase.Instance.GetCombatSettings().HealthStatID && newValue < 1)
                {
                    newValue = 1;
                }
                CombatUtilities.SetCurrentStatValue(entity, statREF.ID, (int) newValue);

                ClampStat(statREF, entity);

                TriggerStatEvents(entity, statREF.ID);
            }
            CombatEvents.Instance.OnStatsChanged(entity);
        }

        public static void SetVitalityToMax()
        {
            foreach (var playerStat in GameState.playerEntity.GetStats().Where(playerStat => playerStat.Value.stat.isVitalityStat))
            {
                playerStat.Value.currentValue = (int)(playerStat.Value.currentMaxValue * (playerStat.Value.stat.startPercentage/100f));
            }
        }

        private static List<TemporaryStatsDATA> AddStatsToTemp (List<TemporaryStatsDATA> tempList, RPGStat statREF, float value)
        {
            foreach (var t in tempList.Where(t => t.stat == statREF))
            {
                t.value += value;
                return tempList;
            }

            TemporaryStatsDATA newTempStatData = new TemporaryStatsDATA {stat = statREF, value = value};
            tempList.Add(newTempStatData);
            return tempList;
        }

        public static void HandleStat(CombatEntity combatEntity, RPGStat stat, CombatData.CombatEntityStat entityStatData,  float amount, bool isPercent, TemporaryStatSourceType sourceType)
        {
            float addedValue = amount;
            if (isPercent)
            {
                tempStatList = AddStatsToTemp(tempStatList, stat, addedValue);
                return;
            }

            if (stat.isVitalityStat)
            {
                entityStatData.currentMaxValue += addedValue;
            }
            else
            {
                entityStatData.currentValue += addedValue;
            }

            switch (sourceType)
            {
                case TemporaryStatSourceType.item:
                    entityStatData.valueFromItem += addedValue;
                    break;
                case TemporaryStatSourceType.effect:
                    entityStatData.valueFromEffect += addedValue;
                    break;
                case TemporaryStatSourceType.bonus:
                    entityStatData.valueFromBonus += addedValue;
                    break;
                case TemporaryStatSourceType.shapeshifting:
                    entityStatData.valueFromShapeshifting += addedValue;
                    break;
                case TemporaryStatSourceType.mount:
                    entityStatData.valueFromMount += addedValue;
                    break;
            }

            ClampStat(stat, combatEntity);
        }

        public static void ProcessLinkedStats (CombatEntity entity, bool triggerEvents)
        {
            foreach (var stat in entity.GetStats().Values)
            {
                foreach (var bonus in stat.stat.statBonuses)
                {
                    if(bonus.statType != RPGStat.STAT_TYPE.VITALITY_BONUS) continue;
                    if(bonus.statID == -1) continue;
                    RPGStat linkedStat = GameDatabase.Instance.GetStats()[bonus.statID];
                    if (!linkedStat.isVitalityStat) continue;
                    float addedValue = stat.currentValue * bonus.modifyValue;
                    entity.GetStats()[bonus.statID].currentMaxValue += addedValue;
                    entity.GetStats()[bonus.statID].valueFromLinkedStats += addedValue;

                    ClampStat(linkedStat, entity);

                    if (triggerEvents)
                    {
                        TriggerStatEvents(entity, linkedStat.ID);
                        CombatEvents.Instance.OnStatsChanged(entity);
                    }
                }
            }
        }
        
        public static void ResetLinkedStats (CombatEntity entity, bool triggerEvents)
        {
            int statsAffectingMovementSpeed = 0;
            foreach (var linkedStat in entity.GetStats().Where(t2 => t2.Value.valueFromLinkedStats != 0))
            {
                if (linkedStat.Value.stat.isVitalityStat)
                {
                    linkedStat.Value.currentMaxValue -= linkedStat.Value.valueFromLinkedStats;
                }
                else
                {
                    linkedStat.Value.currentValue -= linkedStat.Value.valueFromLinkedStats;
                }

                linkedStat.Value.valueFromLinkedStats = 0;
                ClampStat(linkedStat.Value.stat, entity);

                if (triggerEvents)
                {
                    TriggerStatEvents(entity, linkedStat.Value.stat.ID);
                    CombatEvents.Instance.OnStatsChanged(entity);
                }
                
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(entity.GetStats()[linkedStat.Value.stat.ID].stat)) statsAffectingMovementSpeed++;
            }
            
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(entity);
        }
        

        private static void ResetItemStats()
        {
            int statsAffectingMovementSpeed = 0;
            foreach (var statFromItem in GameState.playerEntity.GetStats().Where(t2 => t2.Value.valueFromItem != 0))
            {
                if (statFromItem.Value.stat.isVitalityStat)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(
                        RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, statFromItem.Value.stat.ID);

                    if (statOverride != -1) statFromItem.Value.currentMaxValue = statOverride;
                    else statFromItem.Value.currentMaxValue -= statFromItem.Value.valueFromItem;
                }
                else
                {
                    statFromItem.Value.currentValue -= statFromItem.Value.valueFromItem;
                }

                statFromItem.Value.valueFromItem = 0;
                ClampStat(statFromItem.Value.stat, GameState.playerEntity);
                
                TriggerStatEvents(GameState.playerEntity, statFromItem.Value.stat.ID);
                CombatEvents.Instance.OnStatsChanged(GameState.playerEntity);
                
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(GameState.playerEntity.GetStats()[statFromItem.Value.stat.ID].stat)) statsAffectingMovementSpeed++;
            }
            
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(GameState.playerEntity);
        }

        private static void ResetBonusStats()
        {
            int statsAffectingMovementSpeed = 0;
            foreach (var statFromBonus in GameState.playerEntity.GetStats().Where(t2 => t2.Value.valueFromBonus != 0))
            {
                if (statFromBonus.Value.stat.isVitalityStat)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, statFromBonus.Value.stat.ID);
                    
                    if (statOverride != -1) statFromBonus.Value.currentMaxValue = statOverride;
                    else statFromBonus.Value.currentMaxValue -= statFromBonus.Value.valueFromBonus;
                }
                else
                {
                    statFromBonus.Value.currentValue -= statFromBonus.Value.valueFromBonus;
                }
                
                statFromBonus.Value.valueFromBonus = 0;
                ClampStat(statFromBonus.Value.stat, GameState.playerEntity);
                
                TriggerStatEvents(GameState.playerEntity, statFromBonus.Value.stat.ID);
                CombatEvents.Instance.OnStatsChanged(GameState.playerEntity);
                
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(GameState.playerEntity.GetStats()[statFromBonus.Value.stat.ID].stat)) statsAffectingMovementSpeed++;
            }
            
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(GameState.playerEntity);
        }

        private static void ResetEffectsStats(CombatEntity combatEntity)
        {
            int statsAffectingMovementSpeed = 0;
            
            foreach (var statFromEffect in combatEntity.GetStats().Where(t2 => t2.Value.valueFromEffect != 0))
            {
                if (statFromEffect.Value.stat.isVitalityStat)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(
                        RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, statFromEffect.Value.stat.ID);
                    
                    if (statOverride != -1) statFromEffect.Value.currentMaxValue = statOverride;
                    else statFromEffect.Value.currentMaxValue -= statFromEffect.Value.valueFromEffect;
                }
                else
                {
                    statFromEffect.Value.currentValue -= statFromEffect.Value.valueFromEffect;
                }

                statFromEffect.Value.valueFromEffect = 0;
                ClampStat(statFromEffect.Value.stat, combatEntity);
                
                TriggerStatEvents(combatEntity, statFromEffect.Value.stat.ID);
                CombatEvents.Instance.OnStatsChanged(combatEntity);
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(combatEntity.GetStats()[statFromEffect.Value.stat.ID].stat)) statsAffectingMovementSpeed++;
            }
            
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(combatEntity);
        }

        public static void ResetShapeshiftingStats(CombatEntity combatEntity)
        {
            int statsAffectingMovementSpeed = 0;
            
            foreach (var statFromShapeshifting in combatEntity.GetStats().Where(t2 => t2.Value.valueFromShapeshifting != 0))
            {
                if (statFromShapeshifting.Value.stat.isVitalityStat)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, statFromShapeshifting.Value.stat.ID);
                    
                    if (statOverride != -1) statFromShapeshifting.Value.currentMaxValue = statOverride;
                    else statFromShapeshifting.Value.currentMaxValue -= statFromShapeshifting.Value.valueFromShapeshifting;
                }
                else
                {
                    statFromShapeshifting.Value.currentValue -= statFromShapeshifting.Value.valueFromShapeshifting;
                }
                statFromShapeshifting.Value.valueFromShapeshifting = 0;
                ClampStat(statFromShapeshifting.Value.stat, combatEntity);
                
                TriggerStatEvents(combatEntity, statFromShapeshifting.Value.stat.ID);
                CombatEvents.Instance.OnStatsChanged(combatEntity);
                
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(combatEntity.GetStats()[statFromShapeshifting.Value.stat.ID].stat)) statsAffectingMovementSpeed++;
            }
            
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(combatEntity);
        }
        
        public static void ResetMountStats(CombatEntity combatEntity)
        {
            int statsAffectingMovementSpeed = 0;
            
            foreach (var statFromMount in combatEntity.GetStats().Where(t2 => t2.Value.valueFromMount != 0))
            {
                if (statFromMount.Value.stat.isVitalityStat)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, statFromMount.Value.stat.ID);
                    
                    if (statOverride != -1) statFromMount.Value.currentMaxValue = statOverride;
                    else statFromMount.Value.currentMaxValue -= statFromMount.Value.valueFromMount;
                }
                else
                {
                    statFromMount.Value.currentValue -= statFromMount.Value.valueFromMount;
                }
                statFromMount.Value.valueFromMount = 0;
                ClampStat(statFromMount.Value.stat, combatEntity);
                
                TriggerStatEvents(combatEntity, statFromMount.Value.stat.ID);
                CombatEvents.Instance.OnStatsChanged(combatEntity);
                
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(combatEntity.GetStats()[statFromMount.Value.stat.ID].stat)) statsAffectingMovementSpeed++;
            }
            
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(combatEntity);
        }

        private static List<EconomyData.EquippedItemData> getAllEquippedItems()
        {
            List<EconomyData.EquippedItemData> allItems = (from t in GameState.playerEntity.equippedArmors where t.item != null select new EconomyData.EquippedItemData {item = t.item, itemDataID = t.temporaryItemDataID}).ToList();
            allItems.AddRange(from t in GameState.playerEntity.equippedWeapons where t.item != null select new EconomyData.EquippedItemData {item = t.item, itemDataID = t.temporaryItemDataID});

            return allItems;
        }

        public static List<VitalityStatState> GetVitalityStatsStates(CombatEntity entity)
        {
            List<VitalityStatState> vitStates = new List<VitalityStatState>();
            foreach (var statData in entity.GetStats())
            {
                if (!statData.Value.stat.isVitalityStat) continue;
                VitalityStatState newVitStatState = new VitalityStatState
                {
                    statID = statData.Value.stat.ID,
                    percent = CombatUtilities.GetCurrentStatValue(entity, statData.Value.stat.ID) /
                              CombatUtilities.GetMaxStatValue(entity, statData.Value.stat.ID)
                };
                newVitStatState.percent *= 100;
                if (newVitStatState.percent < 1) newVitStatState.percent = 1;
                newVitStatState.percent = (int) newVitStatState.percent;
                vitStates.Add(newVitStatState);
            }

            return vitStates;
        }

        
        public static void CalculateItemStats()
        {
            tempStatList.Clear();
            List<VitalityStatState> vitalityStatStates = GetVitalityStatsStates(GameState.playerEntity);
            ResetItemStats();

            List<EconomyData.EquippedItemData> allEquippedItems = getAllEquippedItems();
            foreach (var equippedItem in allEquippedItems)
            {
                foreach (var equippedItemStat in equippedItem.item.stats)
                {
                    HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[equippedItemStat.statID], GameState.playerEntity.GetStats()[equippedItemStat.statID], 
                        equippedItemStat.amount, equippedItemStat.isPercent, TemporaryStatSourceType.item);
                }

                CharacterEntries.ItemEntry thisItemEntry = RPGBuilderUtilities.GetItemDataFromDataID(equippedItem.itemDataID);
                if (thisItemEntry != null)
                {
                    if (thisItemEntry.rdmItemID != -1)
                    {
                        int rdmItemIndex = RPGBuilderUtilities.getRandomItemIndexFromID(thisItemEntry.rdmItemID);
                        foreach (var itemRandomStat in equippedItem.item.randomStats)
                        {
                            foreach (var randomizedItemStat in Character.Instance.CharacterData.RandomizedItems[rdmItemIndex]
                                .randomStats)
                            {
                                HandleStat(GameState.playerEntity,
                                    GameDatabase.Instance.GetStats()[itemRandomStat.statID],
                                    GameState.playerEntity.GetStats()[itemRandomStat.statID],
                                    randomizedItemStat.statValue, itemRandomStat.isPercent,
                                    TemporaryStatSourceType.item);
                            }
                        }
                    }

                    if (thisItemEntry.enchantmentID != -1)
                    {
                        RPGEnchantment enchantmentREF =
                            GameDatabase.Instance.GetEnchantments()[thisItemEntry.enchantmentID];
                        if (enchantmentREF == null) continue;
                        foreach (var enchantmentStat in enchantmentREF
                            .enchantmentTiers[thisItemEntry.enchantmentTierIndex].stats)
                        {
                            HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[enchantmentStat.statID],
                                GameState.playerEntity.GetStats()[enchantmentStat.statID],
                                enchantmentStat.amount, enchantmentStat.isPercent, TemporaryStatSourceType.item);
                        }
                    }

                    foreach (var gemStat in from socket in thisItemEntry.sockets where socket.gemItemID != -1 select GameDatabase.Instance.GetItems()[socket.gemItemID] into gemItemRef where gemItemRef != null from gemStat in gemItemRef.gemData.gemStats select gemStat)
                    {
                        HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[gemStat.statID], GameState.playerEntity.GetStats()[gemStat.statID],
                            gemStat.amount, gemStat.isPercent, TemporaryStatSourceType.item);
                    }
                }
            }
            
            List<TemporaryActiveGearSetsDATA> activeGearSets = getActiveGearSets(allEquippedItems);
            foreach (var gearSet in activeGearSets)
            {
                for (int i = 0; i < gearSet.gearSet.gearSetTiers.Count; i++)
                {
                    if(i > gearSet.activeTierIndex) break;
                    foreach (var gearSetStat in gearSet.gearSet.gearSetTiers[i].gearSetTierStats)
                    {
                        HandleStat(GameState.playerEntity, GameDatabase.Instance.GetStats()[gearSetStat.statID],
                            GameState.playerEntity.GetStats()[gearSetStat.statID],
                            gearSetStat.amount, gearSetStat.isPercent, TemporaryStatSourceType.item);
                    }
                }
            }

            ProcessTempStatList(TemporaryStatSourceType.item, GameState.playerEntity);
            ResetLinkedStats(GameState.playerEntity, true);
            ProcessLinkedStats(GameState.playerEntity, true);
            
            ApplyVitalityStatsStates(vitalityStatStates, GameState.playerEntity);
            
            TriggerMovementSpeedChange(GameState.playerEntity);
        }

        public static void CalculateBonusStats()
        {
            tempStatList.Clear();
            ResetBonusStats();
            int statsAffectingMovementSpeed = HandleStatList(GameState.playerEntity, TemporaryStatSourceType.bonus);
            ProcessTempStatList(TemporaryStatSourceType.bonus, GameState.playerEntity);
            
            ResetLinkedStats(GameState.playerEntity, true);
            ProcessLinkedStats(GameState.playerEntity, true);
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(GameState.playerEntity);
        }

        public static void CalculateEffectsStats(CombatEntity combatEntity)
        {
            tempStatList.Clear();
            ResetEffectsStats(combatEntity);
            int statsAffectingMovementSpeed = HandleStatList(combatEntity, TemporaryStatSourceType.effect);
            ProcessTempStatList(TemporaryStatSourceType.effect, combatEntity);
            
            ResetLinkedStats(combatEntity, true);
            ProcessLinkedStats(combatEntity, true);
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(combatEntity);
        }

        public static void CalculateShapeshiftingStats(CombatEntity combatEntity)
        {
            tempStatList.Clear();
            ResetShapeshiftingStats(combatEntity);
            int statsAffectingMovementSpeed = HandleStatList(combatEntity, TemporaryStatSourceType.shapeshifting);
            ProcessTempStatList(TemporaryStatSourceType.shapeshifting, combatEntity);
            
            ResetLinkedStats(combatEntity, true);
            ProcessLinkedStats(combatEntity, true);
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(combatEntity);
        }
        
        public static void CalculateMountStats(CombatEntity combatEntity)
        {
            tempStatList.Clear();
            ResetMountStats(combatEntity);
            int statsAffectingMovementSpeed = HandleStatList(combatEntity, TemporaryStatSourceType.mount);
            ProcessTempStatList(TemporaryStatSourceType.mount, combatEntity);
            
            ResetLinkedStats(combatEntity, true);
            ProcessLinkedStats(combatEntity, true);
            if(statsAffectingMovementSpeed > 0) TriggerMovementSpeedChange(combatEntity);
        }

        private static int HandleStatList(CombatEntity combatEntity, TemporaryStatSourceType sourceType)
        {
            int statsAffectingMovementSpeed = 0;

            List<RPGEffect.STAT_EFFECTS_DATA> statEffects = new List<RPGEffect.STAT_EFFECTS_DATA>();
            switch (sourceType)
            {
                case TemporaryStatSourceType.effect:
                    foreach (var effect in combatEntity.GetStates())
                    {
                        if(effect.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Stat) continue;
                        foreach (var statEffectsData in effect.effectRank.statEffectsData)
                        {
                            statEffects.Add(statEffectsData);
                        }
                    }
                    break;
                case TemporaryStatSourceType.bonus:
                    foreach (var bonus in Character.Instance.CharacterData.Bonuses)
                    {
                        if(!bonus.On) continue;
                        foreach (var statEffectsData in GameDatabase.Instance.GetBonuses()[bonus.ID].ranks[bonus.rank-1].statEffectsData)
                        {
                            statEffects.Add(statEffectsData);
                        }
                    }
                    break;
                case TemporaryStatSourceType.shapeshifting:
                    foreach (var effect in combatEntity.GetStates())
                    {
                        if(effect.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Shapeshifting) continue;
                        foreach (var statEffectsData in effect.effectRank.statEffectsData)
                        {
                            statEffects.Add(statEffectsData);
                        }

                        break;
                    }
                    break;
                case TemporaryStatSourceType.mount:
                    foreach (var effect in combatEntity.GetStates())
                    {
                        if(effect.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Mount) continue;
                        foreach (var statEffectsData in effect.effectRank.statEffectsData)
                        {
                            statEffects.Add(statEffectsData);
                        }

                        break;
                    }
                    break;
            }

            foreach (var statData in statEffects)
            {
                HandleStat(combatEntity, GameDatabase.Instance.GetStats()[statData.statID], combatEntity.GetStats()[statData.statID], statData.statEffectModification, statData.isPercent, sourceType);
                TriggerStatEvents(combatEntity, statData.statID);
                CombatEvents.Instance.OnStatsChanged(combatEntity);
                if (RPGBuilderUtilities.StatAffectsMoveSpeed(GameDatabase.Instance.GetStats()[statData.statID])) statsAffectingMovementSpeed++;
            }

            return statsAffectingMovementSpeed;
        }

        private static void ProcessTempStatList(TemporaryStatSourceType sourceType, CombatEntity combatEntity)
        {
            foreach (var temporaryStat in tempStatList)
            {
                float addedValue;
                CombatData.CombatEntityStat entityStatData = combatEntity.GetStats()[temporaryStat.stat.ID];
                
                if (temporaryStat.stat.isVitalityStat)
                {
                    addedValue = entityStatData.currentValue * (temporaryStat.value / 100);

                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(
                        RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, entityStatData.stat.ID);
                    
                    if (statOverride != -1) entityStatData.currentMaxValue = statOverride;
                    else entityStatData.currentMaxValue += addedValue;
                }
                else
                {
                    addedValue = entityStatData.currentValue * (temporaryStat.value / 100);
                    entityStatData.currentValue += addedValue;
                }

                switch (sourceType)
                {
                    case TemporaryStatSourceType.item:
                        entityStatData.valueFromItem += addedValue;
                        break;
                    case TemporaryStatSourceType.effect:
                        entityStatData.valueFromEffect += addedValue;
                        break;
                    case TemporaryStatSourceType.bonus:
                        entityStatData.valueFromBonus += addedValue;
                        break;
                    case TemporaryStatSourceType.shapeshifting:
                        entityStatData.valueFromShapeshifting += addedValue;
                        break;
                    case TemporaryStatSourceType.mount:
                        entityStatData.valueFromMount += addedValue;
                        break;
                }

                ClampStat(temporaryStat.stat, combatEntity);
                
                TriggerStatEvents(combatEntity, temporaryStat.stat.ID);
                CombatEvents.Instance.OnStatsChanged(combatEntity);
            }
        }

        private static List<TemporaryActiveGearSetsDATA> getActiveGearSets(List<EconomyData.EquippedItemData> allEquippedItems)
        {
            List<TemporaryActiveGearSetsDATA> activeGearSets = new List<TemporaryActiveGearSetsDATA>();
            foreach (var t in allEquippedItems)
            {
                if (t.item != null && RPGBuilderUtilities.isItemPartOfGearSet(t.item.ID))
                {
                    var newSetData = RPGBuilderUtilities.getGearSetState(t.item.ID);
                    if (!ContainsGearSet(newSetData.gearSet, activeGearSets) && newSetData.activeTierIndex != -1)
                    {
                        activeGearSets.Add(newSetData);
                    }
                }
            }

            return activeGearSets;
        }
        
        private static bool ContainsGearSet(RPGGearSet newGearSet, List<TemporaryActiveGearSetsDATA> allActiveGearSets)
        {
            foreach (var activeGearSet in allActiveGearSets)
            {
                if (activeGearSet.gearSet.ID == newGearSet.ID) return true;
            }

            return false;
        }

        public static void ClampStat(RPGStat stat, CombatEntity combatEntity)
        {
            CombatData.CombatEntityStat nodeStat = combatEntity.GetStats()[stat.ID];
            
            if (stat.minCheck && nodeStat.currentValue < getMinValue(nodeStat)) nodeStat.currentValue = (int)nodeStat.currentMinValue;
            if (stat.maxCheck && nodeStat.currentValue > getMaxValue(nodeStat)) nodeStat.currentValue = (int)nodeStat.currentMaxValue;
        }

        private static float getMinValue(CombatData.CombatEntityStat nodeStat)
        {
            float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.Stat + "+" +
                RPGGameModifier.StatModifierType.MinOverride, nodeStat.stat.ID);
            return statOverride != -1 ? statOverride : nodeStat.currentMinValue;
        }

        private static float getMaxValue(CombatData.CombatEntityStat nodeStat)
        {
            float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.Stat + "+" +
                RPGGameModifier.StatModifierType.MaxOverride, nodeStat.stat.ID);
            return statOverride != -1 ? statOverride : nodeStat.currentMaxValue;
        }
    }
}
