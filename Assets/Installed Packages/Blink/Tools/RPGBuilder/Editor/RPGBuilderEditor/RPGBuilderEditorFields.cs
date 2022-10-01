using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

public class RPGBuilderEditorFields : Editor
{
    
    public static GUIStyle GetLabelStyle()
    {
        return RPGBuilderEditor.Instance.EditorSkin.GetStyle("CustomLabel");
    }
    public static GUIStyle GetTitleLabelStyle()
    {
        return RPGBuilderEditor.Instance.EditorSkin.GetStyle("TitleLabel");
    }
    public static GUIStyle GetTextFieldStyle()
    {
        return RPGBuilderEditor.Instance.EditorSkin.GetStyle("CustomTextField");
    }
    
    public static Object DrawHorizontalObject<T>(string labelName, string tooltip, Object content) where T : Object
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip),
            GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = (T) EditorGUILayout.ObjectField(content, typeof(T), false);
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static Object DrawHorizontalSceneObject<T>(string labelName, string tooltip, Object content) where T : Object
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip),
            GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = (T) EditorGUILayout.ObjectField(content, typeof(T), true);
        GUILayout.EndHorizontal();
        return content;
    }

    public static void DrawHorizontalEntryInfo(int ID, string entryType)
    {
        RPGBuilderDatabaseEntry entryRef = RPGBuilderEditorUtility.GetEntryByID(ID, entryType);
        if(entryRef != null) {
            GUILayout.Box(entryRef.entryIcon.texture, RPGBuilderEditor.Instance.EditorSkin.GetStyle("CustomImage"), GUILayout.Width(20),GUILayout.Height(20));
            GUILayout.Space(5);
            DrawLabelExpanded(entryRef.entryName, "");
        }
        else
        {
            DrawLabelExpanded("- No Entry Selected -", "");
        }
    }

    public static bool DrawSmallRemoveButton()
    {
        return GUILayout.Button("X", RPGBuilderEditor.Instance.EditorSkin.GetStyle("SquareRemoveButton"),
            GUILayout.Width(20), GUILayout.Height(20));
    }
    
    public static RPGBuilderDatabaseEntry DrawTypeEntryField(string fieldName, List<RPGBuilderDatabaseEntry> entries, RPGBuilderDatabaseEntry entry)
    {
        DrawHorizontalLabel(fieldName, "");
        int typeEntryIndex = EditorGUILayout.Popup(
            RPGBuilderEditorUtility.GetTypeEntryIndexWithNull(entries, entry),
            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(entries.ToArray()));
        if (typeEntryIndex != -1)
        {
            entry = entries[typeEntryIndex];
            if (DrawSmallRemoveButton())
            {
                entry = null;
            }
        }
        GUILayout.EndHorizontal();
        return entry;
    }

    public static float DrawHorizontalFloatFillBar(string labelTtitle, string labelTooltip, float value)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelTtitle)) EditorGUILayout.LabelField(new GUIContent(labelTtitle, labelTooltip), RPGBuilderEditor.Instance.EditorSkin.GetStyle("FillBarLabel"),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        value = RPGBuilderEditorUtility.DrawHorizontalFloatFillBar(0, 100, value);
        GUILayout.Space(3);
        value = EditorGUILayout.FloatField(Mathf.Round(value * 100.0f) * 0.01f, 
                RPGBuilderEditor.Instance.EditorSkin.GetStyle("SmallTextField"), GUILayout.MaxWidth(45), GUILayout.MinHeight(17));
        EditorGUILayout.EndHorizontal();
        return value;
    }

    public static bool DrawHorizontalAddButton(string title, bool centered)
    {
        if(centered) RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);
        var clicked = GUILayout.Button(title, RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.ExpandWidth(true), GUILayout.Height(25));
        if(centered)  RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);
        return clicked;
    }
    
    public static bool DrawHorizontalEntryRemoveButton(int ID, string entryType)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(4);
        bool removedEntry = false;
        RPGBuilderDatabaseEntry entryRef = RPGBuilderEditorUtility.GetEntryByID(ID, entryType);
        if (entryRef != null)
        {
            RPGBuilderEditorModule module = RPGBuilderEditorUtility.GetModuleByEntryType(entryType);
            if (module != null)
            {
                if (module.ShowIconInList)
                {
                    GUILayout.Box(entryRef.entryIcon != null
                            ? entryRef.entryIcon.texture
                            : RPGBuilderEditor.Instance.EditorData.defaultEntryIcon.texture,
                        RPGBuilderEditor.Instance.EditorSkin.GetStyle("CustomImage"),
                        GUILayout.Width(21), GUILayout.Height(21));
                    GUILayout.Space(4);
                }

                DrawTitleLabelExpanded(entryRef.entryName, "");
            }
        }
        else
        {
            GUILayout.Space(4);
            DrawTitleLabelExpanded("- Select " + entryType + " -", "");
        }

        if (DrawSmallRemoveButton())
        {
            removedEntry = true;
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(3);
        return removedEntry;
    }

    public static List<RPGAbility.AbilityEffectsApplied> DrawEffectsAppliedList(List<RPGAbility.AbilityEffectsApplied> effectsApplied, bool margin)
    {
        if(margin) RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        for (var a = 0; a < effectsApplied.Count; a++)
        {
            GUILayout.Space(5);
            if (DrawHorizontalEntryRemoveButton( effectsApplied[a].effectID, "Effect"))
            {
                effectsApplied.RemoveAt(a);
                return effectsApplied;
            }

            effectsApplied[a].effectID = DrawDatabaseEntryField(effectsApplied[a].effectID, "Effect", "Effect", "");
            effectsApplied[a].effectRank = DrawEffectRankIndexField(effectsApplied[a].effectID, effectsApplied[a].effectRank);
            effectsApplied[a].target = (RPGCombatDATA.TARGET_TYPE) DrawHorizontalEnum("Applied On", "", (int)effectsApplied[a].target,
                Enum.GetNames(typeof(RPGCombatDATA.TARGET_TYPE)));
            effectsApplied[a].chance = DrawHorizontalFloatFillBar("Chance", "", effectsApplied[a].chance);
            effectsApplied[a].delay = DrawHorizontalFloatField("Delay", "", RPGBuilderEditor.Instance.FieldHeight, effectsApplied[a].delay);
            effectsApplied[a].isSpread = DrawHorizontalToggle("Spread to nearby Units?", "", RPGBuilderEditor.Instance.FieldHeight, effectsApplied[a].isSpread);
            if (effectsApplied[a].isSpread)
            {
                effectsApplied[a].spreadUnitMax = DrawHorizontalIntField("Max Unit", "", RPGBuilderEditor.Instance.FieldHeight, effectsApplied[a].spreadUnitMax);
                effectsApplied[a].spreadDistanceMax = DrawHorizontalFloatField("Max Spread Distance", "", RPGBuilderEditor.Instance.FieldHeight, effectsApplied[a].spreadDistanceMax);
            }
        }
        
        if(margin) RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        return effectsApplied;
    }

    public static int DrawHorizontalEnum(string label, string tooltip, int enumValue, string[] availableOptions)
    {
        DrawHorizontalLabel(label, tooltip);
        int enumIndex = EditorGUILayout.Popup(enumValue, availableOptions);
        EditorGUILayout.EndHorizontal();
        return enumIndex;
    }

    private static RequirementsData.RequirementType DrawRequirementType(RequirementsData.RequirementType value)
    {
        value = (RequirementsData.RequirementType) DrawHorizontalEnum("Type", "What type of requirement is it?", (int)value,
            Enum.GetNames(typeof(RequirementsData.RequirementType)));
        return value;
    }
    private static RequirementsData.Rule DrawRequirementCondition(RequirementsData.Rule value)
    {
        value = (RequirementsData.Rule) DrawHorizontalEnum("Rule", "", (int)value,
            Enum.GetNames(typeof(RequirementsData.Rule)));
        return value;
    }
    private static RequirementsData.Knowledge DrawRequirementKnowledge(RequirementsData.Knowledge value)
    {
        value = (RequirementsData.Knowledge) DrawHorizontalEnum("Condition", "", (int)value,
            Enum.GetNames(typeof(RequirementsData.Knowledge)));
        return value;
    }
    private static RequirementsData.State DrawRequirementState(RequirementsData.State value)
    {
        value = (RequirementsData.State) DrawHorizontalEnum("State", "", (int)value,
            Enum.GetNames(typeof(RequirementsData.State)));
        return value;
    }
    private static RequirementsData.Value DrawRequirementValue(RequirementsData.Value value)
    {
        value = (RequirementsData.Value) DrawHorizontalEnum("Value Type", "", (int)value,
            Enum.GetNames(typeof(RequirementsData.Value)));
        return value;
    }
    private static RequirementsData.Entity DrawRequirementEntity(RequirementsData.Entity value)
    {
        value = (RequirementsData.Entity) DrawHorizontalEnum("Entity", "", (int)value,
            Enum.GetNames(typeof(RequirementsData.Entity)));
        return value;
    }
    private static RequirementsData.Comparison DrawRequirementComparison(RequirementsData.Comparison value)
    {
        value = (RequirementsData.Comparison) DrawHorizontalEnum("Check", "", (int)value,
            Enum.GetNames(typeof(RequirementsData.Comparison)));
        return value;
    }
    private static RequirementsData.Ownership DrawRequirementOwnership(RequirementsData.Ownership value)
    {
        value = (RequirementsData.Ownership) DrawHorizontalEnum("Ownership", "", (int)value,
            Enum.GetNames(typeof(RequirementsData.Ownership)));
        return value;
    }
    private static RequirementsData.ItemCondition DrawRequirementItemCondition(RequirementsData.ItemCondition value)
    {
        value = (RequirementsData.ItemCondition) DrawHorizontalEnum("Condition", "", (int)value,
            Enum.GetNames(typeof(RequirementsData.ItemCondition)));
        return value;
    }
    private static bool DrawRequirementBoolValue(string label, bool value)
    {
        value = DrawHorizontalToggle(label, "", RPGBuilderEditor.Instance.FieldHeight, value);
        return value;
    }

    private static RequirementsData.TimeRequirement DrawTimeRequirement(RequirementsData.TimeRequirement value)
    {

        EditorGUILayout.BeginHorizontal();
        value.CheckYear = DrawHorizontalToggle("Check Year?", "",
            RPGBuilderEditor.Instance.FieldHeight, value.CheckYear);
        if (value.CheckYear)
        {
            value.Year = DrawHorizontalIntField("", "", RPGBuilderEditor.Instance.FieldHeight,
                value.Year);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        value.CheckMonth = DrawHorizontalToggle("Check Month?", "",
            RPGBuilderEditor.Instance.FieldHeight, value.CheckMonth);
        if (value.CheckMonth)
        {
            value.Month = DrawHorizontalIntField("", "", RPGBuilderEditor.Instance.FieldHeight,
                value.Month);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        value.CheckWeek = DrawHorizontalToggle("Check Week?", "",
            RPGBuilderEditor.Instance.FieldHeight, value.CheckWeek);
        if (value.CheckWeek)
        {
            value.Week = DrawHorizontalIntField("", "", RPGBuilderEditor.Instance.FieldHeight,
                value.Week);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        value.CheckDay = DrawHorizontalToggle("Check Day?", "",
            RPGBuilderEditor.Instance.FieldHeight, value.CheckDay);
        if (value.CheckDay)
        {
            value.Day = DrawHorizontalIntField("", "", RPGBuilderEditor.Instance.FieldHeight,
                value.Day);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        value.CheckHour = DrawHorizontalToggle("Check Hour?", "",
            RPGBuilderEditor.Instance.FieldHeight, value.CheckHour);
        if (value.CheckHour)
        {
            value.Hour = DrawHorizontalIntField("", "", RPGBuilderEditor.Instance.FieldHeight,
                value.Hour);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        value.CheckMinute = DrawHorizontalToggle("Check Minute?", "",
            RPGBuilderEditor.Instance.FieldHeight, value.CheckMinute);
        if (value.CheckMinute)
        {
            value.Minute = DrawHorizontalIntField("", "", RPGBuilderEditor.Instance.FieldHeight,
                value.Minute);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        value.CheckSecond = DrawHorizontalToggle("Check Second?", "",
            RPGBuilderEditor.Instance.FieldHeight, value.CheckSecond);
        if (value.CheckSecond)
        {
            value.Second = DrawHorizontalIntField("", "", RPGBuilderEditor.Instance.FieldHeight,
                value.Second);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        value.CheckGlobalSpeed = DrawHorizontalToggle("Check Global Speed?", "",
            RPGBuilderEditor.Instance.FieldHeight, value.CheckGlobalSpeed);
        if (value.CheckGlobalSpeed)
        {
            value.GlobalSpeed = DrawHorizontalIntField("", "", RPGBuilderEditor.Instance.FieldHeight,
                value.GlobalSpeed);
        }
        EditorGUILayout.EndHorizontal();

        return value;
    }

    public static List<RequirementsData.RequirementGroup> DrawRequirementGroupsList(List<RequirementsData.RequirementGroup> requirementGroups, bool margin)
    {
        if (margin) RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        int enumIndex = 0;
        
        GUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        RequirementsTemplate template = null;
        template = (RequirementsTemplate) DrawHorizontalObject<RequirementsTemplate>("Template Override", "", template);
        if (EditorGUI.EndChangeCheck())
        {
            if (template != null)
            {
                if (EditorUtility.DisplayDialog("OVERRIDE?",
                    "Are you sure you want to OVERRIDE these requirements with the template?", "Yes", "Cancel"))
                {
                    requirementGroups.Clear();
                    requirementGroups = template.OverrideRequirements(requirementGroups);
                }
            }
        }
        
        for (var groupIndex = 0; groupIndex < requirementGroups.Count; groupIndex++)
        {
            RequirementsData.RequirementGroup group = requirementGroups[groupIndex];
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            DrawTitleLabel("Requirement Group " + (groupIndex + 1), "");
            if (GUILayout.Button("+", RPGBuilderEditor.Instance.EditorSkin.GetStyle("SquareAddButton"),
                GUILayout.Width(20), GUILayout.Height(20)))
            {
                requirementGroups[groupIndex].Requirements.Add(new RequirementsData.Requirement());
            }

            if (DrawSmallRemoveButton())
            {
                requirementGroups.Remove(group);
                return requirementGroups;
            }

            EditorGUILayout.EndHorizontal();
            group.checkCount = DrawHorizontalToggle("Check multiple optional?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                group.checkCount);
            if (group.checkCount)
            {
                group.requiredCount = DrawHorizontalIntField("Required valid optional", "",
                    RPGBuilderEditor.Instance.FieldHeight, group.requiredCount);
            }
            
            for (var index = 0; index < group.Requirements.Count; index++)
            {
                var requirement = group.Requirements[index];
                GUILayout.Space(5);
                RPGBuilderEditorUtility.StartHorizontalMargin(25, true);

                switch (requirement.type)
                {
                    case RequirementsData.RequirementType.Ability:
                        if (DrawHorizontalEntryRemoveButton(requirement.AbilityID, "Ability"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Knowledge = DrawRequirementKnowledge(requirement.Knowledge);
                        requirement.AbilityID = DrawDatabaseEntryField(requirement.AbilityID, "Ability", "Ability", "");

                        if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                        {
                            requirement.BoolBalue1 = DrawRequirementBoolValue("Check rank?", requirement.BoolBalue1);
                            if (requirement.BoolBalue1)
                            {
                                requirement.Amount1 =
                                    DrawAbilityRankIndexField(requirement.AbilityID, requirement.Amount1);
                            }
                        }
                        break;
                    case RequirementsData.RequirementType.Bonus:
                        if (DrawHorizontalEntryRemoveButton(requirement.BonusID, "Bonus"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Knowledge = DrawRequirementKnowledge(requirement.Knowledge);

                        requirement.BonusID = DrawDatabaseEntryField(requirement.BonusID, "Bonus", "Bonus", "");

                        if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                        {
                            requirement.BoolBalue1 = DrawRequirementBoolValue("Check rank?", requirement.BoolBalue1);
                            if (requirement.BoolBalue1)
                            {
                                requirement.Amount1 =
                                    DrawBonusRankIndexField(requirement.BonusID, requirement.Amount1);
                            }
                        }
                        break;
                    case RequirementsData.RequirementType.Recipe:
                        if (DrawHorizontalEntryRemoveButton(requirement.RecipeID, "Recipe"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Knowledge = DrawRequirementKnowledge(requirement.Knowledge);

                        requirement.RecipeID = DrawDatabaseEntryField(requirement.RecipeID, "Recipe", "Recipe", "");

                        if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                        {
                            requirement.BoolBalue1 = DrawRequirementBoolValue("Check rank?", requirement.BoolBalue1);
                            if (requirement.BoolBalue1)
                            {
                                requirement.Amount1 =
                                    DrawRecipeRankIndexField(requirement.RecipeID, requirement.Amount1);
                            }
                        }
                        break;
                    case RequirementsData.RequirementType.Resource:
                        if (DrawHorizontalEntryRemoveButton(requirement.ResourceID, "Resource"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Knowledge = DrawRequirementKnowledge(requirement.Knowledge);

                        requirement.ResourceID =
                            DrawDatabaseEntryField(requirement.ResourceID, "Resource", "Resource", "");

                        if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                        {
                            requirement.BoolBalue1 = DrawRequirementBoolValue("Check rank?", requirement.BoolBalue1);
                            if (requirement.BoolBalue1)
                            {
                                requirement.Amount1 =
                                    DrawResourceRankIndexField(requirement.ResourceID, requirement.Amount1);
                            }
                        }
                        break;
                    case RequirementsData.RequirementType.Effect:
                        if (DrawHorizontalEntryRemoveButton(requirement.EffectID, "Effect"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.State = DrawRequirementState(requirement.State);
                        
                        requirement.EffectCondition =
                            (RequirementsData.EffectCondition) DrawHorizontalEnum("Condition", "",
                                (int)requirement.EffectCondition,
                                Enum.GetNames(typeof(RequirementsData.EffectCondition)));
                        
                        if (requirement.EffectCondition == RequirementsData.EffectCondition.Effect)
                        {
                            requirement.EffectID = DrawDatabaseEntryField(requirement.EffectID, "Effect", "Effect", "");
                        } else if (requirement.EffectCondition == RequirementsData.EffectCondition.EffectType)
                        {
                            requirement.EffectType =
                                (RPGEffect.EFFECT_TYPE) DrawHorizontalEnum("Effect Type", "",
                                    (int)requirement.EffectType,
                                    Enum.GetNames(typeof(RPGEffect.EFFECT_TYPE)));
                        } else if (requirement.EffectCondition == RequirementsData.EffectCondition.EffectTag)
                        {
                            DrawHorizontalLabel("Effect Tag", "");
                            enumIndex = EditorGUILayout.Popup(
                                RPGBuilderEditorUtility.GetTypeEntryIndexWithNull(RPGBuilderEditorUtility.GetModuleByName("Effect Tags").databaseEntries, requirement.EffectTag),
                                RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility.GetModuleByName("Effect Tags").databaseEntries.ToArray()));
                            if (enumIndex != -1)
                            {
                                requirement.EffectTag = (RPGBEffectTag) RPGBuilderEditorUtility
                                    .GetModuleByName("Effect Tags").databaseEntries[enumIndex];
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        if (requirement.State == RequirementsData.State.Active && requirement.EffectCondition == RequirementsData.EffectCondition.Effect)
                        {
                            requirement.BoolBalue1 = DrawRequirementBoolValue("Check stacks?", requirement.BoolBalue1);
                            if (requirement.BoolBalue1)
                            {
                                requirement.Value = DrawRequirementValue(requirement.Value);
                                requirement.Amount1 = DrawHorizontalIntField("Stacks required", "",
                                    RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                            }
                            requirement.Consume = DrawRequirementBoolValue("Consume?", requirement.Consume);
                        }
                        
                        break;
                    case RequirementsData.RequirementType.NPCKilled:
                        if (DrawHorizontalEntryRemoveButton(requirement.NPCID, "NPC"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.NPCID = DrawDatabaseEntryField(requirement.NPCID, "NPC", "NPC", "");

                        requirement.BoolBalue1 = DrawRequirementBoolValue("Check Amount?", requirement.BoolBalue1);
                        if (requirement.BoolBalue1)
                        {
                            requirement.Value = DrawRequirementValue(requirement.Value);
                            requirement.Amount1 = DrawHorizontalIntField("Kills required", "",
                                RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                        }
                        break;
                    case RequirementsData.RequirementType.NPCFamily:
                        EditorGUILayout.BeginHorizontal();
                        requirement.type = DrawRequirementType(requirement.type);
                        if (DrawSmallRemoveButton())
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Comparison = DrawRequirementComparison(requirement.Comparison);
                        
                        DrawHorizontalLabel("NPC Family", "");
                        enumIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndexWithNull(
                                RPGBuilderEditorUtility.GetModuleByName("Families").databaseEntries, requirement.NPCFamily),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility
                                .GetModuleByName("Families").databaseEntries.ToArray()));
                        if (enumIndex != -1)
                        {
                            requirement.NPCFamily = (RPGBNPCFamily) RPGBuilderEditorUtility.GetModuleByName("Families").databaseEntries[enumIndex];
                        }
                        EditorGUILayout.EndHorizontal();
                        break;
                    case RequirementsData.RequirementType.Stat:
                        if (DrawHorizontalEntryRemoveButton(requirement.StatID, "Stat"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Entity = DrawRequirementEntity(requirement.Entity);
                        requirement.StatID = DrawDatabaseEntryField(requirement.StatID, "Stat", "Stat", "");

                            requirement.Value = DrawRequirementValue(requirement.Value);
                        requirement.IsPercent = DrawHorizontalToggle("Percent", "",
                            RPGBuilderEditor.Instance.FieldHeight, requirement.IsPercent);
                        if (requirement.IsPercent)
                        {
                            requirement.Float1 = DrawHorizontalFloatField("Amount", "", RPGBuilderEditor.Instance.FieldHeight, requirement.Float1);
                            if (requirement.Float1 > 100) requirement.Float1 = 100;
                            if (requirement.Float1 < 0) requirement.Float1 = 0;
                        }
                        else
                        {
                            requirement.Amount1 = DrawHorizontalIntField("Amount", "",
                                RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                        }
                        break;
                    case RequirementsData.RequirementType.StatCost:
                        if (requirement.StatID != -1)
                        {
                            RPGStat stat = (RPGStat) RPGBuilderEditorUtility.GetEntryByID(requirement.StatID, "Stat");
                            if (!stat.isVitalityStat) requirement.StatID = -1;
                        }
                        
                        if (DrawHorizontalEntryRemoveButton(requirement.StatID, "Stat"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Entity = DrawRequirementEntity(requirement.Entity);
                        requirement.StatID = DrawDatabaseEntryField(requirement.StatID, "Stat", "Vitality Stat", "");

                        
                        requirement.AmountType = (RequirementsData.AmountType) DrawHorizontalEnum("Amount Type", "", (int)requirement.AmountType,
                            Enum.GetNames(typeof(RequirementsData.AmountType)));
                        requirement.Amount1 = DrawHorizontalIntField("Amount", "",
                            RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                        break;
                    case RequirementsData.RequirementType.Faction:
                        if (DrawHorizontalEntryRemoveButton(requirement.FactionID, "Faction"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Entity = DrawRequirementEntity(requirement.Entity);
                        requirement.Comparison = DrawRequirementComparison(requirement.Comparison);
                        requirement.FactionID = DrawDatabaseEntryField(requirement.FactionID, "Faction", "Faction", "");
                        break;
                    case RequirementsData.RequirementType.FactionStance:
                        if (DrawHorizontalEntryRemoveButton(requirement.FactionID, "Faction"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Entity = DrawRequirementEntity(requirement.Entity);
                        requirement.Comparison = DrawRequirementComparison(requirement.Comparison);
                        requirement.FactionID = DrawDatabaseEntryField(requirement.FactionID, "Faction", "Faction", "");
                        
                        DrawHorizontalLabel("Stance", "");
                        enumIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(RPGBuilderEditorUtility.GetModuleByName("Faction Stances").databaseEntries, requirement.FactionStance),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility.GetModuleByName("Faction Stances").databaseEntries.ToArray()));
                            requirement.FactionStance = (RPGBFactionStance) RPGBuilderEditorUtility
                                .GetModuleByName("Faction Stances").databaseEntries[enumIndex];

                        EditorGUILayout.EndHorizontal();
                        break;
                    case RequirementsData.RequirementType.Combo:
                        if (DrawHorizontalEntryRemoveButton(requirement.ComboID, "Combo"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.State = DrawRequirementState(requirement.State);
                        requirement.ComboID = DrawDatabaseEntryField(requirement.ComboID, "Combo", "Combo", "");
                        break;
                    case RequirementsData.RequirementType.Race:
                        if (DrawHorizontalEntryRemoveButton(requirement.RaceID, "Race"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Comparison = DrawRequirementComparison(requirement.Comparison);
                        requirement.RaceID = DrawDatabaseEntryField(requirement.RaceID, "Race", "Race", "");
                        break;
                    case RequirementsData.RequirementType.Level:
                        EditorGUILayout.BeginHorizontal();
                        requirement.type = DrawRequirementType(requirement.type);
                        if (DrawSmallRemoveButton())
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }
                        EditorGUILayout.EndHorizontal();
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        
                        requirement.Entity = DrawRequirementEntity(requirement.Entity);
                        requirement.Value = DrawRequirementValue(requirement.Value);
                        requirement.Amount1 = DrawHorizontalIntField("Level", "",
                            RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                        break;
                    case RequirementsData.RequirementType.Gender:
                        EditorGUILayout.BeginHorizontal();
                        requirement.type = DrawRequirementType(requirement.type);
                        if (DrawSmallRemoveButton())
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Comparison = DrawRequirementComparison(requirement.Comparison);
                        
                        DrawHorizontalLabel("Gender", "");
                        enumIndex = EditorGUILayout.Popup(
                            RPGBuilderEditorUtility.GetTypeEntryIndex(
                                RPGBuilderEditorUtility.GetModuleByName("Genders").databaseEntries,
                                requirement.Gender),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility
                                .GetModuleByName("Genders").databaseEntries.ToArray()));
                        requirement.Gender = (RPGBGender) RPGBuilderEditorUtility
                            .GetModuleByName("Genders").databaseEntries[enumIndex];
                        EditorGUILayout.EndHorizontal();
                        break;
                    case RequirementsData.RequirementType.Class:
                        if (DrawHorizontalEntryRemoveButton(requirement.ClassID, "Class"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Comparison = DrawRequirementComparison(requirement.Comparison);
                        requirement.ClassID = DrawDatabaseEntryField(requirement.ClassID, "Class", "Class", "");
                        break;
                    case RequirementsData.RequirementType.Species:
                        if (DrawHorizontalEntryRemoveButton(requirement.SpeciesID, "Species"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Entity = DrawRequirementEntity(requirement.Entity);
                        requirement.Comparison = DrawRequirementComparison(requirement.Comparison);
                        requirement.SpeciesID = DrawDatabaseEntryField(requirement.SpeciesID, "Species", "Species", "");
                        break;
                    case RequirementsData.RequirementType.Item:

                        switch (requirement.ItemCondition)
                        {
                            case RequirementsData.ItemCondition.Item:
                                if (DrawHorizontalEntryRemoveButton(requirement.ItemID, "Item"))
                                {
                                    group.Requirements.Remove(requirement);
                                    return requirementGroups;
                                }
                                break;
                            case RequirementsData.ItemCondition.ItemType:
                                EditorGUILayout.BeginHorizontal();
                                DrawTitleLabelExpanded(requirement.ItemType.entryName + " " + requirement.Ownership, "");
                                if (DrawSmallRemoveButton())
                                {
                                    group.Requirements.Remove(requirement);
                                    return requirementGroups;
                                }
                                EditorGUILayout.EndHorizontal();
;                                break;
                            case RequirementsData.ItemCondition.WeaponType:
                                EditorGUILayout.BeginHorizontal();
                                DrawTitleLabelExpanded(requirement.WeaponType.entryName + " " + requirement.Ownership, "");
                                if (DrawSmallRemoveButton())
                                {
                                    group.Requirements.Remove(requirement);
                                    return requirementGroups;
                                }
                                EditorGUILayout.EndHorizontal();
                                break;
                            case RequirementsData.ItemCondition.WeaponSlot:
                                EditorGUILayout.BeginHorizontal();
                                DrawTitleLabelExpanded(requirement.WeaponSlot.entryName + " " + requirement.Ownership, "");
                                if (DrawSmallRemoveButton())
                                {
                                    group.Requirements.Remove(requirement);
                                    return requirementGroups;
                                }
                                EditorGUILayout.EndHorizontal();
                                break;
                            case RequirementsData.ItemCondition.ArmorType:
                                EditorGUILayout.BeginHorizontal();
                                DrawTitleLabelExpanded(requirement.ArmorType.entryName + " " + requirement.Ownership, "");
                                if (DrawSmallRemoveButton())
                                {
                                    group.Requirements.Remove(requirement);
                                    return requirementGroups;
                                }
                                EditorGUILayout.EndHorizontal();
                                break;
                            case RequirementsData.ItemCondition.ArmorSlot:
                                EditorGUILayout.BeginHorizontal();
                                DrawTitleLabelExpanded(requirement.ArmorSlot.entryName + " " + requirement.Ownership, "");
                                if (DrawSmallRemoveButton())
                                {
                                    group.Requirements.Remove(requirement);
                                    return requirementGroups;
                                }
                                EditorGUILayout.EndHorizontal();
                                break;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Ownership = DrawRequirementOwnership(requirement.Ownership);
                        requirement.ItemCondition = DrawRequirementItemCondition(requirement.ItemCondition);

                        switch (requirement.ItemCondition)
                        {
                            case RequirementsData.ItemCondition.Item:
                                requirement.ItemID = DrawDatabaseEntryField(requirement.ItemID, "Item", "Item", "");
                                requirement.Consume = DrawRequirementBoolValue("Consume?", requirement.Consume);
                                requirement.Amount1 = DrawHorizontalIntField("Amount", "",
                                    RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                                break;
                            case RequirementsData.ItemCondition.ItemType:
                                DrawHorizontalLabel("Item Type", "");
                                enumIndex = EditorGUILayout.Popup(
                                    RPGBuilderEditorUtility.GetTypeEntryIndex(RPGBuilderEditorUtility.GetModuleByName("Item Types").databaseEntries, requirement.ItemType),
                                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility.GetModuleByName("Item Types").databaseEntries.ToArray()));
                                requirement.ItemType = (RPGBItemType) RPGBuilderEditorUtility.GetModuleByName("Item Types").databaseEntries[enumIndex];
                                EditorGUILayout.EndHorizontal();
                                break;
                            case RequirementsData.ItemCondition.WeaponType:
                                DrawHorizontalLabel("Weapon Type", "");
                                enumIndex = EditorGUILayout.Popup(
                                    RPGBuilderEditorUtility.GetTypeEntryIndex(RPGBuilderEditorUtility.GetModuleByName("Weapon Types").databaseEntries, requirement.WeaponType),
                                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility.GetModuleByName("Weapon Types").databaseEntries.ToArray()));
                                requirement.WeaponType = (RPGBWeaponType) RPGBuilderEditorUtility.GetModuleByName("Weapon Types").databaseEntries[enumIndex];
                                EditorGUILayout.EndHorizontal();
                                break;
                            case RequirementsData.ItemCondition.ArmorType:
                                DrawHorizontalLabel("Armor Type", "");
                                enumIndex = EditorGUILayout.Popup(
                                    RPGBuilderEditorUtility.GetTypeEntryIndex(RPGBuilderEditorUtility.GetModuleByName("Armor Types").databaseEntries, requirement.ArmorType),
                                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility.GetModuleByName("Armor Types").databaseEntries.ToArray()));
                                requirement.ArmorType = (RPGBArmorType) RPGBuilderEditorUtility.GetModuleByName("Armor Types").databaseEntries[enumIndex];
                                EditorGUILayout.EndHorizontal();
                                break;
                            case RequirementsData.ItemCondition.WeaponSlot:
                                DrawHorizontalLabel("Weapon Slot", "");
                                enumIndex = EditorGUILayout.Popup(
                                    RPGBuilderEditorUtility.GetTypeEntryIndex(RPGBuilderEditorUtility.GetModuleByName("Weapon Hand Slots").databaseEntries, requirement.WeaponSlot),
                                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility.GetModuleByName("Weapon Hand Slots").databaseEntries.ToArray()));
                                requirement.WeaponSlot = (RPGBWeaponHandSlot) RPGBuilderEditorUtility.GetModuleByName("Weapon Hand Slots").databaseEntries[enumIndex];
                                EditorGUILayout.EndHorizontal();
                                break;
                            case RequirementsData.ItemCondition.ArmorSlot:
                                DrawHorizontalLabel("Armor Slot", "");
                                enumIndex = EditorGUILayout.Popup(
                                    RPGBuilderEditorUtility.GetTypeEntryIndex(RPGBuilderEditorUtility.GetModuleByName("Armor Slots").databaseEntries, requirement.ArmorSlot),
                                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility.GetModuleByName("Armor Slots").databaseEntries.ToArray()));
                                requirement.ArmorSlot = (RPGBArmorSlot) RPGBuilderEditorUtility.GetModuleByName("Armor Slots").databaseEntries[enumIndex];
                                EditorGUILayout.EndHorizontal();
                                break;
                        }
                        break;
                    case RequirementsData.RequirementType.Currency:
                        if (DrawHorizontalEntryRemoveButton(requirement.CurrencyID, "Currency"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.CurrencyID = DrawDatabaseEntryField(requirement.CurrencyID, "Currency", "Currency", "");

                        requirement.Value = DrawRequirementValue(requirement.Value);
                        requirement.Amount1 = DrawHorizontalIntField("Amount", "",
                            RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                        requirement.Consume = DrawRequirementBoolValue("Consume?", requirement.Consume);
                        break;
                    case RequirementsData.RequirementType.Point:
                        if (DrawHorizontalEntryRemoveButton(requirement.PointID, "Point"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.PointType =
                            (RequirementsData.PointType) DrawHorizontalEnum("Condition", "",
                                (int)requirement.PointType,
                                Enum.GetNames(typeof(RequirementsData.PointType)));
                        requirement.PointID = DrawDatabaseEntryField(requirement.PointID, "Point", "Point", "");
                        
                        requirement.Value = DrawRequirementValue(requirement.Value);
                        requirement.Amount1 = DrawHorizontalIntField("Amount", "",
                            RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                        if (requirement.PointType == RequirementsData.PointType.Available)
                        {
                            requirement.BoolBalue2 = DrawRequirementBoolValue("Consume?", requirement.BoolBalue2);
                        }
                        break;
                    case RequirementsData.RequirementType.TalentTree:
                        if (DrawHorizontalEntryRemoveButton(requirement.TalentTreeID, "TalentTree"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Knowledge = DrawRequirementKnowledge(requirement.Knowledge);
                        requirement.TalentTreeID = DrawDatabaseEntryField(requirement.TalentTreeID, "TalentTree", "TalentTree", "");
                        break;
                    case RequirementsData.RequirementType.Skill:
                        if (DrawHorizontalEntryRemoveButton(requirement.SkillID, "Skill"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Knowledge = DrawRequirementKnowledge(requirement.Knowledge);
                        requirement.SkillID = DrawDatabaseEntryField(requirement.SkillID, "Skill", "Skill", "");

                        if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                        {
                            requirement.BoolBalue1 = DrawHorizontalToggle("Check level?", "",
                                RPGBuilderEditor.Instance.FieldHeight, requirement.BoolBalue1);
                            if (requirement.BoolBalue1)
                            {
                                requirement.Value = DrawRequirementValue(requirement.Value);
                                requirement.Amount1 = DrawHorizontalIntField("Level", "",
                                    RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                            }
                        }

                        break;
                    case RequirementsData.RequirementType.Spellbook:
                        requirement.type = DrawRequirementType(requirement.type);
                        DrawLabelExpanded("Not yet implemented", "");
                        /*if (DrawHorizontalEntryRemoveButton(requirement.SpellbookID, "Spellbook"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.Knowledge = DrawRequirementKnowledge(requirement.Knowledge);
                        requirement.SpellbookID = DrawDatabaseEntryField(requirement.SpellbookID, "Spellbook", "Spellbook", "");*/
                        break;
                    case RequirementsData.RequirementType.WeaponTemplate:
                        if (DrawHorizontalEntryRemoveButton(requirement.WeaponTemplateID, "WeaponTemplate"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Knowledge = DrawRequirementKnowledge(requirement.Knowledge);
                        requirement.WeaponTemplateID = DrawDatabaseEntryField(requirement.WeaponTemplateID, "WeaponTemplate", "Weapon Template", "");

                        if (requirement.Knowledge == RequirementsData.Knowledge.Known)
                        {
                            requirement.BoolBalue1 = DrawHorizontalToggle("Check level?", "",
                                RPGBuilderEditor.Instance.FieldHeight, requirement.BoolBalue1);
                            if (requirement.BoolBalue1)
                            {
                                requirement.Value = DrawRequirementValue(requirement.Value);
                                requirement.Amount1 = DrawHorizontalIntField("Level", "",
                                    RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                            }
                        }

                        break;
                    case RequirementsData.RequirementType.Enchantment:
                        if (DrawHorizontalEntryRemoveButton(requirement.EnchantmentID, "Enchantment"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.State = DrawRequirementState(requirement.State);
                        requirement.EnchantmentID = DrawDatabaseEntryField(requirement.EnchantmentID, "Enchantment", "Enchantment", "");
                        break;
                    case RequirementsData.RequirementType.GearSet:
                        if (DrawHorizontalEntryRemoveButton(requirement.GearSetID, "GearSet"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.State = DrawRequirementState(requirement.State);
                        requirement.GearSetID = DrawDatabaseEntryField(requirement.GearSetID, "GearSet", "GearSet", "");
                        break;
                    case RequirementsData.RequirementType.GameScene:
                        if (DrawHorizontalEntryRemoveButton(requirement.GameSceneID, "GameScene"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Comparison = DrawRequirementComparison(requirement.Comparison);
                        requirement.GameSceneID = DrawDatabaseEntryField(requirement.GameSceneID, "GameScene", "GameScene", "");
                        break;
                    case RequirementsData.RequirementType.Quest:
                        if (DrawHorizontalEntryRemoveButton(requirement.QuestID, "Quest"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.QuestID = DrawDatabaseEntryField(requirement.QuestID, "Quest", "Quest", "");
                        
                        requirement.QuestState =
                            (QuestManager.questState) DrawHorizontalEnum("State", "",
                                (int)requirement.QuestState,
                                Enum.GetNames(typeof(QuestManager.questState)));
                        break;
                    case RequirementsData.RequirementType.DialogueNode:
                        if (DrawHorizontalEntryRemoveButton(requirement.DialogueID, "Dialogue"))
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }

                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.DialogueID = DrawDatabaseEntryField(requirement.DialogueID, "Dialogue", "Dialogue", "");

                        RPGDialogue dialogue = (RPGDialogue)
                            RPGBuilderEditorUtility.GetEntryByID(requirement.DialogueID, "Dialogue");

                        if (dialogue != null)
                        {
                            DrawHorizontalLabel("Node", "");
                            List<RPGDialogueTextNode> textNodes =
                                RPGBuilderEditorUtility.GetDialogueTextNodes(dialogue.dialogueGraph.nodes);
                            enumIndex = EditorGUILayout.Popup(
                                RPGBuilderEditorUtility.GetDialogueNodeIndex(textNodes, requirement.DialogueNode),
                                RPGBuilderEditorUtility.GetDialogueTextNodesAsStringArray(textNodes.ToArray()));

                            requirement.DialogueNode = textNodes[enumIndex];
                            EditorGUILayout.EndHorizontal();

                            requirement.DialogueNodeState =
                                (RequirementsData.DialogueNodeState) DrawHorizontalEnum("State", "",
                                    (int) requirement.DialogueNodeState,
                                    Enum.GetNames(typeof(RequirementsData.DialogueNodeState)));

                            if (requirement.DialogueNodeState != RequirementsData.DialogueNodeState.Completed)
                            {
                                requirement.BoolBalue1 = DrawHorizontalToggle("Check amount?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, requirement.BoolBalue1);
                                requirement.Value = DrawRequirementValue(requirement.Value);
                                
                                if (requirement.BoolBalue1)
                                {
                                    if (requirement.DialogueNodeState == RequirementsData.DialogueNodeState.Viewed)
                                    {
                                        requirement.Amount1 = DrawHorizontalIntField("Viewed Amount", "",
                                            RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                                    }
                                    else if (requirement.DialogueNodeState ==
                                             RequirementsData.DialogueNodeState.Clicked)
                                    {
                                        requirement.Amount1 = DrawHorizontalIntField("Clicked Amount", "",
                                            RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                                    }
                                }
                            }
                        }

                        break;
                    case RequirementsData.RequirementType.Region:
                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.Comparison = DrawRequirementComparison(requirement.Comparison);
                        requirement.Region = (RegionTemplate) DrawHorizontalObject<RegionTemplate>("Region", "", requirement.Region);
                        break;
                    case RequirementsData.RequirementType.CombatState:
                        EditorGUILayout.BeginHorizontal();
                        requirement.type = DrawRequirementType(requirement.type);
                        if (DrawSmallRemoveButton())
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.BoolBalue1 = DrawHorizontalToggle("In Combat", "",
                            RPGBuilderEditor.Instance.FieldHeight, requirement.BoolBalue1);
                        break;
                    case RequirementsData.RequirementType.Stealth:
                        EditorGUILayout.BeginHorizontal();
                        requirement.type = DrawRequirementType(requirement.type);
                        if (DrawSmallRemoveButton())
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.BoolBalue1 = DrawHorizontalToggle("Stealth", "",
                            RPGBuilderEditor.Instance.FieldHeight, requirement.BoolBalue1);
                        break;
                    case RequirementsData.RequirementType.Mounted:
                        EditorGUILayout.BeginHorizontal();
                        requirement.type = DrawRequirementType(requirement.type);
                        if (DrawSmallRemoveButton())
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.BoolBalue1 = DrawHorizontalToggle("Mounted", "",
                            RPGBuilderEditor.Instance.FieldHeight, requirement.BoolBalue1);
                        break;
                    case RequirementsData.RequirementType.Grounded:
                        EditorGUILayout.BeginHorizontal();
                        requirement.type = DrawRequirementType(requirement.type);
                        if (DrawSmallRemoveButton())
                        {
                            group.Requirements.Remove(requirement);
                            return requirementGroups;
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        requirement.BoolBalue1 = DrawHorizontalToggle("Grounded", "",
                            RPGBuilderEditor.Instance.FieldHeight, requirement.BoolBalue1);
                        break;
                    case RequirementsData.RequirementType.Time:
                        requirement.type = DrawRequirementType(requirement.type);
                        requirement.condition = DrawRequirementCondition(requirement.condition);
                        
                        requirement.TimeType =
                            (RequirementsData.TimeType) DrawHorizontalEnum("Time Type", "",
                                (int)requirement.TimeType,
                                Enum.GetNames(typeof(RequirementsData.TimeType)));
                        
                        requirement.Value = DrawRequirementValue(requirement.Value);

                        if (requirement.TimeType == RequirementsData.TimeType.CombatTime)
                        {
                            requirement.BoolBalue1 = DrawHorizontalToggle("In Combat?", "",
                                RPGBuilderEditor.Instance.FieldHeight, requirement.BoolBalue1);
                            requirement.Amount1 = DrawHorizontalIntField(requirement.BoolBalue1 ? "Time spent in combat" : "Time spent out of combat", "", RPGBuilderEditor.Instance.FieldHeight, requirement.Amount1);
                        }
                        else
                        {
                            if (requirement.TimeValue == RequirementsData.TimeValue.Between)
                            {
                                DrawTitleLabelExpanded("Between:", "", true);
                                requirement.TimeRequirement1 = DrawTimeRequirement(requirement.TimeRequirement1);

                                DrawTitleLabelExpanded("And:", "", true);
                                requirement.TimeRequirement2 = DrawTimeRequirement(requirement.TimeRequirement2);
                            }
                            else
                            {
                                requirement.TimeRequirement1 = DrawTimeRequirement(requirement.TimeRequirement1);
                            }
                        }
                        break;
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(0, true);
            }
            
            GUILayout.Space(20);
        }

        if (margin) RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        return requirementGroups;
    }

    public static List<GameActionsData.GameAction> DrawGameActionsList(List<GameActionsData.GameAction> gameActions,
        bool margin)
    {
        if (margin) RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        int enumIndex = 0;

        GUILayout.Space(10);
        
        EditorGUI.BeginChangeCheck();
        GameActionsTemplate template = null;
        template = (GameActionsTemplate) DrawHorizontalObject<GameActionsTemplate>("Template Override", "", template);
        if (EditorGUI.EndChangeCheck())
        {
            if (template != null)
            {
                if (EditorUtility.DisplayDialog("OVERRIDE?",
                    "Are you sure you want to OVERRIDE these game actions with the template?", "Yes", "Cancel"))
                {
                    gameActions.Clear();
                    gameActions = template.OverrideRequirements(gameActions);
                }
            }
        }

        for (var actionIndex = 0; actionIndex < gameActions.Count; actionIndex++)
        {
            GameActionsData.GameAction action = gameActions[actionIndex];
            GUILayout.Space(5);

            switch (action.type)
            {
                case GameActionsData.GameActionType.Ability:
                    if (DrawHorizontalEntryRemoveButton(action.AbilityID, "Ability"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.AbilityAction = (GameActionsData.AbilityAction) DrawHorizontalEnum("Action", "",
                        (int) action.AbilityAction,
                        Enum.GetNames(typeof(GameActionsData.AbilityAction)));
                    action.AbilityID = DrawDatabaseEntryField(action.AbilityID, "Ability", "Ability", "");
                    break;
                case GameActionsData.GameActionType.Bonus:
                    if (DrawHorizontalEntryRemoveButton(action.BonusID, "Bonus"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.NodeAction = (GameActionsData.NodeAction) DrawHorizontalEnum("Action", "",
                        (int) action.NodeAction,
                        Enum.GetNames(typeof(GameActionsData.NodeAction)));
                    action.BonusID = DrawDatabaseEntryField(action.BonusID, "Bonus", "Bonus", "");
                    break;
                case GameActionsData.GameActionType.Recipe:
                    if (DrawHorizontalEntryRemoveButton(action.RecipeID, "Recipe"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.NodeAction = (GameActionsData.NodeAction) DrawHorizontalEnum("Action", "",
                        (int) action.NodeAction,
                        Enum.GetNames(typeof(GameActionsData.NodeAction)));
                    action.RecipeID = DrawDatabaseEntryField(action.RecipeID, "Recipe", "Recipe", "");
                    break;
                case GameActionsData.GameActionType.Resource:
                    if (DrawHorizontalEntryRemoveButton(action.ResourceID, "Resource"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.NodeAction = (GameActionsData.NodeAction) DrawHorizontalEnum("Action", "",
                        (int) action.NodeAction,
                        Enum.GetNames(typeof(GameActionsData.NodeAction)));
                    action.ResourceID = DrawDatabaseEntryField(action.ResourceID, "Resource", "Resource", "");
                    break;
                case GameActionsData.GameActionType.Effect:
                    if (DrawHorizontalEntryRemoveButton(action.EffectID, "Effect"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.EffectAction = (GameActionsData.EffectAction) DrawHorizontalEnum("Action", "",
                        (int) action.EffectAction,
                        Enum.GetNames(typeof(GameActionsData.EffectAction)));
                    action.EffectID = DrawDatabaseEntryField(action.EffectID, "Effect", "Effect", "");
                    action.Amount = DrawEffectRankIndexField(action.EffectID, action.Amount);
                    break;
                case GameActionsData.GameActionType.NPC:
                    if (DrawHorizontalEntryRemoveButton(action.NPCID, "NPC"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.NPCAction = (GameActionsData.NPCAction) DrawHorizontalEnum("Action", "",
                        (int) action.NPCAction,
                        Enum.GetNames(typeof(GameActionsData.NPCAction)));
                    action.NPCID = DrawDatabaseEntryField(action.NPCID, "NPC", "NPC", "");

                    if (action.NPCID != -1)
                    {
                        if (action.NPCAction == GameActionsData.NPCAction.TriggerPhase)
                        {
                            action.Amount = DrawNPCPhaseIndexField(action.NPCID, action.Amount);
                        }
                        else if (action.NPCAction == GameActionsData.NPCAction.Aggro ||
                                 action.NPCAction == GameActionsData.NPCAction.Kill || 
                                 action.NPCAction == GameActionsData.NPCAction.Spawn)
                        {
                            action.Amount = DrawHorizontalIntField("Amount", "",
                                RPGBuilderEditor.Instance.FieldHeight, action.Amount);
                        }
                    }

                    break;
                case GameActionsData.GameActionType.Faction:
                    if (DrawHorizontalEntryRemoveButton(action.FactionID, "Faction"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.FactionAction = (GameActionsData.FactionAction) DrawHorizontalEnum("Action", "",
                        (int) action.FactionAction,
                        Enum.GetNames(typeof(GameActionsData.FactionAction)));
                    action.FactionID = DrawDatabaseEntryField(action.FactionID, "Faction", "Faction", "");

                    if (action.FactionID != -1)
                    {
                        if (action.FactionAction == GameActionsData.FactionAction.ChangeStance)
                        {
                            DrawHorizontalLabel("Stance", "");
                            enumIndex = EditorGUILayout.Popup(
                                RPGBuilderEditorUtility.GetTypeEntryIndex(
                                    RPGBuilderEditorUtility.GetModuleByName("Faction Stances").databaseEntries,
                                    action.FactionStance),
                                RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility
                                    .GetModuleByName("Faction Stances").databaseEntries.ToArray()));
                            action.FactionStance = (RPGBFactionStance) RPGBuilderEditorUtility
                                .GetModuleByName("Faction Stances").databaseEntries[enumIndex];
                            EditorGUILayout.EndHorizontal();
                        }
                        else if (action.FactionAction == GameActionsData.FactionAction.GainPoints)
                        {
                            action.Amount = DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                                action.Amount);
                        }
                    }

                    break;
                case GameActionsData.GameActionType.Item:
                    if (DrawHorizontalEntryRemoveButton(action.ItemID, "Item"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.AlterAction = (GameActionsData.AlterAction) DrawHorizontalEnum("Action", "",
                        (int) action.AlterAction,
                        Enum.GetNames(typeof(GameActionsData.AlterAction)));
                    action.ItemID = DrawDatabaseEntryField(action.ItemID, "Item", "Item", "");
                    action.Amount = DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                        action.Amount);
                    break;
                case GameActionsData.GameActionType.Currency:
                    if (DrawHorizontalEntryRemoveButton(action.CurrencyID, "Currency"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.AlterAction = (GameActionsData.AlterAction) DrawHorizontalEnum("Action", "",
                        (int) action.AlterAction,
                        Enum.GetNames(typeof(GameActionsData.AlterAction)));
                    action.CurrencyID = DrawDatabaseEntryField(action.CurrencyID, "Currency", "Currency", "");
                    action.Amount = DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                        action.Amount);
                    break;
                case GameActionsData.GameActionType.Point:
                    if (DrawHorizontalEntryRemoveButton(action.PointID, "Point"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.AlterAction = (GameActionsData.AlterAction) DrawHorizontalEnum("Action", "",
                        (int) action.AlterAction,
                        Enum.GetNames(typeof(GameActionsData.AlterAction)));
                    action.PointID = DrawDatabaseEntryField(action.PointID, "Point", "Point", "");
                    action.Amount = DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                        action.Amount);
                    break;
                case GameActionsData.GameActionType.Skill:
                    if (DrawHorizontalEntryRemoveButton(action.SkillID, "Skill"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.ProgressionType = (GameActionsData.ProgressionType) DrawHorizontalEnum("Action", "",
                        (int) action.ProgressionType,
                        Enum.GetNames(typeof(GameActionsData.ProgressionType)));
                    action.SkillID = DrawDatabaseEntryField(action.SkillID, "Skill", "Skill", "");

                    if (action.SkillID != -1)
                    {
                        if (action.ProgressionType == GameActionsData.ProgressionType.GainLevel ||
                            action.ProgressionType == GameActionsData.ProgressionType.LoseLevel ||
                            action.ProgressionType == GameActionsData.ProgressionType.GainExperience)
                        {
                            action.Amount = DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                                action.Amount);
                        }
                    }

                    break;
                case GameActionsData.GameActionType.TalentTree:
                    if (DrawHorizontalEntryRemoveButton(action.TalentTreeID, "TalentTree"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.TreeAction = (GameActionsData.TreeAction) DrawHorizontalEnum("Action", "",
                        (int) action.TreeAction,
                        Enum.GetNames(typeof(GameActionsData.TreeAction)));
                    action.TalentTreeID = DrawDatabaseEntryField(action.TalentTreeID, "TalentTree", "TalentTree", "");
                    break;
                case GameActionsData.GameActionType.WeaponTemplate:
                    if (DrawHorizontalEntryRemoveButton(action.WeaponTemplateID, "WeaponTemplate"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.ProgressionType = (GameActionsData.ProgressionType) DrawHorizontalEnum("Action", "",
                        (int) action.ProgressionType,
                        Enum.GetNames(typeof(GameActionsData.ProgressionType)));
                    action.WeaponTemplateID =
                        DrawDatabaseEntryField(action.WeaponTemplateID, "WeaponTemplate", "WeaponTemplate", "");

                    if (action.WeaponTemplateID != -1)
                    {
                        if (action.ProgressionType == GameActionsData.ProgressionType.GainLevel ||
                            action.ProgressionType == GameActionsData.ProgressionType.LoseLevel ||
                            action.ProgressionType == GameActionsData.ProgressionType.GainExperience)
                        {
                            action.Amount = DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                                action.Amount);
                        }
                    }

                    break;
                case GameActionsData.GameActionType.Quest:
                    if (DrawHorizontalEntryRemoveButton(action.QuestID, "Quest"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.QuestAction = (GameActionsData.QuestAction) DrawHorizontalEnum("Action", "",
                        (int) action.QuestAction,
                        Enum.GetNames(typeof(GameActionsData.QuestAction)));
                    action.QuestID = DrawDatabaseEntryField(action.QuestID, "Quest", "Quest", "");
                    break;
                case GameActionsData.GameActionType.Dialogue:
                    if (DrawHorizontalEntryRemoveButton(action.DialogueID, "Dialogue"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.DialogueAction = (GameActionsData.DialogueAction) DrawHorizontalEnum("Action", "",
                        (int) action.DialogueAction,
                        Enum.GetNames(typeof(GameActionsData.DialogueAction)));
                    action.DialogueID = DrawDatabaseEntryField(action.DialogueID, "Dialogue", "Dialogue", "");
                    break;
                case GameActionsData.GameActionType.DialogueNode:
                    if (DrawHorizontalEntryRemoveButton(action.DialogueID, "Dialogue"))
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    action.CompletionAction = (GameActionsData.CompletionAction) DrawHorizontalEnum("Action", "",
                        (int) action.CompletionAction,
                        Enum.GetNames(typeof(GameActionsData.CompletionAction)));
                    action.DialogueID = DrawDatabaseEntryField(action.DialogueID, "Dialogue", "Dialogue", "");
                    if (action.DialogueID != -1)
                    {
                        RPGDialogue dialogue =
                            (RPGDialogue) RPGBuilderEditorUtility.GetEntryByID(action.DialogueID, "Dialogue");

                        if (dialogue != null)
                        {
                            DrawHorizontalLabel("Node", "");
                            List<RPGDialogueTextNode> textNodes =
                                RPGBuilderEditorUtility.GetDialogueTextNodes(dialogue.dialogueGraph.nodes);
                            enumIndex = EditorGUILayout.Popup(
                                RPGBuilderEditorUtility.GetDialogueNodeIndex(textNodes, action.DialogueNode),
                                RPGBuilderEditorUtility.GetDialogueTextNodesAsStringArray(textNodes.ToArray()));

                            action.DialogueNode = textNodes[enumIndex];
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    break;
                case GameActionsData.GameActionType.CombatState:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();
                    action.CombatStateAction = (GameActionsData.CombatStateAction) DrawHorizontalEnum("Action", "",
                        (int) action.CombatStateAction,
                        Enum.GetNames(typeof(GameActionsData.CombatStateAction)));
                    if (action.CombatStateAction == GameActionsData.CombatStateAction.Set)
                    {
                        action.BoolBalue1 = DrawHorizontalToggle("Set to", "", RPGBuilderEditor.Instance.FieldHeight,
                            action.BoolBalue1);
                    }

                    break;
                case GameActionsData.GameActionType.Dismount:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();
                    break;
                case GameActionsData.GameActionType.GameObject:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();
                    action.GameObjectAction = (GameActionsData.GameObjectAction) DrawHorizontalEnum("Action", "",
                        (int) action.GameObjectAction,
                        Enum.GetNames(typeof(GameActionsData.GameObjectAction)));

                    if (action.GameObjectAction == GameActionsData.GameObjectAction.Spawn)
                    {
                        action.SpawnTypes = (GameActionsData.SpawnTypes) DrawHorizontalEnum("Spawn at", "",
                            (int) action.SpawnTypes,
                            Enum.GetNames(typeof(GameActionsData.SpawnTypes)));
                        action.GameObject =
                            (GameObject) DrawHorizontalObject<GameObject>("Prefab", "", action.GameObject);

                        if (action.SpawnTypes == GameActionsData.SpawnTypes.Position)
                        {
                            action.Position = DrawHorizontalVector3("Position", "",
                                RPGBuilderEditor.Instance.FieldHeight, action.Position);
                            action.Rotation = DrawHorizontalVector3("Rotation", "",
                                RPGBuilderEditor.Instance.FieldHeight, action.Rotation);
                        }
                    }
                    else if (action.GameObjectAction == GameActionsData.GameObjectAction.Deactivate ||
                             action.GameObjectAction == GameActionsData.GameObjectAction.Destroy)
                    {
                        action.stringValue1 = DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                            action.stringValue1);
                    }

                    break;
                case GameActionsData.GameActionType.TriggerVisualEffect:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();
                    
                    action.VisualEffectEntry = DrawVisualEffectEntry(action.VisualEffectEntry, RPGBuilderEditorUtility.GetModuleByName("Node Sockets").databaseEntries);
                    break;
                case GameActionsData.GameActionType.TriggerAnimation:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();

                    action.AnimationEntry = DrawAnimationEntry(action.AnimationEntry);
                    break;
                case GameActionsData.GameActionType.TriggerSound:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();

                    action.SoundEntry = DrawSoundEntry(action.SoundEntry);
                    break;
                case GameActionsData.GameActionType.Teleport:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();
                    action.TeleportType = (GameActionsData.TeleportType) DrawHorizontalEnum("Teleport to", "",
                        (int) action.TeleportType,
                        Enum.GetNames(typeof(GameActionsData.TeleportType)));

                    if (action.TeleportType == GameActionsData.TeleportType.Position)
                    {
                        action.Position = DrawHorizontalVector3("Position", "",
                            RPGBuilderEditor.Instance.FieldHeight, action.Position);
                        action.Rotation = DrawHorizontalVector3("Rotation", "",
                            RPGBuilderEditor.Instance.FieldHeight, action.Rotation);
                    }
                    else if (action.TeleportType == GameActionsData.TeleportType.GameScene)
                    {
                        action.GameSceneID = DrawDatabaseEntryField(action.GameSceneID, "GameScene", "Game Scene", "");
                        action.Position = DrawHorizontalVector3("Position", "",
                            RPGBuilderEditor.Instance.FieldHeight, action.Position);
                        action.Rotation = DrawHorizontalVector3("Rotation", "",
                            RPGBuilderEditor.Instance.FieldHeight, action.Rotation);
                    }

                    break;
                case GameActionsData.GameActionType.SaveCharacter:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();
                    break;
                case GameActionsData.GameActionType.Death:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();
                    break;
                case GameActionsData.GameActionType.ResetSprint:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();
                    break;
                case GameActionsData.GameActionType.ResetBlocking:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }

                    EditorGUILayout.EndHorizontal();
                    break;
                case GameActionsData.GameActionType.Time:
                    EditorGUILayout.BeginHorizontal();
                    action.type = (GameActionsData.GameActionType) DrawHorizontalEnum("Type",
                        "What type of action is it?", (int) action.type,
                        Enum.GetNames(typeof(GameActionsData.GameActionType)));
                    if (DrawSmallRemoveButton())
                    {
                        gameActions.Remove(action);
                        return gameActions;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    action.TimeAction = (GameActionsData.TimeAction) DrawHorizontalEnum("Action",
                        "What type of action is it?", (int) action.TimeAction,
                        Enum.GetNames(typeof(GameActionsData.TimeAction)));
                    if (action.TimeAction == GameActionsData.TimeAction.SetGlobalSpeed || action.TimeAction == GameActionsData.TimeAction.SetTimeScale)
                    {
                        action.FloatValue1 = DrawHorizontalFloatField("Value", "", RPGBuilderEditor.Instance.FieldHeight,
                            action.FloatValue1);
                    }
                    else
                    {
                        action.Amount = DrawHorizontalIntField("Value", "", RPGBuilderEditor.Instance.FieldHeight,
                            action.Amount);
                    }
                    break;
            }

            action.chance = DrawHorizontalFloatFillBar("Chance", "", action.chance);

            GUILayout.Space(20);
        }

        if (margin) RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        return gameActions;
    }

    public static List<CombatData.CustomStatValues> DrawCustomStatValuesList(List<CombatData.CustomStatValues> statList, bool margin)
    {
        if (margin) RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        
        EditorGUI.BeginChangeCheck();
        StatListTemplate template = null;
        template = (StatListTemplate) DrawHorizontalObject<StatListTemplate>("Template Override", "", template);
        if (EditorGUI.EndChangeCheck())
        {
            if (template != null)
            {
                if (EditorUtility.DisplayDialog("OVERRIDE?",
                    "Are you sure you want to OVERRIDE these stats with the template?", "Yes", "Cancel"))
                {
                    statList.Clear();
                    statList = template.OverrideRequirements(statList);
                }
            }
        }
        
        for (var a = 0; a < statList.Count; a++)
        {
            GUILayout.Space(10);
            if (DrawHorizontalEntryRemoveButton(statList[a].statID, "Stat"))
            {
                statList.RemoveAt(a);
                return statList;
            }

            statList[a].statID = DrawDatabaseEntryField(statList[a].statID, "Stat", "Stat", "");

            if (statList[a].statID != -1)
            {
                RPGStat statEntry = (RPGStat) RPGBuilderEditorUtility.GetEntryByID(statList[a].statID, "Stat");
                
                statList[a].addedValue = DrawHorizontalFloatField("Added Value", "",
                    RPGBuilderEditor.Instance.FieldHeight, statList[a].addedValue);
                statList[a].valuePerLevel = DrawHorizontalFloatField("Bonus per level", "",
                    RPGBuilderEditor.Instance.FieldHeight, statList[a].valuePerLevel);

                if (!statEntry.isPercentStat)
                {
                    statList[a].Percent = DrawHorizontalToggle("Percent?", "", RPGBuilderEditor.Instance.FieldHeight,
                        statList[a].Percent);
                }
                
                statList[a].overrideMinValue = DrawHorizontalToggle("Override minimum value?", "",
                    RPGBuilderEditor.Instance.FieldHeight, statList[a].overrideMinValue);
                if (statList[a].overrideMinValue)
                {
                    statList[a].minValue = DrawHorizontalFloatField("Min. Value", "",
                        RPGBuilderEditor.Instance.FieldHeight, statList[a].minValue);
                }
                
                statList[a].overrideMaxValue = DrawHorizontalToggle("Override maximum value?", "",
                    RPGBuilderEditor.Instance.FieldHeight, statList[a].overrideMaxValue);
                if (statList[a].overrideMaxValue)
                {
                    statList[a].maxValue = DrawHorizontalFloatField("Max. Value", "",
                        RPGBuilderEditor.Instance.FieldHeight, statList[a].maxValue);
                }
                
                statList[a].overrideStartPercentage = DrawHorizontalToggle("Override starting percentage?", "",
                    RPGBuilderEditor.Instance.FieldHeight, statList[a].overrideStartPercentage);
                if (statList[a].overrideStartPercentage)
                {
                    statList[a].startPercentage = DrawHorizontalFloatFillBar("Starting percentage", "",
                        statList[a].startPercentage);
                }
                
                statList[a].chance = DrawHorizontalFloatFillBar("Chance", "", statList[a].chance);

                if (statEntry != null && statEntry.isVitalityStat)
                {
                    statList[a].vitalityActions = DrawVitalityActions(statList[a].vitalityActions);
                }
            }
        }
        
        if (margin) RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        return statList;
    }

    public static List<WeaponTransform> DrawWeaponTransformList(List<WeaponTransform> transforms, string WeaponSlot, bool margin)
    {
        if (margin) RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        GUILayout.Space(10);
        
        EditorGUI.BeginChangeCheck();
        WeaponTransformTemplate template = null;
        template = (WeaponTransformTemplate) DrawHorizontalObject<WeaponTransformTemplate>("Template Override", "", template);
        if (EditorGUI.EndChangeCheck())
        {
            if (template != null)
            {
                if (EditorUtility.DisplayDialog("OVERRIDE?",
                    "Are you sure you want to OVERRIDE these weapon transforms with the template?", "Yes", "Cancel"))
                {
                    transforms.Clear();
                    transforms = template.OverrideRequirements(transforms);
                }
            }
        }
        
        for (var a = 0; a < transforms.Count; a++)
        {
            GUILayout.Space(10);
            if (DrawHorizontalEntryRemoveButton(
                transforms[a].raceID, "Race"))
            {
                transforms.RemoveAt(a);
                return transforms;
            }

            transforms[a].raceID = DrawDatabaseEntryField(transforms[a].raceID, "Race", "Race", "");

            if (transforms[a].raceID != -1)
            {
                
                GUILayout.Space(10);
                if (DrawHorizontalAddButton("Add Gender", false))
                {
                    transforms[a].transformValues.Add(new WeaponTransform.TransformValues());
                }
                
                for (var u = 0; u < transforms[a].transformValues.Count; u++)
                {
                    GUILayout.Space(10);
                    
                    DrawHorizontalLabel("Gender", "");
                    int enumIndex = EditorGUILayout.Popup(
                        RPGBuilderEditorUtility.GetTypeEntryIndex(
                            RPGBuilderEditorUtility.GetModuleByName("Genders").databaseEntries,
                            transforms[a].transformValues[u].gender),
                        RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility
                            .GetModuleByName("Genders").databaseEntries.ToArray()));
                    transforms[a].transformValues[u].gender = (RPGBGender) RPGBuilderEditorUtility
                        .GetModuleByName("Genders").databaseEntries[enumIndex];
                    if (DrawSmallRemoveButton())
                    {
                        transforms[a].transformValues.RemoveAt(u);
                        return transforms;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    if(transforms[a].transformValues[u].gender == null) continue;
                    
                    if (WeaponSlot == "TWO HAND" ||
                        WeaponSlot == "MAIN HAND" ||
                        WeaponSlot == "ANY HAND")
                    {
                        DrawTitleLabel((transforms[a].transformValues[u].gender != null ? 
                            transforms[a].transformValues[u].gender.entryName.ToUpper() :
                            "- Select Gender -") + " | IN COMBAT | RIGHT HAND:","", 300);
                        GUILayout.Space(5);
                        GameObject sceneREF = null;
                        sceneREF = (GameObject) DrawHorizontalSceneObject<GameObject>(
                                "Scene Reference 1", "", sceneREF);
                        if (sceneREF != null)
                        {
                            transforms[a].transformValues[u].CombatPosition = sceneREF.transform.localPosition;
                            transforms[a].transformValues[u]
                                .CombatRotation = sceneREF.transform.localEulerAngles;
                            transforms[a].transformValues[u]
                                .CombatScale = sceneREF.transform.localScale;
                        }

                        EditorGUILayout.BeginHorizontal();
                        DrawLabel("P", "", 20);
                        transforms[a].transformValues[u].CombatPosition =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].CombatPosition,
                                GUILayout.MaxWidth(165));
                        GUILayout.Space(30);
                        DrawLabel("R", "", 20);
                        transforms[a].transformValues[u].CombatRotation =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].CombatRotation,
                                GUILayout.MaxWidth(165));
                        GUILayout.Space(30);
                        DrawLabel("S", "", 20);
                        transforms[a].transformValues[u].CombatScale =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].CombatScale,
                                GUILayout.MaxWidth(165));
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);
                        DrawTitleLabel((
                            transforms[a].transformValues[u].gender != null ? 
                                transforms[a].transformValues[u].gender.entryName.ToUpper() :
                                "- Select Gender -") + " | OUT OF COMBAT | RIGHT HAND:", "", 300);
                        GUILayout.Space(5);
                        GameObject sceneREF2 = null;
                        sceneREF2 = (GameObject) DrawHorizontalSceneObject<GameObject>(
                            "Scene Reference 2", "", sceneREF2);
                        if (sceneREF2 != null)
                        {
                            transforms[a].transformValues[u]
                                .RestPosition = sceneREF2.transform.localPosition;
                            transforms[a].transformValues[u]
                                .RestRotation = sceneREF2.transform.localEulerAngles;
                            transforms[a].transformValues[u]
                                .RestScale = sceneREF2.transform.localScale;
                        }

                        EditorGUILayout.BeginHorizontal();
                        DrawLabel("P", "", 20);
                        transforms[a].transformValues[u].RestPosition =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].RestPosition,
                                GUILayout.MaxWidth(165));
                        GUILayout.Space(30);
                        DrawLabel("R", "", 20);
                        transforms[a].transformValues[u].RestRotation =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].RestRotation,
                                GUILayout.MaxWidth(165));
                        GUILayout.Space(30);
                        DrawLabel("S", "", 20);
                        transforms[a].transformValues[u].RestScale =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].RestScale,
                                GUILayout.MaxWidth(165));
                        EditorGUILayout.EndHorizontal();
                    }

                    if (WeaponSlot == "OFF HAND" ||
                        WeaponSlot == "ANY HAND")
                    {
                        GUILayout.Space(15);
                        DrawTitleLabel((
                            transforms[a].transformValues[u].gender != null ? 
                                transforms[a].transformValues[u].gender.entryName.ToUpper() :
                                "- Select Gender -") + " | IN COMBAT | LEFT HAND:", "", 300);
                        GUILayout.Space(5);
                        GameObject sceneREF = null;
                        sceneREF = (GameObject) RPGBuilderEditorFields
                            .DrawHorizontalSceneObject<GameObject>(
                                "Scene Reference 1", "", sceneREF);
                        if (sceneREF != null)
                        {
                            transforms[a].transformValues[u]
                                .CombatPosition2 = sceneREF.transform.localPosition;
                            transforms[a].transformValues[u]
                                .CombatRotation2 = sceneREF.transform.localEulerAngles;
                            transforms[a].transformValues[u]
                                .CombatScale2 = sceneREF.transform.localScale;
                        }

                        EditorGUILayout.BeginHorizontal();
                        DrawLabel("P", "", 20);
                        transforms[a].transformValues[u].CombatPosition2 =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].CombatPosition2,
                                GUILayout.MaxWidth(165));
                        GUILayout.Space(30);
                        DrawLabel("R", "", 20);
                        transforms[a].transformValues[u].CombatRotation2 =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].CombatRotation2,
                                GUILayout.MaxWidth(165));
                        GUILayout.Space(30);
                        DrawLabel("S", "", 20);
                        transforms[a].transformValues[u].CombatScale2 =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].CombatScale2,
                                GUILayout.MaxWidth(165));
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(10);
                        DrawTitleLabel((
                            transforms[a].transformValues[u].gender != null ? 
                                transforms[a].transformValues[u].gender.entryName.ToUpper() :
                                "- Select Gender -") + " | OUT OF COMBAT | LEFT HAND:", "", 300);
                        GUILayout.Space(5);
                        GameObject sceneREF2 = null;
                        sceneREF2 = (GameObject) DrawHorizontalSceneObject<GameObject>(
                            "Scene Reference 2", "", sceneREF2);
                        if (sceneREF2 != null)
                        {
                            transforms[a].transformValues[u]
                                .RestPosition2 = sceneREF2.transform.localPosition;
                            transforms[a].transformValues[u]
                                .RestRotation2 = sceneREF2.transform.localEulerAngles;
                            transforms[a].transformValues[u]
                                .RestScale2 = sceneREF2.transform.localScale;
                        }

                        EditorGUILayout.BeginHorizontal();
                        DrawLabel("P", "", 20);
                        transforms[a].transformValues[u].RestPosition2 =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].RestPosition2,
                                GUILayout.MaxWidth(165));
                        GUILayout.Space(30);
                        DrawLabel("R", "", 20);
                        transforms[a].transformValues[u].RestRotation2 =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].RestRotation2,
                                GUILayout.MaxWidth(165));
                        GUILayout.Space(30);
                        DrawLabel("S", "", 20);
                        transforms[a].transformValues[u].RestScale2 =
                            EditorGUILayout.Vector3Field("", transforms[a].transformValues[u].RestScale2,
                                GUILayout.MaxWidth(165));
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.Space(20);
                }
                GUILayout.Space(50);
            }
        }
        
        if (margin) RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        return transforms;
    }

    public static void DrawID(int id)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.LabelField("ID:", GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        EditorGUILayout.IntField(id,GetTextFieldStyle(),GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }
    
    public static Sprite DrawIcon(Sprite icon, float width, float height)
    {
        var iconWidth = width * RPGBuilderEditorUtility.GetWidthModifier();
        var iconHeight = height * RPGBuilderEditorUtility.GetHeightModifier();
        var iconSize = iconWidth > iconHeight ? iconHeight : iconWidth;
        return (Sprite) EditorGUILayout.ObjectField(icon, typeof(Sprite), false, GUILayout.Width(iconSize), GUILayout.Height(iconSize));
    }
    
    public static int DrawHorizontalLayerField(string labelName, string tooltip, int content)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip),
            GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.LayerField(content, GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static string DrawHorizontalTagField(string labelName, string tooltip, string content)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip),
            GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.TagField(content, GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static string DrawHorizontalTextField(string labelName, string tooltip, float smallFieldHeight, string content)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip),
            GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.TextField(content, GetTextFieldStyle(), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static string DrawHorizontalDescriptionField(string labelName, string tooltip, float smallFieldHeight, string content)
    {
        GUILayout.BeginHorizontal();
        EditorStyles.textArea.wordWrap = true;
        EditorStyles.textArea.clipping = TextClipping.Clip;
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip),  GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.TextField(content, RPGBuilderEditor.Instance.EditorSkin.GetStyle("DescriptionField"), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight*5));
        EditorStyles.textArea.wordWrap = false;
        GUILayout.EndHorizontal();
        return content;
    }
    public static string DrawHorizontalTooltipField(string content, GUIStyle style)
    {
        GUILayout.BeginHorizontal();
        content = EditorGUILayout.TextField(content, style, GUILayout.Height(75));
        GUILayout.EndHorizontal();
        return content;
    }

    public static string DrawFileNameField(string labelName, string tooltip, float smallFieldHeight, string content)
    {
        if (!RPGBuilderEditor.Instance.EditorSettings.ShowFileNames) return content;
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        if (!string.IsNullOrEmpty(labelName))
            EditorGUILayout.LabelField(new GUIContent(labelName, tooltip),
                GetLabelStyle(),
                GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth),
                GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.TextField(content,
            GetTextFieldStyle(),
            GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        return content;
    }

    public static int DrawHorizontalIntField(string labelName, string tooltip, float smallFieldHeight, int content)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.IntField(content, GetTextFieldStyle(), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static void DrawLabel(string labelName, string tooltip)
    {
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
    }
    public static void DrawLabel(string labelName, string tooltip, float width)
    {
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(),
            GUILayout.Width(width), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
    }
    public static void DrawTitleLabel(string labelName, string tooltip)
    {
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetTitleLabelStyle(),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
    }
    public static void DrawTitleLabel(string labelName, string tooltip, bool topMargin)
    {
        if(topMargin) GUILayout.Space(8);
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetTitleLabelStyle(),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
    }
    public static void DrawTitleLabel(string labelName, string tooltip, float width)
    {
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetTitleLabelStyle(),
            GUILayout.Width(width), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
    }
    public static void DrawTitleLabelExpanded(string labelName, string tooltip, bool topMargin)
    {
        if(topMargin) GUILayout.Space(8);
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetTitleLabelStyle(),
            GUILayout.ExpandWidth(true), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
    }
    public static void DrawTitleLabelExpanded(string labelName, string tooltip)
    {
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetTitleLabelStyle(),
            GUILayout.ExpandWidth(true), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
    }
    public static void DrawLabelExpanded(string labelName, string tooltip)
    {
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(),
            GUILayout.ExpandWidth(true), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
    }
    
    public static void DrawPartnerLabel(string labelName, string tooltip, float height)
    {
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), RPGBuilderEditor.Instance.EditorSkin.GetStyle("PartnerLabel"),
             GUILayout.Height(height));
    }
    
    public static void DrawHorizontalLabel(string labelName, string tooltip)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
    }
    
    public static Color DrawHorizontalColorField(string labelName, string tooltip, float smallFieldHeight, Color content)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.ColorField(content, GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static AudioClip DrawHorizontalAudioClipField(string labelName, string tooltip, float smallFieldHeight, AudioClip content)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = (AudioClip) EditorGUILayout.ObjectField(content, typeof(AudioClip),false, GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    public static AudioMixerGroup DrawHorizontalAudioMixerField(string labelName, string tooltip, float smallFieldHeight, AudioMixerGroup content)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = (AudioMixerGroup) EditorGUILayout.ObjectField(content, typeof(AudioMixerGroup),false, GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static Sprite DrawHorizontalSpriteField(string labelName, string tooltip, float smallFieldHeight, Sprite content, float spriteSize)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = (Sprite) EditorGUILayout.ObjectField(
            content, typeof(Sprite), false, GUILayout.Width(spriteSize), GUILayout.Height(spriteSize));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static float DrawHorizontalFloatField(string labelName, string tooltip, float smallFieldHeight, float content)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.FloatField(content, GetTextFieldStyle(),
            GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static Vector2 DrawHorizontalRangeSliderField(string labelName, string tooltip, Vector2 content, float min, float max, float maxWidth)
    {
        EditorGUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth));
        content.x = EditorGUILayout.FloatField(content.x, GUILayout.MaxWidth(maxWidth));
        content.y = EditorGUILayout.FloatField(content.y, GUILayout.MaxWidth(maxWidth));
        if (content.x > content.y) content.x = content.y;
        if (content.y < content.x) content.y = content.x;
        GUILayout.Space(6);
        EditorGUILayout.MinMaxSlider(ref content.x, ref content.y, min, max);
        content.x = Mathf.Round(content.x * 100.0f) * 0.01f;
        content.y = Mathf.Round(content.y * 100.0f) * 0.01f;
        EditorGUILayout.EndHorizontal();
        return content;
    }
    
    public static Vector2 DrawHorizontalRangeSliderIntField(string labelName, string tooltip, Vector2 content, int min, int max, float maxWidth)
    {
        EditorGUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth));
        content.x = EditorGUILayout.IntField((int)content.x, GUILayout.MaxWidth(maxWidth));
        content.y = EditorGUILayout.IntField((int)content.y, GUILayout.MaxWidth(maxWidth));
        if (content.x > content.y) content.x = content.y;
        if (content.y < content.x) content.y = content.x;
        GUILayout.Space(6);
        EditorGUILayout.MinMaxSlider(ref content.x, ref content.y, min, max);
        content.x = Mathf.Round(content.x * 100.0f) * 0.01f;
        content.y = Mathf.Round(content.y * 100.0f) * 0.01f;
        EditorGUILayout.EndHorizontal();
        return content;
    }
    
    public static bool DrawHorizontalToggle(string labelName, string tooltip, float smallFieldHeight, float width, bool toggle)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(), GUILayout.MaxWidth(width), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        toggle = EditorGUILayout.Toggle(toggle, RPGBuilderEditor.Instance.EditorSkin.GetStyle("toggle"), GUILayout.Width(15), GUILayout.Height(15));
        GUILayout.EndHorizontal();
        return toggle;
    }
    public static bool DrawHorizontalToggle(string labelName, string tooltip, float smallFieldHeight, bool toggle)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(smallFieldHeight));
        toggle = EditorGUILayout.Toggle(toggle, RPGBuilderEditor.Instance.EditorSkin.GetStyle("toggle"), GUILayout.Width(15), GUILayout.Height(15));
        GUILayout.EndHorizontal();
        return toggle;
    }
    
    public static float DrawHorizontalFloatSlider(string labelName, string tooltip, float smallFieldHeight, float content, float min, float max)
    {
        GUISkin cachedStyle = GUI.skin;
        GUI.skin = RPGBuilderEditor.Instance.EditorSkin;
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(),
            GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(18));
        content = EditorGUILayout.Slider(content, min, max, GUILayout.Height(18));
        GUILayout.EndHorizontal();
        GUI.skin = cachedStyle;
        return content;
    }
    
    
    
    public static int DrawHorizontalIntSlider(string labelName, string tooltip, float smallFieldHeight, int content, int min, int max)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.IntSlider(content, min, max, GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static Vector3 DrawHorizontalVector3(string labelName, string tooltip, float smallFieldHeight, Vector3 content)
    {
        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(labelName)) EditorGUILayout.LabelField(new GUIContent(labelName, tooltip), GetLabelStyle(), GUILayout.Width(RPGBuilderEditor.Instance.EditorData.labelFieldWidth), GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        content = EditorGUILayout.Vector3Field("", content, GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
        GUILayout.EndHorizontal();
        return content;
    }
    
    public static bool DrawButtonWithPopup(string buttonTitle, GUIStyle style, float maxWidth, float maxHeight, string dialogTitle, string dialogueDescription, string option1, string option2)
    {
        return GUILayout.Button(buttonTitle, style, 
            GUILayout.ExpandWidth(true), GUILayout.MaxWidth(maxWidth), GUILayout.MaxHeight(maxHeight), GUILayout.Height(25)) && EditorUtility.DisplayDialog(dialogTitle, dialogueDescription, option1, option2);
    }
    
    public static List<VisualEffectEntry> DrawVisualEffectsList(List<VisualEffectEntry> visualEffects, List<RPGBuilderDatabaseEntry> allNodeSockets)
    {
        if (DrawHorizontalAddButton("Add Visual Effect", true))
        {
            visualEffects.Add(new VisualEffectEntry());
        }

        for (var a = 0; a < visualEffects.Count; a++)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            EditorGUILayout.BeginHorizontal();
            visualEffects[a].ActivationType =
                (ActivationType) DrawHorizontalEnum("Activate On", "",
                    (int)visualEffects[a].ActivationType,
                    Enum.GetNames(typeof(ActivationType)));
            if (DrawSmallRemoveButton())
            {
                visualEffects.RemoveAt(a);
                return visualEffects;
            }
            EditorGUILayout.EndHorizontal();

            visualEffects[a].Template = (VisualEffectTemplate)
                DrawHorizontalObject<VisualEffectTemplate>("Template", "",
                    visualEffects[a].Template);
            
            DrawTitleLabelExpanded("Transform:", "", true);
            visualEffects[a].UseNodeSocket = DrawHorizontalToggle("Use Node Socket?", "", RPGBuilderEditor.Instance.FieldHeight,visualEffects[a].UseNodeSocket);
            if (visualEffects[a].UseNodeSocket)
            {
                DrawHorizontalLabel("Socket", "");
                int projSocketIndex = EditorGUILayout.Popup(RPGBuilderEditorUtility.GetTypeEntryIndex(allNodeSockets, visualEffects[a].NodeSocket),
                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allNodeSockets.ToArray()));
                visualEffects[a].NodeSocket = (RPGBNodeSocket) allNodeSockets[projSocketIndex];
                EditorGUILayout.EndHorizontal();
                
                visualEffects[a].PositionOffset = DrawHorizontalVector3("Position Offset", "", RPGBuilderEditor.Instance.FieldHeight,
                    visualEffects[a].PositionOffset);
                
                visualEffects[a].ParentedToCaster = DrawHorizontalToggle("Attach to socket?", "", RPGBuilderEditor.Instance.FieldHeight,
                    visualEffects[a].ParentedToCaster);
            }
            else
            {
                visualEffects[a].PositionOffset = DrawHorizontalVector3("Position Offset", "", RPGBuilderEditor.Instance.FieldHeight,
                    visualEffects[a].PositionOffset);

                visualEffects[a].ParentedToCaster = DrawHorizontalToggle("Attach to caster?", "", RPGBuilderEditor.Instance.FieldHeight,
                    visualEffects[a].ParentedToCaster);
            }

            visualEffects[a].Scale = DrawHorizontalVector3("Scale", "", RPGBuilderEditor.Instance.FieldHeight, visualEffects[a].Scale);

            DrawTitleLabelExpanded("Time:", "", true);
            visualEffects[a].Delay = DrawHorizontalFloatField("Delay", "", RPGBuilderEditor.Instance.FieldHeight, visualEffects[a].Delay);
            visualEffects[a].Endless = DrawHorizontalToggle("Endless", "", RPGBuilderEditor.Instance.FieldHeight,
                visualEffects[a].Endless);
            if (!visualEffects[a].Endless)
            {
                visualEffects[a].Duration = DrawHorizontalFloatField("Duration", "", RPGBuilderEditor.Instance.FieldHeight, visualEffects[a].Duration);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        return visualEffects; 
    }

    public static VisualEffectEntry DrawVisualEffectEntry(VisualEffectEntry visualEffects, List<RPGBuilderDatabaseEntry> allNodeSockets)
    {

        GUILayout.Space(10);
        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        visualEffects.Template = (VisualEffectTemplate)
            DrawHorizontalObject<VisualEffectTemplate>("Template", "",
                visualEffects.Template);

        DrawTitleLabelExpanded("Transform:", "", true);
        visualEffects.UseNodeSocket = DrawHorizontalToggle("Use Node Socket?", "",
            RPGBuilderEditor.Instance.FieldHeight, visualEffects.UseNodeSocket);
        if (visualEffects.UseNodeSocket)
        {
            DrawHorizontalLabel("Socket", "");
            int projSocketIndex = EditorGUILayout.Popup(
                RPGBuilderEditorUtility.GetTypeEntryIndex(allNodeSockets, visualEffects.NodeSocket),
                RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allNodeSockets.ToArray()));
            visualEffects.NodeSocket = (RPGBNodeSocket) allNodeSockets[projSocketIndex];
            EditorGUILayout.EndHorizontal();

            visualEffects.PositionOffset = DrawHorizontalVector3("Position Offset", "",
                RPGBuilderEditor.Instance.FieldHeight,
                visualEffects.PositionOffset);

            visualEffects.ParentedToCaster = DrawHorizontalToggle("Attach to socket?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                visualEffects.ParentedToCaster);
        }
        else
        {
            visualEffects.PositionOffset = DrawHorizontalVector3("Position Offset", "",
                RPGBuilderEditor.Instance.FieldHeight,
                visualEffects.PositionOffset);

            visualEffects.ParentedToCaster = DrawHorizontalToggle("Attach to caster?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                visualEffects.ParentedToCaster);
        }

        visualEffects.Scale =
            DrawHorizontalVector3("Scale", "", RPGBuilderEditor.Instance.FieldHeight, visualEffects.Scale);

        DrawTitleLabelExpanded("Time:", "", true);
        visualEffects.Duration = DrawHorizontalFloatField("Duration", "", RPGBuilderEditor.Instance.FieldHeight,
            visualEffects.Duration);
        visualEffects.Delay =
            DrawHorizontalFloatField("Delay", "", RPGBuilderEditor.Instance.FieldHeight, visualEffects.Delay);

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        GUILayout.Space(10);
        return visualEffects;
    }

    public static List<SoundEntry> DrawSoundsList(List<SoundEntry> sounds)
    {
        if (DrawHorizontalAddButton("Add Sound", true))
        {
            sounds.Add(new SoundEntry());
        }

        for (var a = 0; a < sounds.Count; a++)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            EditorGUILayout.BeginHorizontal();
           
           sounds[a].ActivationType =
               (ActivationType) DrawHorizontalEnum("Activate On", "",
                   (int)sounds[a].ActivationType,
                   Enum.GetNames(typeof(ActivationType)));
            if (DrawSmallRemoveButton())
            {
                sounds.RemoveAt(a);
                return sounds;
            }
            EditorGUILayout.EndHorizontal();

            sounds[a].Template = (SoundTemplate)
                DrawHorizontalObject<SoundTemplate>("Template", "",
                    sounds[a].Template);
            sounds[a].Delay = DrawHorizontalFloatField("Delay", "", RPGBuilderEditor.Instance.FieldHeight, sounds[a].Delay);

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        return sounds; 
    }

    public static SoundEntry DrawSoundEntry(SoundEntry sounds)
    {
        sounds.Template = (SoundTemplate)
            DrawHorizontalObject<SoundTemplate>("Template", "",
                sounds.Template);
        sounds.Delay = DrawHorizontalFloatField("Delay", "", RPGBuilderEditor.Instance.FieldHeight, sounds.Delay);

        GUILayout.Space(10);
        return sounds;
    }

    public static List<AnimationEntry> DrawAnimationsList(List<AnimationEntry> animation)
    {
        if (DrawHorizontalAddButton("Add Animation", true))
        {
            animation.Add(new AnimationEntry());
        }

        for (var a = 0; a < animation.Count; a++)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            EditorGUILayout.BeginHorizontal();
            animation[a].ActivationType =
                (ActivationType) DrawHorizontalEnum("Activate On", "",
                    (int)animation[a].ActivationType,
                    Enum.GetNames(typeof(ActivationType)));
            if (DrawSmallRemoveButton())
            {
                animation.RemoveAt(a);
                return animation;
            }
            EditorGUILayout.EndHorizontal();

            animation[a].Template = (AnimationTemplate)
                DrawHorizontalObject<AnimationTemplate>("Template", "",
                    animation[a].Template);
            
            animation[a].Delay = DrawHorizontalFloatField("Delay", "", RPGBuilderEditor.Instance.FieldHeight, animation[a].Delay);
            
            animation[a].ShowWeapons = DrawHorizontalToggle("Show Weapons?", "",
                RPGBuilderEditor.Instance.FieldHeight, animation[a].ShowWeapons);
            if (animation[a].ShowWeapons)
            {
                animation[a].ShowWeaponsDuration = DrawHorizontalFloatField("Weapons Duration", "",
                    RPGBuilderEditor.Instance.FieldHeight, animation[a].ShowWeaponsDuration);
            }

            animation[a].ModifySpeed = DrawHorizontalToggle("Modify Speed?", "",
                RPGBuilderEditor.Instance.FieldHeight, animation[a].ModifySpeed);
            if (animation[a].ModifySpeed)
            {
                animation[a].SpeedParameterName = DrawHorizontalTextField("Speed Parameter", "",
                    RPGBuilderEditor.Instance.FieldHeight, animation[a].SpeedParameterName);
                animation[a].ModifierSpeed = DrawHorizontalFloatField("Speed", "",
                    RPGBuilderEditor.Instance.FieldHeight, animation[a].ModifierSpeed);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        return animation;
    }

    public static AnimationEntry DrawAnimationEntry(AnimationEntry animation)
    {
        animation.Template = (AnimationTemplate)
            DrawHorizontalObject<AnimationTemplate>("Template", "",
                animation.Template);

        animation.Delay =
            DrawHorizontalFloatField("Delay", "", RPGBuilderEditor.Instance.FieldHeight, animation.Delay);

        animation.ShowWeapons = DrawHorizontalToggle("Show Weapons?", "",
            RPGBuilderEditor.Instance.FieldHeight, animation.ShowWeapons);
        if (animation.ShowWeapons)
        {
            animation.ShowWeaponsDuration = DrawHorizontalFloatField("Weapons Duration", "",
                RPGBuilderEditor.Instance.FieldHeight, animation.ShowWeaponsDuration);
        }

        animation.ModifySpeed = DrawHorizontalToggle("Modify Speed?", "",
            RPGBuilderEditor.Instance.FieldHeight, animation.ModifySpeed);
        if (animation.ModifySpeed)
        {
            animation.SpeedParameterName = DrawHorizontalTextField("Speed Parameter", "",
                RPGBuilderEditor.Instance.FieldHeight, animation.SpeedParameterName);
            animation.ModifierSpeed = DrawHorizontalFloatField("Speed", "",
                RPGBuilderEditor.Instance.FieldHeight, animation.ModifierSpeed);
        }
        GUILayout.Space(10);
        return animation;
    }

    public static List<RPGStat.VitalityActions> DrawVitalityActions(List<RPGStat.VitalityActions> vitalityActions)
    {
        GUILayout.Space(10);
        if (DrawHorizontalAddButton("Add Action", true))
        {
            vitalityActions.Add(new RPGStat.VitalityActions());
        }

        GUILayout.Space(10);
        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        for (var a = 0; a < vitalityActions.Count; a++)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            vitalityActions[a].GameActionsTemplate = (GameActionsTemplate)
                DrawHorizontalObject<GameActionsTemplate>("Actions", "",
                    vitalityActions[a].GameActionsTemplate);
            if (DrawSmallRemoveButton())
            {
                vitalityActions.RemoveAt(a);
                return vitalityActions;
            }
            EditorGUILayout.EndHorizontal();

            
            vitalityActions[a].value =
                DrawHorizontalFloatField("Value", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    vitalityActions[a].value);

            DrawHorizontalLabel("Value Type", "");
            vitalityActions[a].valueType =
                (RPGStat.VitalityActionsValueType) EditorGUILayout.EnumPopup(
                    vitalityActions[a].valueType,
                    GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));
            EditorGUILayout.EndHorizontal();

            vitalityActions[a].isPercent =
                DrawHorizontalToggle("Is Percent?", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    vitalityActions[a].isPercent);
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        GUILayout.Space(10);
        return vitalityActions;
    }

    public static void DrawActionAbilityList(List<RPGCombatDATA.ActionAbilityDATA> actionAbilities)
    {
        GUILayout.Space(10);
        if (DrawHorizontalAddButton("Add Ability", true))
        {
            actionAbilities.Add(new RPGCombatDATA.ActionAbilityDATA());
        }

        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        for (var a = 0; a < actionAbilities.Count; a++)
        {
            GUILayout.Space(10);
            if (DrawHorizontalEntryRemoveButton(actionAbilities[a].abilityID,"Ability"))
            {
                actionAbilities.RemoveAt(a);
                return;
            }
            
            actionAbilities[a].abilityID = DrawDatabaseEntryField(actionAbilities[a].abilityID, "Ability", "Ability", "");

            actionAbilities[a].keyType = (RPGCombatDATA.ActionAbilityKeyType) DrawHorizontalEnum("Key type", "",
                (int)actionAbilities[a].keyType,
                Enum.GetNames(typeof(RPGCombatDATA.ActionAbilityKeyType)));

            switch (actionAbilities[a].keyType)
            {
                case RPGCombatDATA.ActionAbilityKeyType.OverrideKey:
                    DrawHorizontalLabel("Key", "");
                    actionAbilities[a].key =  (KeyCode) EditorGUILayout.EnumPopup(actionAbilities[a].key);
                    EditorGUILayout.EndHorizontal();
                    break;
                case RPGCombatDATA.ActionAbilityKeyType.ActionKey:
                    var actionKeyIndex =
                        RPGBuilderEditorUtility.GetIndexFromActionKey(actionAbilities[a].actionKeyName);
                    if (actionKeyIndex == -1) actionKeyIndex = 0;
                    List<string> allActionKeyNames = RPGBuilderEditorUtility.GetActionKeyNamesList();
                    DrawHorizontalLabel("Action Key", "");
                    var tempIndex = EditorGUILayout.Popup(actionKeyIndex,
                        RPGBuilderEditorUtility.GetActionKeyNamesList().ToArray());
                    EditorGUILayout.EndHorizontal();
                    if (RPGBuilderEditor.Instance.GeneralSettings.actionKeys.Count > 0)
                        actionAbilities[a].actionKeyName = allActionKeyNames[tempIndex];

                    break;
            }
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
    }
    
    public static List<CharacterEntries.AllocatedStatEntry> DrawStatAllocationList(
        List<CharacterEntries.AllocatedStatEntry> allocatedStatEntryList)
    {
        GUILayout.Space(10);
        if (DrawHorizontalAddButton("Add Stat", true))
        {
            allocatedStatEntryList.Add(new CharacterEntries.AllocatedStatEntry());
        }

        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        for (var a = 0; a < allocatedStatEntryList.Count; a++)
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            allocatedStatEntryList[a].statID =
                DrawDatabaseEntryField(allocatedStatEntryList[a].statID, "Stat", "Stat", "");
            if (DrawSmallRemoveButton())
            {
                allocatedStatEntryList.RemoveAt(a);
                return allocatedStatEntryList;
            }
            EditorGUILayout.EndHorizontal();

            allocatedStatEntryList[a].maxValue =
                DrawHorizontalIntField("Max Value", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    allocatedStatEntryList[a].maxValue);

            allocatedStatEntryList[a].cost =
                DrawHorizontalIntField("Cost", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    allocatedStatEntryList[a].cost);

            allocatedStatEntryList[a].valueAdded =
                DrawHorizontalIntField("Amount Added", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    allocatedStatEntryList[a].valueAdded);
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        return allocatedStatEntryList;
    }

    public static int DrawEffectRankIndexField(int effectID, int effectRank)
    {
        RPGEffect entryReference = (RPGEffect) RPGBuilderEditorUtility.GetEntryByID(effectID, "Effect");
        
        if (entryReference != null && effectRank > entryReference.ranks.Count-1)
        {
            effectRank = entryReference.ranks.Count-1;
        }
        
        DrawHorizontalLabel("Effect Rank", "");
        effectRank = EditorGUILayout.Popup(effectRank, 
        RPGBuilderEditorUtility.GetRanksAsStringArray(entryReference !=null ? entryReference.ranks.Count : 0));
        EditorGUILayout.EndHorizontal();
        return effectRank;
    }
    
    public static int DrawAbilityRankIndexField(int abilityID, int abilityRank)
    {
        RPGAbility entryReference = (RPGAbility) RPGBuilderEditorUtility.GetEntryByID(abilityID, "Ability");
        
        if (entryReference != null && abilityRank > entryReference.ranks.Count-1)
        {
            abilityRank = entryReference.ranks.Count-1;
        }
        
        DrawHorizontalLabel("Ability Rank", "");
        abilityRank = EditorGUILayout.Popup(abilityRank, 
            RPGBuilderEditorUtility.GetRanksAsStringArray(entryReference !=null ? entryReference.ranks.Count : 0));
        EditorGUILayout.EndHorizontal();
        return abilityRank;
    }
    
    public static int DrawNPCPhaseIndexField(int npcID, int phaseIndex)
    {
        RPGNpc entryReference = (RPGNpc) RPGBuilderEditorUtility.GetEntryByID(npcID, "NPC");
        
        DrawHorizontalLabel("Phase", "");
        phaseIndex = EditorGUILayout.Popup(phaseIndex, 
            RPGBuilderEditorUtility.GetNPCPhasesStringArray(entryReference !=null ? entryReference.Phases.Count : 0));
        EditorGUILayout.EndHorizontal();
        return phaseIndex;
    }
    
    public static int DrawBonusRankIndexField(int bonusID, int bonusRank)
    {
        RPGBonus entryReference = (RPGBonus) RPGBuilderEditorUtility.GetEntryByID(bonusID, "Bonus");
        
        if (entryReference != null && bonusRank > entryReference.ranks.Count-1)
        {
            bonusRank = entryReference.ranks.Count-1;
        }
        
        DrawHorizontalLabel("Bonus Rank", "");
        bonusRank = EditorGUILayout.Popup(bonusRank, 
            RPGBuilderEditorUtility.GetRanksAsStringArray(entryReference !=null ? entryReference.ranks.Count : 0));
        EditorGUILayout.EndHorizontal();
        return bonusRank;
    }
    
    public static int DrawRecipeRankIndexField(int recipeID, int recipeRank)
    {
        RPGCraftingRecipe entryReference = (RPGCraftingRecipe) RPGBuilderEditorUtility.GetEntryByID(recipeID, "Recipe");
        
        if (entryReference != null && recipeRank > entryReference.ranks.Count-1)
        {
            recipeRank = entryReference.ranks.Count-1;
        }
        
        DrawHorizontalLabel("Recipe Rank", "");
        recipeRank = EditorGUILayout.Popup(recipeRank, 
            RPGBuilderEditorUtility.GetRanksAsStringArray(entryReference !=null ? entryReference.ranks.Count : 0));
        EditorGUILayout.EndHorizontal();
        return recipeRank;
    }
    
    public static int DrawResourceRankIndexField(int resourceID, int resourceRank)
    {
        RPGResourceNode entryReference = (RPGResourceNode) RPGBuilderEditorUtility.GetEntryByID(resourceID, "Resoource");
        
        if (entryReference != null && resourceRank > entryReference.ranks.Count-1)
        {
            resourceRank = entryReference.ranks.Count-1;
        }
        
        DrawHorizontalLabel("Resource Rank", "");
        resourceRank = EditorGUILayout.Popup(resourceRank, 
            RPGBuilderEditorUtility.GetRanksAsStringArray(entryReference !=null ? entryReference.ranks.Count : 0));
        EditorGUILayout.EndHorizontal();
        return resourceRank;
    }
    
    public static int DrawDatabaseEntryField(int id, string entryType, string fieldText, string tooltip)
    {
        switch (entryType)
        {
            case "Ability":
            {
                RPGAbility entryRef = id != -1 ? (RPGAbility) RPGBuilderEditorUtility.GetEntryByID(id, "Ability") : null;
                RPGAbility tempRef = (RPGAbility) DrawHorizontalObject<RPGAbility>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Effect":
            {
                RPGEffect entryRef = id != -1 ? (RPGEffect) RPGBuilderEditorUtility.GetEntryByID(id, "Effect") : null;
                RPGEffect tempRef = (RPGEffect) DrawHorizontalObject<RPGEffect>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "NPC":
            {
                RPGNpc entryRef = id != -1 ? (RPGNpc) RPGBuilderEditorUtility.GetEntryByID(id, "NPC") : null;
                RPGNpc tempRef = (RPGNpc) DrawHorizontalObject<RPGNpc>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Stat":
            {
                RPGStat entryRef = id != -1 ? (RPGStat) RPGBuilderEditorUtility.GetEntryByID(id, "Stat") : null;
                RPGStat tempRef = (RPGStat) DrawHorizontalObject<RPGStat>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Point":
            {
                RPGTreePoint entryRef = id != -1 ? (RPGTreePoint) RPGBuilderEditorUtility.GetEntryByID(id, "Point") : null;
                RPGTreePoint tempRef = (RPGTreePoint) DrawHorizontalObject<RPGTreePoint>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Spellbook":
            {
                RPGSpellbook entryRef = id != -1 ? (RPGSpellbook) RPGBuilderEditorUtility.GetEntryByID(id, "Spellbook") : null;
                RPGSpellbook tempRef = (RPGSpellbook) DrawHorizontalObject<RPGSpellbook>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Faction":
            {
                RPGFaction entryRef = id != -1 ? (RPGFaction) RPGBuilderEditorUtility.GetEntryByID(id, "Faction") : null;
                RPGFaction tempRef = (RPGFaction) DrawHorizontalObject<RPGFaction>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "WeaponTemplate":
            {
                RPGWeaponTemplate entryRef = id != -1 ? (RPGWeaponTemplate) RPGBuilderEditorUtility.GetEntryByID(id, "WeaponTemplate") : null;
                RPGWeaponTemplate tempRef = (RPGWeaponTemplate) DrawHorizontalObject<RPGWeaponTemplate>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Item":
            {
                RPGItem entryRef = id != -1 ? (RPGItem) RPGBuilderEditorUtility.GetEntryByID(id, "Item") : null;
                RPGItem tempRef = (RPGItem) DrawHorizontalObject<RPGItem>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Skill":
            {
                RPGSkill entryRef = id != -1 ? (RPGSkill) RPGBuilderEditorUtility.GetEntryByID(id, "Skill") : null;
                RPGSkill tempRef = (RPGSkill) DrawHorizontalObject<RPGSkill>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Levels":
            {
                RPGLevelsTemplate entryRef = id != -1 ? (RPGLevelsTemplate) RPGBuilderEditorUtility.GetEntryByID(id, "Levels") : null;
                RPGLevelsTemplate tempRef = (RPGLevelsTemplate) DrawHorizontalObject<RPGLevelsTemplate>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Race":
            {
                RPGRace entryRef = id != -1 ? (RPGRace) RPGBuilderEditorUtility.GetEntryByID(id, "Race") : null;
                RPGRace tempRef = (RPGRace) DrawHorizontalObject<RPGRace>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Class":
            {
                RPGClass entryRef = id != -1 ? (RPGClass) RPGBuilderEditorUtility.GetEntryByID(id, "Class") : null;
                RPGClass tempRef = (RPGClass) DrawHorizontalObject<RPGClass>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "LootTable":
            {
                RPGLootTable entryRef = id != -1 ? (RPGLootTable) RPGBuilderEditorUtility.GetEntryByID(id, "LootTable") : null;
                RPGLootTable tempRef = (RPGLootTable) DrawHorizontalObject<RPGLootTable>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "MerchantTable":
            {
                RPGMerchantTable entryRef = id != -1 ? (RPGMerchantTable) RPGBuilderEditorUtility.GetEntryByID(id, "MerchantTable") : null;
                RPGMerchantTable tempRef = (RPGMerchantTable) DrawHorizontalObject<RPGMerchantTable>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Currency":
            {
                RPGCurrency entryRef = id != -1 ? (RPGCurrency) RPGBuilderEditorUtility.GetEntryByID(id, "Currency") : null;
                RPGCurrency tempRef = (RPGCurrency) DrawHorizontalObject<RPGCurrency>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Recipe":
            {
                RPGCraftingRecipe entryRef = id != -1 ? (RPGCraftingRecipe) RPGBuilderEditorUtility.GetEntryByID(id, "Recipe") : null;
                RPGCraftingRecipe tempRef = (RPGCraftingRecipe) DrawHorizontalObject<RPGCraftingRecipe>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "CraftingStation":
            {
                RPGCraftingStation entryRef = id != -1 ? (RPGCraftingStation) RPGBuilderEditorUtility.GetEntryByID(id, "CraftingStation") : null;
                RPGCraftingStation tempRef = (RPGCraftingStation) DrawHorizontalObject<RPGCraftingStation>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "TalentTree":
            {
                RPGTalentTree entryRef = id != -1 ? (RPGTalentTree) RPGBuilderEditorUtility.GetEntryByID(id, "TalentTree") : null;
                RPGTalentTree tempRef = (RPGTalentTree) DrawHorizontalObject<RPGTalentTree>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Bonus":
            {
                RPGBonus entryRef = id != -1 ? (RPGBonus) RPGBuilderEditorUtility.GetEntryByID(id, "Bonus") : null;
                RPGBonus tempRef = (RPGBonus) DrawHorizontalObject<RPGBonus>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "GearSet":
            {
                RPGGearSet entryRef = id != -1 ? (RPGGearSet) RPGBuilderEditorUtility.GetEntryByID(id, "GearSet") : null;
                RPGGearSet tempRef = (RPGGearSet) DrawHorizontalObject<RPGGearSet>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Enchantment":
            {
                RPGEnchantment entryRef = id != -1 ? (RPGEnchantment) RPGBuilderEditorUtility.GetEntryByID(id, "Enchantment") : null;
                RPGEnchantment tempRef = (RPGEnchantment) DrawHorizontalObject<RPGEnchantment>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Task":
            {
                RPGTask entryRef = id != -1 ? (RPGTask) RPGBuilderEditorUtility.GetEntryByID(id, "Task") : null;
                RPGTask tempRef = (RPGTask) DrawHorizontalObject<RPGTask>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Quest":
            {
                RPGQuest entryRef = id != -1 ? (RPGQuest) RPGBuilderEditorUtility.GetEntryByID(id, "Quest") : null;
                RPGQuest tempRef = (RPGQuest) DrawHorizontalObject<RPGQuest>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Coordinate":
            {
                RPGWorldPosition entryRef = id != -1 ? (RPGWorldPosition) RPGBuilderEditorUtility.GetEntryByID(id, "Coordinate") : null;
                RPGWorldPosition tempRef = (RPGWorldPosition) DrawHorizontalObject<RPGWorldPosition>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Resource":
            {
                RPGResourceNode entryRef = id != -1 ? (RPGResourceNode) RPGBuilderEditorUtility.GetEntryByID(id, "Resource") : null;
                RPGResourceNode tempRef = (RPGResourceNode) DrawHorizontalObject<RPGResourceNode>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "GameScene":
            {
                RPGGameScene entryRef = id != -1 ? (RPGGameScene) RPGBuilderEditorUtility.GetEntryByID(id, "GameScene") : null;
                RPGGameScene tempRef = (RPGGameScene) DrawHorizontalObject<RPGGameScene>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Dialogue":
            {
                RPGDialogue entryRef = id != -1 ? (RPGDialogue) RPGBuilderEditorUtility.GetEntryByID(id, "Dialogue") : null;
                RPGDialogue tempRef = (RPGDialogue) DrawHorizontalObject<RPGDialogue>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "GameModifier":
            {
                RPGGameModifier entryRef = id != -1 ? (RPGGameModifier) RPGBuilderEditorUtility.GetEntryByID(id, "GameModifier") : null;
                RPGGameModifier tempRef = (RPGGameModifier) DrawHorizontalObject<RPGGameModifier>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Species":
            {
                RPGSpecies entryRef = id != -1 ? (RPGSpecies) RPGBuilderEditorUtility.GetEntryByID(id, "Species") : null;
                RPGSpecies tempRef = (RPGSpecies) DrawHorizontalObject<RPGSpecies>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
            case "Combo":
            {
                RPGCombo entryRef = id != -1 ? (RPGCombo) RPGBuilderEditorUtility.GetEntryByID(id, "Combo") : null;
                RPGCombo tempRef = (RPGCombo) DrawHorizontalObject<RPGCombo>(fieldText, tooltip, entryRef);
                return tempRef != null ? tempRef.ID : -1;
            }
        }
        return -1;
    }
}
