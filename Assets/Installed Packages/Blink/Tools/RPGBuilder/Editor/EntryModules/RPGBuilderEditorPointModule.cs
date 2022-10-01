using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorPointModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGTreePoint> entries = new Dictionary<int, RPGTreePoint>();
    private RPGTreePoint currentEntry;
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.pointFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGTreePoint> dictionary = new Dictionary<int, RPGTreePoint>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGTreePoint>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        { 
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }
        
        currentEntry = CreateInstance<RPGTreePoint>();
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
        RPGTreePoint entryFile = (RPGTreePoint)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.treePointModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.treePointModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.treePointModuleSection.showBaseInfo)
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
            currentEntry.entryDescription = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDescription);
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.treePointModuleSection.showSetup =
            RPGBuilderEditorUtility.HandleModuleBanner("SETUP SETTINGS", RPGBuilderEditor.Instance.EditorFilters.treePointModuleSection.showSetup);
        if (RPGBuilderEditor.Instance.EditorFilters.treePointModuleSection.showSetup)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.startAmount =
                RPGBuilderEditorFields.DrawHorizontalIntField("Starts At",
                    "The amount of points given when creating the character",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.startAmount);
            currentEntry.maxPoints =
                RPGBuilderEditorFields.DrawHorizontalIntField("Max",
                    "Maximum amount of this point type that the character can have",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.maxPoints);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.treePointModuleSection.showGainRequirements =
            RPGBuilderEditorUtility.HandleModuleBanner("GAIN REQUIREMENTS", RPGBuilderEditor.Instance.EditorFilters.treePointModuleSection.showGainRequirements);
        if (RPGBuilderEditor.Instance.EditorFilters.treePointModuleSection.showGainRequirements)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);
            if (GUILayout.Button("+ Add Requirement", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(150),
                GUILayout.ExpandWidth(true)))
            {
                currentEntry.gainPointRequirements.Add(new RPGTreePoint.GainRequirements());
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, false);

            var ThisList = serialObj.FindProperty("gainPointRequirements");
            currentEntry.gainPointRequirements =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList) as List<RPGTreePoint.GainRequirements>;

            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.gainPointRequirements.Count; a++)
            {
                GUILayout.Space(10);

                if (currentEntry.gainPointRequirements.Count > 0)
                {
                    switch (currentEntry.gainPointRequirements[a].gainType)
                    {
                        case RPGTreePoint.TreePointGainRequirementTypes.characterLevelUp:
                            EditorGUILayout.BeginHorizontal();
                            
                            currentEntry.gainPointRequirements[a].gainType =
                                (RPGTreePoint.TreePointGainRequirementTypes) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                    (int)currentEntry.gainPointRequirements[a].gainType,
                                    Enum.GetNames(typeof(RPGTreePoint.TreePointGainRequirementTypes)));
                            if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                            {
                                currentEntry.gainPointRequirements.RemoveAt(a);
                                return;
                            }
                            EditorGUILayout.EndHorizontal();
                            break;
                        case RPGTreePoint.TreePointGainRequirementTypes.itemGained:
                            if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                                currentEntry.gainPointRequirements[a].itemRequiredID,
                                "Item"))
                            {
                                currentEntry.gainPointRequirements.RemoveAt(a);
                                return;
                            }
                            
                            currentEntry.gainPointRequirements[a].gainType =
                                (RPGTreePoint.TreePointGainRequirementTypes) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                    (int)currentEntry.gainPointRequirements[a].gainType,
                                    Enum.GetNames(typeof(RPGTreePoint.TreePointGainRequirementTypes)));
                            currentEntry.gainPointRequirements[a].itemRequiredID = 
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.gainPointRequirements[a].itemRequiredID, "Item", "Item", "");

                            currentEntry.gainPointRequirements[a].itemRequiredCount =
                                RPGBuilderEditorFields.DrawHorizontalIntField("Stacks", "How many of this items is required?",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.gainPointRequirements[a].itemRequiredCount);
                            break;
                        case RPGTreePoint.TreePointGainRequirementTypes.npcKilled:
                            if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                                currentEntry.gainPointRequirements[a].npcRequiredID,
                                "NPC"))
                            {
                                currentEntry.gainPointRequirements.RemoveAt(a);
                                return;
                            }
                            
                            currentEntry.gainPointRequirements[a].gainType =
                                (RPGTreePoint.TreePointGainRequirementTypes) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                    (int)currentEntry.gainPointRequirements[a].gainType,
                                    Enum.GetNames(typeof(RPGTreePoint.TreePointGainRequirementTypes)));
                            currentEntry.gainPointRequirements[a].npcRequiredID = 
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.gainPointRequirements[a].npcRequiredID, "NPC", "NPC", "");
                            break;
                        case RPGTreePoint.TreePointGainRequirementTypes.skillLevelUp:
                            if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                                currentEntry.gainPointRequirements[a].skillRequiredID,
                                "Skill"))
                            {
                                currentEntry.gainPointRequirements.RemoveAt(a);
                                return;
                            }
                            
                            currentEntry.gainPointRequirements[a].gainType =
                                (RPGTreePoint.TreePointGainRequirementTypes) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                    (int)currentEntry.gainPointRequirements[a].gainType,
                                    Enum.GetNames(typeof(RPGTreePoint.TreePointGainRequirementTypes)));
                            currentEntry.gainPointRequirements[a].skillRequiredID = 
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.gainPointRequirements[a].skillRequiredID, "Skill", "Skill", "");
                            break;
                        case RPGTreePoint.TreePointGainRequirementTypes.weaponTemplateLevelUp:
                            if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                                currentEntry.gainPointRequirements[a].weaponTemplateRequiredID,
                                "WeaponTemplate"))
                            {
                                currentEntry.gainPointRequirements.RemoveAt(a);
                                return;
                            }
                            
                            currentEntry.gainPointRequirements[a].gainType =
                                (RPGTreePoint.TreePointGainRequirementTypes) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                    (int)currentEntry.gainPointRequirements[a].gainType,
                                    Enum.GetNames(typeof(RPGTreePoint.TreePointGainRequirementTypes)));
                            currentEntry.gainPointRequirements[a].weaponTemplateRequiredID = 
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.gainPointRequirements[a].weaponTemplateRequiredID, "WeaponTemplate", "Weapon Template", "");
                            break;
                    }

                    currentEntry.gainPointRequirements[a].amountGained =
                        RPGBuilderEditorFields.DrawHorizontalIntField("Gain", "How many points should be gained",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.gainPointRequirements[a].amountGained);
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGTreePoint>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath + OldFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            
            AssetDatabase.MoveAsset(RPGBuilderEditor.Instance.EditorSettings.ResourcePath +
                                    RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                    OldFolderName + "/" + entry._fileName + ".asset", 
                RPGBuilderEditor.Instance.EditorSettings.ResourcePath +
                RPGBuilderEditor.Instance.EditorSettings.DatabasePath + AssetFolderName +"/" + entry._fileName + ".asset");
            
            entry.entryName = entry._name;
            AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorSettings.ResourcePath + 
                                      RPGBuilderEditor.Instance.EditorSettings.DatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
            entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry._displayName;
            entry.entryIcon = entry.icon;
            
            EditorUtility.SetDirty(entry);
        }
        
        FileUtil.DeleteFileOrDirectory(RPGBuilderEditor.Instance.EditorSettings.ResourcePath +
                                       RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                       OldFolderName);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
