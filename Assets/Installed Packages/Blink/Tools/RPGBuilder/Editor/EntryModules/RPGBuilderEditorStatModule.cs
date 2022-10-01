using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorStatModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGStat> entries = new Dictionary<int, RPGStat>();
    private RPGStat currentEntry;
    
    private readonly List<RPGBuilderDatabaseEntry> allStatCategories = new List<RPGBuilderDatabaseEntry>();
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.statFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGStat> dictionary = new Dictionary<int, RPGStat>();
        databaseEntries.Clear();
        allStatCategories.Clear();
        allDamageTypes.Clear();
        allHealingTypes.Clear();
        var allEntries = Resources.LoadAll<RPGStat>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;

        foreach (var typeEntry in Resources.LoadAll<RPGBStatCategory>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allStatCategories.Add(typeEntry);
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
        
        currentEntry = CreateInstance<RPGStat>();
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
        RPGStat entryFile = (RPGStat)updatedEntry;
        entryFile.UpdateEntryData(currentEntry);
    }
    
    public override void ClearEntries()
    {
        databaseEntries.Clear();
        entries.Clear();
        currentEntry = null;
    }

    private string[] getCorrectOppositeStatsList(RPGStat.STAT_TYPE type)
    {

        var oppositeType = RPGStat.STAT_TYPE.NONE;
        if (type == RPGStat.STAT_TYPE.DAMAGE)
            oppositeType = RPGStat.STAT_TYPE.RESISTANCE;
        else if (type == RPGStat.STAT_TYPE.RESISTANCE)
            oppositeType = RPGStat.STAT_TYPE.PENETRATION;
        else if (type == RPGStat.STAT_TYPE.PENETRATION)
            oppositeType = RPGStat.STAT_TYPE.RESISTANCE;
        else if (type == RPGStat.STAT_TYPE.HEALING)
            oppositeType = RPGStat.STAT_TYPE.ABSORBTION;
        else if (type == RPGStat.STAT_TYPE.CC_POWER)
            oppositeType = RPGStat.STAT_TYPE.CC_RESISTANCE;
        else if (type == RPGStat.STAT_TYPE.CC_RESISTANCE)
            oppositeType = RPGStat.STAT_TYPE.CC_POWER;
        else if (type == RPGStat.STAT_TYPE.DMG_DEALT)
            oppositeType = RPGStat.STAT_TYPE.DMG_TAKEN;
        else if (type == RPGStat.STAT_TYPE.DMG_TAKEN)
            oppositeType = RPGStat.STAT_TYPE.DMG_DEALT;
        else if (type == RPGStat.STAT_TYPE.HEAL_RECEIVED)
            oppositeType = RPGStat.STAT_TYPE.HEAL_DONE;
        else if (type == RPGStat.STAT_TYPE.HEAL_DONE)
            oppositeType = RPGStat.STAT_TYPE.HEAL_RECEIVED;
        else if (type == RPGStat.STAT_TYPE.BASE_DAMAGE_TYPE)
            oppositeType = RPGStat.STAT_TYPE.BASE_RESISTANCE_TYPE;
        else if (type == RPGStat.STAT_TYPE.BASE_RESISTANCE_TYPE)
            oppositeType = RPGStat.STAT_TYPE.PENETRATION;

        var statList = new List<string> {"NONE"};
        foreach (var t in entries.Values) statList.AddRange(from t1 in t.statBonuses where t1.statType == oppositeType select t.entryName);

        return statList.ToArray();
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

        RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField(
                "Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            currentEntry.entryDescription =
                RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryDescription);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showSetup =
            RPGBuilderEditorUtility.HandleModuleBanner("SETUP",
                RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showSetup);
        if (RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showSetup)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Values:", "", true);
            currentEntry.minCheck =
                RPGBuilderEditorFields.DrawHorizontalToggle("Check. Min", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.minCheck);
            if (currentEntry.minCheck)
            {
                currentEntry.minValue =
                    RPGBuilderEditorFields.DrawHorizontalFloatField("Value. Min",
                        "The minimum value this stat can get to",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.minValue);
            }

            currentEntry.maxCheck =
                RPGBuilderEditorFields.DrawHorizontalToggle("Check. Max", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.maxCheck);
            if (currentEntry.maxCheck)
            {
                currentEntry.maxValue =
                    RPGBuilderEditorFields.DrawHorizontalFloatField("Value. Max",
                        "The maximum value this stat can get to",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.maxValue);
            }

            currentEntry.baseValue =
                RPGBuilderEditorFields.DrawHorizontalFloatField("Default Value",
                    "The initial value of the stat",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.baseValue);
            currentEntry.isPercentStat =
                RPGBuilderEditorFields.DrawHorizontalToggle("Is Percent?", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isPercentStat);

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Stat Type:", "", true);
            currentEntry.isVitalityStat =
                RPGBuilderEditorFields.DrawHorizontalToggle("Is Vitality?",
                    "Is this stat a resource like health and energy?",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isVitalityStat);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("UI:", "", true);
            currentEntry.StatCategory = (RPGBStatCategory) RPGBuilderEditorFields.DrawTypeEntryField("Category", allStatCategories, currentEntry.StatCategory);

            if (currentEntry.isVitalityStat)
            {
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Time:", "", true);
                currentEntry.IsPersistent =
                    RPGBuilderEditorFields.DrawHorizontalToggle("Persistent?", "Should the stat value be persistent between sessions?",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.IsPersistent);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Starting Values:", "", true);
                currentEntry.startPercentage =
                    RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Starting percentage", "",
                        currentEntry.startPercentage);

                RPGBuilderEditorFields.DrawTitleLabelExpanded("Passive Stat Changes:", "", true);
                currentEntry.isShiftingOutsideCombat =
                    RPGBuilderEditorFields.DrawHorizontalToggle("Out of combat regen/decay?",
                        "Is this stat shifting while outside of combat?",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.isShiftingOutsideCombat);
                if (currentEntry.isShiftingOutsideCombat)
                {
                    currentEntry.shiftAmountOutsideCombat =
                        RPGBuilderEditorFields.DrawHorizontalFloatField("Amount", "The amount that will be shifted",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.shiftAmountOutsideCombat);
                    currentEntry.shiftIntervalOutsideCombat =
                        RPGBuilderEditorFields.DrawHorizontalFloatField("Interval", "The duration between each shift",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.shiftIntervalOutsideCombat);
                }

                currentEntry.isShiftingInCombat =
                    RPGBuilderEditorFields.DrawHorizontalToggle("In combat regen/decay?",
                        "Is this stat shifting while in combat?",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.isShiftingInCombat);
                if (currentEntry.isShiftingInCombat)
                {
                    currentEntry.shiftAmountInCombat =
                        RPGBuilderEditorFields.DrawHorizontalFloatField("Amount", "The amount that will be shifted",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.shiftAmountInCombat);
                    currentEntry.shiftIntervalInCombat =
                        RPGBuilderEditorFields.DrawHorizontalFloatField("Interval", "The duration between each shift",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.shiftIntervalInCombat);
                }

                currentEntry.isShiftingInSprint =
                    RPGBuilderEditorFields.DrawHorizontalToggle("Sprint regen/decay?", "?",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.isShiftingInSprint);
                currentEntry.isShiftingInBlock =
                    RPGBuilderEditorFields.DrawHorizontalToggle("Blocking regen/decay?", "?",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.isShiftingInBlock);


            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        if (currentEntry.isVitalityStat)
        {
            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showVitalityActions =
                RPGBuilderEditorUtility.HandleModuleBanner("VITALITY ACTIONS",
                    RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showVitalityActions);
            if (RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showVitalityActions)
            {
                var ThisList5 = serialObj.FindProperty("vitalityActions");
                currentEntry.vitalityActions =
                    RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList5) as List<RPGStat.VitalityActions>;

                currentEntry.vitalityActions = RPGBuilderEditorFields.DrawVitalityActions(currentEntry.vitalityActions);
            }
        }
        else
        {
            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showBonuses =
                RPGBuilderEditorUtility.HandleModuleBanner("BONUSES",
                    RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showBonuses);
            if (RPGBuilderEditor.Instance.EditorFilters.statModuleSection.showBonuses)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Bonus", true))
                {
                    currentEntry.statBonuses.Add(new RPGStat.StatBonusData());
                }

                var ThisList22 = serialObj.FindProperty("statBonuses");
                currentEntry.statBonuses =
                    RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList22) as List<RPGStat.StatBonusData>;

                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                for (var a = 0; a < currentEntry.statBonuses.Count; a++)
                {
                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    currentEntry.statBonuses[a].statType =
                        (RPGStat.STAT_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Type", "The Stat Type, this will be used in the game logic to trigger some specific actions based on its type.",
                            (int)currentEntry.statBonuses[a].statType, Enum.GetNames(typeof(RPGStat.STAT_TYPE)));
                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                    {
                        currentEntry.statBonuses.RemoveAt(a);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    currentEntry.statBonuses[a].modifyValue =
                        RPGBuilderEditorFields.DrawHorizontalFloatField("Modifier", "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.statBonuses[a].modifyValue);

                    if (currentEntry.statBonuses[a].statType == RPGStat.STAT_TYPE.DAMAGE)
                    {
                        RPGBuilderEditorFields.DrawHorizontalLabel("Damage Type","");
                        int damageType = EditorGUILayout.Popup(RPGBuilderEditorUtility.GetTypeEntryIndexWithNull(allDamageTypes, currentEntry.statBonuses[a].CustomDamageType),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allDamageTypes.ToArray()));
                        if (damageType != -1)
                        {
                            currentEntry.statBonuses[a].CustomDamageType = (RPGBDamageType) allDamageTypes[damageType];
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    if (currentEntry.statBonuses[a].statType == RPGStat.STAT_TYPE.HEALING)
                    {
                        RPGBuilderEditorFields.DrawHorizontalLabel("Healing Type","");
                        int healingType = EditorGUILayout.Popup(RPGBuilderEditorUtility.GetTypeEntryIndexWithNull(allHealingTypes, currentEntry.statBonuses[a].CustomHealingType),
                            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allHealingTypes.ToArray()));
                        if (healingType != -1)
                        {
                            currentEntry.statBonuses[a].CustomHealingType = (RPGBHealingType) allHealingTypes[healingType];
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (currentEntry.statBonuses[a].statType == RPGStat.STAT_TYPE.BASE_DAMAGE_TYPE)
                    {
                        currentEntry.statBonuses[a].MainDamageType = 
                            (RPGEffect.MainDamageTypes) RPGBuilderEditorFields.DrawHorizontalEnum(
                                "Main Damage Type", "",
                                (int) currentEntry.statBonuses[a].MainDamageType,
                                Enum.GetNames(typeof(RPGEffect.MainDamageTypes)));
                    }

                    if (currentEntry.statBonuses[a].statType == RPGStat.STAT_TYPE.DAMAGE || currentEntry.statBonuses[a].statType == RPGStat.STAT_TYPE.BASE_DAMAGE_TYPE)
                    {
                        currentEntry.statBonuses[a].ResistanceStatID = RPGBuilderEditorFields.DrawDatabaseEntryField(
                                currentEntry.statBonuses[a].ResistanceStatID, "Stat", "Resistance Stat", "");
                        currentEntry.statBonuses[a].PenetrationStatID = RPGBuilderEditorFields.DrawDatabaseEntryField(
                            currentEntry.statBonuses[a].PenetrationStatID, "Stat", "Penetration Stat", "");
                    }
                    
                    if (currentEntry.statBonuses[a].statType == RPGStat.STAT_TYPE.HEALING)
                    {
                        currentEntry.statBonuses[a].ResistanceStatID = RPGBuilderEditorFields.DrawDatabaseEntryField(
                            currentEntry.statBonuses[a].ResistanceStatID, "Stat", "Absorption Stat", "");
                    }

                    switch (currentEntry.statBonuses[a].statType)
                    {
                        case RPGStat.STAT_TYPE.VITALITY_REGEN:
                        {
                            currentEntry.statBonuses[a].statID =
                                RPGBuilderEditorFields.DrawDatabaseEntryField(
                                    currentEntry.statBonuses[a].statID, "Stat",
                                    "Stat Regen:", "");
                            break;
                        }
                        case RPGStat.STAT_TYPE.VITALITY_BONUS:
                        {
                            currentEntry.statBonuses[a].statID =
                                RPGBuilderEditorFields.DrawDatabaseEntryField(
                                    currentEntry.statBonuses[a].statID, "Stat",
                                    "Stat Bonus:", "");
                            break;
                        }
                        case RPGStat.STAT_TYPE.EFFECT_TRIGGER:
                        {
                            GUILayout.Space(10);
                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Effect", true))
                            {
                                currentEntry.onHitEffectsData.Add(new RPGStat.OnHitEffectsData());
                            }

                            var ThisList5 = serialObj.FindProperty("onHitEffectsData");
                            currentEntry.onHitEffectsData =
                                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList5) as
                                    List<RPGStat.OnHitEffectsData>;

                            for (var u = 0; u < currentEntry.onHitEffectsData.Count; u++)
                            {
                                GUILayout.Space(10);
                                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                                    currentEntry.onHitEffectsData[a].effectID,
                                    "Effect"))
                                {
                                    currentEntry.onHitEffectsData.RemoveAt(a);
                                    return;
                                }
                                
                                currentEntry.onHitEffectsData[u].effectID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                                        currentEntry.onHitEffectsData[u].effectID,
                                        "Effect", "Effect", "");
                                
                                currentEntry.onHitEffectsData[u].effectRank = RPGBuilderEditorFields.DrawEffectRankIndexField(
                                    currentEntry.onHitEffectsData[u].effectID,
                                    currentEntry.onHitEffectsData[u].effectRank);
                                
                                RPGBuilderEditorFields.DrawHorizontalLabel("Target Type", "");
                                
                                currentEntry.onHitEffectsData[u].targetType =
                                    (RPGCombatDATA.TARGET_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                        (int)currentEntry.onHitEffectsData[u].targetType,
                                        Enum.GetNames(typeof(RPGCombatDATA.TARGET_TYPE)));
                                GUILayout.EndHorizontal();
                                currentEntry.onHitEffectsData[u].tagType =
                                    (RPGAbility.ABILITY_TAGS) RPGBuilderEditorFields.DrawHorizontalEnum("Tag", "",
                                        (int)currentEntry.onHitEffectsData[u].tagType,
                                        Enum.GetNames(typeof(RPGAbility.ABILITY_TAGS)));

                                currentEntry.onHitEffectsData[u].chance =
                                    RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Chance", "",
                                        currentEntry.onHitEffectsData[u].chance);
                            }

                            break;
                        }
                    }
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            }
        }

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGStat>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
             RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryDescription = entry.description;
            EditorUtility.SetDirty(entry);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    public override void ConvertStringsToTypeEntries()
    {
        
    }
}
