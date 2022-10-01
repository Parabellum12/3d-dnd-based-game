using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorLevelsModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGLevelsTemplate> entries = new Dictionary<int, RPGLevelsTemplate>();
    private RPGLevelsTemplate currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.levelsFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGLevelsTemplate> dictionary = new Dictionary<int, RPGLevelsTemplate>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGLevelsTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGLevelsTemplate>();
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
        RPGLevelsTemplate entryFile = (RPGLevelsTemplate)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.levelTemplateModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.levelTemplateModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.levelTemplateModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawID( currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.entryName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.levelTemplateModuleSection.showTemplate =
            RPGBuilderEditorUtility.HandleModuleBanner("TEMPLATE", RPGBuilderEditor.Instance.EditorFilters.levelTemplateModuleSection.showTemplate);
        if (RPGBuilderEditor.Instance.EditorFilters.levelTemplateModuleSection.showTemplate)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            currentEntry.levels =
                RPGBuilderEditorFields.DrawHorizontalIntField("Levels", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.levels);
            currentEntry.baseXPValue =
                RPGBuilderEditorFields.DrawHorizontalIntField("Base XP", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.baseXPValue);
            currentEntry.increaseAmount =
                RPGBuilderEditorFields.DrawHorizontalFloatField("Increase by %", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.increaseAmount);
                    
            currentEntry.levelUpPrefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Level Up Prefab", "",
                currentEntry.levelUpPrefab);

            GUILayout.Space(10);
            if (GUILayout.Button("Click to Generate", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(150),
                GUILayout.ExpandWidth(true)))
            {
                currentEntry.allLevels = new List<RPGLevelsTemplate.LEVELS_DATA>();
                float curXP = currentEntry.baseXPValue;

                var curIncreaseAmount = currentEntry.increaseAmount / 100;

                for (var i = 0; i < currentEntry.levels; i++)
                {
                    var newLevel = new RPGLevelsTemplate.LEVELS_DATA();
                    var currentLevel = i + 1;

                    if (i > 0) curXP += curXP * curIncreaseAmount;

                    if (curXP > 2000000000) curXP = 2000000000;

                    newLevel.XPRequired = (int) curXP;
                    newLevel.levelName = "" + currentLevel;
                    newLevel.level = currentLevel;

                    currentEntry.allLevels.Add(newLevel);
                }
            }

            GUILayout.Space(10);

            var serialProp = serialObj.FindProperty("allLevels");
            EditorGUILayout.PropertyField(serialProp, true);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGLevelsTemplate>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath + OldFolderName);
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
            
            EditorUtility.SetDirty(entry);
        }
        
        FileUtil.DeleteFileOrDirectory(RPGBuilderEditor.Instance.EditorSettings.ResourcePath +
                                       RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                       OldFolderName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
