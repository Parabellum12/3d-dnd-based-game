using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorEditorSettingsModule : RPGBuilderEditorModule
{
    private RPGBuilderEditorSettings currentEntry;
    private List<CharacterData> characters = new List<CharacterData>();

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
        characters.Clear();
        
        currentEntry = Resources.Load<RPGBuilderEditorSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                                AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderEditorSettings>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderEditorSettings>(
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                AssetFolderName + "/" + EntryType);
        }

        databaseEntries.Clear();
        databaseEntries.Add(currentEntry);

        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
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
        RPGBuilderEditorSettings entryFile = (RPGBuilderEditorSettings) updatedEntry;
        entryFile.UpdateEntryData(currentEntry);
        RPGBuilderEditor.Instance.LoadSettings();
        RPGBuilderEditor.Instance.InitializeTextures();
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
        
        RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showEditorInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("EDITOR INFO",
                RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showEditorInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showEditorInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawLabel("RPG Builder Version: " + currentEntry.GetCurrentRPGBuilderVersion(), "");
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showThemeSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("SETTINGS",
                RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showThemeSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showThemeSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            currentEntry.IncreasedEditorRepaint = RPGBuilderEditorFields.DrawHorizontalToggle("Increased Editor Updates",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.IncreasedEditorRepaint);
            currentEntry.ShowFileNames = RPGBuilderEditorFields.DrawHorizontalToggle("Show File Names",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.ShowFileNames);

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showDatabase =
            RPGBuilderEditorUtility.HandleModuleBanner("DATABASE",
                RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showDatabase);
        if (RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showDatabase)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.BeginHorizontal();
            currentEntry.ResourcePath =
                RPGBuilderEditorFields.DrawHorizontalTextField("Resource Folder Path", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.ResourcePath);
            GUILayout.Space(5);
            if (GUILayout.Button("Reset", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(75),
                GUILayout.Height(22)))
            {
                currentEntry.ResourcePath = "Assets/Blink/Tools/RPGBuilder/Resources/";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            currentEntry.DatabasePath = RPGBuilderEditorFields.DrawHorizontalTextField("Database Folder Path", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.DatabasePath);
            GUILayout.Space(5);
            if (GUILayout.Button("Reset", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(75),
                GUILayout.Height(22)))
            {
                currentEntry.DatabasePath = "Database/";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            currentEntry.RPGBEditorDataPath = RPGBuilderEditorFields.DrawHorizontalTextField("Editor Data Folder Path",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.RPGBEditorDataPath);
            GUILayout.Space(5);
            if (GUILayout.Button("Reset", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(75),
                GUILayout.Height(22)))
            {
                currentEntry.RPGBEditorDataPath = "EditorData/";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        }
        
        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showTheme =
            RPGBuilderEditorUtility.HandleModuleBanner("THEME",
                RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showTheme);
        if (RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showTheme)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            currentEntry.EditorTheme =(RPGBuilderEditorTheme)
                RPGBuilderEditorFields.DrawHorizontalObject<RPGBuilderEditorTheme>("Editor Theme", "",
                    currentEntry.EditorTheme);

            if (RPGBuilderEditor.Instance.EditorSettings.EditorTheme != null)
            {
                EditorGUI.BeginChangeCheck();
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Background:", "");
                currentEntry.EditorTheme.BackgroundColor1 =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Background 1", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.BackgroundColor1);
                currentEntry.EditorTheme.BackgroundColor2 =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Background 2", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.BackgroundColor2);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Banners:", "", true);
                currentEntry.EditorTheme.BannerCollapsed =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Banner (Collapsed)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.BannerCollapsed);
                currentEntry.EditorTheme.BannerTextCollapsed =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Banner Text (Collapsed)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.BannerTextCollapsed);
                currentEntry.EditorTheme.BannerExpanded =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Banner (Expanded)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.BannerExpanded);
                currentEntry.EditorTheme.BannerTextExpanded =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Banner Text (Expanded)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.BannerTextExpanded);
                currentEntry.EditorTheme.BannerHovered =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Banner (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.BannerHovered);
                currentEntry.EditorTheme.BannerTextHovered =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Banner Text (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.BannerTextHovered);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Search Bar:", "", true);
                currentEntry.EditorTheme.SearchBar =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Search Bar", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.SearchBar);
                currentEntry.EditorTheme.SearchBarHovered =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Search Bar (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.SearchBarHovered);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Buttons:", "", true);
                currentEntry.EditorTheme.AddButton =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Add Button", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.AddButton);
                currentEntry.EditorTheme.AddButtonHover =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Add Button (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.AddButtonHover);
                currentEntry.EditorTheme.RemoveButton =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Remove Button", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.RemoveButton);
                currentEntry.EditorTheme.RemoveButtonHover =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Remove Button (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.RemoveButtonHover);
                currentEntry.EditorTheme.GenericButton =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Generic Button", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.GenericButton);
                currentEntry.EditorTheme.GenericButtonHover =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Generic Button (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.GenericButtonHover);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Fields:", "", true);
                currentEntry.EditorTheme.TextLabelColor =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Text Label", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.TextLabelColor);
                currentEntry.EditorTheme.TextLabelColorHover =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Text Label (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.TextLabelColorHover);
                currentEntry.EditorTheme.CustomTextField =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Value Field", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.CustomTextField);
                currentEntry.EditorTheme.CustomTextFieldHover =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Value Field (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.CustomTextFieldHover);
                currentEntry.EditorTheme.TextValueColor =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Value Text ", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.TextValueColor);
                currentEntry.EditorTheme.TextValueColorHover =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Value Text (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.TextValueColorHover);
                currentEntry.EditorTheme.TitleLabelColor =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Title Text", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.TitleLabelColor);
                currentEntry.EditorTheme.TitleLabelColorHover =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Title Text (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.TitleLabelColorHover);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Fill bars:", "", true);
                currentEntry.EditorTheme.FillBarBackground =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Fill Bar Background", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.FillBarBackground);
                currentEntry.EditorTheme.FillBar =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Fill Bar", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.FillBar);
                currentEntry.EditorTheme.FillBarHover =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Fill Bar (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.FillBarHover);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Ability Tooltips:", "", true);
                currentEntry.EditorTheme.AbilityTooltipColor1 =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Tooltip Background", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.AbilityTooltipColor1);
                currentEntry.EditorTheme.AbilityTooltipColor2 =
                    RPGBuilderEditorFields.DrawHorizontalColorField(
                        "Tooltip Background (Hovered)", "", RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.EditorTheme.AbilityTooltipColor2);
                if (EditorGUI.EndChangeCheck())
                {
                    RPGBuilderEditor.Instance.InitializeTextures();
                    EditorUtility.SetDirty(currentEntry.EditorTheme);
                }
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showsceneLoaderSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("SCENE LOADER",
                RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showsceneLoaderSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showsceneLoaderSettings)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Scene", true))
            {
                currentEntry.sceneLoaderList.Add(new RPGBuilderEditorSettings.SceneLoaderData());
            }

            for (var a = 0; a < currentEntry.sceneLoaderList.Count; a++)
            {
                GUILayout.Space(10);
                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                GUILayout.BeginHorizontal();
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.sceneLoaderList.RemoveAt(a);
                    return;
                }

                GUILayout.BeginVertical();
                currentEntry.sceneLoaderList[a].sceneName = RPGBuilderEditorFields.DrawHorizontalTextField("Scene Name",
                    "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.sceneLoaderList[a].sceneName);
                currentEntry.sceneLoaderList[a].scene = (SceneAsset) RPGBuilderEditorFields.DrawHorizontalObject<SceneAsset>("Scene", "",
                    currentEntry.sceneLoaderList[a].scene);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            }

            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showCharacterLoader =
            RPGBuilderEditorUtility.HandleModuleBanner("CHARACTER LOADER",
                RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showCharacterLoader);
        if (RPGBuilderEditor.Instance.EditorFilters.editorSettingsModuleSection.showCharacterLoader)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            if (currentEntry.characterSelectedName == "")
            {
                if (GUILayout.Button("Load Characters", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.ExpandWidth(true),
                    GUILayout.ExpandWidth(true),
                    GUILayout.MinHeight(40)))
                {
                    ResetSelectedCharacter();
                    characters.Clear();
                    characters = DataSavingSystem.LoadAllCharacters();
                }

                GUILayout.Space(10);

                if (characters != null && characters.Count > 0)
                {
                    foreach (var character in characters)
                    {
                        if (GUILayout.Button("Select: " + character.CharacterName, RPGBuilderEditor.Instance.EditorSkin.GetStyle("GenericButton"),
                            GUILayout.ExpandWidth(true), GUILayout.MinHeight(25)))
                        {
                            SelectCharacter(character.CharacterName);
                        }
                        GUILayout.Space(5);
                    }
                }
            }
            else
            {
                ClearCharacterIfDeleted();
                GUILayout.BeginHorizontal();
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    ResetSelectedCharacter();
                    characters.Clear();
                    characters = DataSavingSystem.LoadAllCharacters();
                }
                GUILayout.Space(15);
                RPGBuilderEditorFields.DrawLabel("Currently Selected Character: " + currentEntry.characterSelectedName, "", 350);
                GUILayout.EndHorizontal();
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }

    private void ClearCharacterIfDeleted()
    {
        bool exist = false;
        foreach (var character in characters)
        {
            if(character.CharacterName != currentEntry.characterSelectedName) continue;
            exist = true;
        }

        if (!exist)
        {
            ResetSelectedCharacter();
        }
    }

    private void ResetSelectedCharacter()
    {
        RPGBuilderEditorSettings editorSettings = Resources.Load<RPGBuilderEditorSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
            AssetFolderName + "/" + EntryType);
        currentEntry.characterSelectedName = "";
        editorSettings.characterSelectedName = "";
        EditorUtility.SetDirty(editorSettings);
        AssetDatabase.SaveAssets();
    }

    private void SelectCharacter(string characterName)
    {
        RPGBuilderEditorSettings editorSettings = Resources.Load<RPGBuilderEditorSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                                AssetFolderName + "/" + EntryType);
        currentEntry.characterSelectedName = characterName;
        editorSettings.characterSelectedName = characterName;
        EditorUtility.SetDirty(editorSettings);
        AssetDatabase.SaveAssets();
    }
}
