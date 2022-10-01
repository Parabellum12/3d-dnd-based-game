using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using UnityEngine;

namespace BLINK.RPGBuilder.Templates
{
    public class StatListTemplate : RPGBuilderDatabaseEntry
    {
        [RPGDataList] public List<CombatData.CustomStatValues> CustomStats = new List<CombatData.CustomStatValues>();
        
        public void UpdateEntryData(StatListTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            CustomStats = newEntryData.CustomStats;
        }
        
        public List<CombatData.CustomStatValues> OverrideRequirements(List<CombatData.CustomStatValues> statList)
        {
            statList = new List<CombatData.CustomStatValues>();

            foreach (var stat in CustomStats)
            {
                CombatData.CustomStatValues newStat = new CombatData.CustomStatValues
                {
                    statID = stat.statID,
                    addedValue = stat.addedValue,
                    valuePerLevel = stat.valuePerLevel,
                    overrideMinValue = stat.overrideMinValue,
                    minValue = stat.minValue,
                    overrideMaxValue = stat.overrideMaxValue,
                    maxValue = stat.maxValue,
                    overrideStartPercentage = stat.overrideStartPercentage,
                    startPercentage = stat.startPercentage,
                    chance = stat.chance,
                };

                foreach (var newData in stat.vitalityActions)
                {
                    RPGStat.VitalityActions vitAction = new RPGStat.VitalityActions
                    {
                        GameActionsTemplate = newData.GameActionsTemplate,
                        value = newData.value,
                        valueType = newData.valueType,
                        isPercent = newData.isPercent,
                    };
                    newStat.vitalityActions.Add(vitAction);
                }

                statList.Add(newStat);
            }
            return statList;
        }
    }
}
