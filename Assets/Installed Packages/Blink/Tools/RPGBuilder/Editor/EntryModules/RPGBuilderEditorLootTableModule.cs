using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorLootTableModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGLootTable> entries = new Dictionary<int, RPGLootTable>();
    private RPGLootTable currentEntry;
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.lootTablesFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGLootTable> dictionary = new Dictionary<int, RPGLootTable>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGLootTable>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGLootTable>();
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
        RPGLootTable entryFile = (RPGLootTable)updatedEntry;
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

        ScriptableObject scriptableObj = currentEntry;
        var serialObj = new SerializedObject(scriptableObj);
        
        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.MaxWidth(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.ExpandHeight(true));

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.lootTableModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.lootTableModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.lootTableModuleSection.showBaseInfo)
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
        RPGBuilderEditor.Instance.EditorFilters.lootTableModuleSection.showItems =
            RPGBuilderEditorUtility.HandleModuleBanner("ITEMS", RPGBuilderEditor.Instance.EditorFilters.lootTableModuleSection.showItems);
        if (RPGBuilderEditor.Instance.EditorFilters.lootTableModuleSection.showItems)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.LimitDroppedItems = RPGBuilderEditorFields.DrawHorizontalToggle("Limit dropped amount?", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.LimitDroppedItems);
            if (currentEntry.LimitDroppedItems)
            {
                currentEntry.maxDroppedItems = RPGBuilderEditorFields.DrawHorizontalIntField("Max Dropped Items", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.maxDroppedItems);
            }

            GUILayout.Space(5);
            
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Items", true))
            {
                currentEntry.lootItems.Add(new RPGLootTable.LOOT_ITEMS());
            }

            for (var a = 0; a < currentEntry.lootItems.Count; a++)
            {
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.lootItems[a].itemID,
                    "Item"))
                {
                    currentEntry.lootItems.RemoveAt(a);
                    return;
                }
                
                currentEntry.lootItems[a].itemID = 
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.lootItems[a].itemID, "Item", "Item", "");

                currentEntry.lootItems[a].dropRate =
                    RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Chance", "",
                        currentEntry.lootItems[a].dropRate);

                currentEntry.lootItems[a].min = RPGBuilderEditorFields.DrawHorizontalIntField("Min.", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.lootItems[a].min);

                currentEntry.lootItems[a].max = RPGBuilderEditorFields.DrawHorizontalIntField("Max.", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.lootItems[a].max);
                GUILayout.Space(10);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            if (currentEntry.lootItems.Count >= 5)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Item", true))
                {
                    currentEntry.lootItems.Add(new RPGLootTable.LOOT_ITEMS());
                }
            }
        }
        
        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.lootTableModuleSection.showRequirements =
            RPGBuilderEditorUtility.HandleModuleBanner("REQUIREMENTS", RPGBuilderEditor.Instance.EditorFilters.lootTableModuleSection.showRequirements);
        if (RPGBuilderEditor.Instance.EditorFilters.lootTableModuleSection.showRequirements)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
            currentEntry.UseRequirementsTemplate = 
                RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.UseRequirementsTemplate);

            if (currentEntry.UseRequirementsTemplate)
            {
                currentEntry.RequirementsTemplate = (RequirementsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>(
                    "Template", "", currentEntry.RequirementsTemplate);
            }
            else
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Requirement Group", false))
                {
                    currentEntry.Requirements.Add(new RequirementsData.RequirementGroup());
                }

                currentEntry.Requirements = RPGBuilderEditorFields.DrawRequirementGroupsList(currentEntry.Requirements,false);
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
                        
            GUILayout.Space(10);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGLootTable>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
             RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
            EditorUtility.SetDirty(entry);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
