using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorRecipeModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGCraftingRecipe> entries = new Dictionary<int, RPGCraftingRecipe>();
    private RPGCraftingRecipe currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.recipeFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }
    
    public override void LoadEntries()
    {
        Dictionary<int, RPGCraftingRecipe> dictionary = new Dictionary<int, RPGCraftingRecipe>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGCraftingRecipe>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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

        if (currentEntry != null) currentEntry.ranks.Clear();
        
        currentEntry = CreateInstance<RPGCraftingRecipe>();
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
        
        if (currentEntry.ranks.Count == 0)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("0 Ranks", "Create at least 1 rank", "OK");
            return false;
        }
        
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGCraftingRecipe entryFile = (RPGCraftingRecipe)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon = RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID( currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);

            currentEntry.learnedByDefault =
                RPGBuilderEditorFields.DrawHorizontalToggle("Known Automatically", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.learnedByDefault);

            currentEntry.craftingSkillID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.craftingSkillID, "Skill", "Skill", "");
            currentEntry.craftingStationID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.craftingStationID, "CraftingStation", "Crafting Station", "");

            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showRanks =
            RPGBuilderEditorUtility.HandleModuleBanner("RANKS", RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showRanks);
        if (RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showRanks)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Rank", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.MinWidth(150), GUILayout.ExpandWidth(true)))
            {
                var newRankDataElement = new RPGCraftingRecipe.RPGCraftingRecipeRankData();
                currentEntry.ranks.Add(newRankDataElement);
            }

            if (currentEntry.ranks.Count > 0)
            {
                GUILayout.Space(20);
                if (GUILayout.Button("- Remove Rank", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"), GUILayout.MinWidth(150),
                    GUILayout.ExpandWidth(true)))
                {
                    currentEntry.ranks
                        .RemoveAt(currentEntry.ranks
                            .Count - 1);
                    return;
                }
            }

            GUILayout.EndHorizontal();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

            GUILayout.Space(10);

            for (var i = 0; i < currentEntry.ranks.Count; i++)
            {
                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

                var rankNbr = i + 1;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Rank: " + rankNbr, RPGBuilderEditor.Instance.EditorSkin.GetStyle("GenericButton"),
                    GUILayout.ExpandWidth(true)))
                {
                    currentEntry.ranks[i].ShowedInEditor =
                        !currentEntry.ranks[i].ShowedInEditor;
                    GUI.FocusControl(null);
                }

                if (i > 0)
                {
                    GUILayout.Space(5);
                    if (GUILayout.Button("Copy Above", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.ExpandWidth(true)))
                    {
                        currentEntry.CopyEntryData(currentEntry.ranks[i],
                            currentEntry.ranks[i - 1]);
                        GUI.FocusControl(null);
                    }
                }

                GUILayout.EndHorizontal();
                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

                if (currentEntry.ranks[i].ShowedInEditor)
                {
                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showTalentTreeSettings =
                        RPGBuilderEditorUtility.HandleModuleBanner("TALENT TREE SETTINGS",
                            RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showTalentTreeSettings);
                    if (RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showTalentTreeSettings)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                        currentEntry.ranks[i].unlockCost = RPGBuilderEditorFields.DrawHorizontalIntField("Unlock Cost",
                            "Cost in point inside the crafting tree", RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.ranks[i].unlockCost);
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    }

                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showSettings =
                        RPGBuilderEditorUtility.HandleModuleBanner("SETTINGS", RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showSettings);
                    if (RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showSettings)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                        currentEntry.ranks[i].Experience = RPGBuilderEditorFields.DrawHorizontalIntField("Experience", "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.ranks[i].Experience);
                        currentEntry.ranks[i].craftTime = RPGBuilderEditorFields.DrawHorizontalFloatField("Craft Duration",
                            "", RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.ranks[i].craftTime);
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    }

                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showCraftedItems =
                        RPGBuilderEditorUtility.HandleModuleBanner("CRAFTED ITEMS", RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showCraftedItems);
                    if (RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showCraftedItems)
                    {
                        GUILayout.Space(10);
                        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Item", true))
                        {
                            currentEntry.ranks[i].allCraftedItems
                                .Add(new RPGCraftingRecipe.CraftedItemsDATA());
                        }

                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                        for (var a = 0; a < currentEntry.ranks[i].allCraftedItems.Count; a++)
                        {
                            GUILayout.Space(10);
                            if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                                currentEntry.ranks[i].allCraftedItems[a].craftedItemID,
                                "Item"))
                            {
                                currentEntry.ranks[i].allCraftedItems.RemoveAt(a);
                                return;
                            }
                            
                            currentEntry.ranks[i].allCraftedItems[a].craftedItemID =
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].allCraftedItems[a].craftedItemID, "Item", "Crafted Item", "");

                            currentEntry.ranks[i].allCraftedItems[a].chance =
                                RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Chance", "",
                                    currentEntry.ranks[i].allCraftedItems[a].chance);

                            currentEntry.ranks[i].allCraftedItems[a].count =
                                RPGBuilderEditorFields.DrawHorizontalIntField("Count:", "", RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].allCraftedItems[a].count);
                        }

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    }

                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showComponentsRequired =
                        RPGBuilderEditorUtility.HandleModuleBanner("COMPONENTS REQUIRED",
                            RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showComponentsRequired);
                    if (RPGBuilderEditor.Instance.EditorFilters.craftingRecipeModuleSection.showComponentsRequired)
                    {
                        GUILayout.Space(10);
                        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Item", true))
                        {
                            currentEntry.ranks[i].allComponents
                                .Add(new RPGCraftingRecipe.ComponentsRequired());
                        }

                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                        for (var a = 0; a < currentEntry.ranks[i].allComponents.Count; a++)
                        {
                            GUILayout.Space(10);
                            if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                                currentEntry.ranks[i].allComponents[a].componentItemID,
                                "Item"))
                            {
                                currentEntry.ranks[i].allComponents.RemoveAt(a);
                                return;
                            }
                            
                            currentEntry.ranks[i].allComponents[a].componentItemID = 
                                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].allComponents[a].componentItemID, "Item", "Item", "");
                            
                            currentEntry.ranks[i].allComponents[a].count =
                                RPGBuilderEditorFields.DrawHorizontalIntField("Count:", "", RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].allComponents[a].count);
                        }

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    }
                    GUILayout.Space(30);
                }
                GUILayout.Space(20);
            }
        }

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGCraftingRecipe>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath + OldFolderName);
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
            entry.entryIcon = entry.icon;
            
            EditorUtility.SetDirty(entry);
        }
        
        FileUtil.DeleteFileOrDirectory(RPGBuilderEditor.Instance.EditorSettings.ResourcePath +
                                       RPGBuilderEditor.Instance.EditorSettings.DatabasePath +
                                       OldFolderName);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
