using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorFactionModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGFaction> entries = new Dictionary<int, RPGFaction>();
    private RPGFaction currentEntry;

    private readonly List<RPGBuilderDatabaseEntry> allFactionStances = new List<RPGBuilderDatabaseEntry>();

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

        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.factionFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }


    public override void LoadEntries()
    {
        Dictionary<int, RPGFaction> dictionary = new Dictionary<int, RPGFaction>();
        databaseEntries.Clear();
        allFactionStances.Clear();
        var allEntries =
            Resources.LoadAll<RPGFaction>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;

        foreach (var typeEntry in Resources.LoadAll<RPGBFactionStance>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allFactionStances.Add(typeEntry);
        }
    }


    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }

        currentEntry = CreateInstance<RPGFaction>();
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
        RPGFaction entryFile = (RPGFaction) updatedEntry;
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

        RPGBuilderEditor.Instance.EditorFilters.factionModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.factionModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.factionModuleSection.showBaseInfo)
        {
            GUILayout.Space(5);
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
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.factionModuleSection.showStances =
            RPGBuilderEditorUtility.HandleModuleBanner("STANCES",
                RPGBuilderEditor.Instance.EditorFilters.factionModuleSection.showStances);
        if (RPGBuilderEditor.Instance.EditorFilters.factionModuleSection.showStances)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stance", true))
            {
                currentEntry.factionStances.Add(new RPGFaction.Faction_Stance_DATA());
            }

            var ThisList = serialObj.FindProperty("factionStances");
            currentEntry.factionStances =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList) as List<RPGFaction.Faction_Stance_DATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.factionStances.Count; a++)
            {
                GUILayout.Space(10);

                RPGBuilderEditorFields.DrawHorizontalLabel("Stance", "");
                int weaponSlotIndex = EditorGUILayout.Popup(
                    RPGBuilderEditorUtility.GetTypeEntryIndex(allFactionStances,
                        currentEntry.factionStances[a].FactionStance),
                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allFactionStances.ToArray()));
                currentEntry.factionStances[a].FactionStance = (RPGBFactionStance) allFactionStances[weaponSlotIndex];
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.factionStances.RemoveAt(a);
                    return;
                }
                EditorGUILayout.EndHorizontal();

                currentEntry.factionStances[a].AlignementToPlayer =
                    (CombatData.EntityAlignment) RPGBuilderEditorFields.DrawHorizontalEnum("Player Alignment", "The player alignment to this faction when this stance is active",
                        (int)currentEntry.factionStances[a].AlignementToPlayer,
                        Enum.GetNames(typeof(CombatData.EntityAlignment)));

                currentEntry.factionStances[a].pointsRequired =
                    RPGBuilderEditorFields.DrawHorizontalIntField("Points Required",
                        "The amount of points required to reach this stance",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.factionStances[a].pointsRequired);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.factionModuleSection.showInteractions =
            RPGBuilderEditorUtility.HandleModuleBanner("INTERACTIONS",
                RPGBuilderEditor.Instance.EditorFilters.factionModuleSection.showInteractions);
        if (RPGBuilderEditor.Instance.EditorFilters.factionModuleSection.showInteractions)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Interaction", true))
            {
                currentEntry.factionInteractions.Add(new RPGFaction.Faction_Interaction_DATA());
            }

            var ThisList2 = serialObj.FindProperty("factionInteractions");
            currentEntry.factionInteractions =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList2) as
                    List<RPGFaction.Faction_Interaction_DATA>;

            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.factionInteractions.Count; a++)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.factionInteractions[a].factionID,
                    "Faction"))
                {
                    currentEntry.factionInteractions.RemoveAt(a);
                    return;
                }

                currentEntry.factionInteractions[a].factionID = RPGBuilderEditorFields.DrawDatabaseEntryField(
                    currentEntry.factionInteractions[a].factionID,
                    "Faction", "Faction", "");

                RPGBuilderEditorFields.DrawHorizontalLabel("Stance", "");
                int weaponSlotIndex = EditorGUILayout.Popup(
                    RPGBuilderEditorUtility.GetTypeEntryIndex(allFactionStances,
                        currentEntry.factionInteractions[a].DefaultFactionStance),
                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allFactionStances.ToArray()));
                currentEntry.factionInteractions[a].DefaultFactionStance =
                    (RPGBFactionStance) allFactionStances[weaponSlotIndex];
                EditorGUILayout.EndHorizontal();

                currentEntry.factionInteractions[a].startingPoints =
                    RPGBuilderEditorFields.DrawHorizontalIntField("Start Points",
                        "The amount of points the stance start by default",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.factionInteractions[a].startingPoints);

                GUILayout.Space(10);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }

    public override void ConvertDatabaseEntriesAfterUpdate()
    {
        var allEntries =
            Resources.LoadAll<RPGFaction>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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

            foreach (var stance in entry.factionStances)
            {
                if (stance.playerAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ALLY)
                    stance.AlignementToPlayer = CombatData.EntityAlignment.Ally;
                if (stance.playerAlignment == RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL)
                    stance.AlignementToPlayer = CombatData.EntityAlignment.Neutral;
                if (stance.playerAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ENEMY)
                    stance.AlignementToPlayer = CombatData.EntityAlignment.Enemy;
            }
            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public override void ConvertStringsToTypeEntries()
    {
        var allEntries = Resources.LoadAll<RPGFaction>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        RPGBuilderEditorModule factionStances = RPGBuilderEditorUtility.GetModuleByName("Faction Stances");
        
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            {
                foreach (var stance in entry.factionStances)
                {
                    RPGBuilderDatabaseEntry entryFile = factionStances.GetEntryByName(stance.stance);
                    if (entryFile != null)
                    {
                        stance.FactionStance = (RPGBFactionStance) entryFile;
                    }
                }

                foreach (var interaction in entry.factionInteractions)
                {
                    RPGBuilderDatabaseEntry entryFile = factionStances.GetEntryByName(interaction.defaultStance);
                    if (entryFile != null)
                    {
                        interaction.DefaultFactionStance = (RPGBFactionStance) entryFile;
                    }
                }
            }

            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

    

