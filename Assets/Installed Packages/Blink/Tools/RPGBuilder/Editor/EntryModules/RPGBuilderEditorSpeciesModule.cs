using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorSpeciesModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGSpecies> entries = new Dictionary<int, RPGSpecies>();
    private RPGSpecies currentEntry;
    
    private readonly List<RPGBuilderDatabaseEntry> allDamageTypes = new List<RPGBuilderDatabaseEntry>();
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.speciesFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGSpecies> dictionary = new Dictionary<int, RPGSpecies>();
        databaseEntries.Clear();
        allDamageTypes.Clear();
        var allEntries = Resources.LoadAll<RPGSpecies>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;

        foreach (var typeEntry in Resources.LoadAll<RPGBDamageType>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allDamageTypes.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        { 
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }
        
        currentEntry = CreateInstance<RPGSpecies>();
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
        RPGSpecies entryFile = (RPGSpecies)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.speciesModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.speciesModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.speciesModuleSection.showBaseInfo)
        {
            GUILayout.Space(5);
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
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.speciesModuleSection.showStats =
            RPGBuilderEditorUtility.HandleModuleBanner("STATS", RPGBuilderEditor.Instance.EditorFilters.speciesModuleSection.showStats);
        if (RPGBuilderEditor.Instance.EditorFilters.speciesModuleSection.showStats)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.UseStatListTemplate =
                RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.UseStatListTemplate);
            
            if (currentEntry.UseStatListTemplate)
            {
                currentEntry.StatListTemplate =
                    (StatListTemplate) RPGBuilderEditorFields.DrawHorizontalObject<StatListTemplate>(
                        "Template", "", currentEntry.StatListTemplate);
            }
            else
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Stat", false))
                {
                    currentEntry.CustomStats.Add(new CombatData.CustomStatValues());
                }
                GUILayout.Space(10);
                currentEntry.CustomStats =
                    RPGBuilderEditorFields.DrawCustomStatValuesList(currentEntry.CustomStats, false);
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }
        
        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.speciesModuleSection.showTraits =
            RPGBuilderEditorUtility.HandleModuleBanner("TRAITS", RPGBuilderEditor.Instance.EditorFilters.speciesModuleSection.showTraits);
        if (RPGBuilderEditor.Instance.EditorFilters.speciesModuleSection.showTraits)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Trait Effect", true))
            {
                currentEntry.traits.Add(new RPGSpecies.SPECIES_TRAIT());
            }

            var ThisList5 = serialObj.FindProperty("traits");
            currentEntry.traits = RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList5) as List<RPGSpecies.SPECIES_TRAIT>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.traits.Count; a++)
            {
                GUILayout.Space(10);
                
                RPGBuilderEditorFields.DrawHorizontalLabel("Damage Type", "");
                int damageTypeIndex = EditorGUILayout.Popup(RPGBuilderEditorUtility.GetTypeEntryIndex(allDamageTypes, currentEntry.traits[a].damageType),
                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allDamageTypes.ToArray()));
                currentEntry.traits[a].damageType = (RPGBDamageType) allDamageTypes[damageTypeIndex];
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.traits.RemoveAt(a);
                    return;
                }
                EditorGUILayout.EndHorizontal();

                currentEntry.traits[a].modifier = RPGBuilderEditorFields.DrawHorizontalFloatField(
                    "Modifier (%)",
                    "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.traits[a].modifier);

            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
     public override void ConvertDatabaseEntriesAfterUpdate ()
     {
         var allEntries = Resources.LoadAll<RPGSpecies>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
         foreach (var entry in allEntries)
         {
             EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
             RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry.fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
             entry.entryDisplayName = entry.displayName;
             entry.entryIcon = entry.icon;
             entry.entryDescription = entry.description;
             
             foreach (var stat in entry.stats)
             {
                 CombatData.CustomStatValues newStat = new CombatData.CustomStatValues
                 {
                     statID = stat.statID,
                     addedValue = stat.value,
                 };
                 entry.CustomStats.Add(newStat);
             }
             EditorUtility.SetDirty(entry);
         }
        
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
     }
     
     public override void ConvertStringsToTypeEntries()
     {
         var allEntries = Resources.LoadAll<RPGSpecies>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
         RPGBuilderEditorModule damageTypes = RPGBuilderEditorUtility.GetModuleByName("Damage Types");
         
         foreach (var entry in allEntries)
         {
             EditorUtility.SetDirty(entry);
             {
                 foreach (var trait in entry.traits)
                 {
                     RPGBuilderDatabaseEntry entryFile = damageTypes.GetEntryByName(trait.statFunction);
                     if (entryFile != null)
                     {
                         trait.damageType = (RPGBDamageType) entryFile;
                     }
                 }
             }

             EditorUtility.SetDirty(entry);
         }

         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
     }
}
