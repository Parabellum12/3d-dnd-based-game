using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorComboModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGCombo> entries = new Dictionary<int, RPGCombo>();
    private RPGCombo currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.comboFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGCombo> dictionary = new Dictionary<int, RPGCombo>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGCombo>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGCombo>();
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
        RPGCombo entryFile = (RPGCombo)updatedEntry;
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

        RPGBuilderEditor.Instance.EditorFilters.comboModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.comboModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.comboModuleSection.showBaseInfo)
        {
            GUILayout.Space(5);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField(
                "Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            currentEntry.entryDescription =
                RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryDescription);
            currentEntry.initialAbilityID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(
                    currentEntry.initialAbilityID, "Ability", "Start Ability", "");

            currentEntry.StartCancelOtherCombos =
                RPGBuilderEditorFields.DrawHorizontalToggle(
                    "Reset Other Combos?", "Should this combo reset previously active combos when started?",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.StartCancelOtherCombos);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.comboModuleSection.showCombos =
            RPGBuilderEditorUtility.HandleModuleBanner("COMBOS",
                RPGBuilderEditor.Instance.EditorFilters.comboModuleSection.showCombos);
        if (RPGBuilderEditor.Instance.EditorFilters.comboModuleSection.showCombos)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Combo", true))
            {
                currentEntry.combos.Add(new RPGCombo.ComboEntry());
            }

            var ThisList5 = serialObj.FindProperty("combos");
            currentEntry.combos =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList5) as List<RPGCombo.ComboEntry>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.combos.Count; a++)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.combos[a].abilityID,
                    "Ability"))
                {
                    currentEntry.combos.RemoveAt(a);
                    return;
                }
                
                currentEntry.combos[a].abilityID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                        currentEntry.combos[a].abilityID, "Ability", "Ability", "");

                currentEntry.combos[a].abMustBeKnown =
                    RPGBuilderEditorFields.DrawHorizontalToggle(
                        "Ability Known?", "Should this ability be known to be used?",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.combos[a].abMustBeKnown);
                
                currentEntry.combos[a].expireTime =
                    RPGBuilderEditorFields.DrawHorizontalFloatField(
                        "Expire Time",
                        "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.combos[a].expireTime);
                currentEntry.combos[a].readyTime =
                    RPGBuilderEditorFields.DrawHorizontalFloatField(
                        "Charging Time",
                        "How long in seconds will it be possible to use",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.combos[a].readyTime);

                currentEntry.combos[a].keyType = 
                    (RPGCombo.KeyType) RPGBuilderEditorFields.DrawHorizontalEnum("Override Key", "", 
                        (int)currentEntry.combos[a].keyType,
                        Enum.GetNames(typeof(RPGCombo.KeyType)));

                switch (currentEntry.combos[a].keyType)
                {
                    case RPGCombo.KeyType.StartAbilityKey:
                        break;
                    case RPGCombo.KeyType.OverrideKey:
                        currentEntry.combos[a].overrideKey = 
                            (KeyCode) RPGBuilderEditorFields.DrawHorizontalEnum("Key Type", "", 
                                (int)currentEntry.combos[a].overrideKey,
                                Enum.GetNames(typeof(KeyCode)));
                        break;
                    case RPGCombo.KeyType.ActionKey:
                        var actionKeyIndex =
                            RPGBuilderEditorUtility.GetIndexFromActionKey(currentEntry
                                .combos[a].actionKeyName);
                        if (actionKeyIndex == -1) actionKeyIndex = 0;
                        List<string> allActionKeyNames = RPGBuilderEditorUtility.GetActionKeyNamesList();
                        RPGBuilderEditorFields.DrawHorizontalLabel("Action Key", "");
                        var tempIndex = EditorGUILayout.Popup(actionKeyIndex,
                            RPGBuilderEditorUtility.GetActionKeyNamesList().ToArray());
                        EditorGUILayout.EndHorizontal();
                        if (RPGBuilderEditor.Instance.GeneralSettings.actionKeys.Count > 0)
                            currentEntry.combos[a].actionKeyName =
                                allActionKeyNames[tempIndex];

                        break;
                }

                /*GUILayout.Space(10);
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Requirements", "");
                currentEntry.combos[a].UseRequirementsTemplate = 
                    RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.combos[a].UseRequirementsTemplate);

                if (currentEntry.combos[a].UseRequirementsTemplate)
                {
                    currentEntry.combos[a].RequirementsTemplate = (RequirementsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>(
                        "Template", "", currentEntry.combos[a].RequirementsTemplate);
                }
                else
                {
                    GUILayout.Space(10);
                    if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Requirement Group", true))
                    {
                        currentEntry.combos[a].Requirements.Add(new RequirementsData.RequirementGroup());
                    }

                    currentEntry.combos[a].Requirements = RPGBuilderEditorFields.DrawRequirementGroupsList(currentEntry.combos[a].Requirements,false);
                }*/
                        
                GUILayout.Space(10);

            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGCombo>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            entry.entryName = entry._name;
            AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
                                      RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry.fileName + ".asset", entry.entryName + AssetNameSuffix);
            entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryDescription = entry.description;
            EditorUtility.SetDirty(entry);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
