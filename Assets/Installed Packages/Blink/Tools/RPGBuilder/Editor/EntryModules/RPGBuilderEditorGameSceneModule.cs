using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorGameSceneModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGGameScene> entries = new Dictionary<int, RPGGameScene>();
    private RPGGameScene currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.gameSceneFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGGameScene> dictionary = new Dictionary<int, RPGGameScene>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGGameScene>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGGameScene>();
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
        
        
        if (currentEntry.mapBounds.extents.x == 0 ||currentEntry.mapBounds.extents.z == 0)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("Invalid Extent", "Add proper extent in Map Bounds. Cannot be 0", "OK");
            return false;
        }
        
        return true;
    }
    
    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGGameScene entryFile = (RPGGameScene)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showBaseInfo)
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
            currentEntry.isProceduralScene = RPGBuilderEditorFields.DrawHorizontalToggle("Procedural?", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.isProceduralScene);
            if (currentEntry.isProceduralScene)
            {
                currentEntry.SpawnPointName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Spawn Point Name", "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.SpawnPointName);
                currentEntry.AlwaysSpawnAtPoint = RPGBuilderEditorFields.DrawHorizontalToggle("Always spawn at point?", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.AlwaysSpawnAtPoint);
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("SETTINGS", RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Minimap Values", "");
            RPGBuilderEditorFields.DrawHorizontalLabel("Map Bounds", "");
            currentEntry.mapBounds =
                EditorGUILayout.BoundsField("", currentEntry.mapBounds);
            EditorGUILayout.EndHorizontal();
            RPGBuilderEditorFields.DrawHorizontalLabel("Map Size", "");
            currentEntry.mapSize =
                EditorGUILayout.Vector2Field("", currentEntry.mapSize);
            EditorGUILayout.EndHorizontal();

            if (!currentEntry.isProceduralScene)
            {
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Default Character Position", "", true);
                currentEntry.startPositionID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.startPositionID, "Coordinate",
                        "Coordinate", "");
            }
            

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Day/Night", "", true);
            currentEntry.UpdateSunPosition = RPGBuilderEditorFields.DrawHorizontalToggle("Update sun position?", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.UpdateSunPosition);
            if (currentEntry.UpdateSunPosition)
            {
                currentEntry.SunRotationAxis = RPGBuilderEditorFields.DrawHorizontalFloatField("Sun Axis", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.SunRotationAxis);
                RPGBuilderEditorFields.DrawHorizontalLabel("Sun Colors", "");
                currentEntry.SunColors = EditorGUILayout.GradientField(currentEntry.SunColors);
                EditorGUILayout.EndHorizontal();
            }

            currentEntry.UpdateFog = RPGBuilderEditorFields.DrawHorizontalToggle("Update fog color?", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.UpdateFog);
            if (currentEntry.UpdateFog)
            {
                RPGBuilderEditorFields.DrawHorizontalLabel("Fog Colors", "");
                currentEntry.FogColors = EditorGUILayout.GradientField(currentEntry.FogColors);
                EditorGUILayout.EndHorizontal();
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showLoadingScreenSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("LOADING SCREEN SETTINGS",
                RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showLoadingScreenSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showLoadingScreenSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawLabel("Loading Image", "");
            currentEntry.loadingBG = (Sprite) EditorGUILayout.ObjectField(
                currentEntry.loadingBG, typeof(Sprite), false, GUILayout.Width(300), GUILayout.Height(180));
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showMinimapSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("MINIMAP SETTINGS", RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showMinimapSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showMinimapSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawLabel("Minimap Image", "");
            currentEntry.minimapImage = (Sprite) EditorGUILayout.ObjectField(
                currentEntry.minimapImage, typeof(Sprite), false, GUILayout.Width(250),
                GUILayout.Height(250));
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showRegions =
            RPGBuilderEditorUtility.HandleModuleBanner("REGIONS", RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showRegions);
        if (RPGBuilderEditor.Instance.EditorFilters.gameSceneModuleSection.showRegions)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.DefaultRegion =
                (RegionTemplate) RPGBuilderEditorFields.DrawHorizontalObject<RegionTemplate>("Default Region", "",
                    currentEntry.DefaultRegion);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGGameScene>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
