using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorGearSetModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGGearSet> entries = new Dictionary<int, RPGGearSet>();
    private RPGGearSet currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.gearsSetFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGGearSet> dictionary = new Dictionary<int, RPGGearSet>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGGearSet>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGGearSet>();
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
        RPGGearSet entryFile = (RPGGearSet)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.gearSetModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.gearSetModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.gearSetModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.merchantTableModuleSection.showItems =
            RPGBuilderEditorUtility.HandleModuleBanner("ITEMS",
                RPGBuilderEditor.Instance.EditorFilters.merchantTableModuleSection.showItems);
        if (RPGBuilderEditor.Instance.EditorFilters.merchantTableModuleSection.showItems)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Item", true))
            {
                currentEntry.itemsInSet.Add(new RPGGearSet.itemInSet());
            }

            var ThisList2 = serialObj.FindProperty("itemsInSet");
            currentEntry.itemsInSet =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList2) as List<RPGGearSet.itemInSet>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.itemsInSet.Count; a++)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.itemsInSet[a].itemID,
                    "Item"))
                {
                    currentEntry.itemsInSet.RemoveAt(a);
                    return;
                }

                currentEntry.itemsInSet[a].itemID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.itemsInSet[a].itemID, "Item", "", "");
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.gearSetModuleSection.showTiers =
            RPGBuilderEditorUtility.HandleModuleBanner("TIERS",
                RPGBuilderEditor.Instance.EditorFilters.gearSetModuleSection.showTiers);
        if (RPGBuilderEditor.Instance.EditorFilters.gearSetModuleSection.showTiers)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Tier", true))
            {
                currentEntry.gearSetTiers.Add(new RPGGearSet.GearSetTier());
            }

            var ThisList3 = serialObj.FindProperty("gearSetTiers");
            currentEntry.gearSetTiers =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList3) as List<RPGGearSet.GearSetTier>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.gearSetTiers.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                var requirementNumber = a + 1;
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Tier " + requirementNumber + ": ", "", false);

                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.gearSetTiers.RemoveAt(a);
                    return;
                }

                EditorGUILayout.EndHorizontal();

                RPGBuilderEditorFields.DrawTitleLabelExpanded("Condition:", "", true);
                currentEntry.gearSetTiers[a].equippedAmount = RPGBuilderEditorFields.DrawHorizontalIntField(
                    "Equipped Amount", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.gearSetTiers[a].equippedAmount);


                GUILayout.Space(10);
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Stat Bonuses:", "", true);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat", true))
                {
                    currentEntry.gearSetTiers[a].gearSetTierStats
                        .Add(new RPGGearSet.GearSetTier.GearSetTierStat());
                }

                for (var t = 0; t < currentEntry.gearSetTiers[a].gearSetTierStats.Count; t++)
                {
                    var statNumber = t + 1;
                    EditorGUILayout.BeginHorizontal();
                    RPGStat entryReference = (RPGStat)
                        RPGBuilderEditorUtility.GetEntryByID(currentEntry.gearSetTiers[a].gearSetTierStats[t].statID,
                            "Stat");

                    var effectName = "";
                    if (entryReference != null) effectName = entryReference.entryName;

                    RPGBuilderEditorFields.DrawTitleLabelExpanded(effectName, "");
                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                    {
                        currentEntry.gearSetTiers[a].gearSetTierStats.RemoveAt(t);
                        return;
                    }

                    EditorGUILayout.EndHorizontal();

                    currentEntry.gearSetTiers[a].gearSetTierStats[t].statID =
                        RPGBuilderEditorFields.DrawDatabaseEntryField(
                            currentEntry.gearSetTiers[a].gearSetTierStats[t].statID, "Stat", "Stat", "");
                    currentEntry.gearSetTiers[a].gearSetTierStats[t].amount =
                        RPGBuilderEditorFields.DrawHorizontalFloatField(
                            "Amount", "",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.gearSetTiers[a].gearSetTierStats[t].amount);


                    if (entryReference != null)
                    {
                        if (!entryReference.isPercentStat)
                        {
                            currentEntry.gearSetTiers[a].gearSetTierStats[t].isPercent =
                                RPGBuilderEditorFields.DrawHorizontalToggle(
                                    "Is Percent?", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.gearSetTiers[a].gearSetTierStats[t].isPercent);
                        }
                        else
                        {
                            currentEntry.gearSetTiers[a].gearSetTierStats[t].isPercent = false;
                        }
                    }
                }

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
     var allEntries = Resources.LoadAll<RPGGearSet>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
     foreach (var entry in allEntries)
     {
         EditorUtility.SetDirty(entry);
         entry.entryName = entry._name;
         AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
                                   RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
         entry.entryFileName = entry.entryName + AssetNameSuffix;
         entry.entryDisplayName = entry.displayName;
         EditorUtility.SetDirty(entry);
     }
        
     AssetDatabase.SaveAssets();
     AssetDatabase.Refresh();
 }
    
}
