using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Templates
{
    public class RequirementsTemplate : RPGBuilderDatabaseEntry
    {
        public List<RequirementsData.RequirementGroup> Requirements = new List<RequirementsData.RequirementGroup>();
        
        public void UpdateEntryData(RequirementsTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;

            Requirements = new List<RequirementsData.RequirementGroup>();
            for (var index = 0; index < newEntryData.Requirements.Count; index++)
            {
                RequirementsData.RequirementGroup newGroup = new RequirementsData.RequirementGroup
                {
                     checkCount = newEntryData.Requirements[index].checkCount,
                     requiredCount = newEntryData.Requirements[index].requiredCount,
                };

                foreach (var newData in newEntryData.Requirements[index].Requirements)
                {
                    RequirementsData.Requirement requirement = new RequirementsData.Requirement
                    {
                        type = newData.type,
                        condition = newData.condition,
                        AbilityID = newData.AbilityID,
                        BonusID = newData.BonusID,
                        RecipeID = newData.RecipeID,
                        ResourceID = newData.ResourceID,
                        EffectID = newData.EffectID,
                        NPCID = newData.NPCID,
                        StatID = newData.StatID,
                        FactionID = newData.FactionID,
                        ComboID = newData.ComboID,
                        RaceID = newData.RaceID,
                        LevelsID = newData.LevelsID,
                        ClassID = newData.ClassID,
                        SpeciesID = newData.SpeciesID,
                        ItemID = newData.ItemID,
                        CurrencyID = newData.CurrencyID,
                        PointID = newData.PointID,
                        TalentTreeID = newData.TalentTreeID,
                        SkillID = newData.SkillID,
                        SpellbookID = newData.SpellbookID,
                        WeaponTemplateID = newData.WeaponTemplateID,
                        EnchantmentID = newData.EnchantmentID,
                        GearSetID = newData.GearSetID,
                        GameSceneID = newData.GameSceneID,
                        QuestID = newData.QuestID,
                        DialogueID = newData.DialogueID,

                        Knowledge = newData.Knowledge,
                        State = newData.State,
                        Comparison = newData.Comparison,
                        Value = newData.Value,
                        Ownership = newData.Ownership,
                        ItemCondition = newData.ItemCondition,
                        Progression = newData.Progression,
                        Entity = newData.Entity,
                        PointType = newData.PointType,
                        DialogueNodeState = newData.DialogueNodeState,
                        EffectCondition = newData.EffectCondition,
                        AmountType = newData.AmountType,
                        TimeType = newData.TimeType,
                        TimeValue = newData.TimeValue,

                        Amount1 = newData.Amount1,
                        Amount2 = newData.Amount2,
                        Float1 = newData.Float1,
                        Consume = newData.Consume,
                        BoolBalue1 = newData.BoolBalue1,
                        BoolBalue2 = newData.BoolBalue2,
                        BoolBalue3 = newData.BoolBalue3,
                        IsPercent = newData.IsPercent,

                        EffectTag = newData.EffectTag,
                        EffectType = newData.EffectType,
                        FactionStance = newData.FactionStance,
                        ItemType = newData.ItemType,
                        WeaponType = newData.WeaponType,
                        WeaponSlot = newData.WeaponSlot,
                        ArmorType = newData.ArmorType,
                        ArmorSlot = newData.ArmorSlot,
                        Gender = newData.Gender,
                        QuestState = newData.QuestState,
                        DialogueNode = newData.DialogueNode,
                        NPCFamily = newData.NPCFamily,
                        
                        TimeRequirement1 = newData.TimeRequirement1,
                        TimeRequirement2 = newData.TimeRequirement2,
                    };
                    newGroup.Requirements.Add(requirement);
                }
                
                Requirements.Add(newGroup);
            }
        }

        public List<RequirementsData.RequirementGroup> OverrideRequirements(List<RequirementsData.RequirementGroup> requirementGroups)
        {
            requirementGroups = new List<RequirementsData.RequirementGroup>();
            for (var index = 0; index < Requirements.Count; index++)
            {
                RequirementsData.RequirementGroup newGroup = new RequirementsData.RequirementGroup
                {
                     checkCount = Requirements[index].checkCount,
                     requiredCount = Requirements[index].requiredCount,
                };

                foreach (var newData in Requirements[index].Requirements)
                {
                    RequirementsData.Requirement requirement = new RequirementsData.Requirement
                    {
                        type = newData.type,
                        condition = newData.condition,
                        AbilityID = newData.AbilityID,
                        BonusID = newData.BonusID,
                        RecipeID = newData.RecipeID,
                        ResourceID = newData.ResourceID,
                        EffectID = newData.EffectID,
                        NPCID = newData.NPCID,
                        StatID = newData.StatID,
                        FactionID = newData.FactionID,
                        ComboID = newData.ComboID,
                        RaceID = newData.RaceID,
                        LevelsID = newData.LevelsID,
                        ClassID = newData.ClassID,
                        SpeciesID = newData.SpeciesID,
                        ItemID = newData.ItemID,
                        CurrencyID = newData.CurrencyID,
                        PointID = newData.PointID,
                        TalentTreeID = newData.TalentTreeID,
                        SkillID = newData.SkillID,
                        SpellbookID = newData.SpellbookID,
                        WeaponTemplateID = newData.WeaponTemplateID,
                        EnchantmentID = newData.EnchantmentID,
                        GearSetID = newData.GearSetID,
                        GameSceneID = newData.GameSceneID,
                        QuestID = newData.QuestID,
                        DialogueID = newData.DialogueID,

                        Knowledge = newData.Knowledge,
                        State = newData.State,
                        Comparison = newData.Comparison,
                        Value = newData.Value,
                        Ownership = newData.Ownership,
                        ItemCondition = newData.ItemCondition,
                        Progression = newData.Progression,
                        Entity = newData.Entity,
                        PointType = newData.PointType,
                        DialogueNodeState = newData.DialogueNodeState,
                        EffectCondition = newData.EffectCondition,
                        AmountType = newData.AmountType,
                        TimeType = newData.TimeType,
                        TimeValue = newData.TimeValue,

                        Amount1 = newData.Amount1,
                        Amount2 = newData.Amount2,
                        Float1 = newData.Float1,
                        Consume = newData.Consume,
                        BoolBalue1 = newData.BoolBalue1,
                        BoolBalue2 = newData.BoolBalue2,
                        BoolBalue3 = newData.BoolBalue3,
                        IsPercent = newData.IsPercent,

                        EffectTag = newData.EffectTag,
                        EffectType = newData.EffectType,
                        FactionStance = newData.FactionStance,
                        ItemType = newData.ItemType,
                        WeaponType = newData.WeaponType,
                        WeaponSlot = newData.WeaponSlot,
                        ArmorType = newData.ArmorType,
                        ArmorSlot = newData.ArmorSlot,
                        Gender = newData.Gender,
                        QuestState = newData.QuestState,
                        DialogueNode = newData.DialogueNode,
                        NPCFamily = newData.NPCFamily,
                        
                        TimeRequirement1 = new RequirementsData.TimeRequirement(),
                        TimeRequirement2 = new RequirementsData.TimeRequirement(),
                    };

                    requirement.TimeRequirement1.CheckYear = newData.TimeRequirement1.CheckYear;
                    requirement.TimeRequirement1.Year = newData.TimeRequirement1.Year;
                    requirement.TimeRequirement1.CheckMonth = newData.TimeRequirement1.CheckMonth;
                    requirement.TimeRequirement1.Month = newData.TimeRequirement1.Month;
                    requirement.TimeRequirement1.CheckWeek = newData.TimeRequirement1.CheckWeek;
                    requirement.TimeRequirement1.Week = newData.TimeRequirement1.Week;
                    requirement.TimeRequirement1.CheckDay = newData.TimeRequirement1.CheckDay;
                    requirement.TimeRequirement1.Day = newData.TimeRequirement1.Day;
                    requirement.TimeRequirement1.CheckHour = newData.TimeRequirement1.CheckHour;
                    requirement.TimeRequirement1.Hour = newData.TimeRequirement1.Hour;
                    requirement.TimeRequirement1.CheckMinute = newData.TimeRequirement1.CheckMinute;
                    requirement.TimeRequirement1.Minute = newData.TimeRequirement1.Minute;
                    requirement.TimeRequirement1.CheckSecond = newData.TimeRequirement1.CheckSecond;
                    requirement.TimeRequirement1.Second = newData.TimeRequirement1.Second;
                    requirement.TimeRequirement1.GlobalSpeed = newData.TimeRequirement1.GlobalSpeed;
                    requirement.TimeRequirement1.CheckGlobalSpeed = newData.TimeRequirement1.CheckGlobalSpeed;
                    
                    newGroup.Requirements.Add(requirement);
                }
                
                requirementGroups.Add(newGroup);
            }

            return requirementGroups;
        }
    }
}
