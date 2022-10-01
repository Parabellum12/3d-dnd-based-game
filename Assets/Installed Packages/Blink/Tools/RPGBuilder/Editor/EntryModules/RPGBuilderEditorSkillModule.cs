using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorSkillModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGSkill> entries = new Dictionary<int, RPGSkill>();
    private RPGSkill currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.skillFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }
    
    public override void LoadEntries()
    {
        Dictionary<int, RPGSkill> dictionary = new Dictionary<int, RPGSkill>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGSkill>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGSkill>();
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
        
        if (currentEntry.levelTemplateID == -1)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("Missing Levels", "Assign a valid level template", "OK");
            return false;
        }
        
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGSkill entryFile = (RPGSkill)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showBaseInfo)
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
            currentEntry.entryDescription =
                RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryDescription);
            currentEntry.levelTemplateID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(
                    currentEntry.levelTemplateID, "Levels", "Levels", "");
            currentEntry.automaticallyAdded =
                RPGBuilderEditorFields.DrawHorizontalToggle("Known?", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.automaticallyAdded);
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }


        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showStats =
            RPGBuilderEditorUtility.HandleModuleBanner("STATS",
                RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showStats);
        if (RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showStats)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.UseStatListTemplate =
                RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.UseStatListTemplate);
            
            if (currentEntry.UseStatListTemplate)
            {
                currentEntry.StatListTemplate =
                    (StatListTemplate) RPGBuilderEditorFields.DrawHorizontalObject<StatListTemplate>(
                        "Template", "", currentEntry.StatListTemplate);
            }
            else
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat", false))
                {
                    currentEntry.CustomStats.Add(new CombatData.CustomStatValues());
                }
                GUILayout.Space(10);
                currentEntry.CustomStats =
                    RPGBuilderEditorFields.DrawCustomStatValuesList(currentEntry.CustomStats, false);
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }


        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showTalentTrees =
            RPGBuilderEditorUtility.HandleModuleBanner("TALENT TREES",
                RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showTalentTrees);
        if (RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showTalentTrees)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Talent Tree", true))
            {
                currentEntry.talentTrees.Add(new RPGSkill.TalentTreesDATA());
            }

            var ThisList2 = serialObj.FindProperty("talentTrees");
            currentEntry.talentTrees =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList2) as List<RPGSkill.TalentTreesDATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.talentTrees.Count; a++)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.talentTrees[a].talentTreeID,
                    "TalentTree"))
                {
                    currentEntry.talentTrees.RemoveAt(a);
                    return;
                }

                currentEntry.talentTrees[a].talentTreeID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                        currentEntry.talentTrees[a].talentTreeID, "TalentTree",
                        "Talent Tree", "");
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }


        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showActionAbilities =
            RPGBuilderEditorUtility.HandleModuleBanner("ACTION ABILITIES",
                RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showActionAbilities);
        if (RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showActionAbilities)
        {
            RPGBuilderEditorFields.DrawActionAbilityList(currentEntry
                .actionAbilities);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showStartingItems =
            RPGBuilderEditorUtility.HandleModuleBanner("STARTING ITEMS",
                RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showStartingItems);
        if (RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showStartingItems)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Item", true))
            {
                currentEntry.startItems.Add(new RPGItemDATA.StartingItemsDATA());
            }

            var ThisList4 = serialObj.FindProperty("startItems");
            currentEntry.startItems =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList4) as List<RPGItemDATA.StartingItemsDATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.startItems.Count; a++)
            {
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.startItems[a].itemID,
                    "Item"))
                {
                    currentEntry.startItems.RemoveAt(a);
                    return;
                }
                
                currentEntry.startItems[a].itemID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.startItems[a].itemID, "Item", "Item", "");

                currentEntry.startItems[a].count =
                    RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.startItems[a].count);

                currentEntry.startItems[a].equipped =
                    RPGBuilderEditorFields.DrawHorizontalToggle("Equipped", "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.startItems[a].equipped);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showStatAllocationGame =
            RPGBuilderEditorUtility.HandleModuleBanner("STAT ALLOCATION",
                RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showStatAllocationGame);
        if (RPGBuilderEditor.Instance.EditorFilters.skillModuleSection.showStatAllocationGame)
        {
            currentEntry.allocatedStatsEntriesGame =
                RPGBuilderEditorFields.DrawStatAllocationList(currentEntry
                    .allocatedStatsEntriesGame);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGSkill>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
             RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryIcon = entry.icon;
            entry.entryDescription = entry.description;
            
            foreach (var stat in entry.stats)
            {
                CombatData.CustomStatValues newStat = new CombatData.CustomStatValues
                {
                    statID = stat.statID,
                    addedValue = stat.amount,
                    valuePerLevel = stat.bonusPerLevel,
                    Percent = stat.isPercent
                };
                entry.CustomStats.Add(newStat);
            }
            
            EditorUtility.SetDirty(entry);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
