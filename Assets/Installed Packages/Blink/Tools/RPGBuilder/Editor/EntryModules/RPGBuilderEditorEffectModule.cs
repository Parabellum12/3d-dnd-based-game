using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class RPGBuilderEditorEffectModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGEffect> entries = new Dictionary<int, RPGEffect>();
    private RPGEffect currentEntry;
    
    private readonly List<RPGBuilderDatabaseEntry> allEffectTags = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allNodeSockets = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allDamageTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allHealingTypes = new List<RPGBuilderDatabaseEntry>();
    public override void Initialize()
    {
        LoadEntries();
        if (entries.Count != 0)
        {
            currentEntry = Instantiate(entries[RPGBuilderEditor.Instance.CurrentEntryIndex]);
            RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
        }
        else
        {
            CreateNewEntry();
        }
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.effectFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }
    
    public override void LoadEntries()
    {
        Dictionary<int, RPGEffect> dictionary = new Dictionary<int, RPGEffect>();
        databaseEntries.Clear();
        allEffectTags.Clear();
        allNodeSockets.Clear();
        allDamageTypes.Clear();
        allHealingTypes.Clear();
        var allEntries = Resources.LoadAll<RPGEffect>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }
        entries = dictionary;

        foreach (var typeEntry in Resources.LoadAll<RPGBEffectTag>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allEffectTags.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBNodeSocket>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allNodeSockets.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBDamageType>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allDamageTypes.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBHealingType>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allHealingTypes.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        { 
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }

        if (currentEntry != null) currentEntry.ranks.Clear();
        
        currentEntry = CreateInstance<RPGEffect>();
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
        RPGBuilderEditor.Instance.CurrentEntryIndex = -1;
    }

    public override bool SaveConditionsMet()
    {
        if (string.IsNullOrEmpty(currentEntry.entryName))
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("Invalid Name", "Enter a valid name", "OK");
            return false;
        }
        if (ContainsInvalidCharacters(currentEntry.entryName))
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("Invalid Characters", "The Name contains invalid characters", "OK");
            return false;
        }
        
        if (currentEntry.ranks.Count == 0)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("0 Ranks", "Create at least 1 rank", "OK");
            return false;
        }
        
        return true;
    }
    
    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGEffect entryFile = (RPGEffect)updatedEntry;
        entryFile.UpdateEntryData(currentEntry);
    }
    
    public override void ClearEntries()
    {
        databaseEntries.Clear();
        entries.Clear();
        currentEntry = null;
    }

    public override void DrawView()
    {
        if (currentEntry == null)
        {
            if (entries.Count > 0 && entries[0] != null)
            {
                RPGBuilderEditor.Instance.SelectDatabaseEntry(0, true);
            }
            else
            {
                CreateNewEntry();
            }
        }
        
        RPGBuilderEditorUtility.UpdateViewAndFieldData();

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);
        
        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.MaxWidth(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.ExpandHeight(true));

        RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showBaseInfo)
        {
            GUILayout.Space(5);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon = RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID( currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }


        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showType =
            RPGBuilderEditorUtility.HandleModuleBanner("TYPE", RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showType);
        if (RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showType)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            
            currentEntry.effectType = 
                (RPGEffect.EFFECT_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Type", "What type of effect is it?", 
                    (int)currentEntry.effectType,
                    Enum.GetNames(typeof(RPGEffect.EFFECT_TYPE)));

            currentEntry.EffectTag = (RPGBEffectTag) RPGBuilderEditorFields.DrawTypeEntryField("Effect Tag", allEffectTags, currentEntry.EffectTag);
            
            
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        if (currentEntry.effectType == RPGEffect.EFFECT_TYPE.DamageOverTime ||
            currentEntry.effectType == RPGEffect.EFFECT_TYPE.HealOverTime
            || currentEntry.effectType == RPGEffect.EFFECT_TYPE.Immune ||
            currentEntry.effectType == RPGEffect.EFFECT_TYPE.Shapeshifting
            || currentEntry.effectType == RPGEffect.EFFECT_TYPE.Sleep ||
            currentEntry.effectType == RPGEffect.EFFECT_TYPE.Stat
            || currentEntry.effectType == RPGEffect.EFFECT_TYPE.Stun ||
            currentEntry.effectType == RPGEffect.EFFECT_TYPE.Taunt
            || currentEntry.effectType == RPGEffect.EFFECT_TYPE.Root ||
            currentEntry.effectType == RPGEffect.EFFECT_TYPE.Silence ||
            currentEntry.effectType == RPGEffect.EFFECT_TYPE.Flying ||
            currentEntry.effectType == RPGEffect.EFFECT_TYPE.Stealth ||
            currentEntry.effectType == RPGEffect.EFFECT_TYPE.Mount)
        {
            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showStateSettings =
                RPGBuilderEditorUtility.HandleModuleBanner("STATE SETTINGS", RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showStateSettings);

            if (RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showStateSettings)
            {
                GUILayout.Space(10);
                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                currentEntry.isState = true;
                
                RPGBuilderEditorFields.DrawTitleLabel("Time:", "");
                currentEntry.IsPersistent =
                    RPGBuilderEditorFields.DrawHorizontalToggle("Persistent?", "Should this effect stay on the unit between sessions?", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.IsPersistent);
                currentEntry.endless =
                    RPGBuilderEditorFields.DrawHorizontalToggle("Endless?", "Is this effect not ending after a duration?", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.endless);
                if (!currentEntry.endless)
                {
                    currentEntry.duration = RPGBuilderEditorFields.DrawHorizontalFloatField("Duration",
                        "How long does the effect last?",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.duration);
                }
                

                if (currentEntry.effectType == RPGEffect.EFFECT_TYPE.DamageOverTime ||
                    currentEntry.effectType == RPGEffect.EFFECT_TYPE.HealOverTime ||
                    currentEntry.effectType == RPGEffect.EFFECT_TYPE.Stat)
                {
                    RPGBuilderEditorFields.DrawTitleLabel("Stacking:", "", true);
                    currentEntry.allowMultiple =
                        RPGBuilderEditorFields.DrawHorizontalToggle("Multiple?",
                            "Can this effect be active multiple times on the same target?",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.allowMultiple);
                    currentEntry.allowMixedCaster =
                        RPGBuilderEditorFields.DrawHorizontalToggle("Mixed Caster?",
                            "Can effects stack together even if they are not from the same caster?",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.allowMixedCaster);
                    currentEntry.stackLimit = RPGBuilderEditorFields.DrawHorizontalIntField("Max Stacks",
                        "How many times maximum can it stack?",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.stackLimit);
                    currentEntry.pulses = RPGBuilderEditorFields.DrawHorizontalIntField("Pulses Count",
                        "How many times will this effect pulse? Each pulse will trigger the effect again",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.pulses);
                }
                
                RPGBuilderEditorFields.DrawTitleLabel("UI:", "", true);
                currentEntry.showUIState =
                    RPGBuilderEditorFields.DrawHorizontalToggle("Show in UI", "Should this effect be visible in the UI while active?", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.showUIState);
                if (currentEntry.showUIState)
                {
                    currentEntry.isBuffOnSelf =
                        RPGBuilderEditorFields.DrawHorizontalToggle("Buff?", "Is this effect a buff on self?", RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.isBuffOnSelf);
                    currentEntry.canBeManuallyRemoved =
                        RPGBuilderEditorFields.DrawHorizontalToggle("Player can remove?", "", RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.canBeManuallyRemoved);
                }
                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            }
        }
        else
        {
            currentEntry.isState = false;
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showRanks =
            RPGBuilderEditorUtility.HandleModuleBanner("RANKS", RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showRanks);
        if (RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showRanks)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Rank", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(150),
                GUILayout.ExpandWidth(true)))
            {
                var newRankData = new RPGEffect.RPGEffectRankData();
                currentEntry.ranks.Add(newRankData);
            }

            if (currentEntry.ranks.Count > 0)
            {
                GUILayout.Space(20);
                if (GUILayout.Button("Remove Rank", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"),
                    GUILayout.MinWidth(150),
                    GUILayout.ExpandWidth(true)))
                {
                    currentEntry.ranks.RemoveAt(currentEntry.ranks.Count - 1);
                    return;
                }
            }

            GUILayout.EndHorizontal();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

            GUILayout.Space(10);

            for (var i = 0; i < currentEntry.ranks.Count; i++)
            {
                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

                var rankNbr = i + 1;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Rank: " + rankNbr, RPGBuilderEditor.Instance.EditorSkin.GetStyle("GenericButton"),
                    GUILayout.ExpandWidth(true)))
                {
                    currentEntry.ranks[i].ShowedInEditor = !currentEntry.ranks[i].ShowedInEditor;
                    GUI.FocusControl(null);
                }

                if (i > 0)
                {
                    GUILayout.Space(5);
                    if (GUILayout.Button("Copy Above", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.ExpandWidth(true)))
                    {
                        currentEntry.CopyEntryData(currentEntry.ranks[i],
                            currentEntry.ranks[i - 1]);
                        GUI.FocusControl(null);
                    }
                }

                GUILayout.EndHorizontal();
                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

                if (currentEntry.ranks[i].ShowedInEditor)
                {
                    GUILayout.Space(10);
                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    switch (currentEntry.effectType)
                    {
                        case RPGEffect.EFFECT_TYPE.InstantDamage:
                        case RPGEffect.EFFECT_TYPE.DamageOverTime:
                        {
                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Damage Settings:", "", false);
                            currentEntry.ranks[i].Damage = RPGBuilderEditorFields.DrawHorizontalIntField("Damage",
                                "What is the base damage value of this effect?",
                                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].Damage);

                            currentEntry.ranks[i].alteredStatID =
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].alteredStatID,
                                    "Stat", "Altered Vitality Stat", "");

                            RPGStat entryReference =
                                (RPGStat) RPGBuilderEditorUtility.GetEntryByID(currentEntry.ranks[i].alteredStatID,
                                    "Stat");

                            if (entryReference != null && !entryReference.isVitalityStat)
                            {
                                currentEntry.ranks[i].alteredStatID = -1;
                                EditorUtility.DisplayDialog("Warning",
                                    "The altered Stat can only be of type Vitality", "OK");
                            }

                            currentEntry.ranks[i].hitValueType =
                                (RPGAbility.COST_TYPES) RPGBuilderEditorFields.DrawHorizontalEnum("Hit Type",
                                    "Is this a flat value, or a percentage of the altered stat?",
                                    (int) currentEntry.ranks[i].hitValueType,
                                    Enum.GetNames(typeof(RPGAbility.COST_TYPES)));

                            if ((currentEntry.ranks[i].hitValueType == RPGAbility.COST_TYPES.PERCENT_OF_MAX ||
                                 currentEntry.ranks[i].hitValueType ==
                                 RPGAbility.COST_TYPES.PERCENT_OF_CURRENT) &&
                                currentEntry.ranks[i].Damage < 0)
                            {
                                currentEntry.ranks[i].Damage = 0;
                            }

                            currentEntry.ranks[i].FlatCalculation = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Flat Calculation?", "",
                                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].FlatCalculation);

                            if (!currentEntry.ranks[i].FlatCalculation)
                            {

                                currentEntry.ranks[i].mainDamageType =
                                    (RPGEffect.MainDamageTypes) RPGBuilderEditorFields.DrawHorizontalEnum(
                                        "Main Damage Type", "What is the main damage type of this effect?",
                                        (int) currentEntry.ranks[i].mainDamageType,
                                        Enum.GetNames(typeof(RPGEffect.MainDamageTypes)));
                                
                                currentEntry.ranks[i].customDamageType = (RPGBDamageType) RPGBuilderEditorFields.DrawTypeEntryField("Custom Damage Type", allDamageTypes, currentEntry.ranks[i].customDamageType);

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Damage Bonuses:", "", true);
                                currentEntry.ranks[i].weaponDamageModifier =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField(
                                        "Weapon Power",
                                        "The percentage of the weapon damage that will be added to the effect damage",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].weaponDamageModifier);
                                if (currentEntry.ranks[i].weaponDamageModifier > 0)
                                {
                                    currentEntry.ranks[i].useWeapon1Damage =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Weapon 1?", "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].useWeapon1Damage);
                                    currentEntry.ranks[i].useWeapon2Damage =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Weapon 2?", "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].useWeapon2Damage);
                                }

                                currentEntry.ranks[i].maxHealthModifier =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField(
                                        "Max. Health",
                                        "How much of the caster's maximum health in % should be converted to extra damage?",
                                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].maxHealthModifier);
                                currentEntry.ranks[i].missingHealthModifier =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField(
                                        "Missing Health",
                                        "How much extra damage in % should be dealt for each percent of missing health?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].missingHealthModifier);

                                currentEntry.ranks[i].requiredEffectID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                                        currentEntry.ranks[i].requiredEffectID, "Effect", "Effect Required", "");

                                if (currentEntry.ranks[i].requiredEffectID != -1)
                                {
                                    currentEntry.ranks[i].requiredEffectDamageModifier =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField("Effect Damage",
                                            "How much damage is added if the required effect is active on the target? In percent",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].requiredEffectDamageModifier);
                                }

                                currentEntry.ranks[i].damageStatID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].damageStatID,
                                        "Stat", "Stat Damage", "");

                                if (currentEntry.ranks[i].damageStatID != -1)
                                {
                                    currentEntry.ranks[i].damageStatModifier =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField("Stat Damage Modifier",
                                            "The percentage of this stat added as damage",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].damageStatModifier);
                                }

                                currentEntry.ranks[i].skillModifierID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].skillModifierID,
                                        "Skill", "Skill", "");

                                if (currentEntry.ranks[i].skillModifierID != -1)
                                {
                                    currentEntry.ranks[i].skillModifier =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField(
                                            "Skill Damage", "How much damage is added by skill level",
                                            RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].skillModifier);
                                }

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Extra:", "", true);
                                currentEntry.ranks[i].lifesteal = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                    "Lifesteal",
                                    "How much of the damage dealt should be converted to self heals?",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].lifesteal);


                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Settings:", "", true);
                                currentEntry.ranks[i].CannotCrit = RPGBuilderEditorFields.DrawHorizontalToggle(
                                    "No Critical Hit?", "Does this effect have critical hit disabled?",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].CannotCrit);
                            }

                            currentEntry.ranks[i].removeStealth = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Remove Stealth?", "Does this effect remove stealth on hit stealth units?",
                                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].removeStealth);

                            break;
                        }
                        case RPGEffect.EFFECT_TYPE.HealOverTime:
                        case RPGEffect.EFFECT_TYPE.InstantHeal:
                        {
                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Healing Settings:", "", true);
                            currentEntry.ranks[i].Damage =
                                RPGBuilderEditorFields.DrawHorizontalIntField("Healing",
                                    "What is the base healing value?",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].Damage);

                            currentEntry.ranks[i].alteredStatID =
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].alteredStatID,
                                    "Stat", "Altered Vitality Stat", "");

                            RPGStat entryReference =
                                (RPGStat) RPGBuilderEditorUtility.GetEntryByID(currentEntry.ranks[i].alteredStatID,
                                    "Stat");

                            if (entryReference != null && !entryReference.isVitalityStat)
                            {
                                currentEntry.ranks[i].alteredStatID = -1;
                                EditorUtility.DisplayDialog("Warning",
                                    "The altered Stat can only be of type Vitality", "OK");
                            }

                            currentEntry.ranks[i].hitValueType =
                                (RPGAbility.COST_TYPES) RPGBuilderEditorFields.DrawHorizontalEnum("Hit Type",
                                    "Is this a flat value, or a percentage of the altered stat?",
                                    (int) currentEntry.ranks[i].hitValueType,
                                    Enum.GetNames(typeof(RPGAbility.COST_TYPES)));

                            if ((currentEntry.ranks[i].hitValueType == RPGAbility.COST_TYPES.PERCENT_OF_MAX ||
                                 currentEntry.ranks[i].hitValueType ==
                                 RPGAbility.COST_TYPES.PERCENT_OF_CURRENT) &&
                                currentEntry.ranks[i].Damage < 0)
                            {
                                currentEntry.ranks[i].Damage = 0;
                            }

                            currentEntry.ranks[i].FlatCalculation =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Flat Calculation?",
                                    "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].FlatCalculation);

                            if (!currentEntry.ranks[i].FlatCalculation)
                            {
                                currentEntry.ranks[i].customHealingType = (RPGBHealingType) RPGBuilderEditorFields.DrawTypeEntryField("Heal Type", allHealingTypes, currentEntry.ranks[i].customHealingType);


                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Healing Bonuses:", "", true);
                                currentEntry.ranks[i].weaponDamageModifier =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Weapon Power",
                                        "The percentage of the weapon damage that will be added to the effect healing",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].weaponDamageModifier);
                                if (currentEntry.ranks[i].weaponDamageModifier > 0)
                                {
                                    currentEntry.ranks[i].useWeapon1Damage =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Weapon 1?", "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].useWeapon1Damage);
                                    currentEntry.ranks[i].useWeapon2Damage =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Weapon 2?", "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].useWeapon2Damage);
                                }

                                currentEntry.ranks[i].skillModifierID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].skillModifierID,
                                        "Skill", "Skill", "");
                                if (currentEntry.ranks[i].skillModifierID != -1)
                                {
                                    currentEntry.ranks[i].skillModifier =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField("Skill Bonus",
                                            "How much healing is added by this skill level",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].skillModifier);
                                }

                                currentEntry.ranks[i].requiredEffectID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                                        currentEntry.ranks[i].requiredEffectID, "Effect", "Effect Required", "");

                                if (currentEntry.ranks[i].requiredEffectID != -1)
                                {
                                    currentEntry.ranks[i].requiredEffectDamageModifier =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField("Effect Damage Modifier",
                                            "How much damage is added if the required effect is active on the target? In percent",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].requiredEffectDamageModifier);
                                }

                                currentEntry.ranks[i].damageStatID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].damageStatID,
                                        "Stat", "Stat Bonus", "");

                                if (currentEntry.ranks[i].damageStatID != -1)
                                {
                                    currentEntry.ranks[i].damageStatModifier =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField("Damage Modifier",
                                            "The percentage of this stat added as damage",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].damageStatModifier);
                                }
                            }

                            break;
                        }
                        case RPGEffect.EFFECT_TYPE.Stat:
                        {
                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat Bonus", false))
                            {
                                currentEntry.ranks[i].statEffectsData
                                    .Add(new RPGEffect.STAT_EFFECTS_DATA());
                            }

                            for (var a = 0; a < currentEntry.ranks[i].statEffectsData.Count; a++)
                            {
                                GUILayout.Space(10);

                                RPGStat entryReference =
                                    (RPGStat) RPGBuilderEditorUtility.GetEntryByID(
                                        currentEntry.ranks[i].statEffectsData[a].statID, "Stat");

                                EditorGUILayout.BeginHorizontal();
                                currentEntry.ranks[i].statEffectsData[a].statID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                                        currentEntry.ranks[i].statEffectsData[a].statID,
                                        "Stat", "Stat", "");

                                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                                {
                                    currentEntry.ranks[i].statEffectsData.RemoveAt(a);
                                    return;
                                }

                                EditorGUILayout.EndHorizontal();
                                currentEntry.ranks[i].statEffectsData[a].statEffectModification =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Value",
                                        "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].statEffectsData[a].statEffectModification);

                                if (entryReference != null)
                                {
                                    if (!entryReference.isPercentStat)
                                    {
                                        currentEntry.ranks[i].statEffectsData[a].isPercent =
                                            RPGBuilderEditorFields.DrawHorizontalToggle("Is Percent?",
                                                "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].statEffectsData[a].isPercent);
                                    }
                                    else
                                    {
                                        currentEntry.ranks[i].statEffectsData[a].isPercent = false;
                                    }
                                }

                                GUILayout.Space(10);
                            }

                            break;
                        }
                        case RPGEffect.EFFECT_TYPE.Teleport:

                            currentEntry.ranks[i].teleportType =
                                (RPGEffect.TELEPORT_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Type",
                                    "What type of teleport is it?",
                                    (int) currentEntry.ranks[i].teleportType,
                                    Enum.GetNames(typeof(RPGEffect.TELEPORT_TYPE)));

                            switch (currentEntry.ranks[i].teleportType)
                            {
                                case RPGEffect.TELEPORT_TYPE.gameScene:
                                {
                                    currentEntry.ranks[i].gameSceneID =
                                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].gameSceneID,
                                            "GameScene", "Game Scene", "");

                                    currentEntry.ranks[i].teleportPOS =
                                        RPGBuilderEditorFields.DrawHorizontalVector3("Location",
                                            "Position coordinates to teleport to",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].teleportPOS);
                                    break;
                                }
                                case RPGEffect.TELEPORT_TYPE.position:
                                    currentEntry.ranks[i].teleportPOS =
                                        RPGBuilderEditorFields.DrawHorizontalVector3("Location",
                                            "Position coordinates to teleport to",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].teleportPOS);
                                    break;
                                case RPGEffect.TELEPORT_TYPE.directional:

                                    currentEntry.ranks[i].teleportDirectionalType =
                                        (RPGEffect.TELEPORT_DIRECTIONAL_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum(
                                            "Direction Type", "",
                                            (int) currentEntry.ranks[i].teleportDirectionalType,
                                            Enum.GetNames(typeof(RPGEffect.TELEPORT_DIRECTIONAL_TYPE)));

                                    currentEntry.ranks[i].teleportDirectionalDistance =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField("Distance",
                                            "Maximum distance for the teleport",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].teleportDirectionalDistance);

                                    RPGBuilderEditorFields.DrawHorizontalLabel("Block Layers",
                                        "What layers are blocking the teleport?");
                                    LayerMask tempMask = EditorGUILayout.MaskField(
                                        InternalEditorUtility.LayerMaskToConcatenatedLayersMask(
                                            currentEntry.ranks[i].teleportDirectionalBlockLayers),
                                        InternalEditorUtility.layers, RPGBuilderEditorFields.GetTextFieldStyle(),
                                        GUILayout.ExpandWidth(true));
                                    currentEntry.ranks[i].teleportDirectionalBlockLayers =
                                        InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
                                    EditorGUILayout.EndHorizontal();
                                    break;
                            }

                            break;
                        case RPGEffect.EFFECT_TYPE.Pet:
                            currentEntry.ranks[i].petNPCDataID = RPGBuilderEditorFields.DrawDatabaseEntryField(
                                currentEntry.ranks[i].petNPCDataID, "NPC",
                                "NPC", "");
                            currentEntry.ranks[i].petSPawnCount =
                                RPGBuilderEditorFields.DrawHorizontalIntField("Spawn Count",
                                    "How many pets should this effect spawn at once?",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].petSPawnCount);
                            currentEntry.ranks[i].petDuration =
                                RPGBuilderEditorFields.DrawHorizontalFloatField("Duration",
                                    "How long should the pet(s) stay active?",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].petDuration);
                            currentEntry.ranks[i].petScaleWithCharacter =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Character Scaling?",
                                    "Does this pet scale with the character level?",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].petScaleWithCharacter);
                            break;
                        case RPGEffect.EFFECT_TYPE.RollLootTable:
                            currentEntry.ranks[i].lootTableID =
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].lootTableID,
                                    "LootTable",
                                    "Loot Table", "");
                            break;
                        case RPGEffect.EFFECT_TYPE.Knockback:
                            currentEntry.ranks[i].knockbackDistance = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                "Knockback Distance", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].knockbackDistance);
                            break;
                        case RPGEffect.EFFECT_TYPE.Motion:
                            currentEntry.ranks[i].motionDistance = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                "Distance", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].motionDistance);
                            currentEntry.ranks[i].motionSpeed = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                "Speed", "0.5 is standard speed", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].motionSpeed);
                            currentEntry.ranks[i].motionDirection = RPGBuilderEditorFields.DrawHorizontalVector3(
                                "Direction", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].motionDirection);
                            currentEntry.ranks[i].isImmuneDuringMotion = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Immunity?", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].isImmuneDuringMotion);
                            currentEntry.ranks[i].motionIgnoreUseCondition =
                                RPGBuilderEditorFields.DrawHorizontalToggle(
                                    "Ignore Use Conditions?", "", RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].motionIgnoreUseCondition);
                            break;
                        case RPGEffect.EFFECT_TYPE.Blocking:

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Angle:", "");
                            currentEntry.ranks[i].blockAngle = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                "Block Angle", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].blockAngle);

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Time:", "", true);
                            currentEntry.ranks[i].isBlockChargeTime = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Charge Time?", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].isBlockChargeTime);
                            if (currentEntry.ranks[i].isBlockChargeTime)
                            {
                                currentEntry.ranks[i].blockChargeTime = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                    "Charge Duration",
                                    "How long from the moment the block is starting until it is starting to be effective?",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].blockChargeTime);
                            }

                            currentEntry.ranks[i].blockDurationType =
                                (RPGEffect.BLOCK_DURATION_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum(
                                    "Duration Type", "",
                                    (int) currentEntry.ranks[i].blockDurationType,
                                    Enum.GetNames(typeof(RPGEffect.BLOCK_DURATION_TYPE)));

                            switch (currentEntry.ranks[i].blockDurationType)
                            {
                                case RPGEffect.BLOCK_DURATION_TYPE.Time:
                                {
                                    currentEntry.ranks[i].isBlockLimitedDuration =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Limited Duration?", "", RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].isBlockLimitedDuration);
                                    if (currentEntry.ranks[i].isBlockLimitedDuration)
                                    {
                                        currentEntry.ranks[i].blockDuration =
                                            RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                "Block Duration", "How long will the block last",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].blockDuration);
                                    }

                                    break;
                                }
                                case RPGEffect.BLOCK_DURATION_TYPE.HoldKey:
                                    break;
                            }

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Decay:", "", true);
                            currentEntry.ranks[i].isBlockPowerDecay = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Power Decay?", "Is the block power decaying over time?",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].isBlockPowerDecay);
                            if (currentEntry.ranks[i].isBlockPowerDecay)
                            {
                                currentEntry.ranks[i].blockPowerDecay = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                    "Decay Speed", "How fast will the power decay",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].blockPowerDecay);
                            }


                            RPGBuilderEditorFields.DrawTitleLabelExpanded("End Conditions:", "", true);

                            currentEntry.ranks[i].blockEndType =
                                (RPGEffect.BLOCK_END_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("End Type", "",
                                    (int) currentEntry.ranks[i].blockEndType,
                                    Enum.GetNames(typeof(RPGEffect.BLOCK_END_TYPE)));

                            switch (currentEntry.ranks[i].blockEndType)
                            {
                                case RPGEffect.BLOCK_END_TYPE.HitCount:
                                    currentEntry.ranks[i].blockHitCount = RPGBuilderEditorFields.DrawHorizontalIntField(
                                        "Block Count", "", RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].blockHitCount);
                                    break;
                                case RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked:
                                    currentEntry.ranks[i].blockMaxDamage =
                                        RPGBuilderEditorFields.DrawHorizontalIntField(
                                            "Max Damage Blocked", "", RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].blockMaxDamage);
                                    break;
                                case RPGEffect.BLOCK_END_TYPE.Stat:
                                    break;
                            }

                            currentEntry.ranks[i].blockStatID =
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].blockStatID, "Stat",
                                    "Stat", "");

                            currentEntry.ranks[i].blockStatDecay = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Stat Drain?", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].blockStatDecay);
                            if (currentEntry.ranks[i].blockStatDecay)
                            {
                                currentEntry.ranks[i].blockStatDecayAmount =
                                    RPGBuilderEditorFields.DrawHorizontalIntField(
                                        "Drain Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].blockStatDecayAmount);
                                currentEntry.ranks[i].blockStatDecayInterval =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField(
                                        "Drain Interval", "", RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].blockStatDecayInterval);
                            }

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Block Settings:", "", true);
                            currentEntry.ranks[i].blockPowerFlat = RPGBuilderEditorFields.DrawHorizontalIntField(
                                "Block Flat Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].blockPowerFlat);
                            currentEntry.ranks[i].blockPowerModifier = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                "Block Percent Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].blockPowerModifier);

                            currentEntry.ranks[i].isBlockKnockback = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Block Knockback?", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].isBlockKnockback);

                            currentEntry.ranks[i].blockAnyDamage = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Block Any Damage?", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].blockAnyDamage);
                            if (!currentEntry.ranks[i].blockAnyDamage)
                            {
                                currentEntry.ranks[i].blockPhysicalDamage = RPGBuilderEditorFields.DrawHorizontalToggle(
                                    "Block Physical Damage?", "", RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].blockPhysicalDamage);
                                currentEntry.ranks[i].blockMagicalDamage = RPGBuilderEditorFields.DrawHorizontalToggle(
                                    "Block Magical Damage?", "", RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].blockMagicalDamage);

                                GUILayout.Space(10);
                                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Custom Damage Type", true))
                                {
                                    currentEntry.ranks[i].blockedDamageTypes.Add("");
                                }

                                GUILayout.Space(10);
                                for (var a = 0; a < currentEntry.ranks[i].blockedDamageTypes.Count; a++)
                                {
                                    var currentStatFunctionIndex =
                                        GetIndexFromCustomDamageTypes(currentEntry.ranks[i].blockedDamageTypes[a]);
                                    if (currentStatFunctionIndex == -1)
                                    {
                                        currentStatFunctionIndex = 0;
                                    }

                                    List<string> allCustomDamageTypes = GetCustomDamageTypes();
                                    EditorGUILayout.BeginHorizontal();
                                    var tempIndex = EditorGUILayout.Popup(
                                        currentStatFunctionIndex, allCustomDamageTypes.ToArray(),
                                        GUILayout.Height(RPGBuilderEditor.Instance.FieldHeight));

                                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                                    {
                                        currentEntry.ranks[i].blockedDamageTypes.RemoveAt(a);
                                        return;
                                    }

                                    EditorGUILayout.EndHorizontal();

                                    if (allCustomDamageTypes.Count > 0)
                                    {
                                        currentEntry.ranks[i].blockedDamageTypes[a] =
                                            allCustomDamageTypes[tempIndex];
                                    }
                                    else
                                    {
                                        currentEntry.ranks[i].blockedDamageTypes[a] = "";
                                    }

                                    GUILayout.Space(5);
                                }
                            }

                            GUILayout.Space(10);
                            RPGBuilderEditorFields.DrawTitleLabelExpanded("On Block Actions:", "", true);
                            currentEntry.ranks[i].GameActionsTemplate = (GameActionsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<GameActionsTemplate>(
                                "Template", "", currentEntry.ranks[i].GameActionsTemplate);

                            break;

                        case RPGEffect.EFFECT_TYPE.Dispel:

                            currentEntry.ranks[i].dispelType =
                                (RPGEffect.DISPEL_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Dispel Type", "",
                                    (int) currentEntry.ranks[i].dispelType,
                                    Enum.GetNames(typeof(RPGEffect.DISPEL_TYPE)));

                            switch (currentEntry.ranks[i].dispelType)
                            {
                                case RPGEffect.DISPEL_TYPE.EffectType:
                                    currentEntry.ranks[i].dispelEffectType =
                                        (RPGEffect.EFFECT_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Effect Type",
                                            "",
                                            (int) currentEntry.ranks[i].dispelEffectType,
                                            Enum.GetNames(typeof(RPGEffect.EFFECT_TYPE)));
                                    break;
                                case RPGEffect.DISPEL_TYPE.EffectTag:
                                    currentEntry.ranks[i].DispelEffectTag = (RPGBEffectTag) RPGBuilderEditorFields.DrawTypeEntryField("Effect Type", allEffectTags, currentEntry.ranks[i].DispelEffectTag);
                                    break;
                                case RPGEffect.DISPEL_TYPE.Effect:
                                    currentEntry.ranks[i].dispelEffectID =
                                        RPGBuilderEditorFields.DrawDatabaseEntryField(
                                            currentEntry.ranks[i].dispelEffectID, "Effect",
                                            "Effect", "");
                                    break;
                            }

                            break;

                        case RPGEffect.EFFECT_TYPE.Shapeshifting:
                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Visual:", "", false);
                            currentEntry.ranks[i].shapeshiftingModel =
                                (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Model", "",
                                    currentEntry.ranks[i].shapeshiftingModel);

                            currentEntry.ranks[i].shapeshiftingmodelPosition =
                                RPGBuilderEditorFields.DrawHorizontalVector3("Model Position", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].shapeshiftingmodelPosition);
                            currentEntry.ranks[i].shapeshiftingmodelScale =
                                RPGBuilderEditorFields.DrawHorizontalVector3("Model Scale", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].shapeshiftingmodelScale);

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Animations:", "", true);
                            currentEntry.ranks[i].shapeshiftingAnimatorController = (RuntimeAnimatorController)
                                RPGBuilderEditorFields.DrawHorizontalObject<RuntimeAnimatorController>(
                                    "Animator Controller Rest", "", currentEntry.ranks[i].shapeshiftingAnimatorController);
                            currentEntry.ranks[i].shapeshiftingAnimatorControllerCombat = (RuntimeAnimatorController)
                                RPGBuilderEditorFields.DrawHorizontalObject<RuntimeAnimatorController>(
                                    "Animator Controller Combat", "", currentEntry.ranks[i].shapeshiftingAnimatorControllerCombat);

                            currentEntry.ranks[i].shapeshiftingAnimatorAvatar = (Avatar)
                                RPGBuilderEditorFields.DrawHorizontalObject<Avatar>("Avatar", "",
                                    currentEntry.ranks[i].shapeshiftingAnimatorAvatar);

                            currentEntry.ranks[i].shapeshiftingAnimatorUseRootMotion =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Animator Root Motion", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].shapeshiftingAnimatorUseRootMotion);

                            currentEntry.ranks[i].shapeshiftingAnimatorUpdateMode =
                                (AnimatorUpdateMode) RPGBuilderEditorFields.DrawHorizontalEnum("Animator Update Mode",
                                    "",
                                    (int) currentEntry.ranks[i].shapeshiftingAnimatorUpdateMode,
                                    Enum.GetNames(typeof(AnimatorUpdateMode)));

                            currentEntry.ranks[i].shapeshiftingAnimatorCullingMode =
                                (AnimatorCullingMode) RPGBuilderEditorFields.DrawHorizontalEnum("Animator Culling Mode",
                                    "",
                                    (int) currentEntry.ranks[i].shapeshiftingAnimatorCullingMode,
                                    Enum.GetNames(typeof(AnimatorCullingMode)));

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Camera Settings:", "", true);
                            currentEntry.ranks[i].canCameraAim =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Can Camera Aim?", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].canCameraAim);

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Abilities:", "", true);
                            currentEntry.ranks[i].shapeshiftingOverrideMainActionBar =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Override Main Action Bar?", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].shapeshiftingOverrideMainActionBar);

                            if (currentEntry.ranks[i].shapeshiftingOverrideMainActionBar)
                            {
                                GUILayout.Space(10);
                                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Ability", true))
                                {
                                    currentEntry.ranks[i].shapeshiftingActiveAbilities.Add(-1);
                                }

                                for (var a = 0; a < currentEntry.ranks[i].shapeshiftingActiveAbilities.Count; a++)
                                {
                                    GUILayout.Space(5);
                                    if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                                        currentEntry.ranks[i].shapeshiftingActiveAbilities[a],
                                        "Ability"))
                                    {
                                        currentEntry.ranks[i].shapeshiftingActiveAbilities.RemoveAt(a);
                                        return;
                                    }

                                    currentEntry.ranks[i].shapeshiftingActiveAbilities[a] =
                                        RPGBuilderEditorFields.DrawDatabaseEntryField(
                                            currentEntry.ranks[i].shapeshiftingActiveAbilities[a], "Ability",
                                            "Ability", "");

                                    GUILayout.Space(10);
                                }
                            }


                            GUILayout.Space(10);
                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Effect", true))
                            {
                                currentEntry.ranks[i].nestedEffects.Add(new RPGAbility.AbilityEffectsApplied());
                            }

                            currentEntry.ranks[i].nestedEffects =
                                RPGBuilderEditorFields.DrawEffectsAppliedList(currentEntry.ranks[i].nestedEffects,
                                    false);

                            GUILayout.Space(10);
                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat", true))
                            {
                                currentEntry.ranks[i].statEffectsData.Add(new RPGEffect.STAT_EFFECTS_DATA());
                            }

                            for (var a = 0; a < currentEntry.ranks[i].statEffectsData.Count; a++)
                            {
                                GUILayout.Space(10);

                                RPGStat entryReference =
                                    (RPGStat) RPGBuilderEditorUtility.GetEntryByID(
                                        currentEntry.ranks[i].statEffectsData[a].statID, "Stat");

                                EditorGUILayout.BeginHorizontal();
                                currentEntry.ranks[i].statEffectsData[a].statID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                                        currentEntry.ranks[i].statEffectsData[a].statID,
                                        "Stat", "Stat", "");
                                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                                {
                                    currentEntry.ranks[i].statEffectsData.RemoveAt(a);
                                    return;
                                }

                                EditorGUILayout.EndHorizontal();

                                currentEntry.ranks[i].statEffectsData[a].statEffectModification =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Value",
                                        "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].statEffectsData[a].statEffectModification);

                                if (currentEntry.ranks[i].statEffectsData[a].statID != -1)
                                {
                                    if (!entryReference.isPercentStat)
                                    {
                                        currentEntry.ranks[i].statEffectsData[a].isPercent =
                                            RPGBuilderEditorFields.DrawHorizontalToggle("Is Percent?",
                                                "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].statEffectsData[a].isPercent);
                                    }
                                    else
                                    {
                                        currentEntry.ranks[i].statEffectsData[a].isPercent = false;
                                    }
                                }

                                GUILayout.Space(10);
                            }

                            break;


                        case RPGEffect.EFFECT_TYPE.Stealth:
                            currentEntry.ranks[i].showStealthActionBar = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Show Stealth Action Bar?", "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].showStealthActionBar);

                            GUILayout.Space(10);
                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Effect", true))
                            {
                                currentEntry.ranks[i].nestedEffects.Add(new RPGAbility.AbilityEffectsApplied());
                            }

                            currentEntry.ranks[i].nestedEffects =
                                RPGBuilderEditorFields.DrawEffectsAppliedList(currentEntry.ranks[i].nestedEffects,
                                    false);
                            GUILayout.Space(10);
                            break;

                        case RPGEffect.EFFECT_TYPE.Mount:

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Visual:", "", false);
                            currentEntry.ranks[i].MountPrefab =
                                (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Mount", "",
                                    currentEntry.ranks[i].MountPrefab);

                            currentEntry.ranks[i].MountPosition = RPGBuilderEditorFields.DrawHorizontalVector3(
                                "Mount Position", "",
                                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].MountPosition);
                            currentEntry.ranks[i].MountScale = RPGBuilderEditorFields.DrawHorizontalVector3(
                                "Mount Scale", "",
                                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].MountScale);

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Animations:", "", true);
                            currentEntry.ranks[i].MountAnimatorController = (RuntimeAnimatorController)
                                RPGBuilderEditorFields.DrawHorizontalObject<RuntimeAnimatorController>(
                                    "Animator Controller", "", currentEntry.ranks[i].MountAnimatorController);

                            currentEntry.ranks[i].MountAvatar = (Avatar)
                                RPGBuilderEditorFields.DrawHorizontalObject<Avatar>("Avatar", "",
                                    currentEntry.ranks[i].MountAvatar);

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Rider Settings:", "", true);
                            currentEntry.ranks[i].ReParentCharacterArmature =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Parent Armature?", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].ReParentCharacterArmature);
                            if (currentEntry.ranks[i].ReParentCharacterArmature)
                            {
                                currentEntry.ranks[i].RiderReParentName =
                                    RPGBuilderEditorFields.DrawHorizontalTextField("Mount Bone Name", "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].RiderReParentName);
                            }

                            currentEntry.ranks[i].RiderPosition = RPGBuilderEditorFields.DrawHorizontalVector3(
                                "Position", "",
                                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].RiderPosition);
                            currentEntry.ranks[i].RiderRotation = RPGBuilderEditorFields.DrawHorizontalVector3(
                                "Rotation", "",
                                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].RiderRotation);

                            currentEntry.ranks[i].MountAnimationParameter =
                                RPGBuilderEditorFields.DrawHorizontalTextField("Mounted Animation Parameter", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].MountAnimationParameter);

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Camera Settings:", "", true);
                            currentEntry.ranks[i].mountCanAim =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Can Camera Aim?", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].mountCanAim);
                            currentEntry.ranks[i].shapeshiftingNoActionAbilities =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Prevent action abilities?", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].shapeshiftingNoActionAbilities);

                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Extras:", "", true);
                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Effect", true))
                            {
                                currentEntry.ranks[i].nestedEffects.Add(new RPGAbility.AbilityEffectsApplied());
                            }

                            currentEntry.ranks[i].nestedEffects =
                                RPGBuilderEditorFields.DrawEffectsAppliedList(currentEntry.ranks[i].nestedEffects,
                                    false);
                            GUILayout.Space(10);

                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat", true))
                            {
                                currentEntry.ranks[i].statEffectsData.Add(new RPGEffect.STAT_EFFECTS_DATA());
                            }

                            for (var a = 0; a < currentEntry.ranks[i].statEffectsData.Count; a++)
                            {
                                GUILayout.Space(10);
                                RPGBuilderEditorUtility.StartHorizontalMargin(
                                    RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                                var requirementNumber = a + 1;
                                EditorGUILayout.BeginHorizontal();
                                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                                {
                                    currentEntry.ranks[i].statEffectsData.RemoveAt(a);
                                    return;
                                }

                                RPGStat entryReference =
                                    (RPGStat) RPGBuilderEditorUtility.GetEntryByID(
                                        currentEntry.ranks[i].statEffectsData[a].statID, "Stat");
                                var statName = "";
                                if (entryReference != null) statName = entryReference.entryName;

                                EditorGUILayout.LabelField("" + requirementNumber + ": " + statName);
                                EditorGUILayout.EndHorizontal();

                                currentEntry.ranks[i].statEffectsData[a].statID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                                        currentEntry.ranks[i].statEffectsData[a].statID,
                                        "Stat", "Stat", "");

                                currentEntry.ranks[i].statEffectsData[a].statEffectModification =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Value",
                                        "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].statEffectsData[a].statEffectModification);

                                if (currentEntry.ranks[i].statEffectsData[a].statID != -1)
                                {
                                    if (!entryReference.isPercentStat)
                                    {
                                        currentEntry.ranks[i].statEffectsData[a].isPercent =
                                            RPGBuilderEditorFields.DrawHorizontalToggle("Is Percent?",
                                                "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].statEffectsData[a].isPercent);
                                    }
                                    else
                                    {
                                        currentEntry.ranks[i].statEffectsData[a].isPercent = false;
                                    }
                                }

                                RPGBuilderEditorUtility.EndHorizontalMargin(
                                    RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                                GUILayout.Space(10);
                            }

                            break;
                        case RPGEffect.EFFECT_TYPE.GameAction:
                            GUILayout.Space(10);
                            currentEntry.ranks[i].UseGameActionsTemplate = 
                                RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].UseGameActionsTemplate);

                            if (currentEntry.ranks[i].UseGameActionsTemplate)
                            {
                                currentEntry.ranks[i].GameActionsTemplate = (GameActionsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<GameActionsTemplate>(
                                    "Template", "", currentEntry.ranks[i].GameActionsTemplate);
                            }
                            else
                            {
                                GUILayout.Space(10);
                                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Game Action", false))
                                {
                                    currentEntry.ranks[i].GameActions.Add(new GameActionsData.GameAction());
                                }

                                currentEntry.ranks[i].GameActions =
                                    RPGBuilderEditorFields.DrawGameActionsList(currentEntry.ranks[i].GameActions,
                                        false);
                            }

                            break;
                    }

                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    GUILayout.Space(10);

                    if (currentEntry.effectType == RPGEffect.EFFECT_TYPE.Sleep
                        || currentEntry.effectType == RPGEffect.EFFECT_TYPE.Root ||
                        currentEntry.effectType == RPGEffect.EFFECT_TYPE.Silence
                        || currentEntry.effectType == RPGEffect.EFFECT_TYPE.Taunt)
                    {
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);
                        RPGBuilderEditorFields.DrawTitleLabel("NOT YET IMPLEMENTED", "");
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showVisualEffects =
                        RPGBuilderEditorUtility.HandleModuleBanner("VISUALS EFFECTS",
                            RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showVisualEffects);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showVisualEffects)
                    {
                        currentEntry.ranks[i].VisualEffectEntries =
                            RPGBuilderEditorFields.DrawVisualEffectsList(currentEntry.ranks[i].VisualEffectEntries, allNodeSockets);
                    }
                    
                    RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showVisualAnimations =
                        RPGBuilderEditorUtility.HandleModuleBanner("VISUAL ANIMATIONS", RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showVisualAnimations);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showVisualAnimations)
                    {
                        currentEntry.ranks[i].AnimationEntries =
                            RPGBuilderEditorFields.DrawAnimationsList(currentEntry.ranks[i].AnimationEntries);
                    }
                    
                    RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showSounds =
                        RPGBuilderEditorUtility.HandleModuleBanner("SOUNDS",
                            RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showSounds);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.effectModuleSection.showSounds)
                    {
                        currentEntry.ranks[i].SoundEntries = RPGBuilderEditorFields.DrawSoundsList(currentEntry.ranks[i].SoundEntries);
                    }
                    
                    GUILayout.Space(30);
                }
                GUILayout.Space(20);
            }
        }

        GUILayout.Space(25);
        EditorGUILayout.EndScrollView();
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGEffect>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
             RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryIcon = entry.icon;
            EditorUtility.SetDirty(entry);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public override void ConvertStringsToTypeEntries()
    {
        var allEntries = Resources.LoadAll<RPGEffect>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        RPGBuilderEditorModule effectTags = RPGBuilderEditorUtility.GetModuleByName("Effect Tags");
        RPGBuilderEditorModule damageTypes = RPGBuilderEditorUtility.GetModuleByName("Damage Types");
        
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);

            {
                
                RPGBuilderDatabaseEntry entryFile = effectTags.GetEntryByName(entry.effectTag);
                if (entryFile != null)
                {
                    entry.EffectTag = (RPGBEffectTag) entryFile;
                }
            }

            foreach (var rank in entry.ranks)
            {
                {
                    RPGBuilderDatabaseEntry entryFile = damageTypes.GetEntryByName(rank.secondaryDamageType);
                    if (entryFile != null)
                    {
                        rank.customDamageType = (RPGBDamageType) entryFile;
                    }
                }

                for (var index = 0; index < rank.blockedDamageTypes.Count; index++)
                {
                    var blockedDmg = rank.blockedDamageTypes[index];
                    RPGBuilderDatabaseEntry entryFile = damageTypes.GetEntryByName(blockedDmg);
                    if (entryFile != null)
                    {
                        rank.blockedCustomDamageTypes[index] = (RPGBDamageType) entryFile;
                    }
                }

                {
                    RPGBuilderDatabaseEntry entryFile = effectTags.GetEntryByName(rank.dispelEffectTag);
                    if (entryFile != null)
                    {
                        rank.DispelEffectTag = (RPGBEffectTag) entryFile;
                    }
                }
            }

            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private int GetIndexFromCustomDamageTypes(string customDamageType)
    {
        var list = GetCustomDamageTypes();
        for (var index = 0; index < list.Count; index++)
        {
            if (list[index] != customDamageType) continue;
            return index;
        }

        return -1;
    }
    
    private List<string> GetCustomDamageTypes()
    {
        List<string> allCustomTypes = new List<string>();

        foreach (var statFunction in RPGBuilderEditor.Instance.CharacterSettings.StatFunctionsList)
        {
            if (statFunction == "Neutral" || statFunction == "Physical" || statFunction == "Magical"
                || statFunction == "PhysicalResistance" || statFunction == "MagicalResistance") continue;
            allCustomTypes.Add(statFunction);
        }

        return allCustomTypes;
    }
}
