using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorAIPhaseAbilitiesModule : RPGBuilderEditorModule
{
    private Dictionary<int, AIPhaseAbilitiesTemplate> entries = new Dictionary<int, AIPhaseAbilitiesTemplate>();
    private AIPhaseAbilitiesTemplate currentEntry;

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
        Dictionary<int, AIPhaseAbilitiesTemplate> dictionary = new Dictionary<int, AIPhaseAbilitiesTemplate>();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<AIPhaseAbilitiesTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
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

        currentEntry = CreateInstance<AIPhaseAbilitiesTemplate>();
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
        AIPhaseAbilitiesTemplate entryFile = (AIPhaseAbilitiesTemplate) updatedEntry;
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
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Ability List", "", true);
        currentEntry.CheckMaxAbilities = RPGBuilderEditorFields.DrawHorizontalToggle("Limited amount?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.CheckMaxAbilities);
        if (currentEntry.CheckMaxAbilities)
        {
            currentEntry.MaxAbilities = RPGBuilderEditorFields.DrawHorizontalIntField("Maximum Abilities", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.MaxAbilities);
        }
        
        GUILayout.Space(10);
        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Ability", true))
        {
            currentEntry.Abilities.Add(new AIData.AIPhaseAbility());
        }

        foreach (var ability in currentEntry.Abilities)
        {
            if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(ability.abilityID, "Ability"))
            {
                currentEntry.Abilities.Remove(ability);
                return;
            }

            ability.abilityID = RPGBuilderEditorFields.DrawDatabaseEntryField(ability.abilityID, "Ability", "Ability", "");
            
            ability.abilityRank = RPGBuilderEditorFields.DrawAbilityRankIndexField(ability.abilityID, ability.abilityRank);
            ability.RequirementsTemplate = (RequirementsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>("Requirements", "", ability.RequirementsTemplate);

            ability.optional = RPGBuilderEditorFields.DrawHorizontalToggle("Is optional?", "",
                RPGBuilderEditor.Instance.FieldHeight, ability.optional);
            if (ability.optional)
            {
                ability.chance = RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Chance", "", ability.chance);
            }
            ability.LimitedUse = RPGBuilderEditorFields.DrawHorizontalToggle("Limited use amount?", "",
                RPGBuilderEditor.Instance.FieldHeight, ability.LimitedUse);
            if (ability.LimitedUse)
            {
                ability.MaxUseAmount = RPGBuilderEditorFields.DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,  ability.MaxUseAmount);
            }
            GUILayout.Space(15);
        }
        
        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }
}
