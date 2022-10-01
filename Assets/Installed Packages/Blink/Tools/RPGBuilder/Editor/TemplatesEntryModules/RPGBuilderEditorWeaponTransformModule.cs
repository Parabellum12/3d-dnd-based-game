using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorWeaponTransformModule : RPGBuilderEditorModule
{
    private Dictionary<int, WeaponTransformTemplate> entries = new Dictionary<int, WeaponTransformTemplate>();
    private WeaponTransformTemplate currentEntry;

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
        Dictionary<int, WeaponTransformTemplate> dictionary = new Dictionary<int, WeaponTransformTemplate>();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<WeaponTransformTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
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

        currentEntry = CreateInstance<WeaponTransformTemplate>();
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
        WeaponTransformTemplate entryFile = (WeaponTransformTemplate) updatedEntry;
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

        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Race", false))
        {
            currentEntry.WeaponTransforms.Add(new WeaponTransform());
        }
        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

        currentEntry.WeaponTransforms = RPGBuilderEditorFields.DrawWeaponTransformList(currentEntry.WeaponTransforms, "ANY HAND", false);

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }
}
