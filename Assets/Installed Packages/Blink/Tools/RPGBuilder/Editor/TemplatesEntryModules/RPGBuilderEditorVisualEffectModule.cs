using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorVisualEffectModule : RPGBuilderEditorModule
{
    private Dictionary<int, VisualEffectTemplate> entries = new Dictionary<int, VisualEffectTemplate>();
    private VisualEffectTemplate currentEntry;
    
    private readonly List<RPGBuilderDatabaseEntry> allNodeSockets = new List<RPGBuilderDatabaseEntry>();
    
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
        Dictionary<int, VisualEffectTemplate> dictionary = new Dictionary<int, VisualEffectTemplate>();
        databaseEntries.Clear();
        allNodeSockets.Clear();
        var allEntries = Resources.LoadAll<VisualEffectTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<VisualEffectTemplate>();
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
        VisualEffectTemplate entryFile = (VisualEffectTemplate)updatedEntry;
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
        currentEntry.entryName = RPGBuilderEditorFields.DrawHorizontalTextField("Name",
            "", RPGBuilderEditor.Instance.FieldHeight + 15, currentEntry.entryName);
        currentEntry.entryFileName = currentEntry.entryName + AssetNameSuffix;

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Visual:", "");
        var serialProp = serialObj.FindProperty("Prefabs");
        EditorGUILayout.PropertyField(serialProp, true);
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Sound:", "", true);
        var serialProp2 = serialObj.FindProperty("SoundTemplates");
        EditorGUILayout.PropertyField(serialProp2, true);
        currentEntry.ParentSoundToPrefab = RPGBuilderEditorFields.DrawHorizontalToggle("Attach Sound To Prefab?", "",
            RPGBuilderEditor.Instance.FieldHeight,
            currentEntry.ParentSoundToPrefab);

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Destroy Conditions:", "", true);

        currentEntry.IsDestroyedOnDeath =
            RPGBuilderEditorFields.DrawHorizontalToggle("Death", "Is this visual effect destroyed if the caster dies?",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.IsDestroyedOnDeath);
        currentEntry.IsDestroyedOnStun =
            RPGBuilderEditorFields.DrawHorizontalToggle("Stun",
                "Is this visual effect destroyed if the caster is stunned?", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.IsDestroyedOnStun);
        currentEntry.IsDestroyedOnStealth =
            RPGBuilderEditorFields.DrawHorizontalToggle("Stealth",
                "Is this visual effect destroyed if the caster stealth?", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.IsDestroyedOnStealth);
        currentEntry.IsDestroyedOnStealthEnd =
            RPGBuilderEditorFields.DrawHorizontalToggle("Stealth End",
                "Is this visual effect destroyed if the caster go out of stealth?",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.IsDestroyedOnStealthEnd);

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }

    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        
    }
}
