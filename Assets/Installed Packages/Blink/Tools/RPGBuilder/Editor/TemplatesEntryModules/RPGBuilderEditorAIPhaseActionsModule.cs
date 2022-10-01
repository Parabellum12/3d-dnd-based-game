using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorAIPhaseActionsModule : RPGBuilderEditorModule
{
    private Dictionary<int, AIPhaseActionsTemplate> entries = new Dictionary<int, AIPhaseActionsTemplate>();
    private AIPhaseActionsTemplate currentEntry;

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
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, AIPhaseActionsTemplate> dictionary = new Dictionary<int, AIPhaseActionsTemplate>();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<AIPhaseActionsTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                      AssetFolderName);
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

        currentEntry = CreateInstance<AIPhaseActionsTemplate>();
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
        AIPhaseActionsTemplate entryFile = (AIPhaseActionsTemplate) updatedEntry;
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

        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        currentEntry.entryName = RPGBuilderEditorFields.DrawHorizontalTextField("Name",
            "", RPGBuilderEditor.Instance.FieldHeight + 15, currentEntry.entryName);
        currentEntry.entryFileName = currentEntry.entryName + AssetNameSuffix;
        
        GUILayout.Space(10);
        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Action", true))
        {
            currentEntry.PhaseActions.Add(new AIData.AIPhaseAction());
        }
        GUILayout.Space(10);

        foreach (var action in currentEntry.PhaseActions)
        {
            EditorGUILayout.BeginHorizontal();
            action.Activation = (AIData.ActivationType) RPGBuilderEditorFields.DrawHorizontalEnum("Activate On", "",
                (int)action.Activation, Enum.GetNames(typeof(AIData.ActivationType)));
            if(RPGBuilderEditorFields.DrawSmallRemoveButton())
            {
                currentEntry.PhaseActions.Remove(action);
                return;
            }
            EditorGUILayout.EndHorizontal();
            
            action.RequirementsTarget =
                (RPGCombatDATA.TARGET_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Required on", "",
                    (int)action.RequirementsTarget,
                    Enum.GetNames(typeof(RPGCombatDATA.TARGET_TYPE)));

            action.RequirementsTemplate = (RequirementsTemplate)
                RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>("Requirements",
                    "", action.RequirementsTemplate);

            action.ActionsTarget =
                (RPGCombatDATA.TARGET_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Triggered On", "",
                    (int)action.ActionsTarget,
                    Enum.GetNames(typeof(RPGCombatDATA.TARGET_TYPE)));

            action.GameActionsTemplate = (GameActionsTemplate)
                RPGBuilderEditorFields.DrawHorizontalObject<GameActionsTemplate>("Actions",
                    "", action.GameActionsTemplate);
            
            GUILayout.Space(15);
        }
        
        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }
}
