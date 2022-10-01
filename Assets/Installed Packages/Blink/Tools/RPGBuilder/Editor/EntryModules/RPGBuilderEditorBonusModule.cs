using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorBonusModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGBonus> entries = new Dictionary<int, RPGBonus>();
    private RPGBonus currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.bonusFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGBonus> dictionary = new Dictionary<int, RPGBonus>();
        databaseEntries.Clear();
        allWeaponTypes.Clear();
        var allEntries = Resources.LoadAll<RPGBonus>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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

        if (currentEntry != null) currentEntry.ranks.Clear();
        
        currentEntry = CreateInstance<RPGBonus>();
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
        RPGBonus entryFile = (RPGBonus)updatedEntry;
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

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon = RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
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
        RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showRanks =
            RPGBuilderEditorUtility.HandleModuleBanner("RANKS", RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showRanks);
        if (RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showRanks)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            GUILayout.BeginHorizontal();
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Rank", false))
            {
                var newRankDataElement = new RPGBonus.RPGBonusRankDATA();
                currentEntry.ranks.Add(newRankDataElement);
            }

            if (currentEntry.ranks.Count > 0)
            {
                GUILayout.Space(20);
                if (GUILayout.Button("- Remove Rank", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"), GUILayout.MinWidth(150),
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
                    currentEntry.ranks[i].ShowedInEditor =
                        !currentEntry.ranks[i].ShowedInEditor;
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
                    RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showTalentTreeSettings =
                        RPGBuilderEditorUtility.HandleModuleBanner("TALENT TREE SETTINGS",
                            RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showTalentTreeSettings);
                    if (RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showTalentTreeSettings)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                        currentEntry.ranks[i].unlockCost = RPGBuilderEditorFields.DrawHorizontalIntField("Unlock Cost",
                            "Cost in point inside the crafting tree", RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.ranks[i].unlockCost);
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    }

                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showRequirements =
                        RPGBuilderEditorUtility.HandleModuleBanner("REQUIREMENTS", RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showRequirements);
                    if (RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showRequirements)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
                        currentEntry.ranks[i].UseRequirementsTemplate = 
                            RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].UseRequirementsTemplate);

                        if (currentEntry.ranks[i].UseRequirementsTemplate)
                        {
                            currentEntry.ranks[i].RequirementsTemplate = (RequirementsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>(
                                "Template", "", currentEntry.ranks[i].RequirementsTemplate);
                        }
                        else
                        {
                            GUILayout.Space(10);
                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Requirement Group", false))
                            {
                                currentEntry.ranks[i].Requirements.Add(new RequirementsData.RequirementGroup());
                            }

                            currentEntry.ranks[i].Requirements = RPGBuilderEditorFields.DrawRequirementGroupsList(currentEntry.ranks[i].Requirements,false);
                        }
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
                        
                        GUILayout.Space(10);
                    }

                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showBonuses =
                        RPGBuilderEditorUtility.HandleModuleBanner("BONUSES", RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showBonuses);
                    if (RPGBuilderEditor.Instance.EditorFilters.bonusModuleSection.showBonuses)
                    {
                        GUILayout.Space(10);
                        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat", true))
                        {
                            currentEntry.ranks[i].statEffectsData
                                .Add(new RPGEffect.STAT_EFFECTS_DATA());
                        }

                            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                            for (var a = 0; a < currentEntry.ranks[i].statEffectsData.Count; a++)
                            {
                                GUILayout.Space(10);
                                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                                
                                EditorGUILayout.BeginHorizontal();
                                currentEntry.ranks[i].statEffectsData[a].statID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].statEffectsData[a].statID,
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

                                RPGStat entryReference = (RPGStat) RPGBuilderEditorUtility.GetEntryByID(currentEntry.ranks[i].statEffectsData[a].statID, "Stat");
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

                                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                                GUILayout.Space(10);
                            }
                        
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    }
                }
                GUILayout.Space(20);
            }
        }

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGBonus>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
}
