using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorAIPhasesModule : RPGBuilderEditorModule
{
    private Dictionary<int, AIPhaseTemplate> entries = new Dictionary<int, AIPhaseTemplate>();
    private AIPhaseTemplate currentEntry;
    private readonly List<RPGBuilderDatabaseEntry> allNodeSockets = new List<RPGBuilderDatabaseEntry>();

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
        Dictionary<int, AIPhaseTemplate> dictionary = new Dictionary<int, AIPhaseTemplate>();
        allNodeSockets.Clear();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<AIPhaseTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                               AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;

        foreach (var typeEntry in Resources.LoadAll<RPGBNodeSocket>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allNodeSockets.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }

        currentEntry = CreateInstance<AIPhaseTemplate>();
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
        AIPhaseTemplate entryFile = (AIPhaseTemplate) updatedEntry;
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

        RPGBuilderEditorFields.DrawTitleLabelExpanded("On Spawn", "", true);
        currentEntry.SpawnedAnimationTemplate = (AnimationTemplate)
            RPGBuilderEditorFields.DrawHorizontalObject<AnimationTemplate>("Spawn Animation", "",
                currentEntry.SpawnedAnimationTemplate);
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Entering this phase:", "", true);
        currentEntry.TransitionDuration =
            RPGBuilderEditorFields.DrawHorizontalFloatField("Transition Duration", "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.TransitionDuration);
        currentEntry.EnterPhaseRequirementsTemplate =
            (RequirementsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>("Requirements", "", currentEntry.EnterPhaseRequirementsTemplate);
         
        GUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        RPGBuilderEditorFields.DrawTitleLabel("Potential Behaviors:", "", 145);
        GUILayout.Space(10);
        if (GUILayout.Button("+", RPGBuilderEditor.Instance.EditorSkin.GetStyle("SquareAddButton"),
            GUILayout.Width(20), GUILayout.Height(20)))
        {
            currentEntry.PotentialBehaviors.Add(new AIData.AIPhasePotentialBehavior());
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);

        foreach (var behavior in currentEntry.PotentialBehaviors)
        {
            EditorGUILayout.BeginHorizontal();
            behavior.BehaviorTemplate =
                (AIBehaviorTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIBehaviorTemplate>("Behavior", "",
                    behavior.BehaviorTemplate);
            if(RPGBuilderEditorFields.DrawSmallRemoveButton())
            {
                currentEntry.PotentialBehaviors.Remove(behavior);
                return;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
            behavior.chance = RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Chance", "", behavior.chance);
            GUILayout.Space(10);
        }

        float chanceLeft = 100;
        for (var index = 0; index < currentEntry.PotentialBehaviors.Count; index++)
        {
            var behavior = currentEntry.PotentialBehaviors[index];
            if (index == 0)
            {
                chanceLeft -= behavior.chance;
                continue;
            }

            if (behavior.chance > chanceLeft) behavior.chance = chanceLeft;
            chanceLeft -= behavior.chance;
            if (index + 1 == currentEntry.PotentialBehaviors.Count && chanceLeft > 0)
            {
                behavior.chance += chanceLeft;
            }
        }

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Actions", "", true);
        currentEntry.ActionsTemplate =
            (AIPhaseActionsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIPhaseActionsTemplate>(
                "Actions Template", "", currentEntry.ActionsTemplate);
        
        GUILayout.Space(10);
            currentEntry.VisualEffectEntries = RPGBuilderEditorFields.DrawVisualEffectsList(currentEntry.VisualEffectEntries, allNodeSockets);
        
        GUILayout.Space(10);
            currentEntry.AnimationEntries = RPGBuilderEditorFields.DrawAnimationsList(currentEntry.AnimationEntries);
        
        GUILayout.Space(10);
            currentEntry.SoundEntries = RPGBuilderEditorFields.DrawSoundsList(currentEntry.SoundEntries);

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }
}
