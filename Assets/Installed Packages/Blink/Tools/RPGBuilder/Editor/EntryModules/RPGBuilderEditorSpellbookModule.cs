using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorSpellbookModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGSpellbook> entries = new Dictionary<int, RPGSpellbook>();
    private RPGSpellbook currentEntry;
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.spellbookFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGSpellbook> dictionary = new Dictionary<int, RPGSpellbook>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGSpellbook>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGSpellbook>();
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
        RPGSpellbook entryFile = (RPGSpellbook)updatedEntry;
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
        
        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.spellbookModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.spellbookModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.spellbookModuleSection.showBaseInfo)
        {GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon = RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID( currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            currentEntry.entryDescription = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDescription);

            currentEntry.sourceType =
                (RPGSpellbook.spellbookSourceType) RPGBuilderEditorFields.DrawHorizontalEnum("Type", "",
                    (int)currentEntry.sourceType,
                    Enum.GetNames(typeof(RPGSpellbook.spellbookSourceType)));
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.spellbookModuleSection.showNodes =
            RPGBuilderEditorUtility.HandleModuleBanner("ABILITIES & BONUSES", RPGBuilderEditor.Instance.EditorFilters.spellbookModuleSection.showNodes);
        if (RPGBuilderEditor.Instance.EditorFilters.spellbookModuleSection.showNodes)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add", true))
            {
                currentEntry.nodeList.Add(new RPGSpellbook.Node_DATA());
            }

            var ThisList = serialObj.FindProperty("nodeList");
            currentEntry.nodeList =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList) as List<RPGSpellbook.Node_DATA>;

            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.nodeList.Count; a++)
            {
                GUILayout.Space(10);
                
                
                EditorGUILayout.BeginHorizontal();
                currentEntry.nodeList[a].nodeType =
                    (RPGSpellbook.SpellbookNodeType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                        (int)currentEntry.nodeList[a].nodeType,
                        Enum.GetNames(typeof(RPGSpellbook.SpellbookNodeType)));
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.nodeList.RemoveAt(a);
                    return;
                }
                EditorGUILayout.EndHorizontal();

                if (currentEntry.nodeList[a].nodeType == RPGSpellbook.SpellbookNodeType.ability)
                {
                    Texture icon = RPGBuilderEditor.Instance.EditorData.abilityNullSprite;
                    
                    RPGBuilderDatabaseEntry entryReference = RPGBuilderEditorUtility.GetEntryByID(currentEntry.nodeList[a].abilityID, "Ability");
                    
                    if (entryReference != null && entryReference.entryIcon != null) icon = entryReference.entryIcon.texture;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Box(icon, GUILayout.Width(40), GUILayout.Height(40));

                    EditorGUILayout.BeginVertical();
                    currentEntry.nodeList[a].abilityID = 
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.nodeList[a].abilityID, "Ability", "Ability", "");
                    
                    currentEntry.nodeList[a].unlockLevel =
                        RPGBuilderEditorFields.DrawHorizontalIntField("Level", "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.nodeList[a].unlockLevel);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    Texture icon = RPGBuilderEditor.Instance.EditorData.abilityNullSprite;
                    RPGBuilderDatabaseEntry entryReference = RPGBuilderEditorUtility.GetEntryByID(currentEntry.nodeList[a].bonusID, "Bonus");
                    
                    if (entryReference != null && entryReference.entryIcon != null) icon = entryReference.entryIcon.texture;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Box(icon, GUILayout.Width(40), GUILayout.Height(40));

                    EditorGUILayout.BeginVertical();

                    currentEntry.nodeList[a].bonusID = 
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.nodeList[a].bonusID, "Bonus", "Bonus", "");

                    currentEntry.nodeList[a].unlockLevel =
                        RPGBuilderEditorFields.DrawHorizontalIntField("Level", "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.nodeList[a].unlockLevel);
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }
        
        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGSpellbook>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
            EditorUtility.SetDirty(entry);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
