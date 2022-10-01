using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class RequirementsManager : MonoBehaviour
    {
        public static RequirementsManager Instance { get; private set; }
        
        [Serializable]
        public class RequirementResult
        {
            public bool Result;
            public List<int> EffectsToConsume = new List<int>();
            public List<EntryCost> StatCosts = new List<EntryCost>();
            public List<EntryCost> ItemCosts = new List<EntryCost>();
            public List<EntryCost> CurrencyCosts = new List<EntryCost>();
            public List<EntryCost> PointCosts = new List<EntryCost>();
        }
        
        [Serializable]
        public class EntryCost
        {
            public int ID = -1;
            public int amount;
        }
        
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public RequirementResult RequirementsMet(CombatEntity checkedEntity, List<RequirementsData.RequirementGroup> requirementGroups)
        {
            RequirementResult requirementResult = new RequirementResult();
            if (requirementGroups.Count == 0)
            {
                requirementResult.Result = true;
                return requirementResult;
            }
            List<bool> groupResults = new List<bool>();

            foreach (var group in requirementGroups)
            {
                List<bool> mandatoryResults = new List<bool>();
                List<bool> optionalResults = new List<bool>();
                foreach (var requirement in group.Requirements)
                {
                    bool result = IsRequirementMet(checkedEntity, requirement, requirementResult);
                    if(requirement.condition == RequirementsData.Rule.Mandatory)
                        mandatoryResults.Add(result);
                    else if(requirement.condition == RequirementsData.Rule.Optional)
                        optionalResults.Add(result);
                }

                switch (mandatoryResults.Count > 0)
                {
                    case true when optionalResults.Count == 0 && mandatoryResults.Contains(false):
                        requirementResult.Result = false;
                        return requirementResult;
                    case true when optionalResults.Count > 0:
                    {
                        if (mandatoryResults.Contains(false))
                        {
                            requirementResult.Result = false;
                            return requirementResult;
                        }
                    
                        if (group.checkCount) 
                            groupResults.Add(GetValidOptionalCount(optionalResults) >= group.requiredCount);
                        else 
                            groupResults.Add(optionalResults.Contains(true));
                        break;
                    }
                    default:
                    {
                        if (mandatoryResults.Count == 0 && optionalResults.Count > 0)
                        {
                            if (group.checkCount) 
                                groupResults.Add(GetValidOptionalCount(optionalResults) >= group.requiredCount);
                            else 
                                groupResults.Add(optionalResults.Contains(true));
                        }
                        else
                        {
                            ProcessConsume(checkedEntity, requirementResult);
                            requirementResult.Result = true;
                            return requirementResult;
                        }

                        break;
                    }
                }
            }

            requirementResult.Result = !groupResults.Contains(false);
            if (requirementResult.Result)
            {
                ProcessConsume(checkedEntity, requirementResult);
            }
            return requirementResult;
        }

        private void ProcessConsume(CombatEntity checkedEntity, RequirementResult requirementResult)
        {
            foreach (var effect in requirementResult.EffectsToConsume)
            {
                checkedEntity.RemoveEffectByID(effect);
            }
            foreach (var stat in requirementResult.StatCosts)
            {
                CombatUtilities.UpdateCurrentStatValue(checkedEntity, stat.ID, stat.amount);
            }
            foreach (var item in requirementResult.ItemCosts)
            {
                InventoryManager.Instance.RemoveItem(item.ID, -1, item.amount, -1, -1, false);
            }
            foreach (var currency in requirementResult.CurrencyCosts)
            {
                InventoryManager.Instance.RemoveCurrency(currency.ID, currency.amount);
            }
            foreach (var point in requirementResult.PointCosts)
            {
                TreePointsManager.Instance.RemoveTreePoint(point.ID, point.amount);
            }
        }
        private int GetValidOptionalCount(List<bool> optionalResults)
        {
            int valid = 0;
            foreach (var result in optionalResults)
            {
                if (result) valid++;
            }

            return valid;
        }

        private bool ValueIsValid(int valueToCheck, int valueRequired, RequirementsData.Value valueType)
        {
            switch (valueType)
            {
                case RequirementsData.Value.Equal:
                    return valueToCheck == valueRequired;
                case RequirementsData.Value.EqualOrBelow:
                    return valueToCheck <= valueRequired;
                case RequirementsData.Value.EqualOrAbove:
                    return valueToCheck >= valueRequired;
                case RequirementsData.Value.Below:
                    return valueToCheck < valueRequired;
                case RequirementsData.Value.Above:
                    return valueToCheck > valueRequired;
            }

            return false;
        }
        
        private bool ValueIsValid(float valueToCheck, float valueRequired, RequirementsData.Value valueType)
        {
            switch (valueType)
            {
                case RequirementsData.Value.Equal:
                    return valueToCheck == valueRequired;
                case RequirementsData.Value.EqualOrBelow:
                    return valueToCheck <= valueRequired;
                case RequirementsData.Value.EqualOrAbove:
                    return valueToCheck >= valueRequired;
                case RequirementsData.Value.Below:
                    return valueToCheck < valueRequired;
                case RequirementsData.Value.Above:
                    return valueToCheck > valueRequired;
            }

            return false;
        }
        private bool IsRequirementMet(CombatEntity checkedEntity, RequirementsData.Requirement requirement, RequirementResult requirementResult)
        {
            switch (requirement.type)
            {
                case RequirementsData.RequirementType.Ability:
                    if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                    {
                        if (!CombatUtilities.IsAbilityKnown(requirement.AbilityID)) return false;
                        if (requirement.BoolBalue1)
                        {
                            return ValueIsValid(RPGBuilderUtilities.GetCharacterAbilityRank(requirement.AbilityID),
                                requirement.Amount1, requirement.Value);
                        }

                        return true;
                    }
                    else
                    {
                        return !CombatUtilities.IsAbilityKnown(requirement.AbilityID);
                    }
                case RequirementsData.RequirementType.Bonus:
                    if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                    {
                        if (!RPGBuilderUtilities.isBonusKnown(requirement.BonusID)) return false;
                        if (requirement.BoolBalue1)
                        {
                            return ValueIsValid(RPGBuilderUtilities.getBonusRank(requirement.BonusID),
                                requirement.Amount1, requirement.Value);
                        }

                        return true;
                    }
                    else
                    {
                        return !RPGBuilderUtilities.isBonusKnown(requirement.BonusID);
                    }
                case RequirementsData.RequirementType.Recipe:
                    if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                    {
                        if (!RPGBuilderUtilities.isRecipeKnown(requirement.RecipeID)) return false;
                        if (requirement.BoolBalue1)
                        {
                            return ValueIsValid(RPGBuilderUtilities.getRecipeRank(requirement.RecipeID),
                                requirement.Amount1, requirement.Value);
                        }

                        return true;
                    }
                    else
                    {
                        return !RPGBuilderUtilities.isRecipeKnown(requirement.RecipeID);
                    }
                case RequirementsData.RequirementType.Resource:
                    if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                    {
                        if (!RPGBuilderUtilities.isResourceNodeKnown(requirement.ResourceID)) return false;
                        if (requirement.BoolBalue1)
                        {
                            return ValueIsValid(RPGBuilderUtilities.getResourceNodeRank(requirement.ResourceID),
                                requirement.Amount1, requirement.Value);
                        }

                        return true;
                    }
                    else
                    {
                        return !RPGBuilderUtilities.isResourceNodeKnown(requirement.ResourceID);
                    }
                case RequirementsData.RequirementType.Effect:
                    if (requirement.State == RequirementsData.State.Active)
                    {
                        if (requirement.Consume)
                        {
                            requirementResult.EffectsToConsume.Add(requirement.EffectID);
                        }
                        
                        switch (requirement.EffectCondition)
                        {
                            case RequirementsData.EffectCondition.Effect
                                when !CombatUtilities.IsEffectActiveOnTarget(checkedEntity, requirement.EffectID):
                                return false;
                            case RequirementsData.EffectCondition.Effect when requirement.BoolBalue1:
                                return ValueIsValid(
                                    CombatUtilities.GetEffectStacks(checkedEntity, requirement.EffectID),
                                    requirement.Amount1, requirement.Value);
                            case RequirementsData.EffectCondition.Effect:
                                return true;
                            case RequirementsData.EffectCondition.EffectTag:
                                return CombatUtilities.IsEffectTagActiveOnTarget(checkedEntity, requirement.EffectTag);
                            case RequirementsData.EffectCondition.EffectType:
                                return CombatUtilities.IsEffectTypeActiveOnTarget(checkedEntity,
                                    requirement.EffectType);
                        }
                    }
                    else
                    {
                        return !CombatUtilities.IsEffectActiveOnTarget(checkedEntity, requirement.EffectID);
                    }

                    break;
                case RequirementsData.RequirementType.NPCKilled:
                    foreach (var npcKilled in Character.Instance.CharacterData.KilledNPCs)
                    {
                        if (npcKilled.npcID != requirement.NPCID) continue;
                        return !requirement.BoolBalue1 || ValueIsValid(npcKilled.killedAmount, requirement.Amount1,
                            requirement.Value);
                    }

                    return false;
                case RequirementsData.RequirementType.NPCFamily:
                    if (checkedEntity.GetNPCData().npcFamily == null) return false;
                    if (requirement.Comparison == RequirementsData.Comparison.Equal)
                    {
                        return checkedEntity.GetNPCData().npcFamily == requirement.NPCFamily;
                    }
                    else if (requirement.Comparison == RequirementsData.Comparison.Different)
                    {
                        return checkedEntity.GetNPCData().npcFamily != requirement.NPCFamily;
                    }

                    break;
                case RequirementsData.RequirementType.Stat:
                    if (requirement.IsPercent)
                    {
                        float percent = (CombatUtilities.GetCurrentStatValue(checkedEntity, requirement.StatID) / CombatUtilities.GetMaxStatValue(checkedEntity, requirement.StatID)) * 100f;
                        return ValueIsValid(percent, requirement.Float1, requirement.Value);
                    }
                    else
                    {
                        return ValueIsValid((int) CombatUtilities.GetCurrentStatValue(checkedEntity, requirement.StatID),
                            requirement.Amount1, requirement.Value);
                    }
                case RequirementsData.RequirementType.StatCost:

                    if (requirement.Consume)
                    {
                        switch (requirement.AmountType)
                        {
                            case RequirementsData.AmountType.Value:
                                requirementResult.StatCosts.Add(new EntryCost{ID = requirement.StatID, amount = requirement.Amount1});
                                break;
                            case RequirementsData.AmountType.PercentOfCurrent:
                            {
                                float amount = (CombatUtilities.GetCurrentStatValue(checkedEntity, requirement.StatID) /
                                                requirement.Amount1);
                                requirementResult.StatCosts.Add(new EntryCost
                                    {ID = requirement.StatID, amount = (int) amount});
                                break;
                            }
                            case RequirementsData.AmountType.PercentOfMax:
                            {
                                float amount = (requirement.Amount1 * CombatUtilities.GetMaxStatValue(checkedEntity, requirement.StatID)) / 100;
                                requirementResult.StatCosts.Add(new EntryCost{ID = requirement.StatID, amount = (int)amount});
                                break;
                            }
                        }
                    }

                    switch (requirement.AmountType)
                    {
                        case RequirementsData.AmountType.Value:
                            return CombatUtilities.GetCurrentStatValue(checkedEntity, requirement.StatID) >=
                                   requirement.Amount1;
                        case RequirementsData.AmountType.PercentOfCurrent:
                            return true;
                        case RequirementsData.AmountType.PercentOfMax:
                            return CombatUtilities.GetCurrentStatValue(checkedEntity, requirement.StatID) >=
                                   (requirement.Amount1 *
                                    CombatUtilities.GetMaxStatValue(checkedEntity, requirement.StatID)) / 100;
                    }

                    break;
                case RequirementsData.RequirementType.Faction:
                {
                    CombatEntity entityToCheck = requirement.Entity == RequirementsData.Entity.Caster
                        ? checkedEntity
                        : checkedEntity.GetTarget();
                    if (entityToCheck == null) return false;

                    switch (requirement.Comparison)
                    {
                        case RequirementsData.Comparison.Equal:
                            return entityToCheck.GetFaction().ID == requirement.FactionID;
                        case RequirementsData.Comparison.Different:
                            return entityToCheck.GetFaction().ID != requirement.FactionID;
                    }
                }
                    break;
                case RequirementsData.RequirementType.FactionStance:
                {
                    CombatEntity entityToCheck = requirement.Entity == RequirementsData.Entity.Caster
                        ? checkedEntity
                        : checkedEntity.GetTarget();
                    if (entityToCheck == null) return false;

                    if (requirement.Comparison == RequirementsData.Comparison.Equal)
                    {
                        return FactionManager.Instance.GetEntityStanceToFaction(entityToCheck,
                                   GameDatabase.Instance.GetFactions()[requirement.FactionID]) ==
                               requirement.FactionStance;
                    }

                    return !FactionManager.Instance.GetEntityStanceToFaction(entityToCheck,
                               GameDatabase.Instance.GetFactions()[requirement.FactionID]) ==
                           requirement.FactionStance;
                }
                case RequirementsData.RequirementType.Combo:
                    if (requirement.Comparison == RequirementsData.Comparison.Equal)
                    {
                        return RPGBuilderUtilities.IsComboActive(checkedEntity, requirement.ComboID);
                    }
                    else if (requirement.Comparison == RequirementsData.Comparison.Different)
                    {
                        return !RPGBuilderUtilities.IsComboActive(checkedEntity, requirement.ComboID);
                    }

                    break;
                case RequirementsData.RequirementType.Race:
                    if (requirement.Comparison == RequirementsData.Comparison.Equal)
                    {
                        return Character.Instance.CharacterData.RaceID == requirement.RaceID;
                    }
                    else if (requirement.Comparison == RequirementsData.Comparison.Different)
                    {
                        return Character.Instance.CharacterData.RaceID != requirement.RaceID;
                    }

                    break;
                case RequirementsData.RequirementType.Level:
                    return ValueIsValid(Character.Instance.CharacterData.Level, requirement.Amount1, requirement.Value);
                case RequirementsData.RequirementType.Gender:
                    if (requirement.Comparison == RequirementsData.Comparison.Equal)
                    {
                        return Character.Instance.CharacterData.Gender == requirement.Gender.entryName;
                    }
                    else if (requirement.Comparison == RequirementsData.Comparison.Different)
                    {
                        return Character.Instance.CharacterData.Gender != requirement.Gender.entryName;
                    }

                    break;
                case RequirementsData.RequirementType.Class:
                    if (requirement.Comparison == RequirementsData.Comparison.Equal)
                    {
                        return Character.Instance.CharacterData.ClassID == requirement.ClassID;
                    }
                    else if (requirement.Comparison == RequirementsData.Comparison.Different)
                    {
                        return Character.Instance.CharacterData.ClassID != requirement.ClassID;
                    }

                    break;
                case RequirementsData.RequirementType.Species:
                    if (requirement.Comparison == RequirementsData.Comparison.Equal)
                    {
                        return checkedEntity.GetTarget().GetSpecies().ID == requirement.SpeciesID;
                    }
                    else if (requirement.Comparison == RequirementsData.Comparison.Different)
                    {
                        return checkedEntity.GetTarget().GetSpecies().ID != requirement.SpeciesID;
                    }

                    break;
                case RequirementsData.RequirementType.Item:
                    
                    if (requirement.Consume)
                    {
                        requirementResult.ItemCosts.Add(new EntryCost{ID = requirement.ItemID, amount = requirement.Amount1});
                    }
                    
                    switch (requirement.Ownership)
                    {
                        case RequirementsData.Ownership.Owned:
                            switch (requirement.ItemCondition)
                            {
                                case RequirementsData.ItemCondition.Item:
                                    return EconomyUtilities.IsItemOwned(requirement.ItemID, requirement.Amount1);
                                case RequirementsData.ItemCondition.ItemType:
                                    return EconomyUtilities.IsItemTypeOwned(requirement.ItemType);
                                case RequirementsData.ItemCondition.WeaponType:
                                    return EconomyUtilities.IsWeaponTypeOwned(requirement.WeaponType);
                                case RequirementsData.ItemCondition.WeaponSlot:
                                    return EconomyUtilities.IsWeaponHandSlotOwned(requirement.WeaponSlot);
                                case RequirementsData.ItemCondition.ArmorType:
                                    return EconomyUtilities.IsArmorTypeOwned(requirement.ArmorType);
                                case RequirementsData.ItemCondition.ArmorSlot:
                                    return EconomyUtilities.IsArmorSlotOwned(requirement.ArmorSlot);
                            }

                            break;
                        case RequirementsData.Ownership.NotOwned:
                            switch (requirement.ItemCondition)
                            {
                                case RequirementsData.ItemCondition.Item:
                                    return !EconomyUtilities.IsItemOwned(requirement.ItemID, requirement.Amount1);
                                case RequirementsData.ItemCondition.ItemType:
                                    return !EconomyUtilities.IsItemTypeOwned(requirement.ItemType);
                                case RequirementsData.ItemCondition.WeaponType:
                                    return !EconomyUtilities.IsWeaponTypeOwned(requirement.WeaponType);
                                case RequirementsData.ItemCondition.WeaponSlot:
                                    return !EconomyUtilities.IsWeaponHandSlotOwned(requirement.WeaponSlot);
                                case RequirementsData.ItemCondition.ArmorType:
                                    return !EconomyUtilities.IsArmorTypeOwned(requirement.ArmorType);
                                case RequirementsData.ItemCondition.ArmorSlot:
                                    return !EconomyUtilities.IsArmorSlotOwned(requirement.ArmorSlot);
                            }

                            break;
                        case RequirementsData.Ownership.Equipped:
                            switch (requirement.ItemCondition)
                            {
                                case RequirementsData.ItemCondition.Item:
                                    return EconomyUtilities.IsItemEquipped(requirement.ItemID);
                                case RequirementsData.ItemCondition.ItemType:
                                    return EconomyUtilities.IsItemTypeEquipped(requirement.ItemType);
                                case RequirementsData.ItemCondition.WeaponType:
                                    return EconomyUtilities.IsWeaponTypeEquipped(requirement.WeaponType);
                                case RequirementsData.ItemCondition.WeaponSlot:
                                    return EconomyUtilities.IsWeaponHandSlotEquipped(requirement.WeaponSlot);
                                case RequirementsData.ItemCondition.ArmorType:
                                    return EconomyUtilities.IsArmorTypeEquipped(requirement.ArmorType);
                                case RequirementsData.ItemCondition.ArmorSlot:
                                    return EconomyUtilities.IsArmorSlotEquipped(requirement.ArmorSlot);
                            }

                            break;
                        case RequirementsData.Ownership.NotEquipped:
                            switch (requirement.ItemCondition)
                            {
                                case RequirementsData.ItemCondition.Item:
                                    return !EconomyUtilities.IsItemEquipped(requirement.ItemID);
                                case RequirementsData.ItemCondition.ItemType:
                                    return !EconomyUtilities.IsItemTypeEquipped(requirement.ItemType);
                                case RequirementsData.ItemCondition.WeaponType:
                                    return !EconomyUtilities.IsWeaponTypeEquipped(requirement.WeaponType);
                                case RequirementsData.ItemCondition.WeaponSlot:
                                    return !EconomyUtilities.IsWeaponHandSlotEquipped(requirement.WeaponSlot);
                                case RequirementsData.ItemCondition.ArmorType:
                                    return !EconomyUtilities.IsArmorTypeEquipped(requirement.ArmorType);
                                case RequirementsData.ItemCondition.ArmorSlot:
                                    return !EconomyUtilities.IsArmorSlotEquipped(requirement.ArmorSlot);
                            }

                            break;
                    }

                    break;
                case RequirementsData.RequirementType.Currency:
                    if (requirement.Consume)
                    {
                        requirementResult.CurrencyCosts.Add(new EntryCost{ID = requirement.CurrencyID, amount = requirement.Amount1});
                    }
                    
                    return ValueIsValid(
                        Character.Instance.getCurrencyAmount(
                            GameDatabase.Instance.GetCurrencies()[requirement.CurrencyID]), requirement.Amount1,
                        requirement.Value);
                case RequirementsData.RequirementType.Point:
                    if (requirement.Consume)
                    {
                        requirementResult.PointCosts.Add(new EntryCost{ID = requirement.PointID, amount = requirement.Amount1});
                    }
                    return ValueIsValid(
                        Character.Instance.getTreePointsAmountByPoint(requirement.PointID), requirement.Amount1,
                        requirement.Value);
                case RequirementsData.RequirementType.TalentTree:
                    if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                    {
                        return ProgressionUtilities.HasTalentTree(requirement.TalentTreeID);
                    }
                    else if (requirement.Knowledge == RequirementsData.Knowledge.Unknown)
                    {
                        return !ProgressionUtilities.HasTalentTree(requirement.TalentTreeID);
                    }

                    break;
                case RequirementsData.RequirementType.Skill:
                    if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                    {
                        return requirement.BoolBalue1 ? ValueIsValid(RPGBuilderUtilities.getSkillLevel(requirement.SkillID), requirement.Amount1, requirement.Value) : ProgressionUtilities.HasSkill(requirement.SkillID);
                    }
                    else if (requirement.Knowledge == RequirementsData.Knowledge.Unknown)
                    {
                        return !ProgressionUtilities.HasSkill(requirement.SkillID);
                    }

                    break;
                case RequirementsData.RequirementType.Spellbook:
                    // TODO 
                    // Spellbooks are not currently added to the character directly, they are instead part of their respective progression templates
                    break;
                case RequirementsData.RequirementType.WeaponTemplate:
                    if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                    {
                        return requirement.BoolBalue1 ? ValueIsValid(RPGBuilderUtilities.getWeaponTemplateLevel(requirement.WeaponTemplateID), requirement.Amount1, requirement.Value) : ProgressionUtilities.HasWeaponTemplate(requirement.WeaponTemplateID);
                    }
                    else if (requirement.Knowledge == RequirementsData.Knowledge.Unknown)
                    {
                        return !ProgressionUtilities.HasWeaponTemplate(requirement.WeaponTemplateID);
                    }

                    break;
                case RequirementsData.RequirementType.Enchantment:
                    if (requirement.State == RequirementsData.State.Active)
                    {
                        return ProgressionUtilities.HasTalentTree(requirement.TalentTreeID);
                    }
                    else if (requirement.State == RequirementsData.State.Inactive)
                    {
                        return !ProgressionUtilities.HasTalentTree(requirement.TalentTreeID);
                    }

                    break;
                case RequirementsData.RequirementType.GearSet:
                    if (requirement.State == RequirementsData.State.Active)
                    {
                        return ProgressionUtilities.IsGearSetActive(requirement.GearSetID);
                    }
                    else if (requirement.State == RequirementsData.State.Inactive)
                    {
                        return !ProgressionUtilities.IsGearSetActive(requirement.GearSetID);
                    }

                    break;
                case RequirementsData.RequirementType.GameScene:
                    if (requirement.Comparison == RequirementsData.Comparison.Equal)
                    {
                        return Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].GameSceneID ==
                               requirement.GameSceneID;
                    }
                    else if (requirement.Comparison == RequirementsData.Comparison.Different)
                    {
                        return Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].GameSceneID !=
                               requirement.GameSceneID;
                    }

                    break;
                case RequirementsData.RequirementType.Quest:
                    foreach (var quest in Character.Instance.CharacterData.Quests)
                    {
                        if(quest.questID != requirement.QuestID) continue;
                        if (quest.state != requirement.QuestState) continue;
                        return true;
                    }

                    return false;
                case RequirementsData.RequirementType.DialogueNode:
                    foreach (var dialogue in Character.Instance.CharacterData.Dialogues)
                    {
                        foreach (var node in dialogue.nodes)
                        {
                            switch (requirement.DialogueNodeState)
                            {
                                case RequirementsData.DialogueNodeState.Viewed:
                                    if (requirement.BoolBalue1)
                                    {
                                        return ValueIsValid(node.currentlyViewedCount, requirement.Amount1,
                                            requirement.Value);
                                    }
                                    else
                                    {
                                        return node.currentlyViewedCount > 0;
                                    }
                                case RequirementsData.DialogueNodeState.Clicked:
                                    if (requirement.BoolBalue1)
                                    {
                                        return ValueIsValid(node.currentlyClickedCount, requirement.Amount1,
                                            requirement.Value);
                                    }
                                    else
                                    {
                                        return node.currentlyClickedCount > 0;
                                    }
                                case RequirementsData.DialogueNodeState.Completed:
                                    return node.lineCompleted;
                            }
                        }
                    }

                    break;
                case RequirementsData.RequirementType.Region:
                    if (RegionManager.Instance.CurrentRegion == null) return false; 
                    if (requirement.Comparison == RequirementsData.Comparison.Equal)
                    {
                        return RegionManager.Instance.CurrentRegion == requirement.Region;
                    }
                    else if (requirement.Comparison == RequirementsData.Comparison.Different)
                    {
                        return RegionManager.Instance.CurrentRegion != requirement.Region;
                    }
                    break;
                case RequirementsData.RequirementType.CombatState:
                    return requirement.BoolBalue1 ? checkedEntity.IsInCombat() : !checkedEntity.IsInCombat();
                case RequirementsData.RequirementType.Stealth:
                    return requirement.BoolBalue1 ? checkedEntity.IsStealth() : !checkedEntity.IsStealth();
                case RequirementsData.RequirementType.Mounted:
                    return requirement.BoolBalue1 ? checkedEntity.IsMounted() : !checkedEntity.IsMounted();
                case RequirementsData.RequirementType.Grounded:
                    return requirement.BoolBalue1
                        ? GameState.playerEntity.controllerEssentials.IsGrounded()
                        : !GameState.playerEntity.controllerEssentials.IsGrounded();
                case RequirementsData.RequirementType.Time:
                    switch (requirement.TimeType)
                    {
                        case RequirementsData.TimeType.GameTime:
                            return requirement.TimeValue == RequirementsData.TimeValue.Between
                                ? IsGameTimeInBetween(requirement)
                                : IsGameTimeValid(requirement);
                        case RequirementsData.TimeType.SystemTime:
                            return requirement.TimeValue == RequirementsData.TimeValue.Between
                                ? IsSystemTimeInBetween(requirement)
                                : IsSystemTimeValid(requirement);
                        case RequirementsData.TimeType.CombatTime:
                            return ValueIsValid(
                                requirement.BoolBalue1
                                    ? (int) checkedEntity.GetInCombatTime()
                                    : (int) checkedEntity.GetOutOfCombatTime(), requirement.Amount1, requirement.Value);
                    }
                    break;
            }

            return true;
        }

        private bool IsGameTimeInBetween(RequirementsData.Requirement requirement)
        {
            List<bool> results = new List<bool>();
            if (requirement.TimeRequirement1.CheckYear)
            {
                results.Add(IsGameTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckMonth)
            {
                results.Add(IsGameTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckWeek)
            {
                results.Add(IsGameTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckDay)
            {
                results.Add(IsGameTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckHour)
            {
                results.Add(IsGameTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckMinute)
            {
                results.Add(IsGameTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckSecond)
            {
                results.Add(IsGameTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckGlobalSpeed)
            {
                results.Add(IsGameTimeValueMatching(requirement));
            }

            return !results.Contains(false);
        }
        private bool IsGameTimeValueMatching(RequirementsData.Requirement requirement)
        {
            string minString = "";
            string maxString = "";
            string current = "";

            if (requirement.TimeRequirement1.CheckYear)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Year.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Year.ToString());
                current += HandleStringValue(Character.Instance.CharacterData.Time.CurrentYear.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckMonth)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Month.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Month.ToString());
                current += HandleStringValue(Character.Instance.CharacterData.Time.CurrentMonth.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckWeek)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Week.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Week.ToString());
                current += HandleStringValue(Character.Instance.CharacterData.Time.CurrentWeek.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckDay)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Day.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Day.ToString());
                current += HandleStringValue(Character.Instance.CharacterData.Time.CurrentDay.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckHour)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Hour.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Hour.ToString());
                current += HandleStringValue(Character.Instance.CharacterData.Time.CurrentHour.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckMinute)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Minute.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Minute.ToString());
                current += HandleStringValue(Character.Instance.CharacterData.Time.CurrentMinute.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckSecond)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Second.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Second.ToString());
                current += HandleStringValue(Character.Instance.CharacterData.Time.CurrentSecond.ToString());
            }
            
            var minInt = int.Parse(minString);
            var maxInt = int.Parse(maxString);
            var curInt = int.Parse(current);
            return curInt >= minInt && curInt <= maxInt;
        }
        private bool IsGameTimeValid(RequirementsData.Requirement requirement)
        {
            List<bool> results = new List<bool>();
            switch (requirement.TimeValue)
            {
                case RequirementsData.TimeValue.Equal:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(Character.Instance.CharacterData.Time.CurrentYear == requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(Character.Instance.CharacterData.Time.CurrentMonth == requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(Character.Instance.CharacterData.Time.CurrentWeek == requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(Character.Instance.CharacterData.Time.CurrentDay == requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(Character.Instance.CharacterData.Time.CurrentHour == requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(Character.Instance.CharacterData.Time.CurrentMinute == requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(Character.Instance.CharacterData.Time.CurrentSecond == requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed == requirement.TimeRequirement1.GlobalSpeed);
                    break;
                case RequirementsData.TimeValue.EqualOrBelow:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(Character.Instance.CharacterData.Time.CurrentYear <= requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(Character.Instance.CharacterData.Time.CurrentMonth <= requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(Character.Instance.CharacterData.Time.CurrentWeek <= requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(Character.Instance.CharacterData.Time.CurrentDay <= requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(Character.Instance.CharacterData.Time.CurrentHour <= requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(Character.Instance.CharacterData.Time.CurrentMinute <= requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(Character.Instance.CharacterData.Time.CurrentSecond <= requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed <= requirement.TimeRequirement1.GlobalSpeed);
                    break;
                case RequirementsData.TimeValue.EqualOrAbove:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(Character.Instance.CharacterData.Time.CurrentYear >= requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(Character.Instance.CharacterData.Time.CurrentMonth >= requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(Character.Instance.CharacterData.Time.CurrentWeek >= requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(Character.Instance.CharacterData.Time.CurrentDay >= requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(Character.Instance.CharacterData.Time.CurrentHour >= requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(Character.Instance.CharacterData.Time.CurrentMinute >= requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(Character.Instance.CharacterData.Time.CurrentSecond >= requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed >= requirement.TimeRequirement1.GlobalSpeed);
                    break;
                case RequirementsData.TimeValue.Below:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(Character.Instance.CharacterData.Time.CurrentYear < requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(Character.Instance.CharacterData.Time.CurrentMonth < requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(Character.Instance.CharacterData.Time.CurrentWeek < requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(Character.Instance.CharacterData.Time.CurrentDay < requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(Character.Instance.CharacterData.Time.CurrentHour < requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(Character.Instance.CharacterData.Time.CurrentMinute < requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(Character.Instance.CharacterData.Time.CurrentSecond < requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed < requirement.TimeRequirement1.GlobalSpeed);
                    break;
                case RequirementsData.TimeValue.Above:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(Character.Instance.CharacterData.Time.CurrentYear > requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(Character.Instance.CharacterData.Time.CurrentMonth > requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(Character.Instance.CharacterData.Time.CurrentWeek > requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(Character.Instance.CharacterData.Time.CurrentDay > requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(Character.Instance.CharacterData.Time.CurrentHour > requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(Character.Instance.CharacterData.Time.CurrentMinute > requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(Character.Instance.CharacterData.Time.CurrentSecond > requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed > requirement.TimeRequirement1.GlobalSpeed);
                    break;
            }

            return !results.Contains(false);
        }
        private bool IsSystemTimeInBetween(RequirementsData.Requirement requirement)
        {
            List<bool> results = new List<bool>();
            
            if (requirement.TimeRequirement1.CheckYear)
            {
                results.Add(IsSystemTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckMonth)
            {
                results.Add(IsSystemTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckWeek)
            {
                results.Add(IsSystemTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckDay)
            {
                results.Add(IsSystemTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckHour)
            {
                results.Add(IsSystemTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckMinute)
            {
                results.Add(IsSystemTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckSecond)
            {
                results.Add(IsSystemTimeValueMatching(requirement));
            } else if (requirement.TimeRequirement1.CheckGlobalSpeed)
            {
                results.Add(IsSystemTimeValueMatching(requirement));
            }

            return !results.Contains(false);
        }
        private bool IsSystemTimeValueMatching(RequirementsData.Requirement requirement)
        {
            string minString = "";
            string maxString = "";
            string current = "";

            if (requirement.TimeRequirement1.CheckYear)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Year.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Year.ToString());
                current += HandleStringValue(DateTime.Now.Year.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckMonth)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Month.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Month.ToString());
                current += HandleStringValue(DateTime.Now.Month.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckWeek)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Week.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Week.ToString());
                current += HandleStringValue(GetWeekNumber().ToString());
            }
            
            if (requirement.TimeRequirement1.CheckDay)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Day.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Day.ToString());
                current += HandleStringValue(DateTime.Now.Day.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckHour)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Hour.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Hour.ToString());
                current += HandleStringValue(DateTime.Now.Hour.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckMinute)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Minute.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Minute.ToString());
                current += HandleStringValue(DateTime.Now.Minute.ToString());
            }
            
            if (requirement.TimeRequirement1.CheckSecond)
            {
                minString += HandleStringValue(requirement.TimeRequirement1.Second.ToString());
                maxString += HandleStringValue(requirement.TimeRequirement2.Second.ToString());
                current += HandleStringValue(DateTime.Now.Second.ToString());
            }
            
            var minInt = int.Parse(minString);
            var maxInt = int.Parse(maxString);
            var curInt = int.Parse(current);
            return curInt >= minInt && curInt <= maxInt;
        }
        private bool IsSystemTimeValid(RequirementsData.Requirement requirement)
        {
            List<bool> results = new List<bool>();
            switch (requirement.TimeValue)
            {
                case RequirementsData.TimeValue.Equal:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(DateTime.Now.Year == requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(DateTime.Now.Month == requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(GetWeekNumber() == requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(DateTime.Now.Day == requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(DateTime.Now.Hour == requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(DateTime.Now.Minute == requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(DateTime.Now.Second == requirement.TimeRequirement1.Second);
                    Debug.LogError(DateTime.Now.Second + " " + requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed == requirement.TimeRequirement1.GlobalSpeed);
                    break;
                case RequirementsData.TimeValue.EqualOrBelow:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(DateTime.Now.Year <= requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(DateTime.Now.Month <= requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(GetWeekNumber() <= requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(DateTime.Now.Day <= requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(DateTime.Now.Hour <= requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(DateTime.Now.Minute <= requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(DateTime.Now.Second <= requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed <= requirement.TimeRequirement1.GlobalSpeed);
                    break;
                case RequirementsData.TimeValue.EqualOrAbove:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(DateTime.Now.Year >= requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(DateTime.Now.Month >= requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(GetWeekNumber() >= requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(DateTime.Now.Day >= requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(DateTime.Now.Hour >= requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(DateTime.Now.Minute >= requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(DateTime.Now.Second >= requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed >= requirement.TimeRequirement1.GlobalSpeed);
                    break;
                case RequirementsData.TimeValue.Below:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(DateTime.Now.Year < requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(DateTime.Now.Month < requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(GetWeekNumber() < requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(DateTime.Now.Day < requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(DateTime.Now.Hour < requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(DateTime.Now.Minute < requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(DateTime.Now.Second < requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed < requirement.TimeRequirement1.GlobalSpeed);
                    break;
                case RequirementsData.TimeValue.Above:
                    if(requirement.TimeRequirement1.CheckYear) results.Add(DateTime.Now.Year > requirement.TimeRequirement1.Year);
                    if(requirement.TimeRequirement1.CheckMonth) results.Add(DateTime.Now.Month > requirement.TimeRequirement1.Month);
                    if(requirement.TimeRequirement1.CheckWeek) results.Add(GetWeekNumber() > requirement.TimeRequirement1.Week);
                    if(requirement.TimeRequirement1.CheckDay) results.Add(DateTime.Now.Day > requirement.TimeRequirement1.Day);
                    if(requirement.TimeRequirement1.CheckHour) results.Add(DateTime.Now.Hour > requirement.TimeRequirement1.Hour);
                    if(requirement.TimeRequirement1.CheckMinute) results.Add(DateTime.Now.Minute > requirement.TimeRequirement1.Minute);
                    if(requirement.TimeRequirement1.CheckSecond) results.Add(DateTime.Now.Second > requirement.TimeRequirement1.Second);
                    if(requirement.TimeRequirement1.CheckGlobalSpeed) results.Add(Character.Instance.CharacterData.Time.GlobalSpeed > requirement.TimeRequirement1.GlobalSpeed);
                    break;
            }

            return !results.Contains(false);
        }
        private int GetWeekNumber()
        {
            if (DateTime.Now.Day <= 7) return 1;
            if (DateTime.Now.Day <= 14) return 2;
            if (DateTime.Now.Day <= 21) return 3;
            if (DateTime.Now.Day <= 31) return 4;
            return -1;
        }
        private string HandleStringValue(string value)
        {
            if (value.Length == 1) return "0" + value;
            return value;
        }
    }
}