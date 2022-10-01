using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorAnimationModule : RPGBuilderEditorModule
{
    private Dictionary<int, AnimationTemplate> entries = new Dictionary<int, AnimationTemplate>();
    private AnimationTemplate currentEntry;
    
    ScriptableObject scriptableObj;
    SerializedObject serialObj;

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
        
        InitSerializedObject();
    }

    protected void InitSerializedObject()
    {
        
        scriptableObj = currentEntry;
        serialObj = new SerializedObject(scriptableObj);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
        
        InitSerializedObject();
    }

    public override void LoadEntries()
    {
        Dictionary<int, AnimationTemplate> dictionary = new Dictionary<int, AnimationTemplate>();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<AnimationTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
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

        currentEntry = CreateInstance<AnimationTemplate>();
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
        AnimationTemplate entryFile = (AnimationTemplate) updatedEntry;
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

#if !UNITY_2021
        InitSerializedObject();
#endif

        RPGBuilderEditorUtility.UpdateViewAndFieldData();

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);
        
        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.MaxWidth(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.ExpandHeight(true));

        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Parameter Settings", "");
        currentEntry.entryName = RPGBuilderEditorFields.DrawHorizontalTextField("Name",
            "", RPGBuilderEditor.Instance.FieldHeight + 15, currentEntry.entryName);
        currentEntry.entryFileName = currentEntry.entryName + AssetNameSuffix;
        
        currentEntry.ParameterType = (AnimationParameterType) RPGBuilderEditorFields.DrawHorizontalEnum("Type", "",
            (int)currentEntry.ParameterType, Enum.GetNames(typeof(AnimationParameterType)));
        
        currentEntry.EntryParameterType = (AnimationEntryParameterType) RPGBuilderEditorFields.DrawHorizontalEnum("Parameter Type", "",
            (int)currentEntry.EntryParameterType,
            Enum.GetNames(typeof(AnimationEntryParameterType)));

        switch (currentEntry.EntryParameterType)
        {
            case AnimationEntryParameterType.Single:
                currentEntry.ParameterName = RPGBuilderEditorFields.DrawHorizontalTextField("Parameter Name", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ParameterName);
                break;
            case AnimationEntryParameterType.List:
            case AnimationEntryParameterType.Sequence:
                var serialProp = serialObj.FindProperty("ParameterNames");
                EditorGUILayout.PropertyField(serialProp, true);
                break;
        }

        switch (currentEntry.ParameterType)
        {
            case AnimationParameterType.Bool:
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Bool Settings", "", true);
                currentEntry.IsToggle = RPGBuilderEditorFields.DrawHorizontalToggle("Toggle?", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.IsToggle);
                if (!currentEntry.IsToggle)
                {currentEntry.BoolValue = RPGBuilderEditorFields.DrawHorizontalToggle("Bool Value", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.BoolValue);
                    currentEntry.ResetAfterDuration = RPGBuilderEditorFields.DrawHorizontalToggle("Reset after duration?",
                        "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.ResetAfterDuration);
                    if (currentEntry.ResetAfterDuration)
                    {
                        currentEntry.Duration = RPGBuilderEditorFields.DrawHorizontalFloatField("Duration", "",
                            RPGBuilderEditor.Instance.FieldHeight, currentEntry.Duration);
                    }
                
                    RPGBuilderEditorFields.DrawTitleLabelExpanded("Toggle Settings", "", true);
                    currentEntry.ToggleOtherBool = RPGBuilderEditorFields.DrawHorizontalToggle("Toggle other bool?",
                        "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.ToggleOtherBool);
                    if (currentEntry.ToggleOtherBool)
                    {
                        currentEntry.ToggledParameterName = RPGBuilderEditorFields.DrawHorizontalTextField("Toggled Parameter Name", "",
                            RPGBuilderEditor.Instance.FieldHeight, currentEntry.ToggledParameterName);
                        currentEntry.ToggledBoolValue = RPGBuilderEditorFields.DrawHorizontalToggle("Toggled Bool Value", "",
                            RPGBuilderEditor.Instance.FieldHeight, currentEntry.ToggledBoolValue);
                    }
                }
                
                break;
            case AnimationParameterType.Int:
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Int Settings", "", true);
                currentEntry.IntValue = RPGBuilderEditorFields.DrawHorizontalIntField("Value", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.IntValue);
                break;
            case AnimationParameterType.Float:
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Float Settings", "", true);
                currentEntry.FloatValue = RPGBuilderEditorFields.DrawHorizontalFloatField("Value", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.FloatValue);
                break;
        }
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Root Motion Settings", "", true);
        currentEntry.EnableRootMotion = RPGBuilderEditorFields.DrawHorizontalToggle("Enable Root Motion?",
            "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.EnableRootMotion);
        if (currentEntry.EnableRootMotion)
        {
            currentEntry.RootMotionDuration = RPGBuilderEditorFields.DrawHorizontalFloatField("Root Motion Duration", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.RootMotionDuration);
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }

    public override void ConvertDatabaseEntriesAfterUpdate()
    {

    }
}
