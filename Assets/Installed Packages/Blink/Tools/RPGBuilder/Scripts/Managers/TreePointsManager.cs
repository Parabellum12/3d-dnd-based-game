using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UI;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class TreePointsManager : MonoBehaviour
    {
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        private void OnEnable()
        {
            GameEvents.CharacterLevelChanged += UpdateNeeded;
            GameEvents.WeaponTemplateLevelChanged += UpdateNeeded;
            GameEvents.SkillLevelChanged += UpdateNeeded;
            GeneralEvents.PlayerGainedItem += CheckIfItemGainPoints;
            CombatEvents.PlayerKilledNPC += CheckIfNPCkKilledGainPoints;
        }

        private void OnDisable()
        {
            GameEvents.CharacterLevelChanged -= UpdateNeeded;
            GameEvents.WeaponTemplateLevelChanged -= UpdateNeeded;
            GameEvents.SkillLevelChanged -= UpdateNeeded;
            GeneralEvents.PlayerGainedItem -= CheckIfItemGainPoints;
            CombatEvents.PlayerKilledNPC -= CheckIfNPCkKilledGainPoints;
        }

        private void UpdateNeeded(RPGWeaponTemplate weaponTemplate, int newLevel)
        {
            CheckIfWeaponTemplateLevelUpGainPoints(weaponTemplate);
        }
        private void UpdateNeeded(RPGSkill skill, int newLevel)
        {
            CheckIfSkillLevelUpGainPoints(skill);
        }
        private void UpdateNeeded(int newLevel)
        {
            CheckIfCharacterLevelUpGainPoints(newLevel);
        }

        public static TreePointsManager Instance { get; private set; }

        private void CheckIfItemGainPoints(RPGItem item, int amount)
        {
            foreach (var t in GameDatabase.Instance.GetPoints().Values)
            foreach (var t1 in t.gainPointRequirements.Where(t1 => t1.gainType ==
                                                                   RPGTreePoint.TreePointGainRequirementTypes.itemGained
                                                                   && t1.itemRequiredID == item.ID))
            {
                AddTreePoint(t.ID,
                    t1.amountGained);
            }
        }

        private void CheckIfNPCkKilledGainPoints(RPGNpc npc)
        {
            foreach (var t in GameDatabase.Instance.GetPoints().Values)
            foreach (var t1 in t.gainPointRequirements.Where(t1 => t1.gainType ==
                                                                   RPGTreePoint.TreePointGainRequirementTypes.npcKilled
                                                                   && t1.npcRequiredID == npc.ID))
            {
                AddTreePoint(t.ID,
                    t1.amountGained);
            }
        }

        public void CheckIfCharacterLevelUpGainPoints(int newLevel)
        {
            foreach (var t in GameDatabase.Instance.GetPoints().Values)
            foreach (var t1 in t.gainPointRequirements.Where(t1 => t1.gainType == RPGTreePoint.TreePointGainRequirementTypes.characterLevelUp))
            {
                AddTreePoint(t.ID, t1.amountGained);
            }
        }

        private void CheckIfWeaponTemplateLevelUpGainPoints(RPGWeaponTemplate weaponTemplate)
        {
            foreach (var t in GameDatabase.Instance.GetPoints().Values)
            foreach (var t1 in t.gainPointRequirements.Where(t1 => t1.gainType ==
                                                                   RPGTreePoint.TreePointGainRequirementTypes
                                                                       .weaponTemplateLevelUp
                                                                   && t1.weaponTemplateRequiredID == weaponTemplate.ID))
            {
                AddTreePoint(t.ID, t1.amountGained);
            }
        }

        private void CheckIfSkillLevelUpGainPoints(RPGSkill _skill)
        {
            foreach (var t in GameDatabase.Instance.GetPoints().Values)
            foreach (var t1 in t.gainPointRequirements.Where(t1 => t1.gainType ==
                                                                   RPGTreePoint.TreePointGainRequirementTypes.skillLevelUp
                                                                   && t1.skillRequiredID == _skill.ID))
            {
                AddTreePoint(t.ID,
                    t1.amountGained);
            }
        }

        public void AddTreePoint(int treeTypeID, int amount)
        {
            foreach (var t in Character.Instance.CharacterData.Points)
            {
                if (t.treePointID != treeTypeID) continue;
                RPGTreePoint pointREF = GameDatabase.Instance.GetPoints()[t.treePointID];
                amount = getGainValue(pointREF, amount);
                t.amount += amount;
                Clamp(pointREF, t);
            }
            
            GeneralEvents.Instance.OnPlayerPointsChanged();
        }

        public void RemoveTreePoint(int ID, int amount)
        {
            foreach (var t in Character.Instance.CharacterData.Points.Where(t => t.treePointID == ID))
            {
                t.amount -= amount;
                if (t.amount == 0)
                {
                    GeneralEvents.Instance.OnPlayerPointsChanged();
                }
            }
        }


        private static void Clamp(RPGTreePoint treePoint, CharacterEntries.TreePointEntry pointEntry)
        {
            int maxValue = getMaxValue(treePoint);
            if (treePoint.maxPoints > 0 && Character.Instance.getTreePointsAmountByPoint(treePoint.ID) > maxValue)
            {
                pointEntry.amount = maxValue;
            }
        }



        private static int getMaxValue(RPGTreePoint treePoint)
        {
            return (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.TreePoint + "+" +
                RPGGameModifier.PointModifierType.Max, treePoint.maxPoints, treePoint.ID, -1);
        }

        private static int getGainValue(RPGTreePoint treePoint, int baseGainAmount)
        {
            return (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.TreePoint + "+" +
                RPGGameModifier.PointModifierType.Gain_Value, baseGainAmount, treePoint.ID, -1);
        }
    }
}