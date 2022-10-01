using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorExportDatabaseModule : RPGBuilderEditorModule
{
    private RPGBuilderEditorExportDatabaseUtility currentEntry;

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
        currentEntry = Resources.Load<RPGBuilderEditorExportDatabaseUtility>(
            RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
            AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderEditorExportDatabaseUtility>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderEditorExportDatabaseUtility>(
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
        RPGBuilderEditorExportDatabaseUtility entryFile = (RPGBuilderEditorExportDatabaseUtility) updatedEntry;
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
            RPGBuilderEditorUtility.HandleModuleBanner("EXPORT DATABASE",
                RPGBuilderEditor.Instance.EditorFilters.exportDatabaseModuleSection.showExportDatabase);
        if (RPGBuilderEditor.Instance.EditorFilters.exportDatabaseModuleSection.showExportDatabase)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.BeginHorizontal();
            currentEntry.exportDirectoryPath =
                RPGBuilderEditorFields.DrawHorizontalTextField("Path", "The path of the new Directory",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.exportDirectoryPath);
            if (GUILayout.Button("Browse", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.MinWidth(75), GUILayout.Height(22)))
            {
                currentEntry.exportDirectoryPath =
                    EditorUtility.SaveFolderPanel("Path", currentEntry.exportDirectoryPath, Application.dataPath);
            }
            GUILayout.EndHorizontal();
            currentEntry.exportDirectoryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Directory Name", "The name of the new directory",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.exportDirectoryName);
            if (!currentEntry.exportDirectoryPath.EndsWith("/"))
            {
                currentEntry.exportDirectoryPath += "/";
            }

            EditorGUI.BeginDisabledGroup(true);
            currentEntry.exportDirectoryFullPath = RPGBuilderEditorFields.DrawHorizontalTextField("Full Path",
                "The name of the new directory", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.exportDirectoryPath + currentEntry.exportDirectoryName);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            foreach (var category in RPGBuilderEditor.Instance.EditorCategories)
            {
                if (category.category.IsEnabled && category.category.IsDatabaseCategory)
                {
                    if (category.category.OrderIndex <= 2)
                    {
                        RPGBuilderEditorFields.DrawTitleLabel(category.category.CategoryName, "");
                        foreach (RPGBuilderEditorModule module in category.modules)
                        {
                            if (!module.IsEnabled) continue;
                            module.ExportEnabled = RPGBuilderEditorFields.DrawHorizontalToggle("Export " + module.ModuleName, "", 18, module.ExportEnabled);
                            GUILayout.Space(2);
                        }

                        GUILayout.Space(7);
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            foreach (RPGBuilderEditor.EditorCategoryData category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled && category.category.IsDatabaseCategory))
            {
                if(category.category.OrderIndex <= 2 || category.category.OrderIndex > 5) continue;
                RPGBuilderEditorFields.DrawTitleLabel(category.category.CategoryName, "");
                foreach (RPGBuilderEditorModule module in category.modules)
                {
                    if (!module.IsEnabled) continue;
                    module.ExportEnabled = RPGBuilderEditorFields.DrawHorizontalToggle("Export " + module.ModuleName, "", 18, module.ExportEnabled);
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
                    if (!module.IsEnabled) continue;
                    module.ExportEnabled = RPGBuilderEditorFields.DrawHorizontalToggle("Export " + module.ModuleName, "", 18, module.ExportEnabled);
                    GUILayout.Space(2);
                }
                GUILayout.Space(7);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
            string projectPath = Application.dataPath;
            projectPath = projectPath.Remove(projectPath.Length - 6, 6);

            GUILayout.BeginHorizontal();
            GUILayout.Space(200);
            if (GUILayout.Button("ALL", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.ExpandWidth(true),
                GUILayout.Height(22)))
            {
                foreach (var module in RPGBuilderEditor.Instance.EditorCategories.SelectMany(category =>
                    category.modules.Where(module => module.IsEnabled)))
                {
                    module.ExportEnabled = true;
                }
            }

            GUILayout.Space(25);
            if (GUILayout.Button("NONE", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"),
                GUILayout.ExpandWidth(true),
                GUILayout.Height(22)))
            {
                foreach (var module in RPGBuilderEditor.Instance.EditorCategories.SelectMany(category =>
                    category.modules.Where(module => module.IsEnabled)))
                {
                    module.ExportEnabled = false;
                }
            }

            GUILayout.Space(200);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (GUILayout.Button("EXPORT DATABASE", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.ExpandWidth(true), GUILayout.Height(32)))
            {
                if (!string.IsNullOrEmpty(currentEntry.exportDirectoryFullPath))
                {
                    string sourcePath = projectPath + RPGBuilderEditor.Instance.EditorSettings.ResourcePath +
                                        RPGBuilderEditor.Instance.EditorSettings.DatabasePath;

                    var folder = Directory.CreateDirectory(currentEntry.exportDirectoryFullPath);

                    if (Directory.Exists(sourcePath))
                    {
                        string[] files = Directory.GetFiles(sourcePath);

                        foreach (string s in files)
                        {
                            string fileName = Path.GetFileName(s);
                            string nameRef = fileName.Replace(".meta", "");
                            if (!isModuleExported(nameRef)) continue;
                            string destFile = Path.Combine(currentEntry.exportDirectoryFullPath, fileName);
                            File.Copy(s, destFile, true);
                        }

                        string databaseFolderPath = RPGBuilderEditor.Instance.EditorSettings.ResourcePath +
                                                    RPGBuilderEditor.Instance.EditorSettings.DatabasePath;
                        databaseFolderPath = databaseFolderPath.Remove(databaseFolderPath.Length - 1, 1);
                        var folders = AssetDatabase.GetSubFolders(databaseFolderPath);
                        foreach (var f in folders)
                        {
                            string subFolderName = f;

                            string subFolderShortName =
                                subFolderName.Replace(RPGBuilderEditor.Instance.EditorSettings.ResourcePath, "");
                            subFolderShortName =
                                subFolderShortName.Replace(RPGBuilderEditor.Instance.EditorSettings.DatabasePath, "");

                            if (!isModuleExported(subFolderShortName)) continue;
                            var subFolder = folder.CreateSubdirectory(subFolderShortName);

                            string subFolderSourcePath = projectPath + subFolderName;
                            string[] entryFiles = Directory.GetFiles(subFolderSourcePath);

                            // Copy the files and overwrite destination files if they already exist.
                            foreach (string s in entryFiles)
                            {
                                // Use static Path methods to extract only the file name from the path.
                                string fileName = Path.GetFileName(s);
                                string destFile =
                                    Path.Combine(currentEntry.exportDirectoryFullPath + "/" + subFolderShortName,
                                        fileName);
                                File.Copy(s, destFile, true);
                            }
                        }
                    }

                    GUI.FocusControl(null);
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(60);
        GUILayout.EndScrollView();
    }

    private bool isModuleExported(string nameContain)
    {
        if (nameContain == "PersistentData" || nameContain == "Settings") return true;

        return (from category in RPGBuilderEditor.Instance.EditorCategories
            from module in category.modules.Where(module => module.AssetFolderName.Contains(nameContain))
            select module.ExportEnabled).FirstOrDefault();
    }
}
