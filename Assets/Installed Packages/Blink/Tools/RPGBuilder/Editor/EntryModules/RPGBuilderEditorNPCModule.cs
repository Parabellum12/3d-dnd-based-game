using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class RPGBuilderEditorNPCModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGNpc> entries = new Dictionary<int, RPGNpc>();
    private RPGNpc currentEntry;
    
    private readonly List<RPGBuilderDatabaseEntry> allNPCFamilies = new List<RPGBuilderDatabaseEntry>();
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
        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.NPCFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }
    
    public override void LoadEntries()
    {
        Dictionary<int, RPGNpc> dictionary = new Dictionary<int, RPGNpc>();
        databaseEntries.Clear();
        allNPCFamilies.Clear();
        var allEntries = Resources.LoadAll<RPGNpc>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        for (var index = 0; index < allEntries.Length; index++)
        {
            var entry = allEntries[index];
            dictionary.Add(index, entry);
            databaseEntries.Add(entry);
        }

        entries = dictionary;

        foreach (var typeEntry in Resources.LoadAll<RPGBNPCFamily>(RPGBuilderEditor.Instance.EditorSettings.DatabasePath))
        {
            allNPCFamilies.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        { 
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }
        
        currentEntry = CreateInstance<RPGNpc>();
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
        if (currentEntry.factionID == -1)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("Faction Missing", "A Faction is required on NPCs", "OK");
            return false;
        }
        if (currentEntry.Phases.Count == 0)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("No Phases", "At least 1 Phase is required on NPCs", "OK");
            return false;
        }
        
        if (currentEntry.AILogicTemplate == null)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("No AI Logic Template", "AI Logic Template is required on NPCs", "OK");
            return false;
        }

        foreach (var phase in currentEntry.Phases)
        {
            if (phase.PhaseTemplate == null)
            {
                RPGBuilderEditorUtility.DisplayDialogueWindow("Phase Template Missing", "A Phase has no Template assigned", "OK");
                return false;
            }
            if (phase.Preset == null)
            {
                RPGBuilderEditorUtility.DisplayDialogueWindow("NPC Preset Missing", "A Phase has no NPC Preset assigned", "OK");
                return false;
            }
        }
        
        return true;
    }
    
    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGNpc entryFile = (RPGNpc)updatedEntry;
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
        
        RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showBaseInfo);
        if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showBaseInfo)
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
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showLogicSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("AI SETTINGS", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showLogicSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showLogicSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Tags & Layer:", "", false);
            
            currentEntry.SetTag = RPGBuilderEditorFields.DrawHorizontalToggle("Set GameObject Tag?", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.SetTag);
            if (currentEntry.SetTag)
            {
                currentEntry.GameObjectTag = RPGBuilderEditorFields.DrawHorizontalTagField("Tag", "", currentEntry.GameObjectTag);
            }
            
            currentEntry.SetLayer = RPGBuilderEditorFields.DrawHorizontalToggle("Set GameObject Layer?", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.SetLayer);
            if (currentEntry.SetLayer)
            {
                currentEntry.GameObjectLayer = RPGBuilderEditorFields.DrawHorizontalLayerField("Layer", "", currentEntry.GameObjectLayer);
            }
            
            currentEntry.npcFamily = (RPGBNPCFamily) RPGBuilderEditorFields.DrawTypeEntryField("Family", allNPCFamilies, currentEntry.npcFamily);

            RPGBuilderEditorFields.DrawTitleLabelExpanded("AI Logic Template:", "", true);
            EditorGUILayout.BeginHorizontal();
            currentEntry.AILogicTemplate =
                (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("AI Template", "",
                    currentEntry.AILogicTemplate);
            if (currentEntry.AILogicTemplate == null)
            {
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Find Default", false))
                {
                    currentEntry.AILogicTemplate = RPGBuilderEditor.Instance.EditorData.AILogicTemplate;
                }
            }
            EditorGUILayout.EndHorizontal();
            

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            RPGBuilderEditorFields.DrawTitleLabel("Phases & Appearances:", "", 145);
            GUILayout.Space(10);
            if (GUILayout.Button("+", RPGBuilderEditor.Instance.EditorSkin.GetStyle("SquareAddButton"),
                GUILayout.Width(20), GUILayout.Height(20)))
            {
                currentEntry.Phases.Add(new AIData.AIPhase());
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            for (var phase = 0; phase < currentEntry.Phases.Count; phase++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                currentEntry.Phases[phase].PhaseTemplate =
                    (AIPhaseTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIPhaseTemplate>("Phase", "",
                        currentEntry.Phases[phase].PhaseTemplate);
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.Phases.RemoveAt(phase);
                    return;
                }
                EditorGUILayout.EndHorizontal();

                currentEntry.Phases[phase].Preset =
                    (NPCPresetTemplate) RPGBuilderEditorFields.DrawHorizontalObject<NPCPresetTemplate>("NPC Preset", "", currentEntry.Phases[phase].Preset);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showFunctions =
            RPGBuilderEditorUtility.HandleModuleBanner("FUNCTIONS", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showFunctions);
        if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showFunctions)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Combat:", "", false);
            currentEntry.isDummyTarget =
                RPGBuilderEditorFields.DrawHorizontalToggle("Training Dummy?", "If this is on, the NPC will never die. Yep, NERF PLEASE.",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isDummyTarget);
            currentEntry.isCombatEnabled =
                RPGBuilderEditorFields.DrawHorizontalToggle("Combat Enabled?", "If this is on, the NPC will be able to fight and be attacked.",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isCombatEnabled);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Actions:", "", true);
            currentEntry.isMerchant =
                RPGBuilderEditorFields.DrawHorizontalToggle("Merchant?", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isMerchant);
            if (currentEntry.isMerchant)
            {
                currentEntry.MerchantText = RPGBuilderEditorFields.DrawHorizontalTextField("Merchant Text", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.MerchantText);
            }
            
            currentEntry.isQuestGiver =
                RPGBuilderEditorFields.DrawHorizontalToggle("Quest?", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isQuestGiver);
            if (currentEntry.isQuestGiver)
            {
                currentEntry.QuestText = RPGBuilderEditorFields.DrawHorizontalTextField("Quest Text", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.QuestText);
            }
            
            currentEntry.isDialogue =
                RPGBuilderEditorFields.DrawHorizontalToggle("Dialogue?", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isDialogue);
            if (currentEntry.isDialogue)
            {
                currentEntry.DialogueText = RPGBuilderEditorFields.DrawHorizontalTextField("Dialogue Text", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.DialogueText);
            }
            
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Interactions:", "", true);
            currentEntry.isTargetable =
                RPGBuilderEditorFields.DrawHorizontalToggle("Can be targeted?", "If this is on, the NPC will be possible to target.",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isTargetable);
            currentEntry.isPlayerInteractable =
                RPGBuilderEditorFields.DrawHorizontalToggle("Player Interactable?", "If this is on, the NPC be set on the interaction layer and can be interacted with the interact key.",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isPlayerInteractable);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("UI:", "", true);
            currentEntry.isNameplateEnabled =RPGBuilderEditorFields.DrawHorizontalToggle("Show Nameplate?", "If this is on, the NPC will have a nameplate above it.",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.isNameplateEnabled);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Navigation:", "", true);
            currentEntry.isMovementEnabled =
                RPGBuilderEditorFields.DrawHorizontalToggle("Movement Enabled?", "If this is on, the NPC will be able to perform movement actions.",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isMovementEnabled);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Settings:", "", true);
            currentEntry.isCollisionEnabled =
                RPGBuilderEditorFields.DrawHorizontalToggle("Collision Enabled?", "If this is on, the NPC will collide with other NPCs and the player.",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isCollisionEnabled);

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }
        
        if (currentEntry.isMerchant)
        {
            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showMerchant =
                RPGBuilderEditorUtility.HandleModuleBanner("MERCHANT TABLES",
                    RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showMerchant);
            if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showMerchant)
            {
                GUILayout.Space(10);
                if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Merchant Table", true))
                {
                    currentEntry.MerchantTables.Add(new AIData.NPCMerchantTable());
                }

                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                for (var a = 0; a < currentEntry.MerchantTables.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    currentEntry.MerchantTables[a].MerchantTableID =
                        RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.MerchantTables[a].MerchantTableID,
                            "MerchantTable",
                            "Merchant Table", "");
                    if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                    {
                        currentEntry.MerchantTables.RemoveAt(a);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    currentEntry.MerchantTables[a].RequirementsTemplate = (RequirementsTemplate)
                        RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>("Requirements", "",
                            currentEntry.MerchantTables[a].RequirementsTemplate);
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            }
        }

        if (currentEntry.isQuestGiver)
        {
            GUILayout.Space(10);
                RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showQuestGiven =
                    RPGBuilderEditorUtility.HandleModuleBanner("QUESTS GIVEN", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showQuestGiven);
                if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showQuestGiven)
                {
                    GUILayout.Space(10);
                    if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Quest Given", true))
                    {
                        currentEntry.questGiven.Add(new RPGNpc.NPC_QUEST_DATA());
                    }

                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    for (var a = 0; a < currentEntry.questGiven.Count; a++)
                    {
                        GUILayout.Space(10);
                        EditorGUILayout.BeginHorizontal();
                        currentEntry.questGiven[a].questID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.questGiven[a].questID, "Quest",
                                "Quest", "");
                        if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                        {
                            currentEntry.questGiven.RemoveAt(a);
                            return;
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                }

                GUILayout.Space(10);
                RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showQuestCompleted =
                    RPGBuilderEditorUtility.HandleModuleBanner("QUESTS COMPLETED", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showQuestCompleted);
                if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showQuestCompleted)
                {
                    GUILayout.Space(10);
                    if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Quest Completed", true))
                    {
                        currentEntry.questCompleted.Add(new RPGNpc.NPC_QUEST_DATA());
                    }

                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                    for (var a = 0; a < currentEntry.questCompleted.Count; a++)
                    {
                        GUILayout.Space(10);
                        EditorGUILayout.BeginHorizontal();
                        currentEntry.questCompleted[a].questID =
                            RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.questCompleted[a].questID,
                                "Quest", "Quest", "");
                        if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                        {
                            currentEntry.questCompleted.RemoveAt(a);
                            return;
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                }
        }
        if (currentEntry.isDialogue)
        {
            GUILayout.Space(10);
            RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showDialogue =
                RPGBuilderEditorUtility.HandleModuleBanner("DIALOGUE", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showDialogue);
            if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showDialogue)
            {
                GUILayout.Space(10);
                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
                currentEntry.dialogueID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.dialogueID, "Dialogue", "Dialogue", "");
                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            }
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showCombat =
            RPGBuilderEditorUtility.HandleModuleBanner("COMBAT", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showCombat);
        if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showCombat)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Identity:", "", false);
            currentEntry.factionID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.factionID, "Faction", "Faction", "");
            currentEntry.speciesID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.speciesID, "Species", "Species:", "");

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Levels:", "", true);
            currentEntry.isScalingWithPlayer =
                RPGBuilderEditorFields.DrawHorizontalToggle("Scale With Player?", "If this is on, the NPC level will be the player class level.",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.isScalingWithPlayer);
            if (!currentEntry.isScalingWithPlayer)
            {
                currentEntry.MinLevel =
                    RPGBuilderEditorFields.DrawHorizontalIntField("Level. Min", "The minimum level of the NPC",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.MinLevel);
                currentEntry.MaxLevel =
                    RPGBuilderEditorFields.DrawHorizontalIntField("Level. Max", "The maximum level of the NPC",
                        RPGBuilderEditor.Instance.FieldHeight,
                        currentEntry.MaxLevel);

                if (currentEntry.MaxLevel < currentEntry.MinLevel)
                    currentEntry.MaxLevel = currentEntry.MinLevel;
                if (currentEntry.MinEXP > currentEntry.MaxEXP)
                    currentEntry.MaxLevel = currentEntry.MinLevel;
            }
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Phases:", "", true);
            currentEntry.ResetPhaseAfterCombat =
                RPGBuilderEditorFields.DrawHorizontalToggle("Reset phase after combat?", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.ResetPhaseAfterCombat);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("After combat:", "", true);
            currentEntry.InstantlyHealAfterCombat =
                RPGBuilderEditorFields.DrawHorizontalToggle("Instantly heal?", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.InstantlyHealAfterCombat);

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }
        
        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showAggroLinks =
            RPGBuilderEditorUtility.HandleModuleBanner("AGGRO LINKS", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showAggroLinks);
        if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showAggroLinks)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Aggro Link", true))
            {
                currentEntry.aggroLinks.Add(new RPGNpc.NPC_AGGRO_LINK());
            }

            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.aggroLinks.Count; a++)
            {
                EditorGUILayout.BeginHorizontal();
                
                currentEntry.aggroLinks[a].type =
                    (AIData.AggroLinkType) RPGBuilderEditorFields.DrawHorizontalEnum("Type", "",
                        (int)currentEntry.aggroLinks[a].type,
                        Enum.GetNames(typeof(AIData.AggroLinkType)));
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.aggroLinks.RemoveAt(a);
                    return; 
                }
                EditorGUILayout.EndHorizontal();
                
                
                if (currentEntry.aggroLinks[a].type == AIData.AggroLinkType.NPC)
                {
                    currentEntry.aggroLinks[a].npcID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.aggroLinks[a].npcID, "NPC", "NPC:", "");
                }
                else
                {
                    currentEntry.aggroLinks[a].npcFamily =
                        (RPGBNPCFamily) RPGBuilderEditorFields.DrawHorizontalObject<RPGBNPCFamily>("Family", "",
                            currentEntry.aggroLinks[a].npcFamily);
                }
                currentEntry.aggroLinks[a].maxDistance = RPGBuilderEditorFields.DrawHorizontalFloatField(
                    "Max Distance", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.aggroLinks[a].maxDistance);

                GUILayout.Space(10);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showRespawn =
            RPGBuilderEditorUtility.HandleModuleBanner("RESPAWN", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showRespawn);
        if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showRespawn)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.MinRespawn = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Respawn. Min", "How long should the minimum respawn time be?",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.MinRespawn);
            currentEntry.MaxRespawn = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Respawn. Max", "How long should the maximum respawn time be?",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.MaxRespawn);
            currentEntry.corpseDespawnTime = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Corpse Despawn Time", "How long should the dead body stay in the world?",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.corpseDespawnTime);

            if (currentEntry.MaxRespawn < currentEntry.MinRespawn)
                currentEntry.MaxRespawn = currentEntry.MinRespawn;
            if (currentEntry.MinRespawn > currentEntry.MaxRespawn)
                currentEntry.MaxRespawn = currentEntry.MinRespawn;

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showRewards =
            RPGBuilderEditorUtility.HandleModuleBanner("REWARDS", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showRewards);
        if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showRewards)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Loot Visual:", "", false);
            currentEntry.lootBagPrefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Loot Bag Prefab", "", currentEntry.lootBagPrefab);
            currentEntry.LootBagDuration = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Bag duration", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.LootBagDuration);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Experience:", "", true);
            currentEntry.MinEXP = RPGBuilderEditorFields.DrawHorizontalIntField(
                "EXP. Min", "What is the minimum amount of Experience that this NPC will reward?",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.MinEXP);
            currentEntry.MaxEXP = RPGBuilderEditorFields.DrawHorizontalIntField(
                "EXP. Max", "What is the maximum amount of Experience that this NPC will reward?",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.MaxEXP);
            currentEntry.EXPBonusPerLevel = RPGBuilderEditorFields.DrawHorizontalIntField(
                "Level Bonus EXP",
                "For each level of the NPC, what extra amount of Experience will be given?",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.EXPBonusPerLevel);
            
            currentEntry.LowerLevelEXPModifier = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "% For each lower player LvL", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.LowerLevelEXPModifier);
            
            currentEntry.HigherLevelEXPModifier = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "% For each higher player LvL", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.HigherLevelEXPModifier);

            if (currentEntry.LowerLevelEXPModifier < -100) currentEntry.LowerLevelEXPModifier = -100;
            if (currentEntry.HigherLevelEXPModifier < -100) currentEntry.HigherLevelEXPModifier = -100;
            
            if (currentEntry.MaxEXP < currentEntry.MinEXP)
                currentEntry.MaxEXP = currentEntry.MinEXP;
            if (currentEntry.MinEXP > currentEntry.MaxEXP)
                currentEntry.MaxEXP = currentEntry.MinEXP;
            
            

            GUILayout.Space(10);
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Faction Points:", "", false);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Faction Reward", true))
            {
                currentEntry.factionRewards.Add(new RPGCombatDATA.Faction_Reward_DATA());
            }

            for (var a = 0; a < currentEntry.factionRewards.Count; a++)
            {
                if (RPGBuilderEditorFields.DrawHorizontalEntryRemoveButton(
                    currentEntry.factionRewards[a].factionID,
                    "Faction"))
                {
                    currentEntry.factionRewards.RemoveAt(a);
                    return;
                }
                currentEntry.factionRewards[a].factionID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.factionRewards[a].factionID, "Faction", "Faction", "");

                currentEntry.factionRewards[a].amount = RPGBuilderEditorFields.DrawHorizontalIntField(
                    "Amount",
                    "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.factionRewards[a].amount);
                GUILayout.Space(10);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showLootTables =
            RPGBuilderEditorUtility.HandleModuleBanner("LOOT TABLES", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showLootTables);
        if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showLootTables)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Loot Table", true))
            {
                currentEntry.lootTables.Add(new RPGNpc.LOOT_TABLES());
            }
            GUILayout.Space(5);

            var ThisList4 = serialObj.FindProperty("lootTables");
            currentEntry.lootTables = RPGBuilderEditor.Instance.GetTargetObjectOfProperty(ThisList4) as List<RPGNpc.LOOT_TABLES>;

            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            for (var a = 0; a < currentEntry.lootTables.Count; a++)
            {
                EditorGUILayout.BeginHorizontal();
                currentEntry.lootTables[a].lootTableID =
                    RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.lootTables[a].lootTableID, "LootTable", "Loot Table", "");
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.lootTables.RemoveAt(a);
                    return;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
                
                currentEntry.lootTables[a].dropRate =
                    RPGBuilderEditorFields.DrawHorizontalFloatFillBar("Chance", "",
                        currentEntry.lootTables[a].dropRate);
                GUILayout.Space(10);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showStats =
            RPGBuilderEditorUtility.HandleModuleBanner("STATS", RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showStats);
        if (RPGBuilderEditor.Instance.EditorFilters.npcModuleSection.showStats)
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

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }
    
     public override void ConvertDatabaseEntriesAfterUpdate ()
     {
         var allEntries = Resources.LoadAll<RPGNpc>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
         foreach (var entry in allEntries)
         {
             EditorUtility.SetDirty(entry);
             entry.entryName = entry._name;
             AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
                                       RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
             entry.entryFileName = entry.entryName + AssetNameSuffix;
             entry.entryDisplayName = entry.displayName;
             entry.entryIcon = entry.icon;

             foreach (var stat in entry.stats)
             {
                 CombatData.CustomStatValues newStat = new CombatData.CustomStatValues
                 {
                     statID = stat.statID,
                     minValue = stat.minValue,
                     maxValue = stat.maxValue,
                     valuePerLevel = stat.bonusPerLevel,
                     startPercentage = stat.baseValue
                 };

                 RPGStat statEntry = (RPGStat)RPGBuilderEditorUtility.GetEntryByID(stat.statID, "Stat");

                 if (stat.minValue != statEntry.minValue)
                 {
                     newStat.overrideMinValue = true;
                 }
                 if (stat.maxValue != statEntry.maxValue)
                 {
                     newStat.overrideMaxValue = true;
                 }
                 
                 entry.CustomStats.Add(newStat);
             }
             
             EditorUtility.SetDirty(entry);
         }
        
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
     }
}
