using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorTaskModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGTask> entries = new Dictionary<int, RPGTask>();
    private RPGTask currentEntry;
    private readonly List<RPGBuilderDatabaseEntry> allNPCFamilies = new List<RPGBuilderDatabaseEntry>();
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.taskFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGTask> dictionary = new Dictionary<int, RPGTask>();
        databaseEntries.Clear();
        allNPCFamilies.Clear();
        var allEntries = Resources.LoadAll<RPGTask>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;

        foreach (var typeEntry in Resources.LoadAll<RPGBNPCFamily>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allNPCFamilies.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        { 
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }
        
        currentEntry = CreateInstance<RPGTask>();
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
        RPGTask entryFile = (RPGTask)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.taskModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.taskModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.taskModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawID( currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            currentEntry.entryDescription = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDescription);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.taskModuleSection.showTaskData =
            RPGBuilderEditorUtility.HandleModuleBanner("TASK SETTINGS", RPGBuilderEditor.Instance.EditorFilters.taskModuleSection.showTaskData);
        if (RPGBuilderEditor.Instance.EditorFilters.taskModuleSection.showTaskData)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            EditorGUILayout.BeginHorizontal();
            currentEntry.taskType =
                (RPGTask.TASK_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Type", "",
                    (int)currentEntry.taskType,
                    Enum.GetNames(typeof(RPGTask.TASK_TYPE)));
            EditorGUILayout.EndHorizontal();
            
            switch (currentEntry.taskType)
            {
                case RPGTask.TASK_TYPE.enterRegion:
                    break;
                case RPGTask.TASK_TYPE.enterScene:
                    currentEntry.sceneName = RPGBuilderEditorFields.DrawHorizontalTextField("Scene Name:", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.sceneName);
                    break;
                case RPGTask.TASK_TYPE.getItem:
                {
                    currentEntry.itemToGetID = 
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.itemToGetID, "Item", "Item", "");

                    currentEntry.taskValue = RPGBuilderEditorFields.DrawHorizontalIntField("Count", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.taskValue);

                    currentEntry.keepItems = RPGBuilderEditorFields.DrawHorizontalToggle("Keep Items:", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.keepItems);
                    break;
                }
                case RPGTask.TASK_TYPE.killNPC:
                {
                    currentEntry.npcToKillID = 
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.npcToKillID, "NPC", "NPC", "");

                    currentEntry.taskValue = RPGBuilderEditorFields.DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.taskValue);
                    break;
                }
                case RPGTask.TASK_TYPE.learnAbility:
                {
                    currentEntry.abilityToLearnID = 
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.abilityToLearnID, "Ability", "Ability", "");
                    break;
                }
                case RPGTask.TASK_TYPE.learnRecipe:
                    break;
                case RPGTask.TASK_TYPE.reachLevel:
                    currentEntry.taskValue = RPGBuilderEditorFields.DrawHorizontalIntField("Level", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.taskValue);
                    break;
                case RPGTask.TASK_TYPE.reachSkillLevel:
                {
                    currentEntry.skillRequiredID = 
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.skillRequiredID, "Skill", "Skill", "");

                    currentEntry.taskValue = RPGBuilderEditorFields.DrawHorizontalIntField("Level", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.taskValue);
                    break;
                }
                case RPGTask.TASK_TYPE.talkToNPC:
                {
                    currentEntry.npcToTalkToID = 
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.npcToTalkToID, "NPC", "NPC", "");
                    break;
                }
                case RPGTask.TASK_TYPE.useItem:
                {
                    currentEntry.npcToTalkToID = 
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.itemToUseID, "Item", "Item", "");
                    break;
                }
                case RPGTask.TASK_TYPE.reachWeaponTemplateLevel:
                {
                    currentEntry.weaponTemplateRequiredID = 
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.weaponTemplateRequiredID, "WeaponTemplate", "Weapon Template", "");

                    currentEntry.taskValue = RPGBuilderEditorFields.DrawHorizontalIntField("Level", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.taskValue);
                    break;
                }
                case RPGTask.TASK_TYPE.killNPCFamily:
                {
                    RPGBuilderEditorFields.DrawHorizontalLabel("Family", "");
                    int npcFamilyIndex = EditorGUILayout.Popup(
                        RPGBuilderEditorUtility.GetTypeEntryIndex(allNPCFamilies, currentEntry.NPCFamily),
                        RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allNPCFamilies.ToArray()));
                    if (npcFamilyIndex != -1)
                    {
                        currentEntry.NPCFamily = (RPGBNPCFamily) allNPCFamilies[npcFamilyIndex];
                    }
                    EditorGUILayout.EndHorizontal();

                    currentEntry.taskValue = RPGBuilderEditorFields.DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.taskValue);
                    break;
                }
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGTask>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
}
