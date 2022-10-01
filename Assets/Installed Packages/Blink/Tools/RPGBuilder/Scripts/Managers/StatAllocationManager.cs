using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class StatAllocationManager : MonoBehaviour
    {
        public static StatAllocationManager Instance { get; private set; }
    
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        public List<StatAllocationSlot> SpawnStatAllocationSlot(CharacterEntries.AllocatedStatEntry allocatedStatEntry, GameObject prefab, Transform parent, List<StatAllocationSlot> holderList, StatAllocationSlot.SlotType slotType)
        {
            if(allocateStatSlotHasStatID(holderList, allocatedStatEntry.statID)) return holderList;
            RPGStat statREF = GameDatabase.Instance.GetStats()[allocatedStatEntry.statID];
                
            GameObject newStatSlot = Instantiate(prefab, parent);
            newStatSlot.transform.localPosition = Vector3.zero;

            StatAllocationSlot dataHolder = newStatSlot.GetComponent<StatAllocationSlot>();
            dataHolder.thisStat = statREF;
            dataHolder.statNameText.text = statREF.entryDisplayName;
            dataHolder.slotType = slotType;
                
            holderList.Add(dataHolder);
            return holderList;
        }
        
        public void HandleStatAllocationButtons(int points, int maxPoints, List<StatAllocationSlot> curStatAllocationSlots, StatAllocationSlot.SlotType slotType)
        {
            if (points == 0)
            {
                foreach (var slot in curStatAllocationSlots)
                {
                    if (getAllocatedStatValue(slot.thisStat.ID, slotType) == 0)
                    {
                        slot.DecreaseButton.interactable = false;
                        slot.IncreaseButton.interactable = false;
                    }
                    else
                    {
                        slot.DecreaseButton.interactable = true;
                        slot.IncreaseButton.interactable = false;
                    }
                }
            }
            else if (points > 0 && points < maxPoints)
            {
                foreach (var slot in curStatAllocationSlots)
                {
                    if (getAllocatedStatValue(slot.thisStat.ID, slotType) == 0)
                    {
                        slot.DecreaseButton.interactable = false;
                        slot.IncreaseButton.interactable = true;
                    }
                    else
                    {
                        slot.DecreaseButton.interactable = true;
                        slot.IncreaseButton.interactable = true;
                    }
                }
            }
            else
            {
                foreach (var slot in curStatAllocationSlots)
                {
                    slot.DecreaseButton.interactable = slotType == StatAllocationSlot.SlotType.Game;
                    slot.IncreaseButton.interactable = true;
                }
            }

            foreach (var allocatedStatSlot in curStatAllocationSlots)
            {
                int allocatedStatSlotValue = getAllocatedStatValue(allocatedStatSlot.thisStat.ID, slotType);
                if (!allocatedStatSlot.thisStat.isVitalityStat && allocatedStatSlot.thisStat.minCheck && allocatedStatSlotValue <= allocatedStatSlot.thisStat.minValue)
                {
                    allocatedStatSlot.DecreaseButton.interactable = false;
                }
                if (!allocatedStatSlot.thisStat.isVitalityStat && allocatedStatSlot.thisStat.maxCheck && allocatedStatSlotValue >= allocatedStatSlot.thisStat.maxValue)
                {
                    allocatedStatSlot.IncreaseButton.interactable = false;
                }
                
                float max = getMaxAllocatedStatValue(allocatedStatSlot.thisStat);
                if (max > 0 && allocatedStatSlotValue >= max)
                {
                    allocatedStatSlot.IncreaseButton.interactable = false;
                }
                
                int cost = GetStatAllocationCostAmount(allocatedStatSlot.thisStat.ID, slotType);
                if (cost > points)
                {
                    allocatedStatSlot.IncreaseButton.interactable = false;
                }

                if (slotType == StatAllocationSlot.SlotType.Game && !GameDatabase.Instance.GetCharacterSettings().CanRefundStatPointInGame)
                {
                    allocatedStatSlot.DecreaseButton.interactable = false;
                }
            }
        }
        
        public void AlterAllocatedStat(int statID, bool increase, List<StatAllocationSlot> curStatAllocationSlots, StatAllocationSlot.SlotType slotType)
        {
            int statIndex = allocatedStatsHaveStatID(statID);
            int statSlotIndex = getAllocateStatSlotIndex(statID, curStatAllocationSlots);
            
            RPGStat statREF = GameDatabase.Instance.GetStats()[statID];
            float max = getMaxAllocatedStatValue(statREF);
            int cost = GetStatAllocationCostAmount(statID, slotType);
            int currentPoints = getCurrentPoints(slotType);
            if (increase && cost > currentPoints) return;
            int valueAdded = GetStatAllocationAddAmount(statID, slotType);
            if (statIndex == -1)
            {
                CharacterEntries.AllocatedStatData newAllocatedStat = new CharacterEntries.AllocatedStatData
                {
                    statID = statID, statName = statREF.entryName
                };
                if (slotType == StatAllocationSlot.SlotType.Game)
                {
                    newAllocatedStat.valueGame += increase ? valueAdded : -valueAdded;
                    newAllocatedStat.maxValueGame = max >= 0 ? (int)max : 0;
                    float currentValue = curStatAllocationSlots[statSlotIndex].thisStat.baseValue + newAllocatedStat.valueGame;
                    curStatAllocationSlots[statSlotIndex].curValueText.text = newAllocatedStat.maxValueGame > 0 ? currentValue + " / " + newAllocatedStat.maxValueGame :
                        currentValue.ToString();
                }
                else
                {
                    newAllocatedStat.value += increase ? valueAdded : -valueAdded;
                    float currentValue = newAllocatedStat.value;
                    newAllocatedStat.maxValue = max >= 0 ? (int)max : 0;

                    RPGStat stat = GameDatabase.Instance.GetStats()[statID];
                    if (stat.isVitalityStat)
                    {
                        curStatAllocationSlots[statSlotIndex].curValueText.text = newAllocatedStat.maxValue > 0 ? currentValue + " / " + newAllocatedStat.maxValue :
                            currentValue.ToString();
                    }
                    else
                    {
                        int totalValue = MainMenuManager.Instance.GetCurrentTotalStatValue(stat);
                        curStatAllocationSlots[statSlotIndex].curValueText.text = newAllocatedStat.maxValue > 0 ? (totalValue+currentValue) + " / " + newAllocatedStat.maxValue :
                            (totalValue+currentValue).ToString();
                    }
                }
                Character.Instance.CharacterData.AllocatedStats.Add(newAllocatedStat);
            }
            else
            {
                CharacterEntries.AllocatedStatData allocatedStat = Character.Instance.CharacterData.AllocatedStats[statIndex];
                if (increase && max > 0 && allocatedStat.value >= max) return;
                if (slotType == StatAllocationSlot.SlotType.Game)
                {
                    allocatedStat.valueGame += increase ? valueAdded : -valueAdded;
                    allocatedStat.maxValueGame = max >= 0 ? (int)max : 0;
                    float currentValue = curStatAllocationSlots[statSlotIndex].thisStat.baseValue + allocatedStat.valueGame;
                    curStatAllocationSlots[statSlotIndex].curValueText.text = allocatedStat.maxValueGame > 0 ? currentValue + " / " + allocatedStat.maxValueGame :
                        currentValue.ToString();
                }
                else
                {
                    allocatedStat.value += increase ? valueAdded : -valueAdded;
                    allocatedStat.maxValue = max >= 0 ? (int)max : 0;
                    float currentValue = allocatedStat.value;
                    
                    RPGStat stat = GameDatabase.Instance.GetStats()[statID];
                    if (stat.isVitalityStat)
                    {
                        curStatAllocationSlots[statSlotIndex].curValueText.text = allocatedStat.maxValue > 0 ? currentValue + " / " + allocatedStat.maxValue :
                            currentValue.ToString();
                    }
                    else
                    {
                        int totalValue = MainMenuManager.Instance.GetCurrentTotalStatValue(stat);
                        curStatAllocationSlots[statSlotIndex].curValueText.text = allocatedStat.maxValue > 0 ? (totalValue+currentValue) + " / " + allocatedStat.maxValue :
                            (totalValue+currentValue).ToString();
                    }
                }
            }

            if (slotType == StatAllocationSlot.SlotType.Game)
            {
                if (increase)
                {
                    TreePointsManager.Instance.RemoveTreePoint(GameDatabase.Instance.GetCharacterSettings().StatAllocationPointID, cost);
                    StatCalculator.UpdateStatAllocation(statID, valueAdded);
                }
                else
                {
                    TreePointsManager.Instance.AddTreePoint(GameDatabase.Instance.GetCharacterSettings().StatAllocationPointID, cost);
                    StatCalculator.UpdateStatAllocation(statID, -valueAdded);
                }
                HandleStatAllocationButtons(
                    Character.Instance.getTreePointsAmountByPoint(GameDatabase.Instance.GetCharacterSettings().StatAllocationPointID),
                    0, (List<StatAllocationSlot>)UIEvents.Instance.GetPanelEntryData("Stats_Allocation", "allSlots"), slotType);
                
                UIEvents.Instance.OnUpdateStatAllocationPanel();
            }
            else
            {
                Character.Instance.CharacterData.MainMenuStatAllocationPoints += increase ? -cost : cost;
                HandleStatAllocationButtons(
                    Character.Instance.CharacterData.MainMenuStatAllocationPoints,
                    Character.Instance.CharacterData.MainMenuStatAllocationMaxPoints, MainMenuManager.Instance.curStatAllocationSlots, slotType);
                MainMenuManager.Instance.UpdateAllocationPointsText();
            }
            
            HandleBodyScaleFromStats(slotType);
        }

        private int getCurrentPoints(StatAllocationSlot.SlotType slotType)
        {
            if (slotType == StatAllocationSlot.SlotType.Game)
            {
                return Character.Instance.getTreePointsAmountByPoint(GameDatabase.Instance.GetCharacterSettings().StatAllocationPointID);
            }

            return Character.Instance.CharacterData.MainMenuStatAllocationPoints;
        }

        private int GetStatAllocationAddAmount(int statID, StatAllocationSlot.SlotType slotType)
        {
            int addAmount = 0;
            if (slotType == StatAllocationSlot.SlotType.Game)
            {
                if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
                {
                    RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                    foreach (var statEntry in classREF.allocatedStatsEntriesGame)
                    {
                        if (statEntry.statID != statID) continue;
                        if (statEntry.valueAdded > addAmount) addAmount = statEntry.valueAdded;
                    }
                }

                addAmount = (from skill in Character.Instance.CharacterData.Skills select GameDatabase.Instance.GetSkills()[skill.skillID] into skillREF from statEntry in skillREF.allocatedStatsEntriesGame where statEntry.statID == statID select statEntry.valueAdded).Prepend(addAmount).Max();

                addAmount = Character.Instance.CharacterData.WeaponTemplates.Select(weaponTemplate => GameDatabase.Instance.GetWeaponTemplates()[weaponTemplate.weaponTemplateID]).Aggregate(addAmount, (current, weaponTemplateREF) => (from statEntry in weaponTemplateREF.allocatedStatsEntriesGame where statEntry.statID == statID select statEntry.valueAdded).Prepend(current).Max());
            }
            else
            {
                RPGRace raceREF = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
                foreach (var statEntry in raceREF.allocatedStatsEntries.Where(statEntry => statEntry.statID == statID))
                {
                    addAmount = statEntry.valueAdded;
                }

                if (GameDatabase.Instance.GetCharacterSettings().NoClasses) return addAmount;
                RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                addAmount = (from statEntry in classREF.allocatedStatsEntries where statEntry.statID == statID select statEntry.valueAdded).Prepend(addAmount).Max();
            }

            return addAmount;
        }

        private int GetStatAllocationCostAmount(int statID, StatAllocationSlot.SlotType slotType)
        {
            int cost = 0;
            if (slotType == StatAllocationSlot.SlotType.Game)
            {
                if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
                {
                    RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                    cost = (from statEntry in classREF.allocatedStatsEntriesGame where statEntry.statID == statID select statEntry.cost).Prepend(cost).Max();
                }

                cost = Character.Instance.CharacterData.Skills.Select(skill => GameDatabase.Instance.GetSkills()[skill.skillID]).Aggregate(cost, (current, skillREF) => (from statEntry in skillREF.allocatedStatsEntriesGame where statEntry.statID == statID select current).Prepend(current).Max());

                cost = Character.Instance.CharacterData.WeaponTemplates.Select(weaponTemplate => GameDatabase.Instance.GetWeaponTemplates()[weaponTemplate.weaponTemplateID]).Aggregate(cost, (current, weaponTemplateREF) => (from statEntry in weaponTemplateREF.allocatedStatsEntriesGame where statEntry.statID == statID select current).Prepend(current).Max());
            }
            else
            {
                RPGRace raceREF = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
                foreach (var statEntry in raceREF.allocatedStatsEntries.Where(statEntry => statEntry.statID == statID))
                {
                    cost = statEntry.cost;
                }

                if (GameDatabase.Instance.GetCharacterSettings().NoClasses) return cost;
                RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                cost = (from statEntry in classREF.allocatedStatsEntries where statEntry.statID == statID select statEntry.cost).Prepend(cost).Max();
            }

            return cost;
        }


        private void HandleBodyScaleFromStats(StatAllocationSlot.SlotType slotType)
        {
            float bodyScaleModifier = 1;
            
            if (slotType == StatAllocationSlot.SlotType.Game)
            {
                bodyScaleModifier += GameState.playerEntity.GetStats().Sum(stat => stat.Value.stat.statBonuses.Where(bonus => bonus.statType == RPGStat.STAT_TYPE.BODY_SCALE).Sum(bonus => stat.Value.currentValue * bonus.modifyValue));

                GameState.playerEntity.appearance.InitBodyScale(bodyScaleModifier);
            }
            else
            {
                foreach (var allocatedStat in Character.Instance.CharacterData.AllocatedStats)
                {
                    RPGStat statREF = GameDatabase.Instance.GetStats()[allocatedStat.statID];
                    if(statREF==null) continue;
                    foreach (var bonus in statREF.statBonuses.Where(bonus => bonus.statType == RPGStat.STAT_TYPE.BODY_SCALE))
                    {
                        if (slotType == StatAllocationSlot.SlotType.Game)
                        {
                            bodyScaleModifier += bonus.modifyValue * allocatedStat.valueGame;
                        }
                        else
                        {
                            bodyScaleModifier += bonus.modifyValue * allocatedStat.value;
                        }
                    }
                }
                
                MainMenuManager.Instance.curPlayerModel.GetComponent<PlayerAppearance>().InitBodyScale(bodyScaleModifier);
            }
        }

        private int allocatedStatsHaveStatID(int statID)
        {
            for (var index = 0; index < Character.Instance.CharacterData.AllocatedStats.Count; index++)
            {
                var allocatedStat = Character.Instance.CharacterData.AllocatedStats[index];
                if (allocatedStat.statID == statID) return index;
            }

            return -1;
        }
        private int getAllocateStatSlotIndex(int statID, List<StatAllocationSlot> curStatAllocationSlots)
        {
            for (var index = 0; index < curStatAllocationSlots.Count; index++)
            {
                var allocatedStat = curStatAllocationSlots[index];
                if (allocatedStat.thisStat.ID == statID) return index;
            }

            return -1;
        }
        
        public float getMaxAllocatedStatValue(RPGStat statREF)
        {
            RPGRace raceREF = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];

            float max = raceREF.allocatedStatsEntries.Where(allocatedStatEntry => allocatedStatEntry.statID == statREF.ID && allocatedStatEntry.maxValue > 0).Aggregate<CharacterEntries.AllocatedStatEntry, float>(0, (current, allocatedStatEntry) => current + allocatedStatEntry.maxValue);

            if (GameDatabase.Instance.GetCharacterSettings().NoClasses) return max;
            {
                RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];

                max = classREF.allocatedStatsEntries.Where(allocatedStatEntry => allocatedStatEntry.statID == statREF.ID && allocatedStatEntry.maxValue > 0).Aggregate(max, (current, allocatedStatEntry) => current + allocatedStatEntry.maxValue);
            }

            return max;
        }

        public int getAllocatedStatValue(int statID, StatAllocationSlot.SlotType slotType)
        {
            return (from allocatedStat in Character.Instance.CharacterData.AllocatedStats
                where allocatedStat.statID == statID
                select slotType == StatAllocationSlot.SlotType.Game
                    ? allocatedStat.valueGame
                    : allocatedStat.value).FirstOrDefault();
        }

        private bool allocateStatSlotHasStatID(List<StatAllocationSlot> holderList, int statID)
        {
            return holderList.Any(allocatedStat => allocatedStat.thisStat.ID == statID);
        }
    }
}
