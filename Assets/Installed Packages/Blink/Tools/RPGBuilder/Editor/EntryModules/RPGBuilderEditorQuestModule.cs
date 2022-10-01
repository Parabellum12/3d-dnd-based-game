using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorQuestModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGQuest> entries = new Dictionary<int, RPGQuest>();
    private RPGQuest currentEntry;
    
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.questFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGQuest> dictionary = new Dictionary<int, RPGQuest>();
        databaseEntries.Clear();
        var allEntries = Resources.LoadAll<RPGQuest>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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
        
        currentEntry = CreateInstance<RPGQuest>();
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
        RPGQuest entryFile = (RPGQuest)updatedEntry;
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
        RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showBaseInfo)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawID( currentEntry.ID);
            currentEntry.entryName =
                RPGBuilderEditorFields.DrawHorizontalTextField("Name", "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField("File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showUIData =
            RPGBuilderEditorUtility.HandleModuleBanner("UI PRESENTATION", RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showUIData);
        if (RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showUIData)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.entryDescription = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Description:", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.entryDescription);
            currentEntry.ObjectiveText = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Objective:", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ObjectiveText);
            currentEntry.ProgressText = RPGBuilderEditorFields.DrawHorizontalDescriptionField("Progress:", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ProgressText);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showQuestSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("SETTINGS", RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showQuestSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showQuestSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.canBeTurnedInWithoutNPC = RPGBuilderEditorFields.DrawHorizontalToggle("Turn in without NPC", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.canBeTurnedInWithoutNPC);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showItemsGiven =
            RPGBuilderEditorUtility.HandleModuleBanner("ITEMS GIVEN", RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showItemsGiven);
        if (RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showItemsGiven)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Item", true))
            {
                currentEntry.itemsGiven.Add(new RPGQuest.QuestItemsGivenDATA());
            }

            var ThisList7 = serialObj.FindProperty("itemsGiven");
            currentEntry.itemsGiven =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList7) as List<RPGQuest.QuestItemsGivenDATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.itemsGiven.Count; a++)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.itemsGiven[a].itemID,
                    "Item"))
                {
                    currentEntry.itemsGiven.RemoveAt(a);
                    return;
                }

                currentEntry.itemsGiven[a].itemID = 
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.itemsGiven[a].itemID, "Item", "Item", "");

                currentEntry.itemsGiven[a].count = RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.itemsGiven[a].count);

            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showRequirements =
            RPGBuilderEditorUtility.HandleModuleBanner("REQUIREMENTS", RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showRequirements);
        if (RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showRequirements)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
            currentEntry.UseRequirementsTemplate = 
                RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.UseRequirementsTemplate);

            if (currentEntry.UseRequirementsTemplate)
            {
                currentEntry.RequirementsTemplate = (RequirementsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>(
                    "Template", "", currentEntry.RequirementsTemplate);
            }
            else
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Requirement Group", false))
                {
                    currentEntry.Requirements.Add(new RequirementsData.RequirementGroup());
                }

                currentEntry.Requirements = RPGBuilderEditorFields.DrawRequirementGroupsList(currentEntry.Requirements,false);
            }
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
                        
            GUILayout.Space(10);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showObjectives =
            RPGBuilderEditorUtility.HandleModuleBanner("OBJECTIVES", RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showObjectives);
        if (RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showObjectives)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Objective", true))
            {
                currentEntry.objectives.Add(new RPGQuest.QuestObjectiveDATA());
            }

            var ThisList2 = serialObj.FindProperty("objectives");
            currentEntry.objectives = RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList2) as List<RPGQuest.QuestObjectiveDATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.objectives.Count; a++)
            {
                GUILayout.Space(10);

                if (currentEntry.objectives[a].objectiveType == RPGQuest.QuestObjectiveType.task)
                {
                    EditorGUILayout.BeginHorizontal();
                    currentEntry.objectives[a].taskID =
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.objectives[a].taskID, "Task", "",
                            "");
                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                    {
                        currentEntry.objectives.RemoveAt(a);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showAutomaticRewards =
            RPGBuilderEditorUtility.HandleModuleBanner("AUTOMATIC REWARDS", RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showAutomaticRewards);
        if (RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showAutomaticRewards)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Reward", true))
            {
                currentEntry.rewardsGiven.Add(new RPGQuest.QuestRewardDATA());
            }

            var ThisList3 = serialObj.FindProperty("rewardsGiven");
            currentEntry.rewardsGiven =
                RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList3) as List<RPGQuest.QuestRewardDATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.rewardsGiven.Count; a++)
            {
                GUILayout.Space(10);
                switch (currentEntry.rewardsGiven[a].rewardType)
                {
                    case RPGQuest.QuestRewardType.currency:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsGiven[a].currencyID,
                            "Currency"))
                        {
                            currentEntry.rewardsGiven.RemoveAt(a);
                            return;
                        }
                        currentEntry.rewardsGiven[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsGiven[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsGiven[a].currencyID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsGiven[a].currencyID,
                                "Currency", "Currency", "");

                        currentEntry.rewardsGiven[a].count =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsGiven[a].count);
                        break;
                    }
                    case RPGQuest.QuestRewardType.Experience:

                        EditorGUILayout.BeginHorizontal();
                        currentEntry.rewardsGiven[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsGiven[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));
                        if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                        {
                            currentEntry.rewardsGiven.RemoveAt(a);
                            return;
                        }
                        EditorGUILayout.EndHorizontal();

                        currentEntry.rewardsGiven[a].Experience =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Experience", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsGiven[a].Experience);
                        break;
                    case RPGQuest.QuestRewardType.item:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsGiven[a].itemID,
                            "Item"))
                        {
                            currentEntry.rewardsGiven.RemoveAt(a);
                            return;
                        }

                        currentEntry.rewardsGiven[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsGiven[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsGiven[a].itemID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsGiven[a].itemID, "Item",
                                "Item", "");

                        currentEntry.rewardsGiven[a].count =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsGiven[a].count);
                        break;
                    }
                    case RPGQuest.QuestRewardType.treePoint:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsGiven[a].treePointID,
                            "Point"))
                        {
                            currentEntry.rewardsGiven.RemoveAt(a);
                            return;
                        }

                        currentEntry.rewardsGiven[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsGiven[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsGiven[a].treePointID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsGiven[a].treePointID,
                                "Point", "Point", "");

                        currentEntry.rewardsGiven[a].count =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsGiven[a].count);
                        break;
                    }
                    case RPGQuest.QuestRewardType.FactionPoint:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsGiven[a].factionID,
                            "Faction"))
                        {
                            currentEntry.rewardsGiven.RemoveAt(a);
                            return;
                        }

                        currentEntry.rewardsGiven[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsGiven[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsGiven[a].factionID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsGiven[a].factionID,
                                "Faction", "Faction", "");

                        currentEntry.rewardsGiven[a].count =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsGiven[a].count);
                        break;
                    }
                    case RPGQuest.QuestRewardType.weaponTemplateEXP:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsGiven[a].weaponTemplateID,
                            "WeaponTemplate"))
                        {
                            currentEntry.rewardsGiven.RemoveAt(a);
                            return;
                        }

                        currentEntry.rewardsGiven[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsGiven[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsGiven[a].weaponTemplateID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsGiven[a].weaponTemplateID,
                                "WeaponTemplate", "Weapon Template", "");

                        currentEntry.rewardsGiven[a].Experience =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Experience", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsGiven[a].Experience);
                        break;
                    }
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showRewardsToPick =
            RPGBuilderEditorUtility.HandleModuleBanner("REWARDS TO PICK", RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showRewardsToPick);
        if (RPGBuilderEditor.Instance.EditorFilters.questModuleSection.showRewardsToPick)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Reward", true))
            {
                currentEntry.rewardsToPick.Add(new RPGQuest.QuestRewardDATA());
            }

            var ThisList4 = serialObj.FindProperty("rewardsToPick");
            currentEntry.rewardsToPick = RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList4) as List<RPGQuest.QuestRewardDATA>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.rewardsToPick.Count; a++)
            {
                GUILayout.Space(10);
                    switch (currentEntry.rewardsToPick[a].rewardType)
                    {
                        case RPGQuest.QuestRewardType.currency:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsToPick[a].currencyID,
                            "Currency"))
                        {
                            currentEntry.rewardsToPick.RemoveAt(a);
                            return;
                        }
                        currentEntry.rewardsToPick[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsToPick[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsToPick[a].currencyID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsToPick[a].currencyID,
                                "Currency", "Currency", "");

                        currentEntry.rewardsToPick[a].count =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsToPick[a].count);
                        break;
                    }
                    case RPGQuest.QuestRewardType.Experience:

                        EditorGUILayout.BeginHorizontal();
                        currentEntry.rewardsToPick[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsToPick[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));
                        if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                        {
                            currentEntry.rewardsToPick.RemoveAt(a);
                            return;
                        }
                        EditorGUILayout.EndHorizontal();

                        currentEntry.rewardsToPick[a].Experience =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Experience", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsToPick[a].Experience);
                        break;
                    case RPGQuest.QuestRewardType.item:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsToPick[a].itemID,
                            "Item"))
                        {
                            currentEntry.rewardsToPick.RemoveAt(a);
                            return;
                        }

                        currentEntry.rewardsToPick[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsToPick[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsToPick[a].itemID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsToPick[a].itemID, "Item",
                                "Item", "");

                        currentEntry.rewardsToPick[a].count =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsToPick[a].count);
                        break;
                    }
                    case RPGQuest.QuestRewardType.treePoint:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsToPick[a].treePointID,
                            "Point"))
                        {
                            currentEntry.rewardsToPick.RemoveAt(a);
                            return;
                        }

                        currentEntry.rewardsToPick[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsToPick[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsToPick[a].treePointID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsToPick[a].treePointID,
                                "Point", "Point", "");

                        currentEntry.rewardsToPick[a].count =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsToPick[a].count);
                        break;
                    }
                    case RPGQuest.QuestRewardType.FactionPoint:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsToPick[a].factionID,
                            "Faction"))
                        {
                            currentEntry.rewardsToPick.RemoveAt(a);
                            return;
                        }

                        currentEntry.rewardsToPick[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsToPick[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsToPick[a].factionID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsToPick[a].factionID,
                                "Faction", "Faction", "");

                        currentEntry.rewardsToPick[a].count =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Count", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsToPick[a].count);
                        break;
                    }
                    case RPGQuest.QuestRewardType.weaponTemplateEXP:
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                            currentEntry.rewardsToPick[a].weaponTemplateID,
                            "WeaponTemplate"))
                        {
                            currentEntry.rewardsToPick.RemoveAt(a);
                            return;
                        }

                        currentEntry.rewardsToPick[a].rewardType =
                            (RPGQuest.QuestRewardType) RPGBuilderEditorFields.DrawHorizontalEnum("", "",
                                (int)currentEntry.rewardsToPick[a].rewardType,
                                Enum.GetNames(typeof(RPGQuest.QuestRewardType)));

                        currentEntry.rewardsToPick[a].weaponTemplateID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.rewardsToPick[a].weaponTemplateID,
                                "WeaponTemplate", "Weapon Template", "");

                        currentEntry.rewardsToPick[a].Experience =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Experience", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.rewardsToPick[a].Experience);
                        break;
                    }
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(30);
        }

        serialObj.ApplyModifiedProperties();
        GUILayout.Space(25);
        GUILayout.EndScrollView();
    
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var allEntries = Resources.LoadAll<RPGQuest>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
             RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryDescription = entry.description;
            EditorUtility.SetDirty(entry);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
