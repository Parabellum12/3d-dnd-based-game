using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorAIBehaviorsModule : RPGBuilderEditorModule
{
    private Dictionary<int, AIBehaviorTemplate> entries = new Dictionary<int, AIBehaviorTemplate>();
    private AIBehaviorTemplate currentEntry;

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
        Dictionary<int, AIBehaviorTemplate> dictionary = new Dictionary<int, AIBehaviorTemplate>();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<AIBehaviorTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
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

        currentEntry = CreateInstance<AIBehaviorTemplate>();
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
        if (currentEntry.DefaultState == null)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("Default State Missing", "A Default State is required", "OK");
            return false;
        }
        if (currentEntry.DefaultStateTemplate == null)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("Default Template Missing", "A Default Template is required", "OK");
            return false;
        }
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        AIBehaviorTemplate entryFile = (AIBehaviorTemplate) updatedEntry;
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
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Phase Settings", "", true);
        currentEntry.PhaseCheckInterval = RPGBuilderEditorFields.DrawHorizontalFloatField("Check Interval", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.PhaseCheckInterval);
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Target Settings", "", true);
        currentEntry.MinDistanceFromTarget = RPGBuilderEditorFields.DrawHorizontalFloatField("Min. Target Distance", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.MinDistanceFromTarget);
        currentEntry.MaxDistanceFromTarget = RPGBuilderEditorFields.DrawHorizontalFloatField("Max. Target Distance", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.MaxDistanceFromTarget);
        currentEntry.LookAtTargetSpeed = RPGBuilderEditorFields.DrawHorizontalFloatField("Look at target speed", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.LookAtTargetSpeed);
        currentEntry.CheckTargetInterval = RPGBuilderEditorFields.DrawHorizontalFloatField("Target check interval", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.CheckTargetInterval);
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Reset Target", "", true);
        currentEntry.ResetTargetAfterDistanceFromSpawner = RPGBuilderEditorFields.DrawHorizontalToggle("If too far from spawner?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.ResetTargetAfterDistanceFromSpawner);
        if (currentEntry.ResetTargetAfterDistanceFromSpawner)
        {
            currentEntry.SpawnerDistanceMax = RPGBuilderEditorFields.DrawHorizontalFloatField("Spawner Distance Max", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.SpawnerDistanceMax);
        }
        currentEntry.ResetTargetAfterDistanceFromSpawnpoint = RPGBuilderEditorFields.DrawHorizontalToggle("If too far from spawn point?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.ResetTargetAfterDistanceFromSpawnpoint);
        if (currentEntry.ResetTargetAfterDistanceFromSpawnpoint)
        {
            currentEntry.SpawnPointDistanceMax = RPGBuilderEditorFields.DrawHorizontalFloatField("Spanw Point Distance Max", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.SpawnPointDistanceMax);
        }
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Animator Settings", "", true);
        currentEntry.StrafeParameterName = RPGBuilderEditorFields.DrawHorizontalTextField("Strafe Parameter", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.StrafeParameterName);
        currentEntry.CombatParameterName = RPGBuilderEditorFields.DrawHorizontalTextField("Combat Parameter", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.CombatParameterName);
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Aggro Settings", "", true);
        currentEntry.CanAggroAlly = RPGBuilderEditorFields.DrawHorizontalToggle("Can Aggro Allies?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.CanAggroAlly);
        currentEntry.CanAggroNeutral = RPGBuilderEditorFields.DrawHorizontalToggle("Can Aggro Neutral?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.CanAggroNeutral);
        currentEntry.CanAggroEnemy = RPGBuilderEditorFields.DrawHorizontalToggle("Can Aggro Enemy?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.CanAggroEnemy);
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Default State", "", true);
        currentEntry.DefaultState =
            (AIStateIdle) RPGBuilderEditorFields.DrawHorizontalObject<AIStateIdle>("State", "", currentEntry.DefaultState);
        if (currentEntry.DefaultState != null)
        {
            currentEntry.DefaultStateTemplate =
                (AIStateIdleTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIStateIdleTemplate>("Template", "",
                    currentEntry.DefaultStateTemplate);
        }
        currentEntry.DefaultPetState =
            (AIStateIdle) RPGBuilderEditorFields.DrawHorizontalObject<AIStateIdle>("Pet State", "", currentEntry.DefaultPetState);
        if (currentEntry.DefaultPetState != null)
        {
            currentEntry.DefaultPetStateTemplate =
                (AIStateIdleTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIStateIdleTemplate>("Pet Template", "",
                    currentEntry.DefaultPetStateTemplate);
        }

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Chase State", "", true);
        currentEntry.ChaseState =
            (AIStateChase) RPGBuilderEditorFields.DrawHorizontalObject<AIStateChase>("State", "", currentEntry.ChaseState);
        if (currentEntry.ChaseState != null)
        {
            currentEntry.ChaseTemplate =
                (AIStateChaseTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIStateChaseTemplate>("Template", "",
                    currentEntry.ChaseTemplate);
        }

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Combat Idle State", "", true);
        currentEntry.CombatIdleState =
            (AIStateCombatIdle) RPGBuilderEditorFields.DrawHorizontalObject<AIStateCombatIdle>("State", "", currentEntry.CombatIdleState);
        if (currentEntry.CombatIdleState != null)
        {
            currentEntry.CombatIdleTemplate =
                (AIStateCombatIdleTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIStateCombatIdleTemplate>(
                    "Template", "",
                    currentEntry.CombatIdleTemplate);
        }

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Combat State", "", true);
        currentEntry.CombatState =
            (AIStateCombat) RPGBuilderEditorFields.DrawHorizontalObject<AIStateCombat>("State", "", currentEntry.CombatState);
        if (currentEntry.CombatState != null)
        {
            currentEntry.CombatTemplate =
                (AIStateCombatTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIStateCombatTemplate>("Template",
                    "",
                    currentEntry.CombatTemplate);
        }

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Walk Backward State", "", true);
        currentEntry.WalkBackwardState =
            (AIStateWalkBackward) RPGBuilderEditorFields.DrawHorizontalObject<AIStateWalkBackward>("State", "", currentEntry.WalkBackwardState);
        if (currentEntry.WalkBackwardState != null)
        {
            currentEntry.WalkBackwardTemplate =
                (AIStateWalkBackwardTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIStateWalkBackwardTemplate>(
                    "Template", "",
                    currentEntry.WalkBackwardTemplate);
        }

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Flee State", "", true);
        currentEntry.FleeState =
            (AIStateFlee) RPGBuilderEditorFields.DrawHorizontalObject<AIStateFlee>("State", "", currentEntry.FleeState);
        if (currentEntry.FleeState != null)
        {
            currentEntry.FleeTemplate =
                (AIStateFleeTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIStateFleeTemplate>("Template", "",
                    currentEntry.FleeTemplate);
            currentEntry.FleeRequirementsTemplate = (RequirementsTemplate)
                RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>("Requirements Template", "",
                    currentEntry.FleeRequirementsTemplate);
            currentEntry.FleeCheckInterval = RPGBuilderEditorFields.DrawHorizontalFloatField("Flee check interval", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.FleeCheckInterval);
        }

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        RPGBuilderEditorFields.DrawTitleLabel("Potential Abilities:", "", 145);
        GUILayout.Space(10);
        if (GUILayout.Button("+", RPGBuilderEditor.Instance.EditorSkin.GetStyle("SquareAddButton"),
            GUILayout.Width(20), GUILayout.Height(20)))
        {
            currentEntry.PotentialAbilities.Add(new AIData.AIPhasePotentialAbilities());
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        for (var index = 0; index < currentEntry.PotentialAbilities.Count; index++)
        {
            EditorGUILayout.BeginHorizontal();
            currentEntry.PotentialAbilities[index].AbilitiesTemplate = (AIPhaseAbilitiesTemplate)
                RPGBuilderEditorFields.DrawHorizontalObject<AIPhaseAbilitiesTemplate>("Ability Template", "",
                    currentEntry.PotentialAbilities[index].AbilitiesTemplate);
            if(RPGBuilderEditorFields.DrawSmallRemoveButton())
            {
                currentEntry.PotentialAbilities.Remove(currentEntry.PotentialAbilities[index]);
                return;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
            currentEntry.PotentialAbilities[index].chance = RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Chance", "", currentEntry.PotentialAbilities[index].chance);
            GUILayout.Space(10);
        }
        
        float chanceLeft2 = 100;
        for (var index = 0; index < currentEntry.PotentialAbilities.Count; index++)
        {
            var abilityTemplate = currentEntry.PotentialAbilities[index];
            if (index == 0)
            {
                chanceLeft2 -= abilityTemplate.chance;
                continue;
            }

            if (abilityTemplate.chance > chanceLeft2) abilityTemplate.chance = chanceLeft2;
            chanceLeft2 -= abilityTemplate.chance;
            if (index + 1 == currentEntry.PotentialAbilities.Count && chanceLeft2 > 0)
            {
                abilityTemplate.chance += chanceLeft2;
            }
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }
}
