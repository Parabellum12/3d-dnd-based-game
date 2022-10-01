using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorResourceModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGResourceNode> entries = new Dictionary<int, RPGResourceNode>();
    private RPGResourceNode currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.resourceFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGResourceNode> dictionary = new Dictionary<int, RPGResourceNode>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGResourceNode>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGResourceNode>();
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
        RPGResourceNode entryFile = (RPGResourceNode)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon =
                RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField(
                "Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField(
                "File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);

            currentEntry.learnedByDefault =
                RPGBuilderEditorFields.DrawHorizontalToggle("Known Automatically", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.learnedByDefault);

            currentEntry.skillRequiredID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(
                    currentEntry.skillRequiredID, "Skill", "Skill", "");

            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showRanks =
            RPGBuilderEditorUtility.HandleModuleBanner("RANKS",
                RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showRanks);
        if (RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showRanks)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Rank", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.MinWidth(150), GUILayout.ExpandWidth(true)))
            {
                var newRankDataElement = new RPGResourceNode.RPGResourceNodeRankData();
                currentEntry.ranks.Add(newRankDataElement);
            }

            if (currentEntry.ranks.Count > 0)
            {
                GUILayout.Space(20);
                if (GUILayout.Button("- Remove Rank",
                    RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"), GUILayout.MinWidth(150),
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
                    if (GUILayout.Button("Copy Above", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                        GUILayout.ExpandWidth(true)))
                    {
                        currentEntry.CopyEntryData(
                            currentEntry.ranks[i],
                            currentEntry.ranks[i - 1]);
                        GUI.FocusControl(null);
                    }
                }

                GUILayout.EndHorizontal();
                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

                if (currentEntry.ranks[i].ShowedInEditor)
                {
                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showTalentTreeSettings =
                        RPGBuilderEditorUtility.HandleModuleBanner("TALENT TREE SETTINGS",
                            RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showTalentTreeSettings);
                    if (RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showTalentTreeSettings)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                        currentEntry.ranks[i].unlockCost =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Unlock Cost", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].unlockCost);
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    }

                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showLootSettings =
                        RPGBuilderEditorUtility.HandleModuleBanner("SETTINGS",
                            RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showLootSettings);
                    if (RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showLootSettings)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);

                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Loot:", "");
                        currentEntry.ranks[i].lootTableID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(
                                currentEntry.ranks[i].lootTableID, "LootTable",
                                "Loot Table", "");

                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Settings:", "", true);
                        currentEntry.ranks[i].distanceMax =
                            RPGBuilderEditorFields.DrawHorizontalFloatField("Distance",
                                "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].distanceMax);
                        currentEntry.ranks[i].gatherTime =
                            RPGBuilderEditorFields.DrawHorizontalFloatField("Gather Time",
                                "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].gatherTime);

                        currentEntry.ranks[i].respawnTime =
                            RPGBuilderEditorFields.DrawHorizontalFloatField("Respawn Time",
                                "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].respawnTime);

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    }

                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showSkillSettings =
                        RPGBuilderEditorUtility.HandleModuleBanner("SKILL SETTINGS",
                            RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showSkillSettings);
                    if (RPGBuilderEditor.Instance.EditorFilters.resourceNodeModuleSection.showSkillSettings)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);

                        currentEntry.ranks[i].skillLevelRequired =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Level",
                                "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].skillLevelRequired);

                        currentEntry.ranks[i].Experience =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Experience",
                                "", RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].Experience);

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
        var allEntries = Resources.LoadAll<RPGResourceNode>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            entry.entryName = entry._name;
            AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
                                      RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
            entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryIcon = entry.icon;
            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
