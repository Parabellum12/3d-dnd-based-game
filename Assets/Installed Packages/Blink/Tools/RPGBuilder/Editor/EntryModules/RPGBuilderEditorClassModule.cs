using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorClassModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGClass> entries = new Dictionary<int, RPGClass>();
    private RPGClass currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.classFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGClass> dictionary = new Dictionary<int, RPGClass>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGClass>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGClass>();
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
        RPGClass entryFile = (RPGClass)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon = RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            currentEntry.entryDescription = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDescription);

            currentEntry.levelTemplateID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.levelTemplateID, "Levels",
                    "Levels", "");
            currentEntry.autoAttackAbilityID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.autoAttackAbilityID, "Ability",
                    "Auto Attack", "");

            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStats =
            RPGBuilderEditorUtility.HandleModuleBanner("STATS", RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStats);
        if (RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStats)
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
        RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showTalentTree =
            RPGBuilderEditorUtility.HandleModuleBanner("TALENT TREES",
                RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showTalentTree);
        if (RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showTalentTree)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Talent Tree", true))
            {
                currentEntry.talentTrees.Add(new RPGClass.TalentTreesDATA());
            }

            var ThisList2 = serialObj.FindProperty("talentTrees");
            currentEntry.talentTrees = RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList2) as List<RPGClass.TalentTreesDATA>;

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
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.talentTrees[a].talentTreeID,
                        "TalentTree", "Talent Tree", "");
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showSpellbook =
            RPGBuilderEditorUtility.HandleModuleBanner("SPELLBOOK", RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showSpellbook);
        if (RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showSpellbook)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Spellbook", true))
            {
                currentEntry.spellbooks.Add(new RPGClass.SpellbookDATA());
            }

            var ThisList18 = serialObj.FindProperty("spellbooks");
            currentEntry.spellbooks = RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList18) as List<RPGClass.SpellbookDATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.spellbooks.Count; a++)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.spellbooks[a].spellbookID,
                    "Spellbook"))
                {
                    currentEntry.spellbooks.RemoveAt(a);
                    return;
                }
                
                currentEntry.spellbooks[a].spellbookID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.spellbooks[a].spellbookID,
                        "Spellbook", "Spellbook", "");
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showActionAbilities =
            RPGBuilderEditorUtility.HandleModuleBanner("ACTION ABILITIES",
                RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showActionAbilities);
        if (RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showActionAbilities)
        {
            RPGBuilderEditorFields.DrawActionAbilityList(currentEntry.actionAbilities);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStartingItems =
            RPGBuilderEditorUtility.HandleModuleBanner("STARTING ITEMS",
                RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStartingItems);
        if (RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStartingItems)
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
        RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStatAllocation =
            RPGBuilderEditorUtility.HandleModuleBanner("STAT ALLOCATION MENU",
                RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStatAllocation);
        if (RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStatAllocation)
        {
            GUILayout.Space(10);

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.allocationStatPoints =
                RPGBuilderEditorFields.DrawHorizontalIntField("Points To Allocate", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.allocationStatPoints);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);

            currentEntry.allocatedStatsEntries =
                RPGBuilderEditorFields.DrawStatAllocationList(currentEntry.allocatedStatsEntries);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStatAllocationGame =
            RPGBuilderEditorUtility.HandleModuleBanner("STAT ALLOCATION GAME",
                RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStatAllocationGame);
        if (RPGBuilderEditor.Instance.EditorFilters.classModuleSection.showStatAllocationGame)
        {
            currentEntry.allocatedStatsEntriesGame =
                RPGBuilderEditorFields.DrawStatAllocationList(currentEntry.allocatedStatsEntriesGame);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGClass>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            entry.entryName = entry._name;
            AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath +
                                      RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" +
                                      entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
            entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryIcon = entry.icon;

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
