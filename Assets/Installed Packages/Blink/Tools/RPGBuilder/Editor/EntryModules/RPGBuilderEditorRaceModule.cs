using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorRaceModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGRace> entries = new Dictionary<int, RPGRace>();
    private RPGRace currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.raceFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

     public override void LoadEntries()
     {
         Dictionary<int, RPGRace> dictionary = new Dictionary<int, RPGRace>();
        databaseEntries.Clear();
         var allEntries = Resources.LoadAll<RPGRace>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
         currentEntry = CreateInstance<RPGRace>();
         RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
         RPGBuilderEditor.Instance.CurrentEntryIndex = -1;
     }

     public override bool SaveConditionsMet()
     {
         if (ContainsInvalidCharacters(currentEntry.entryName))
         {
             RPGBuilderEditorUtility.DisplayDialogueWindow("Invalid Characters", "The Name contains invalid characters", "OK");
             return false;
         }
         if (string.IsNullOrEmpty(currentEntry.entryName))
         {
             RPGBuilderEditorUtility.DisplayDialogueWindow("Invalid Name", "Enter a valid name", "OK");
             return false;
         }
         if (currentEntry.Genders.Count == 0)
         {
             RPGBuilderEditorUtility.DisplayDialogueWindow("Gender Missing", "At least 1 Gender is required", "OK");
             return false;
         }
         
         if (currentEntry.factionID == -1)
         {
             RPGBuilderEditorUtility.DisplayDialogueWindow("Faction Missing", "A Faction is required on Races", "OK");
             return false;
         }
         
         if (currentEntry.startingSceneID == -1)
         {
             RPGBuilderEditorUtility.DisplayDialogueWindow("Game Scene Missing", "A Starting Game Scene is required on Races", "OK");
             return false;
         }
         
         if (currentEntry.startingPositionID == -1)
         {
             RPGBuilderEditorUtility.DisplayDialogueWindow("Coordinate Missing", "A Starting Coordinate is required on Races", "OK");
             return false;
         }
         
         if (!RPGBuilderEditor.Instance.CharacterSettings.NoClasses && currentEntry.availableClasses.Count == 0)
         {
             RPGBuilderEditorUtility.DisplayDialogueWindow("Classes Missing", "At least 1 Class is required", "OK");
             return false;
         }

         if (RPGBuilderEditor.Instance.CharacterSettings.NoClasses && currentEntry.levelTemplateID == -1)
         {
             RPGBuilderEditorUtility.DisplayDialogueWindow("Level Template Missing", "A Level Template is required on Races when Classes are disabled", "OK");
             return false;
         }

         return true;
     }

     public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
     {
         RPGRace entryFile = (RPGRace)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon = RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
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
            currentEntry.entryDescription = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDescription);

            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showVisual =
            RPGBuilderEditorUtility.HandleModuleBanner("GENDERS", RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showVisual);
        if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showVisual)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Gender", true))
            {
                currentEntry.Genders.Add(new RPGRace.RaceGenderData());
            }

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.Genders.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                RPGBuilderEditorFields.DrawHorizontalLabel("Gender", "");
                int enumIndex = EditorGUILayout.Popup(
                    RPGBuilderEditorUtility.GetTypeEntryIndex(
                        RPGBuilderEditorUtility.GetModuleByName("Genders").databaseEntries,
                        currentEntry.Genders[a].Gender),
                    RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(RPGBuilderEditorUtility
                        .GetModuleByName("Genders").databaseEntries.ToArray()));
                currentEntry.Genders[a].Gender = (RPGBGender) RPGBuilderEditorUtility
                    .GetModuleByName("Genders").databaseEntries[enumIndex];
                EditorGUILayout.EndHorizontal();
                
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.Genders.Remove(currentEntry.Genders[a]);
                    return;
                }
                EditorGUILayout.EndHorizontal();
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Visual:", "");
                currentEntry.Genders[a].Prefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Prefab", "", currentEntry.Genders[a].Prefab);
                currentEntry.Genders[a].Icon = RPGBuilderEditorFields.DrawHorizontalSpriteField("Icon", "", 0,
                    currentEntry.Genders[a].Icon, 100);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Animations:", "", true);
                currentEntry.Genders[a].DynamicAnimator = RPGBuilderEditorFields.DrawHorizontalToggle("Dynamic Animator ?", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.Genders[a].DynamicAnimator);
                if (currentEntry.Genders[a].DynamicAnimator)
                {
                    currentEntry.Genders[a].RestAnimatorController = (RuntimeAnimatorController)
                        RPGBuilderEditorFields.DrawHorizontalObject<RuntimeAnimatorController>("Rest Controller", "", currentEntry.Genders[a].RestAnimatorController);
                    currentEntry.Genders[a].CombatAnimatorController = (RuntimeAnimatorController)
                        RPGBuilderEditorFields.DrawHorizontalObject<RuntimeAnimatorController>("Combat Controller", "",currentEntry.Genders[a].CombatAnimatorController);
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStartingSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("START SETTINGS", RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStartingSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStartingSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Scene:", "", true);
            currentEntry.startingSceneID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.startingSceneID, "GameScene", "Game Scene", "");
            currentEntry.startingPositionID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.startingPositionID, "Coordinate", "Coordinate", "");

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Identity:", "", true);
            currentEntry.factionID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.factionID, "Faction", "Faction", "");
            currentEntry.speciesID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.speciesID, "Species", "Species:", "");

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        if (!RPGBuilderEditor.Instance.CharacterSettings.NoClasses)
        {
            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showClasses =
                RPGBuilderEditorUtility.HandleModuleBanner("CLASSES",
                    RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showClasses);
            if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showClasses)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Class", true))
                {
                    currentEntry.availableClasses.Add(new RPGRace.RACE_CLASSES_DATA());
                }

                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                for (var a = 0; a < currentEntry.availableClasses.Count; a++)
                {
                    GUILayout.Space(10);
                    if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                        currentEntry.availableClasses[a].classID,
                        "Class"))
                    {
                        currentEntry.availableClasses.RemoveAt(a);
                        return;
                    }

                    currentEntry.availableClasses[a].classID =
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.availableClasses[a].classID, "Class",
                            "Class", "");
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            }
        }
        else
        {
            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showClasses =
                RPGBuilderEditorUtility.HandleModuleBanner("LEVELS",
                    RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showClasses);
            if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showClasses)
            {
                GUILayout.Space(10);
                

                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                currentEntry.levelTemplateID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.levelTemplateID, "Levels",
                        "Level Template", "");

                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            }
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showWeaponTemplate =
            RPGBuilderEditorUtility.HandleModuleBanner("WEAPON TEMPLATES", RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showWeaponTemplate);
        if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showWeaponTemplate)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Weapon Template", true))
            {
                currentEntry.weaponTemplates.Add(new RPGRace.WEAPON_TEMPLATES_DATA());
            }

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.weaponTemplates.Count; a++)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.weaponTemplates[a].weaponTemplateID,
                    "WeaponTemplate"))
                {
                    currentEntry.weaponTemplates.RemoveAt(a);
                    return;
                }
                
                currentEntry.weaponTemplates[a].weaponTemplateID = 
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.weaponTemplates[a].weaponTemplateID, "WeaponTemplate", "Weapon Template", "");
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }


        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStats =
            RPGBuilderEditorUtility.HandleModuleBanner("STATS", RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStats);
        if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStats)
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
        RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showActionAbilities =
            RPGBuilderEditorUtility.HandleModuleBanner("ACTION ABILITIES", RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showActionAbilities);
        if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showActionAbilities)
        {
            RPGBuilderEditorFields.DrawActionAbilityList(currentEntry.actionAbilities);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStartingItems =
            RPGBuilderEditorUtility.HandleModuleBanner("STARTING ITEMS", RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStartingItems);
        if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStartingItems)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Item", true))
            {
                currentEntry.startItems.Add(new RPGItemDATA.StartingItemsDATA());
            }

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.startItems.Count; a++)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.startItems[a].itemID,
                    "Item"))
                {
                    currentEntry.startItems.RemoveAt(a);
                    return;
                }
                
                currentEntry.startItems[a].itemID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.startItems[a].itemID, "Item", "Item", "");

                currentEntry.startItems[a].count =
                    RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.startItems[a].count);

                currentEntry.startItems[a].equipped =
                    RPGBuilderEditorFields.DrawHorizontalToggle("Equipped", "",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.startItems[a].equipped);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }
        
        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStatAllocation =
            RPGBuilderEditorUtility.HandleModuleBanner("STAT ALLOCATION MENU", RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStatAllocation);
        if (RPGBuilderEditor.Instance.EditorFilters.raceModuleSection.showStatAllocation)
        {
            GUILayout.Space(10);
            
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.allocationStatPoints =
                RPGBuilderEditorFields.DrawHorizontalIntField("Points To Allocate", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.allocationStatPoints);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
            
            currentEntry.allocatedStatsEntries = RPGBuilderEditorFields.DrawStatAllocationList(currentEntry.allocatedStatsEntries);
        }

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
     public override void ConvertDatabaseEntriesAfterUpdate ()
     {
         var allEntries = Resources.LoadAll<RPGRace>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
         foreach (var entry in allEntries)
         {
             EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
                                       RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
             entry.entryDisplayName = entry.displayName;
             entry.entryDescription = entry.description;
             
             foreach (var stat in entry.stats)
             {
                 CombatData.CustomStatValues newStat = new CombatData.CustomStatValues
                 {
                     statID = stat.statID,
                     addedValue = stat.amount,
                     Percent = stat.isPercent
                 };
                 entry.CustomStats.Add(newStat);
             }
             EditorUtility.SetDirty(entry);
         }
        
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
     }
}
