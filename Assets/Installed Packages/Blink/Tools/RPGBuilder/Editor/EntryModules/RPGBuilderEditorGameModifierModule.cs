using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorGameModifierModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGGameModifier> entries = new Dictionary<int, RPGGameModifier>();
    private RPGGameModifier currentEntry;
    
    private readonly List<RPGBuilderDatabaseEntry> allWeaponTypes = new List<RPGBuilderDatabaseEntry>();

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

        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.gameModifierFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGGameModifier> dictionary = new Dictionary<int, RPGGameModifier>();
        databaseEntries.Clear();
        allWeaponTypes.Clear();
        var allEntries =
            Resources.LoadAll<RPGGameModifier>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;

        foreach (var typeEntry in Resources.LoadAll<RPGBWeaponType>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allWeaponTypes.Add(typeEntry);
        }
    }



    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }

        currentEntry = CreateInstance<RPGGameModifier>();
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
        
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGGameModifier entryFile = (RPGGameModifier) updatedEntry;
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

        ScriptableObject scriptableObj = currentEntry;
        var serialObj = new SerializedObject(scriptableObj);

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);
        
        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.MaxWidth(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.ExpandHeight(true));

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.gameModifierModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.gameModifierModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.gameModifierModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon = RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            currentEntry.entryDescription = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDescription);
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.gameModifierModuleSection.showSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("SETTINGS",
                RPGBuilderEditor.Instance.EditorFilters.gameModifierModuleSection.showSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.gameModifierModuleSection.showSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.unlockType =
                (RPGGameModifier.GameModifierUnlockType) EditorGUILayout.EnumPopup("Available in",
                    currentEntry.unlockType);
            if (currentEntry.unlockType != RPGGameModifier.GameModifierUnlockType.World)
            {
                currentEntry.gameModifierType =
                    (RPGGameModifier.GameModifierType) EditorGUILayout.EnumPopup("Type",
                        currentEntry.gameModifierType);
                if (currentEntry.gameModifierType == RPGGameModifier.GameModifierType.Positive)
                {
                    currentEntry.cost =
                        RPGBuilderEditorFields.DrawHorizontalIntField("Cost", "",
                            RPGBuilderEditor.Instance.FieldHeight, currentEntry.cost);
                }
                else
                {
                    currentEntry.gain =
                        RPGBuilderEditorFields.DrawHorizontalIntField("Gain", "",
                            RPGBuilderEditor.Instance.FieldHeight, currentEntry.gain);
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.gameModifierModuleSection.showModifiers =
            RPGBuilderEditorUtility.HandleModuleBanner("MODIFIERS",
                RPGBuilderEditor.Instance.EditorFilters.gameModifierModuleSection.showModifiers);
        if (RPGBuilderEditor.Instance.EditorFilters.gameModifierModuleSection.showModifiers)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);
            if (GUILayout.Button("+ Add Modifier", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.MinWidth(150),
                GUILayout.ExpandWidth(true)))
            {
                currentEntry.gameModifiersList.Add(new RPGGameModifier.GameModifierDATA());
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);

            GUILayout.Space(10);

            var ThisList2 = serialObj.FindProperty("gameModifiersList");
            currentEntry.gameModifiersList =
                RPGBuilderEditor.Instance
                    .GetTargetObjectOfProperty(ThisList2) as List<RPGGameModifier.GameModifierDATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.gameModifiersList.Count; a++)
            {
                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
                var rankNbr = a + 1;

                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.gameModifiersList.RemoveAt(a);
                    return;
                }

                GUILayout.Space(8);

                if (GUILayout.Button("Modifier: " + rankNbr,
                    RPGBuilderEditor.Instance.EditorSkin.GetStyle("GenericButton"),
                    GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    currentEntry.gameModifiersList[a].showModifier =
                        !currentEntry.gameModifiersList[a].showModifier;
                    GUI.FocusControl(null);
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

                if (currentEntry.gameModifiersList[a].showModifier)
                {
                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    GUILayout.Space(10);
                    var requirementNumber = a + 1;
                    var effectName = currentEntry.gameModifiersList[a].categoryType;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName);

                    currentEntry.gameModifiersList[a].categoryType =
                        (RPGGameModifier.CategoryType) EditorGUILayout.EnumPopup("Category",
                            currentEntry.gameModifiersList[a].categoryType);
                    switch (currentEntry.gameModifiersList[a].categoryType)
                    {
                        case RPGGameModifier.CategoryType.Combat:
                            currentEntry.gameModifiersList[a].combatModuleType =
                                (RPGGameModifier.CombatModuleType) EditorGUILayout.EnumPopup("Module",
                                    currentEntry.gameModifiersList[a].combatModuleType);

                            if (currentEntry.gameModifiersList[a].combatModuleType !=
                                RPGGameModifier.CombatModuleType.Stat)
                            {
                                currentEntry.gameModifiersList[a].isGlobal =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Is Global?",
                                        "Is this game modifier affecting all entries of this type?", 20,
                                        currentEntry.gameModifiersList[a].isGlobal);
                                if (!currentEntry.gameModifiersList[a].isGlobal)
                                {
                                    currentEntry.gameModifiersList[a].showEntryList =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Show " +
                                            currentEntry.gameModifiersList[a].combatModuleType +
                                            " List",
                                            "", 20,
                                            currentEntry.gameModifiersList[a].showEntryList);
                                }

                                if (!currentEntry.gameModifiersList[a].isGlobal &&
                                    currentEntry.gameModifiersList[a].showEntryList)
                                {
                                    DrawGameModifierIDsAndList(
                                        currentEntry.gameModifiersList[a].entryIDs,
                                        currentEntry.gameModifiersList[a].combatModuleType.ToString());
                                    GUILayout.Space(10);
                                }
                            }


                            switch (currentEntry.gameModifiersList[a].combatModuleType)
                            {
                                case RPGGameModifier.CombatModuleType.Ability:
                                    currentEntry.gameModifiersList[a].abilityModifierType =
                                        (RPGGameModifier.AbilityModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].abilityModifierType);

                                    switch (currentEntry.gameModifiersList[a].abilityModifierType)
                                    {
                                        case RPGGameModifier.AbilityModifierType.Unlock_Cost:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false,
                                                currentEntry.gameModifiersList[a].combatModuleType
                                                    .ToString(),
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.AbilityModifierType.No_Use_Requirement:
                                        case RPGGameModifier.AbilityModifierType.No_Effect_Requirement:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].combatModuleType + "+" +
                                        currentEntry.gameModifiersList[a].abilityModifierType;

                                    break;
                                case RPGGameModifier.CombatModuleType.Effect:
                                    currentEntry.gameModifiersList[a].modifierTypeName = "";
                                    break;
                                case RPGGameModifier.CombatModuleType.NPC:
                                    currentEntry.gameModifiersList[a].npcModifierType =
                                        (RPGGameModifier.NPCModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].npcModifierType);

                                    switch (currentEntry.gameModifiersList[a].npcModifierType)
                                    {
                                        case RPGGameModifier.NPCModifierType.Exp:
                                        case RPGGameModifier.NPCModifierType.Aggro_Range:
                                        case RPGGameModifier.NPCModifierType.Faction_Reward:
                                        case RPGGameModifier.NPCModifierType.Level:
                                        case RPGGameModifier.NPCModifierType.Respawn_Time:
                                        case RPGGameModifier.NPCModifierType.Roam_Range:
                                        case RPGGameModifier.NPCModifierType.Loot_Table_Chance:
                                        case RPGGameModifier.NPCModifierType.Reset_Target_Distance:
                                            DrawGameModifierModuleValue(
                                                currentEntry.gameModifiersList[a].amountModifier, false,
                                                false,
                                                currentEntry.gameModifiersList[a].combatModuleType
                                                    .ToString(),
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.NPCModifierType.Faction:
                                            DrawGameModifierModuleValue(
                                                currentEntry.gameModifiersList[a].amountModifier, true,
                                                false,
                                                "Faction", false, false, false);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].combatModuleType + "+" +
                                        currentEntry.gameModifiersList[a].npcModifierType;

                                    break;
                                case RPGGameModifier.CombatModuleType.Stat:
                                    currentEntry.gameModifiersList[a].statModifierType =
                                        (RPGGameModifier.StatModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].statModifierType);

                                    switch (currentEntry.gameModifiersList[a].statModifierType)
                                    {
                                        case RPGGameModifier.StatModifierType.Settings:
                                            GUILayout.Space(10);
                                            RPGBuilderEditorUtility.StartHorizontalMargin(35, false);
                                            if (GUILayout.Button("+ Add",
                                                RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                                                GUILayout.MinWidth(150), GUILayout.ExpandWidth(true)))
                                            {
                                                currentEntry.gameModifiersList[a].statModifierData
                                                    .Add(new RPGGameModifier.StatDataModifier());
                                            }

                                            RPGBuilderEditorUtility.EndHorizontalMargin(35, false);
                                            GUILayout.Space(10);

                                            foreach (var stat in currentEntry.gameModifiersList[a].statModifierData)
                                            {
                                                EditorGUILayout.BeginHorizontal();
                                                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                                                {
                                                    currentEntry.gameModifiersList[a]
                                                        .statModifierData.Remove(stat);
                                                    return;
                                                }

                                                RPGStat entryRef = (RPGStat)
                                                    RPGBuilderEditorUtility.GetEntryByID(stat.statID, "Stat");
                                                RPGStat tempRef = (RPGStat) EditorGUILayout.ObjectField(entryRef,
                                                    typeof(RPGStat), false, GUILayout.Height(20));
                                                stat.statID = tempRef != null ? tempRef.ID : -1;
                                                EditorGUILayout.EndHorizontal();
                                                GUILayout.Space(5);

                                                stat.dataModifierType =
                                                    (RPGGameModifier.DataModifierType) EditorGUILayout.EnumPopup(
                                                        "Modifier Type", stat.dataModifierType);
                                                stat.unitType =
                                                    (RPGGameModifier.UnitType) EditorGUILayout.EnumPopup(
                                                        "Unit Type", stat.unitType);

                                                if (stat.unitType == RPGGameModifier.UnitType.All ||
                                                    stat.unitType == RPGGameModifier.UnitType.NPC)
                                                {
                                                    currentEntry.gameModifiersList[a].isGlobal =
                                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                                            "Is Global?",
                                                            "Is this game modifier affecting all entries of this type?",
                                                            20,
                                                            currentEntry.gameModifiersList[a].isGlobal);
                                                    if (!currentEntry.gameModifiersList[a].isGlobal)
                                                    {
                                                        currentEntry.gameModifiersList[a].showEntryList =
                                                            RPGBuilderEditorFields.DrawHorizontalToggle(
                                                                "Show NPC List",
                                                                "", 20,
                                                                currentEntry.gameModifiersList[a].showEntryList);
                                                    }

                                                    if (currentEntry.gameModifiersList[a].showEntryList)
                                                    {
                                                        DrawGameModifierIDsAndList(
                                                            currentEntry.gameModifiersList[a].entryIDs, "NPC");
                                                    }

                                                }

                                                stat.checkMin = RPGBuilderEditorFields.DrawHorizontalToggle(
                                                    "Check. Min", "", RPGBuilderEditor.Instance.FieldHeight,
                                                    stat.checkMin);
                                                if (stat.checkMin)
                                                {
                                                    stat.valueMin = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                        "Value. Min", "",
                                                        RPGBuilderEditor.Instance.FieldHeight,
                                                        stat.valueMin);
                                                }

                                                stat.checkMax = RPGBuilderEditorFields.DrawHorizontalToggle(
                                                    "Check. Max", "", RPGBuilderEditor.Instance.FieldHeight,
                                                    stat.checkMax);
                                                if (stat.checkMax)
                                                {
                                                    stat.valueMax = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                        "Value. Max", "",
                                                        RPGBuilderEditor.Instance.FieldHeight,
                                                        stat.valueMax);
                                                }

                                                stat.valueDefault = RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                    "Default Value", "",
                                                    RPGBuilderEditor.Instance.FieldHeight,
                                                    stat.valueDefault);

                                                if (tempRef != null && tempRef.isVitalityStat)
                                                {
                                                    stat.restShifting =
                                                        RPGBuilderEditorFields.DrawHorizontalToggle("Rest Shifting?",
                                                            "Is this stat shifting while outside of combat?",
                                                            RPGBuilderEditor.Instance.FieldHeight,
                                                            stat.restShifting);
                                                    if (stat.restShifting)
                                                    {
                                                        stat.restShiftAmount =
                                                            RPGBuilderEditorFields.DrawHorizontalFloatField("Amount",
                                                                "The amount that will be shifted",
                                                                RPGBuilderEditor.Instance.FieldHeight,
                                                                stat.restShiftAmount);
                                                        stat.restShiftInterval =
                                                            RPGBuilderEditorFields.DrawHorizontalFloatField("Interval",
                                                                "The duration between each shift",
                                                                RPGBuilderEditor.Instance.FieldHeight,
                                                                stat.restShiftInterval);
                                                    }

                                                    stat.CombatShifting =
                                                        RPGBuilderEditorFields.DrawHorizontalToggle("Combat Shifting?",
                                                            "Is this stat shifting while in combat?",
                                                            RPGBuilderEditor.Instance.FieldHeight,
                                                            stat.CombatShifting);
                                                    if (stat.CombatShifting)
                                                    {
                                                        stat.combatShiftAmount =
                                                            RPGBuilderEditorFields.DrawHorizontalFloatField("Amount",
                                                                "The amount that will be shifted",
                                                                RPGBuilderEditor.Instance.FieldHeight,
                                                                stat.combatShiftAmount);
                                                        stat.combatShiftInterval =
                                                            RPGBuilderEditorFields.DrawHorizontalFloatField("Interval",
                                                                "The duration between each shift",
                                                                RPGBuilderEditor.Instance.FieldHeight,
                                                                stat.combatShiftInterval);
                                                    }
                                                }

                                                GUILayout.Space(15);
                                            }

                                            break;
                                        case RPGGameModifier.StatModifierType.MinOverride:
                                        case RPGGameModifier.StatModifierType.MaxOverride:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                "Stat",
                                                false, true, false);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].combatModuleType + "+" +
                                        currentEntry.gameModifiersList[a].statModifierType;

                                    break;
                                case RPGGameModifier.CombatModuleType.TreePoint:
                                    currentEntry.gameModifiersList[a].treePointModifierType =
                                        (RPGGameModifier.PointModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].treePointModifierType);

                                    switch (currentEntry.gameModifiersList[a].treePointModifierType)
                                    {
                                        case RPGGameModifier.PointModifierType.Max:
                                        case RPGGameModifier.PointModifierType.Gain_Value:
                                        case RPGGameModifier.PointModifierType.Start_At:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false,
                                                currentEntry.gameModifiersList[a].combatModuleType
                                                    .ToString(),
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].combatModuleType + "+" +
                                        currentEntry.gameModifiersList[a].treePointModifierType;

                                    break;
                                case RPGGameModifier.CombatModuleType.Spellbook:
                                    currentEntry.gameModifiersList[a].spellbookModifierType =
                                        (RPGGameModifier.SpellbookModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].spellbookModifierType);

                                    switch (currentEntry.gameModifiersList[a].spellbookModifierType)
                                    {
                                        case RPGGameModifier.SpellbookModifierType.Ability_Level_Required:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                "Ability",
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.SpellbookModifierType.Bonus_Level_Required:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                "Bonus",
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].combatModuleType + "+" +
                                        currentEntry.gameModifiersList[a].spellbookModifierType;

                                    break;
                                case RPGGameModifier.CombatModuleType.Faction:
                                    currentEntry.gameModifiersList[a].factionModifierType =
                                        (RPGGameModifier.FactionModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].factionModifierType);

                                    switch (currentEntry.gameModifiersList[a].factionModifierType)
                                    {
                                        case RPGGameModifier.FactionModifierType.Interaction_Start_Point:
                                        case RPGGameModifier.FactionModifierType.Stance_Point_Required:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                currentEntry.gameModifiersList[a].combatModuleType
                                                    .ToString(),
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].combatModuleType + "+" +
                                        currentEntry.gameModifiersList[a].factionModifierType;

                                    break;
                                case RPGGameModifier.CombatModuleType.WeaponTemplate:
                                    currentEntry.gameModifiersList[a].weaponTemplateModifierType =
                                        (RPGGameModifier.WeaponTemplateModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a]
                                                .weaponTemplateModifierType);

                                    switch (currentEntry.gameModifiersList[a].weaponTemplateModifierType)
                                    {
                                        case RPGGameModifier.WeaponTemplateModifierType.Exp_Mod:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, true,
                                                "Weapon Type",
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.WeaponTemplateModifierType.No_Starting_Items:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                        case RPGGameModifier.WeaponTemplateModifierType.Stat_Amount:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                "Stat",
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].combatModuleType + "+" +
                                        currentEntry.gameModifiersList[a].weaponTemplateModifierType;

                                    break;
                            }

                            break;
                        case RPGGameModifier.CategoryType.General:
                            currentEntry.gameModifiersList[a].generalModuleType =
                                (RPGGameModifier.GeneralModuleType) EditorGUILayout.EnumPopup("Module",
                                    currentEntry.gameModifiersList[a].generalModuleType);

                            currentEntry.gameModifiersList[a].isGlobal = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Is Global?",
                                "Is this game modifier affecting all entries of this type?", 20,
                                currentEntry.gameModifiersList[a].isGlobal);
                            if (!currentEntry.gameModifiersList[a].isGlobal)
                            {
                                currentEntry.gameModifiersList[a].showEntryList =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Show " +
                                        currentEntry.gameModifiersList[a].generalModuleType +
                                        " List",
                                        "", 20,
                                        currentEntry.gameModifiersList[a].showEntryList);
                            }

                            if (!currentEntry.gameModifiersList[a].isGlobal &&
                                currentEntry.gameModifiersList[a].showEntryList)
                            {
                                DrawGameModifierIDsAndList(
                                    currentEntry.gameModifiersList[a].entryIDs,
                                    currentEntry.gameModifiersList[a].generalModuleType.ToString());
                                GUILayout.Space(10);
                            }

                            switch (currentEntry.gameModifiersList[a].generalModuleType)
                            {
                                case RPGGameModifier.GeneralModuleType.Item:
                                    currentEntry.gameModifiersList[a].itemModifierType =
                                        (RPGGameModifier.ItemModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].itemModifierType);

                                    switch (currentEntry.gameModifiersList[a].itemModifierType)
                                    {
                                        case RPGGameModifier.ItemModifierType.Attack_Speed:
                                        case RPGGameModifier.ItemModifierType.Max_Damage:
                                        case RPGGameModifier.ItemModifierType.Min_Damage:
                                        case RPGGameModifier.ItemModifierType.Sell_Price:
                                        case RPGGameModifier.ItemModifierType.Stack_Amount:
                                        case RPGGameModifier.ItemModifierType.Gem_Bonus_Amount:
                                        case RPGGameModifier.ItemModifierType.Random_Stat_Max:
                                        case RPGGameModifier.ItemModifierType.Random_Stat_Min:
                                        case RPGGameModifier.ItemModifierType.Max_Random_Stat_Amount:
                                        case RPGGameModifier.ItemModifierType.Random_Stats_Chance:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false,
                                                currentEntry.gameModifiersList[a].combatModuleType
                                                    .ToString(),
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.ItemModifierType.No_Requirement:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                        case RPGGameModifier.ItemModifierType.Stat_Amount:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                "Stat", true, true, true);
                                            break;
                                        case RPGGameModifier.ItemModifierType.Overriden_Auto_Attack:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                "Ability", true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].itemModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.Skill:
                                    currentEntry.gameModifiersList[a].skillModifierType =
                                        (RPGGameModifier.SkillModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].skillModifierType);

                                    switch (currentEntry.gameModifiersList[a].skillModifierType)
                                    {
                                        case RPGGameModifier.SkillModifierType.Alloc_Points:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false,
                                                currentEntry.gameModifiersList[a].combatModuleType
                                                    .ToString(),
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.SkillModifierType.No_Starting_Items:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                        case RPGGameModifier.SkillModifierType.Stat_Amount:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                "Stat", true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].skillModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.LevelTemplate:
                                    currentEntry.gameModifiersList[a].levelTemplateModifierType =
                                        (RPGGameModifier.LevelTemplateModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].levelTemplateModifierType);

                                    switch (currentEntry.gameModifiersList[a].levelTemplateModifierType)
                                    {
                                        case RPGGameModifier.LevelTemplateModifierType.MaxEXPToLevel:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].levelTemplateModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.Race:
                                    currentEntry.gameModifiersList[a].raceModifierType =
                                        (RPGGameModifier.RaceModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].raceModifierType);

                                    switch (currentEntry.gameModifiersList[a].raceModifierType)
                                    {
                                        case RPGGameModifier.RaceModifierType.Faction:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false, "Faction",
                                                false, false, false);
                                            break;
                                        case RPGGameModifier.RaceModifierType.Male_Prefab:
                                            currentEntry.gameModifiersList[a].raceOverridenMalePrefab =
                                                (GameObject) EditorGUILayout.ObjectField(
                                                    new GUIContent("Prefab", ""),
                                                    currentEntry.gameModifiersList[a]
                                                        .raceOverridenMalePrefab,
                                                    typeof(GameObject), false);
                                            break;
                                        case RPGGameModifier.RaceModifierType.Female_Prefab:
                                            currentEntry.gameModifiersList[a].raceOverridenFemalePrefab =
                                                (GameObject) EditorGUILayout.ObjectField(
                                                    new GUIContent("Prefab", ""),
                                                    currentEntry.gameModifiersList[a]
                                                        .raceOverridenFemalePrefab,
                                                    typeof(GameObject), false);
                                            break;
                                        case RPGGameModifier.RaceModifierType.Start_Scene:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false, "GameScene",
                                                false, false, false);
                                            break;
                                        case RPGGameModifier.RaceModifierType.Start_Position:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false, "WorldPosition",
                                                false, false, false);
                                            break;
                                        case RPGGameModifier.RaceModifierType.Stat_Amount:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false, "Stat",
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.RaceModifierType.No_Starting_Items:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                        case RPGGameModifier.RaceModifierType.Alloc_Points:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].raceModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.Class:
                                    currentEntry.gameModifiersList[a].classModifierType =
                                        (RPGGameModifier.ClassModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].classModifierType);

                                    switch (currentEntry.gameModifiersList[a].classModifierType)
                                    {
                                        case RPGGameModifier.ClassModifierType.Alloc_Points:
                                        case RPGGameModifier.ClassModifierType.Alloc_Points_Menu:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false,
                                                currentEntry.gameModifiersList[a].combatModuleType
                                                    .ToString(),
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.ClassModifierType.No_Starting_Items:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                        case RPGGameModifier.ClassModifierType.Stat_Amount:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                "Stat", true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].classModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.TalentTree:
                                    currentEntry.gameModifiersList[a].modifierTypeName = "";
                                    break;
                                case RPGGameModifier.GeneralModuleType.LootTable:
                                    currentEntry.gameModifiersList[a].modifierTypeName = "";
                                    break;
                                case RPGGameModifier.GeneralModuleType.MerchantTable:
                                    currentEntry.gameModifiersList[a].merchantTableModifierType =
                                        (RPGGameModifier.MerchantTableModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].merchantTableModifierType);

                                    switch (currentEntry.gameModifiersList[a].merchantTableModifierType)
                                    {
                                        case RPGGameModifier.MerchantTableModifierType.Cost:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.MerchantTableModifierType.Currency:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false,
                                                "Currency", false, false, false);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].merchantTableModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.Currency:
                                    currentEntry.gameModifiersList[a].currencyModifierType =
                                        (RPGGameModifier.CurrencyModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].currencyModifierType);

                                    switch (currentEntry.gameModifiersList[a].currencyModifierType)
                                    {
                                        case RPGGameModifier.CurrencyModifierType.Max:
                                        case RPGGameModifier.CurrencyModifierType.Min:
                                        case RPGGameModifier.CurrencyModifierType.Start_At:
                                        case RPGGameModifier.CurrencyModifierType.Amount_For_Convertion:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].currencyModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.CraftingRecipe:
                                    currentEntry.gameModifiersList[a].recipeModifierType =
                                        (RPGGameModifier.RecipeModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].recipeModifierType);

                                    switch (currentEntry.gameModifiersList[a].recipeModifierType)
                                    {
                                        case RPGGameModifier.RecipeModifierType.Crafted_Chance:
                                        case RPGGameModifier.RecipeModifierType.Crafted_Count:
                                        case RPGGameModifier.RecipeModifierType.Unlock_Cost:
                                        case RPGGameModifier.RecipeModifierType.Component_Required_Count:
                                        case RPGGameModifier.RecipeModifierType.EXP:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].recipeModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.CraftingStation:
                                    currentEntry.gameModifiersList[a].modifierTypeName = "";
                                    break;
                                case RPGGameModifier.GeneralModuleType.Bonus:
                                    currentEntry.gameModifiersList[a].bonusModifierType =
                                        (RPGGameModifier.BonusModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].bonusModifierType);

                                    switch (currentEntry.gameModifiersList[a].bonusModifierType)
                                    {
                                        case RPGGameModifier.BonusModifierType.Unlock_Cost:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.BonusModifierType.No_Requirement:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].bonusModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.GearSet:
                                    currentEntry.gameModifiersList[a].gearSetModifierType =
                                        (RPGGameModifier.GearSetModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].gearSetModifierType);

                                    switch (currentEntry.gameModifiersList[a].gearSetModifierType)
                                    {
                                        case RPGGameModifier.GearSetModifierType.Equipped_Amount:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.GearSetModifierType.Stat_Bonuses:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false, "Stat",
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].gearSetModifierType;

                                    break;
                                case RPGGameModifier.GeneralModuleType.Enchantment:
                                    currentEntry.gameModifiersList[a].enchantmentModifierType =
                                        (RPGGameModifier.EnchantmentModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].enchantmentModifierType);

                                    switch (currentEntry.gameModifiersList[a].enchantmentModifierType)
                                    {
                                        case RPGGameModifier.EnchantmentModifierType.No_Requirement:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                        case RPGGameModifier.EnchantmentModifierType.Price:
                                        case RPGGameModifier.EnchantmentModifierType.Time:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].generalModuleType + "+" +
                                        currentEntry.gameModifiersList[a].enchantmentModifierType;

                                    break;
                            }

                            break;
                        case RPGGameModifier.CategoryType.World:
                            currentEntry.gameModifiersList[a].worldModuleType =
                                (RPGGameModifier.WorldModuleType) EditorGUILayout.EnumPopup("Module",
                                    currentEntry.gameModifiersList[a].worldModuleType);

                            currentEntry.gameModifiersList[a].isGlobal = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Is Global?",
                                "Is this game modifier affecting all entries of this type?", 20,
                                currentEntry.gameModifiersList[a].isGlobal);
                            if (!currentEntry.gameModifiersList[a].isGlobal)
                            {
                                currentEntry.gameModifiersList[a].showEntryList =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Show " +
                                        currentEntry.gameModifiersList[a].worldModuleType +
                                        " List",
                                        "", 20,
                                        currentEntry.gameModifiersList[a].showEntryList);
                            }

                            if (!currentEntry.gameModifiersList[a].isGlobal &&
                                currentEntry.gameModifiersList[a].showEntryList)
                            {
                                DrawGameModifierIDsAndList(
                                    currentEntry.gameModifiersList[a].entryIDs,
                                    currentEntry.gameModifiersList[a].worldModuleType.ToString());
                                GUILayout.Space(10);
                            }

                            switch (currentEntry.gameModifiersList[a].worldModuleType)
                            {
                                case RPGGameModifier.WorldModuleType.Task:
                                    currentEntry.gameModifiersList[a].modifierTypeName = "";
                                    break;
                                case RPGGameModifier.WorldModuleType.Quest:
                                    currentEntry.gameModifiersList[a].questModifierType =
                                        (RPGGameModifier.QuestModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].questModifierType);

                                    switch (currentEntry.gameModifiersList[a].questModifierType)
                                    {
                                        case RPGGameModifier.QuestModifierType.No_Requirement:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].worldModuleType + "+" +
                                        currentEntry.gameModifiersList[a].questModifierType;

                                    break;
                                case RPGGameModifier.WorldModuleType.WorldPosition:
                                    currentEntry.gameModifiersList[a].modifierTypeName = "";
                                    break;
                                case RPGGameModifier.WorldModuleType.ResourceNode:
                                    currentEntry.gameModifiersList[a].resourceNodeModifierType =
                                        (RPGGameModifier.ResourceNodeModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].resourceNodeModifierType);

                                    switch (currentEntry.gameModifiersList[a].resourceNodeModifierType)
                                    {
                                        case RPGGameModifier.ResourceNodeModifierType.Gather_Time:
                                        case RPGGameModifier.ResourceNodeModifierType.Level_Required:
                                        case RPGGameModifier.ResourceNodeModifierType.Respawn_Time:
                                        case RPGGameModifier.ResourceNodeModifierType.Unlock_Cost:
                                        case RPGGameModifier.ResourceNodeModifierType.EXP:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].worldModuleType + "+" +
                                        currentEntry.gameModifiersList[a].resourceNodeModifierType;

                                    break;
                                case RPGGameModifier.WorldModuleType.GameScene:
                                    currentEntry.gameModifiersList[a].modifierTypeName = "";
                                    break;
                                case RPGGameModifier.WorldModuleType.Dialogue:
                                    currentEntry.gameModifiersList[a].modifierTypeName = "";
                                    break;
                            }

                            break;
                        case RPGGameModifier.CategoryType.Settings:
                            currentEntry.gameModifiersList[a].settingsModuleType =
                                (RPGGameModifier.SettingsModuleType) EditorGUILayout.EnumPopup("Module",
                                    currentEntry.gameModifiersList[a].settingsModuleType);

                            currentEntry.gameModifiersList[a].isGlobal = RPGBuilderEditorFields.DrawHorizontalToggle(
                                "Is Global?",
                                "Is this game modifier affecting all entries of this type?", 20,
                                currentEntry.gameModifiersList[a].isGlobal);
                            if (!currentEntry.gameModifiersList[a].isGlobal)
                            {
                                currentEntry.gameModifiersList[a].showEntryList =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Show " +
                                        currentEntry.gameModifiersList[a].settingsModuleType +
                                        " List",
                                        "", 20,
                                        currentEntry.gameModifiersList[a].showEntryList);
                            }

                            if (!currentEntry.gameModifiersList[a].isGlobal &&
                                currentEntry.gameModifiersList[a].showEntryList)
                            {
                                DrawGameModifierIDsAndList(
                                    currentEntry.gameModifiersList[a].entryIDs,
                                    currentEntry.gameModifiersList[a].settingsModuleType.ToString());
                                GUILayout.Space(10);
                            }

                            switch (currentEntry.gameModifiersList[a].settingsModuleType)
                            {
                                case RPGGameModifier.SettingsModuleType.GeneralSettings:
                                    currentEntry.gameModifiersList[a].generalSettingModifierType =
                                        (RPGGameModifier.GeneralSettingModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a]
                                                .generalSettingModifierType);

                                    switch (currentEntry.gameModifiersList[a].generalSettingModifierType)
                                    {
                                        case RPGGameModifier.GeneralSettingModifierType.No_Auto_Save:
                                            currentEntry.gameModifiersList[a].boolValue = true;
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].settingsModuleType + "+" +
                                        currentEntry.gameModifiersList[a].generalSettingModifierType;

                                    break;
                                case RPGGameModifier.SettingsModuleType.CombatSettings:
                                    currentEntry.gameModifiersList[a].combatSettingModifierType =
                                        (RPGGameModifier.CombatSettingModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].combatSettingModifierType);

                                    switch (currentEntry.gameModifiersList[a].combatSettingModifierType)
                                    {
                                        case RPGGameModifier.CombatSettingModifierType.Critical_Bonus:
                                        case RPGGameModifier.CombatSettingModifierType.Action_Bar_Slots:
                                        case RPGGameModifier.CombatSettingModifierType.Combat_Reset_Timer:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                        case RPGGameModifier.CombatSettingModifierType.Health_Stat:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false, "Stat",
                                                false, false, false);
                                            break;
                                        case RPGGameModifier.CombatSettingModifierType.Alloc_Tree_Point:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, true, false, "TreePoint",
                                                false, false, false);
                                            break;
                                        case RPGGameModifier.CombatSettingModifierType.Can_Decrease_Alloc_Point:
                                            currentEntry.gameModifiersList[a].boolValue
                                                = RPGBuilderEditorFields.DrawHorizontalToggle("Can Decrease", "",
                                                    RPGBuilderEditor.Instance.FieldHeight,
                                                    currentEntry.gameModifiersList[a]
                                                        .boolValue);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].settingsModuleType + "+" +
                                        currentEntry.gameModifiersList[a].combatSettingModifierType;

                                    break;
                                case RPGGameModifier.SettingsModuleType.SceneSettings:
                                    currentEntry.gameModifiersList[a].worldSettingModifierType =
                                        (RPGGameModifier.WorldSettingModifierType) EditorGUILayout.EnumPopup("Type",
                                            currentEntry.gameModifiersList[a].worldSettingModifierType);

                                    switch (currentEntry.gameModifiersList[a].worldSettingModifierType)
                                    {
                                        case RPGGameModifier.WorldSettingModifierType.Game_Audio:
                                        case RPGGameModifier.WorldSettingModifierType.Light_Intensity:
                                        case RPGGameModifier.WorldSettingModifierType.Camera_FOV:
                                            DrawGameModifierModuleValue(currentEntry
                                                    .gameModifiersList[a]
                                                    .amountModifier, false, false, "",
                                                true, true, true);
                                            break;
                                    }

                                    currentEntry.gameModifiersList[a].modifierTypeName =
                                        currentEntry.gameModifiersList[a].categoryType + "+" +
                                        currentEntry.gameModifiersList[a].settingsModuleType + "+" +
                                        currentEntry.gameModifiersList[a].worldSettingModifierType;

                                    break;
                            }

                            break;
                    }

                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                }

                GUILayout.Space(10);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGGameModifier>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
             RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryIcon = entry.icon;
            entry.entryDescription = entry.description;
            EditorUtility.SetDirty(entry);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private void DrawGameModifierModuleValue(RPGGameModifier.ModuleAmountModifier moduleAmountModifier,
        bool showEntryID, bool showEntryName, string assetTypeName, bool showModifierType, bool showAmount,
        bool showIsPercent)
    {
        if (showModifierType)
        {
            moduleAmountModifier.dataModifierType =
                (RPGGameModifier.DataModifierType) EditorGUILayout.EnumPopup("Modifier Type",
                    moduleAmountModifier.dataModifierType);
        }

        if (showEntryName)
        {
            if (assetTypeName == "Weapon Type")
            {
                
            }
            else
            {
                moduleAmountModifier.entryName = RPGBuilderEditorFields.DrawHorizontalTextField(assetTypeName, "",
                    RPGBuilderEditor.Instance.FieldHeight, moduleAmountModifier.entryName);
            }
        }

        if (showEntryID)
        {
            moduleAmountModifier.entryID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(moduleAmountModifier.entryID, assetTypeName,
                    assetTypeName, "");
        }
        else
        {
            moduleAmountModifier.entryID = -1;
        }

        if (showAmount)
        {
            moduleAmountModifier.alterAmount =
                RPGBuilderEditorFields.DrawHorizontalFloatField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                    moduleAmountModifier.alterAmount);
        }

        if (moduleAmountModifier.dataModifierType != RPGGameModifier.DataModifierType.Override && showIsPercent)
        {
            moduleAmountModifier.isPercent =
                RPGBuilderEditorFields.DrawHorizontalToggle("Is Percent?", "", RPGBuilderEditor.Instance.FieldHeight,
                    moduleAmountModifier.isPercent);
        }
    }
    
    private void DrawGameModifierIDsAndList(List<int> entryIDs, string assetTypeName)
    {
        if (assetTypeName != "Stat") DrawAddEntryIDButton(assetTypeName, entryIDs);
        GUILayout.Space(5);
        DrawGameModifierEntryList(entryIDs, assetTypeName);
    }

    private void DrawGameModifierEntryList(List<int> entryIDs, string assetTypeName)
    {
        for (var i = 0; i < entryIDs.Count; i++)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (RPGBuilderEditorFields.DrawSmallRemoveButton())
            {
                entryIDs.RemoveAt(i);
                return;
            }

            entryIDs[i] = RPGBuilderEditorFields.DrawDatabaseEntryField(entryIDs[i], assetTypeName, assetTypeName, "");
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawAddEntryIDButton(string buttonName, List<int> intList)
    {
        RPGBuilderEditorUtility.StartHorizontalMargin(60, false);
        if (GUILayout.Button("+ Add " + buttonName, RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(150),
            GUILayout.ExpandWidth(true)))
        {
            intList.Add(-1);
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(60, false);
    }

}
