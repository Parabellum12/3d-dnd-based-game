using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorGeneralSettingsModule : RPGBuilderEditorModule
{
    private RPGBuilderGeneralSettings currentEntry;
    
    private readonly List<RPGBuilderDatabaseEntry> allActionKeyCategories = new List<RPGBuilderDatabaseEntry>();

    public override void Initialize()
    {
        LoadEntries();
    }

    public override void InstantiateCurrentEntry(int index)
    {
        LoadEntries();
    }

    public override void LoadEntries()
    {
        currentEntry = Resources.Load<RPGBuilderGeneralSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                                 AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderGeneralSettings>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderGeneralSettings>(
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                AssetFolderName + "/" + EntryType);
        }

        databaseEntries.Clear();
        databaseEntries.Add(currentEntry);

        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
        
        allActionKeyCategories.Clear();
        foreach (var typeEntry in Resources.LoadAll<RPGBActionKeyCategory>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allActionKeyCategories.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
    }

    public override bool SaveConditionsMet()
    {
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGBuilderGeneralSettings entryFile = (RPGBuilderGeneralSettings) updatedEntry;
        entryFile.UpdateEntryData(currentEntry);
    }

    public override void ClearEntries()
    {
        databaseEntries.Clear();
        currentEntry = null;
    }

    public override void DrawView()
    {
        RPGBuilderEditorUtility.UpdateViewAndFieldData();

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);

        float panelWidth = RPGBuilderEditorUtility.GetScreenWidth();
        panelWidth -= panelWidth * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent;
        float panelHeight = RPGBuilderEditorUtility.GetScreenHeight();
        Rect panelRect = new Rect(
            RPGBuilderEditorUtility.GetScreenWidth() * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent, 0,
            panelWidth, panelHeight);

        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(panelRect.width),
            GUILayout.MaxWidth(panelRect.width),
            GUILayout.ExpandHeight(true));

        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showSavingSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("SAVING",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showSavingSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showSavingSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.automaticSave =
                RPGBuilderEditorFields.DrawHorizontalToggle("Auto Save?", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.automaticSave);

            if (currentEntry.automaticSave)
            {
                currentEntry.automaticSaveDelay = RPGBuilderEditorFields.DrawHorizontalFloatField("Save Delay", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.automaticSaveDelay);

                if (currentEntry.automaticSaveDelay < 1) currentEntry.automaticSaveDelay = 1;
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showLoadingScreenSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("LOADING SCREEN",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showLoadingScreenSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showLoadingScreenSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.clickToLoadScene = RPGBuilderEditorFields.DrawHorizontalToggle("Click to end?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.clickToLoadScene);
            currentEntry.DelayAfterSceneLoad = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Delay After Scene Load", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.DelayAfterSceneLoad);
            currentEntry.LoadingScreenEndDelay = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Loading Screen End Delay", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.LoadingScreenEndDelay);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showMainMenuSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("MAIN MENU",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showMainMenuSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showMainMenuSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.mainMenuSceneName = RPGBuilderEditorFields.DrawHorizontalTextField("Main Menu Scene Name",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.mainMenuSceneName);
            currentEntry.mainMenuLoadingName = RPGBuilderEditorFields.DrawHorizontalTextField(
                "Main Menu Display Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.mainMenuLoadingName);
            currentEntry.mainMenuLoadingDescription = RPGBuilderEditorFields.DrawHorizontalTextField(
                "Main Menu Scene Description", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.mainMenuLoadingDescription);

            RPGBuilderEditorFields.DrawLabel("Main Menu Image:", "");
            currentEntry.mainMenuLoadingImage = (Sprite) EditorGUILayout.ObjectField(
                currentEntry.mainMenuLoadingImage, typeof(Sprite), false, GUILayout.Width(270),
                GUILayout.Height(180));

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showDeveloperSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("DEVELOPER",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showDeveloperSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showDeveloperSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.enableDevPanel = RPGBuilderEditorFields.DrawHorizontalToggle("Show Dev Panel?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.enableDevPanel);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showActionKeys =
            RPGBuilderEditorUtility.HandleModuleBanner("ACTION KEYS",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showActionKeys);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showActionKeys)
        {

            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showActionKeyList =
                RPGBuilderEditorUtility.HandleModuleBanner("ACTION KEY LIST",
                    RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showActionKeyList);
            if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showActionKeyList)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Action Key", true))
                {
                    currentEntry.actionKeys.Add(new RPGGeneralDATA.ActionKey());
                }

                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                for (var a = 0; a < currentEntry.actionKeys.Count; a++)
                {
                    GUILayout.Space(10);
                    var requirementNumber = a + 1;
                    EditorGUILayout.BeginHorizontal();
                    

                    var effectName = currentEntry.actionKeys[a].actionName;
                    RPGBuilderEditorFields.DrawTitleLabelExpanded("" + requirementNumber + ": " + effectName, "");
                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                    {
                        currentEntry.actionKeys.RemoveAt(a);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    currentEntry.actionKeys[a].actionName = RPGBuilderEditorFields.DrawHorizontalTextField(
                        "Action Name", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.actionKeys[a].actionName);
                    currentEntry.actionKeys[a].actionDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField(
                        "Display Name", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.actionKeys[a].actionDisplayName);

                    RPGBuilderEditorFields.DrawHorizontalLabel("Default Key", "");
                    currentEntry.actionKeys[a].defaultKey = (KeyCode) EditorGUILayout.EnumPopup(currentEntry.actionKeys[a].defaultKey);
                    EditorGUILayout.EndHorizontal();

                    currentEntry.actionKeys[a].isUnique = RPGBuilderEditorFields.DrawHorizontalToggle("Is Unique?",
                        "If Unique, no other action key can have the same Key assigned",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.actionKeys[a].isUnique);

                    RPGBuilderEditorFields.DrawHorizontalLabel("Category", "");
                    int actionKeyCategory = EditorGUILayout.Popup(RPGBuilderEditorUtility.GetTypeEntryIndex(allActionKeyCategories, currentEntry.actionKeys[a].Category),
                        RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allActionKeyCategories.ToArray()));
                    currentEntry.actionKeys[a].Category = (RPGBActionKeyCategory) allActionKeyCategories[actionKeyCategory];
                    EditorGUILayout.EndHorizontal();
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

                if (currentEntry.actionKeys.Count > 4)
                {
                    GUILayout.Space(10);
                    if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Action Key", true))
                    {
                        currentEntry.actionKeys.Add(new RPGGeneralDATA.ActionKey());
                    }
                }
            }
        }
        
        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showCursors =
            RPGBuilderEditorUtility.HandleModuleBanner("CURSORS",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showCursors);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showCursors)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            
            EditorGUILayout.BeginHorizontal();
            RPGBuilderEditorFields.DrawLabel("Default Cursor", "", 175);
            currentEntry.defaultCursor = (Texture2D) EditorGUILayout.ObjectField(currentEntry.defaultCursor,
                typeof(Texture2D),
                false, GUILayout.Width(40), GUILayout.Height(40));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            RPGBuilderEditorFields.DrawLabel("Merchant Cursor", "", 175);
            currentEntry.merchantCursor = (Texture2D) EditorGUILayout.ObjectField(currentEntry.merchantCursor,
                typeof(Texture2D),
                false, GUILayout.Width(40), GUILayout.Height(40));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            RPGBuilderEditorFields.DrawLabel("Quest Giver Cursor", "", 175);
            currentEntry.questGiverCursor = (Texture2D) EditorGUILayout.ObjectField(currentEntry.questGiverCursor,
                typeof(Texture2D),
                false, GUILayout.Width(40), GUILayout.Height(40));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            RPGBuilderEditorFields.DrawLabel("Crafting Station", "", 175);
            currentEntry.craftingStationCursor = (Texture2D) EditorGUILayout.ObjectField(
                currentEntry.craftingStationCursor, typeof(Texture2D),
                false, GUILayout.Width(40), GUILayout.Height(40));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            RPGBuilderEditorFields.DrawLabel("Interactive Object Cursor", "", 175);
            currentEntry.interactiveObjectCursor = (Texture2D) EditorGUILayout.ObjectField(
                currentEntry.interactiveObjectCursor, typeof(Texture2D),
                false, GUILayout.Width(40), GUILayout.Height(40));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            RPGBuilderEditorFields.DrawLabel("Enemy Cursor", "", 175);
            currentEntry.enemyCursor = (Texture2D) EditorGUILayout.ObjectField(currentEntry.enemyCursor,
                typeof(Texture2D),
                false, GUILayout.Width(40), GUILayout.Height(40));
            EditorGUILayout.EndHorizontal();

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var entry = Resources.Load<RPGBuilderGeneralSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType);
        var oldSettings = Resources.Load<RPGGeneralDATA>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + "GeneralSettings");

        if (entry == null)
        {
            Debug.LogError("Could not find " + EntryType);
            return;
        }
        if (oldSettings == null)
        {
            Debug.LogError("Could not find " + "currentEntry");
            return;
        }
        
        EditorUtility.SetDirty(entry);

        entry.automaticSave = oldSettings.automaticSave;
        entry.automaticSaveDelay = oldSettings.automaticSaveDelay;
        entry.LoadingScreenEndDelay = oldSettings.LoadingScreenEndDelay;
        entry.DelayAfterSceneLoad = oldSettings.DelayAfterSceneLoad;
        entry.clickToLoadScene = oldSettings.clickToLoadScene;
        entry.mainMenuLoadingDescription = oldSettings.mainMenuLoadingDescription;
        entry.mainMenuLoadingName = oldSettings.mainMenuLoadingName;
        entry.mainMenuSceneName = oldSettings.mainMenuSceneName;
        entry.mainMenuLoadingImage = oldSettings.mainMenuLoadingImage;
        entry.enableDevPanel = oldSettings.enableDevPanel;
        entry.useOldController = oldSettings.useOldController;
        entry.actionKeys = oldSettings.actionKeys;
        entry.ActionKeyCategoryList = oldSettings.ActionKeyCategoryList;
        entry.defaultCursor = oldSettings.defaultCursor;
        entry.enemyCursor = oldSettings.enemyCursor;
        entry.merchantCursor = oldSettings.merchantCursor;
        entry.craftingStationCursor = oldSettings.craftingStationCursor;
        entry.interactiveObjectCursor = oldSettings.interactiveObjectCursor;
        entry.questGiverCursor = oldSettings.questGiverCursor;
        
        EditorUtility.SetDirty(entry);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
