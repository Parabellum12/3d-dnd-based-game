using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorBodyCullingModule : RPGBuilderEditorModule
{
    private Dictionary<int, BodyCullingTemplate> entries = new Dictionary<int, BodyCullingTemplate>();
    private BodyCullingTemplate currentEntry;
    private readonly List<RPGBuilderDatabaseEntry> allGenders = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allBodyParts = new List<RPGBuilderDatabaseEntry>();

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
        Dictionary<int, BodyCullingTemplate> dictionary = new Dictionary<int, BodyCullingTemplate>();
        databaseEntries.Clear();
        allGenders.Clear();
        allBodyParts.Clear();
        var allEntries =
            Resources.LoadAll<BodyCullingTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                   AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBGender>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allGenders.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBBodyPart>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allBodyParts.Add(typeEntry);
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

        currentEntry = CreateInstance<BodyCullingTemplate>();
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
        BodyCullingTemplate entryFile = (BodyCullingTemplate) updatedEntry;
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

        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Race", false))
        {
            currentEntry.HiddenBodyParts.Add(new HiddenBodyPart());
        }

        for (var a = 0; a < currentEntry.HiddenBodyParts.Count; a++)
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            currentEntry.HiddenBodyParts[a].raceID = RPGBuilderEditorFields.DrawDatabaseEntryField(
                currentEntry.HiddenBodyParts[a].raceID,
                "Race", "Race", "");
            if (RPGBuilderEditorFields.DrawSmallRemoveButton())
            {
                currentEntry.HiddenBodyParts.RemoveAt(a);
                return;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Gender", true))
            {
                currentEntry.HiddenBodyParts[a].Values.Add(new HiddenBodyPartValues());
            }

            for (var u = 0; u < currentEntry.HiddenBodyParts[a].Values.Count; u++)
            {
                GUILayout.Space(10);

                RPGBuilderEditorUtility.StartHorizontalMargin(40, true);
                EditorGUILayout.BeginHorizontal();
                int gender = EditorGUILayout.Popup(
                    RPGBuilderEditorUtility.GetTypeEntryIndex(allGenders,
                        currentEntry.HiddenBodyParts[a].Values[u].Gender),
                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allGenders.ToArray()));
                currentEntry.HiddenBodyParts[a].Values[u].Gender = (RPGBGender) allGenders[gender];
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.HiddenBodyParts[a].Values.RemoveAt(u);
                    return;
                }
                GUILayout.Space(5);

                if (GUILayout.Button("+", RPGBuilderEditor.Instance.EditorSkin.GetStyle("SquareAddButton"),
                    GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (allBodyParts.Count > 0)
                    {
                        currentEntry.HiddenBodyParts[a].Values[u].BodyParts.Add((RPGBBodyPart) allBodyParts[0]);
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
                
                RPGBuilderEditorUtility.StartHorizontalMargin(50, true);
                for (var t = 0; t < currentEntry.HiddenBodyParts[a].Values[u].BodyParts.Count; t++)
                {
                    EditorGUILayout.BeginHorizontal();
                    int bodyPartIndex = EditorGUILayout.Popup(
                        RPGBuilderEditorUtility.GetTypeEntryIndex(allBodyParts,
                            currentEntry.HiddenBodyParts[a].Values[u].BodyParts[t]),
                        RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allBodyParts.ToArray()));
                    currentEntry.HiddenBodyParts[a].Values[u].BodyParts[t] = (RPGBBodyPart) allBodyParts[bodyPartIndex];
                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                    {
                        currentEntry.HiddenBodyParts[a].Values[u].BodyParts.RemoveAt(t);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(3);
                }
                RPGBuilderEditorUtility.EndHorizontalMargin(90, true);
                RPGBuilderEditorUtility.EndHorizontalMargin(40, true);
            }
            
            GUILayout.Space(25);
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }
}
