using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorImportDatabaseModule : RPGBuilderEditorModule
{
    private RPGBuilderEditorImportDatabaseUtility currentEntry;
    
    // Data for the injected Database Entries when injecting an other Database in the current one
    private class InjectedEntries
    {
        public RPGBuilderEditorModule module;
        public List<string> allInjectedFilePaths = new List<string>();
    }

    private List<InjectedEntries> allInjectedAssets = new List<InjectedEntries>();

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
        currentEntry = Resources.Load<RPGBuilderEditorImportDatabaseUtility>(
            RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
            AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderEditorImportDatabaseUtility>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderEditorImportDatabaseUtility>(
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
        RPGBuilderEditorImportDatabaseUtility entryFile = (RPGBuilderEditorImportDatabaseUtility) updatedEntry;
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

        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(
            RPGBuilderEditor.Instance.ViewScroll, false, false, GUIStyle.none, GUIStyle.none, GUIStyle.none,
            GUILayout.Width(panelRect.width), GUILayout.MaxWidth(panelRect.width),
            GUILayout.Height(RPGBuilderEditor.Instance.ViewRect.height - 40));

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.exportDatabaseModuleSection.showExportDatabase =
            RPGBuilderEditorUtility.HandleModuleBanner("IMPORT DATABASE",
                RPGBuilderEditor.Instance.EditorFilters.exportDatabaseModuleSection.showExportDatabase);
        if (RPGBuilderEditor.Instance.EditorFilters.exportDatabaseModuleSection.showExportDatabase)
        {
            GUILayout.Space(10);
                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            currentEntry.overrideDatabaseWhenImporting = RPGBuilderEditorFields.DrawHorizontalToggle("Override current database?",
                "Should the imported database replace the current one?", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.overrideDatabaseWhenImporting);

            GUILayout.BeginHorizontal();
            currentEntry.importDirectoryPath =
                RPGBuilderEditorFields.DrawHorizontalTextField("Path", "The path of the new Directory", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.importDirectoryPath);
            GUILayout.Space(2);
            if (GUILayout.Button("Browse", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(75),
                GUILayout.Height(20)))
            {
                currentEntry.importDirectoryPath =
                    EditorUtility.SaveFolderPanel("Path", currentEntry.importDirectoryPath, Application.dataPath);
            }

            GUILayout.EndHorizontal();
            if (!currentEntry.importDirectoryPath.EndsWith("/"))
            {
                currentEntry.importDirectoryPath += "/";
            }

            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            foreach (RPGBuilderEditor.EditorCategoryData category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled && category.category.IsDatabaseCategory))
            {
                if(category.category.OrderIndex > 2) continue;
                RPGBuilderEditorFields.DrawTitleLabel(category.category.CategoryName, "");
                foreach (RPGBuilderEditorModule module in category.modules)
                {
                    module.ImportEnabled = RPGBuilderEditorFields.DrawHorizontalToggle("Import " + module.ModuleName, "", RPGBuilderEditor.Instance.FieldHeight,
                        module.ImportEnabled);
                    GUILayout.Space(2);
                }
                GUILayout.Space(7);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            foreach (RPGBuilderEditor.EditorCategoryData category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled && category.category.IsDatabaseCategory))
            {
                if(category.category.OrderIndex <= 2 || category.category.OrderIndex > 5) continue;
                RPGBuilderEditorFields.DrawTitleLabel(category.category.CategoryName, "");
                foreach (RPGBuilderEditorModule module in category.modules)
                {
                    module.ImportEnabled = RPGBuilderEditorFields.DrawHorizontalToggle("Import " + module.ModuleName, "", RPGBuilderEditor.Instance.FieldHeight,
                        module.ImportEnabled);
                    GUILayout.Space(2);
                }
                GUILayout.Space(7);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            foreach (RPGBuilderEditor.EditorCategoryData category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled && category.category.IsDatabaseCategory))
            {
                if(category.category.OrderIndex < 6) continue;
                RPGBuilderEditorFields.DrawTitleLabel(category.category.CategoryName, "");
                foreach (RPGBuilderEditorModule module in category.modules)
                {
                    module.ImportEnabled = RPGBuilderEditorFields.DrawHorizontalToggle("Import " + module.ModuleName, "", RPGBuilderEditor.Instance.FieldHeight,
                        module.ImportEnabled);
                    GUILayout.Space(2);
                }
                GUILayout.Space(7);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(200);
            if (GUILayout.Button("ALL", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.ExpandWidth(true),
                GUILayout.Height(22)))
            {
                foreach (var module in RPGBuilderEditor.Instance.EditorCategories.SelectMany(category => category.modules).Where(module => module.IsEnabled))
                {
                    module.ImportEnabled = true;
                }
            }

            GUILayout.Space(25);
            if (GUILayout.Button("NONE", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"), GUILayout.ExpandWidth(true),
                GUILayout.Height(22)))
            {
                foreach (var module in RPGBuilderEditor.Instance.EditorCategories.SelectMany(category => category.modules).Where(module => module.IsEnabled))
                {
                    module.ImportEnabled = false;
                }
            }

            GUILayout.Space(200);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (currentEntry.overrideDatabaseWhenImporting)
            {
                if (RPGBuilderEditorFields.DrawButtonWithPopup("OVERRIDE DATABASE", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"),
                    1000, 35, "Database Override", "Are you sure that you want to delete the current database and replace it by the new imported one?",
                    "Yes", "Cancel"))
                {
                    if (!string.IsNullOrEmpty(currentEntry.importDirectoryPath))
                    {
                        ClearAllDatabase();

                        string sourcePath = currentEntry.importDirectoryPath;

                        if (Directory.Exists(sourcePath))
                        {
                            foreach (var category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled))
                            {
                                foreach (var module in category.modules)
                                {
                                    if (!module.ImportEnabled) continue;
                                    string[] files = Directory.GetFiles(sourcePath + module.AssetFolderName + "/");

                                    foreach (string s in files)
                                    {
                                        string fileName = Path.GetFileName(s);
                                        string destFile =
                                            Path.Combine(
                                                RPGBuilderEditor.Instance.EditorSettings.ResourcePath + RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                                module.AssetFolderName + "/", fileName);
                                        File.Copy(s, destFile, true);
                                    }
                                    module.Initialize();
                                }
                            }

                            string[] persistentFiles = Directory.GetFiles(sourcePath + "PersistentData" + "/");
                            foreach (string s in persistentFiles)
                            {
                                string fileName = Path.GetFileName(s);
                                string destFile =
                                    Path.Combine(
                                        RPGBuilderEditor.Instance.EditorSettings.ResourcePath + RPGBuilderEditor.Instance.EditorSettings.DatabasePath + "PersistentData" + "/",
                                        fileName);
                                File.Copy(s, destFile, true);
                            }
                        }

                        GUI.FocusControl(null);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
            else
            {
                if (RPGBuilderEditorFields.DrawButtonWithPopup("INJECT IN DATABASE", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                    1000, 35, "Database Injection",
                    "Are you sure that you want to inject the imported database inside the current one?",
                    "Yes", "Cancel"))
                {
                    allInjectedAssets.Clear();
                    if (!string.IsNullOrEmpty(currentEntry.importDirectoryPath))
                    {
                        string sourcePath = currentEntry.importDirectoryPath;

                        if (Directory.Exists(sourcePath))
                        {
                            foreach (var category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled))
                            {
                                foreach (var module in category.modules)
                                {
                                    if (!module.ImportEnabled) continue;
                                    string[] files = Directory.GetFiles(sourcePath + module.AssetFolderName + "/");

                                    InjectedEntries newInjectedAsset = new InjectedEntries {module = module};

                                    foreach (string s in files)
                                    {
                                        string fileName = Path.GetFileName(s);
                                        string destFile =
                                            Path.Combine(
                                                RPGBuilderEditor.Instance.EditorSettings.ResourcePath + RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                                module.AssetFolderName + "/", fileName);
                                        File.Copy(s, destFile, true);
                                        newInjectedAsset.allInjectedFilePaths.Add(destFile);
                                    }

                                    allInjectedAssets.Add(newInjectedAsset);
                                }
                            }

                            AssetDatabase.Refresh();
                            TriggerSaveInjectedEntry();
                        }

                        GUI.FocusControl(null);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        
                        foreach (var module in RPGBuilderEditor.Instance.EditorCategories.SelectMany(category => category.modules.Where(module => module.IsEnabled)))
                        {
                            module.Initialize();
                        }
                    }
                }
            }
            
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(60);
        GUILayout.EndScrollView();
    }
    
    private void ClearAllDatabase()
    {
        foreach (RPGBuilderEditor.EditorCategoryData category in RPGBuilderEditor.Instance.EditorCategories)
        {
            foreach (RPGBuilderEditorModule module in category.modules)
            {
                if(!module.ImportEnabled) continue;
                module.DeleteAllEntries();
            }
        }
    }
    
    private void TriggerSaveInjectedEntry()
    {
        foreach (var injectedAsset in allInjectedAssets)
        {
            injectedAsset.module.InjectEntries(injectedAsset.allInjectedFilePaths);
        }
    }

}
