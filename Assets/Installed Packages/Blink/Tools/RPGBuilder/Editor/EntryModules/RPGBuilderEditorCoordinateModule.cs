using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorCoordinateModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGWorldPosition> entries = new Dictionary<int, RPGWorldPosition>();
    private RPGWorldPosition currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.coordinateFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGWorldPosition> dictionary = new Dictionary<int, RPGWorldPosition>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGWorldPosition>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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

        currentEntry = CreateInstance<RPGWorldPosition>();
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
        RPGWorldPosition entryFile = (RPGWorldPosition)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.worldPositionModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.worldPositionModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.worldPositionModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField(
                "File Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.worldPositionModuleSection.showSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("SETTINGS",
                RPGBuilderEditor.Instance.EditorFilters.worldPositionModuleSection.showSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.worldPositionModuleSection.showSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.position = RPGBuilderEditorFields.DrawHorizontalVector3(
                "Position", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.position);

            currentEntry.useRotation = RPGBuilderEditorFields.DrawHorizontalToggle(
                "Use Rotation?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.useRotation);
            if (currentEntry.useRotation)
            {
                currentEntry.rotation =
                    RPGBuilderEditorFields.DrawHorizontalVector3("Rotation", "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.rotation);
            }

            GameObject sceneREF = null;
            sceneREF = (GameObject) RPGBuilderEditorFields.DrawHorizontalSceneObject<GameObject>("Scene Reference", "", sceneREF);
            if (sceneREF != null)
            {
                currentEntry.position = sceneREF.transform.position;
                currentEntry.rotation = sceneREF.transform.localEulerAngles;
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGWorldPosition>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath + OldFolderName);
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
            entry.entryDisplayName = entry.displayName;
            
            EditorUtility.SetDirty(entry);
        }
        
        FileUtil.DeleteFileOrDirectory(RPGBuilderEditor.Instance.EditorSettings.ResourcePath +
                                       RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                       OldFolderName);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
