using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorResetDatabaseModule : RPGBuilderEditorModule
{
    private RPGBuilderEditorResetDatabaseUtility currentEntry;

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
        currentEntry = Resources.Load<RPGBuilderEditorResetDatabaseUtility>(
            RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
            AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderEditorResetDatabaseUtility>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderEditorResetDatabaseUtility>(
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
        RPGBuilderEditorResetDatabaseUtility entryFile = (RPGBuilderEditorResetDatabaseUtility) updatedEntry;
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
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            EditorGUILayout.HelpBox("!!! BE CAREFUL !!! THERE IS NO UNDO FUNCTION !!! ONLY DELETE WHAT YOU ARE SURE ABOUT !!!", MessageType.Error, true);
            GUILayout.Space(25);
            
            if (GUILayout.Button("Delete All Characters", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"), 
                GUILayout.ExpandWidth(true), GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Confirm DELETE", "Are you sure you want to delete ALL Characters ?",
                    "YES",
                    "Cancel"))
                {
                    var path = Application.persistentDataPath;
                    var di = new DirectoryInfo(path);
                    var files = di.GetFiles().Where(o => o.Name.Contains("_RPGBCharacter.txt")).ToArray();
                    foreach (var t in files)
                    {
                        File.Delete(t.FullName);
                    }
                }
            }
            GUILayout.Space(10);

            if (GUILayout.Button("Reset The Database", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"), 
                GUILayout.ExpandWidth(true), GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("BE CAREFUL! CONFIRM DELETE DATABASE", "Are you sure? This will delete everything in your database. Only do this if you want to start from nothing.",
                    "YES",
                    "Cancel"))
                {
                    ResetAllModuleEntries();
                }
            }
            
            string buttonTitle = "Delete ";
            string buttonStyle = "RemoveAbilityRankButton";
            string dialogTitle = "!! BE CAREFUL !!";
            string dialogDescription = "You are about to delete ALL ";
            string option1 = "Yes", option2 = "Cancel";
            string barTitle = "Delete All ";
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            foreach (RPGBuilderEditor.EditorCategoryData category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled && category.category.IsDatabaseCategory))
            {
                if(category.category.OrderIndex > 2) continue;
                RPGBuilderEditorFields.DrawLabel(category.category.CategoryName, "");
                foreach (RPGBuilderEditorModule module in category.modules)
                {
                    if (RPGBuilderEditorFields.DrawButtonWithPopup(buttonTitle + module.ModuleName, RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"), 230, 30,
                        dialogTitle, dialogDescription + module.ModuleName, option1, option2))
                    {
                        module.DeleteAllEntries();
                    }
                    GUILayout.Space(5);
                }
                GUILayout.Space(7);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            foreach (RPGBuilderEditor.EditorCategoryData category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled && category.category.IsDatabaseCategory))
            {
                if(category.category.OrderIndex <= 2 || category.category.OrderIndex > 5) continue;
                RPGBuilderEditorFields.DrawLabel(category.category.CategoryName, "");
                foreach (RPGBuilderEditorModule module in category.modules)
                {
                    if (RPGBuilderEditorFields.DrawButtonWithPopup(buttonTitle + module.ModuleName, RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"), 230, 30, 
                        dialogTitle, dialogDescription + module.ModuleName, option1, option2))
                    {
                        module.DeleteAllEntries();
                    }
                    GUILayout.Space(5);
                }
                GUILayout.Space(7);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            foreach (RPGBuilderEditor.EditorCategoryData category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled && category.category.IsDatabaseCategory))
            {
                if(category.category.OrderIndex < 6) continue;
                RPGBuilderEditorFields.DrawLabel(category.category.CategoryName, "");
                foreach (RPGBuilderEditorModule module in category.modules)
                {
                    if (RPGBuilderEditorFields.DrawButtonWithPopup(buttonTitle + module.ModuleName, RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"), 230, 30,
                        dialogTitle, dialogDescription + module.ModuleName, option1, option2))
                    {
                        module.DeleteAllEntries();
                    }
                    GUILayout.Space(5);
                }
                GUILayout.Space(7);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        GUILayout.Space(60);
        GUILayout.EndScrollView();
    }
    
    private void ResetAllModuleEntries()
    {
        foreach (var category in RPGBuilderEditor.Instance.EditorCategories.Where(category => category.category.IsEnabled))
        {
            foreach (var module in category.modules.Where(module => module.IsEnabled))
            {
                module.DeleteAllEntries();
            }
        }
    }
}
