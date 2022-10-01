using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class RPGBuilderEditorAbilityModule : RPGBuilderEditorModule
{
    private Dictionary<int, RPGAbility> entries = new Dictionary<int, RPGAbility>();
    private RPGAbility currentEntry;

    private readonly List<RPGBuilderDatabaseEntry> allNodeSockets = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allWeaponTypes = new List<RPGBuilderDatabaseEntry>();
    private readonly List<RPGBuilderDatabaseEntry> allAbilityCooldownTag = new List<RPGBuilderDatabaseEntry>();

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

        RPGBuilderEditor.Instance.InitializeFilters(RPGBuilderEditor.Instance.EditorFilters.abilityFilters);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RPGAbility> dictionary = new Dictionary<int, RPGAbility>();
        databaseEntries.Clear();
        allNodeSockets.Clear();
        allWeaponTypes.Clear();
        allAbilityCooldownTag.Clear();
        var allEntries =
            Resources.LoadAll<RPGAbility>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
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

        foreach (var typeEntry in Resources.LoadAll<RPGBWeaponType>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allWeaponTypes.Add(typeEntry);
        }

        foreach (var typeEntry in Resources.LoadAll<RPGBAbilityCooldownTag>(RPGBuilderEditor.Instance.EditorSettings
            .DatabasePath))
        {
            allAbilityCooldownTag.Add(typeEntry);
        }
    }

    public override void CreateNewEntry()
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }

        if (currentEntry != null) currentEntry.ranks.Clear();

        currentEntry = CreateInstance<RPGAbility>();
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
        
        if (currentEntry.ranks.Count == 0)
        {
            RPGBuilderEditorUtility.DisplayDialogueWindow("0 Ranks", "Create at least 1 rank", "OK");
            return false;
        }
        
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGAbility entryFile = (RPGAbility) updatedEntry;
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

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);

        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.MaxWidth(RPGBuilderEditor.Instance.ViewWidth),
            GUILayout.ExpandHeight(true));

        RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showBaseInfo =
            RPGBuilderEditorUtility.HandleModuleBanner("BASE INFO",
                RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showBaseInfo);

        if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showBaseInfo)
        {
            GUILayout.Space(5);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            currentEntry.entryIcon =
                RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 100, 100);
            GUILayout.BeginVertical();
            RPGBuilderEditorFields.DrawID(currentEntry.ID);
            currentEntry.entryName = RPGBuilderEditorFields.DrawHorizontalTextField("Name",
                "", RPGBuilderEditor.Instance.FieldHeight, currentEntry.entryName);
            currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField(
                "Display Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryDisplayName);
            currentEntry.entryFileName = RPGBuilderEditorFields.DrawFileNameField(
                "File Name", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.entryName + AssetNameSuffix);
            currentEntry.learnedByDefault =
                RPGBuilderEditorFields.DrawHorizontalToggle("Known",
                    "Is this currentAbilityEntry automatically known?",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.learnedByDefault);

            currentEntry.abilityType = (RPGAbility.AbilityType) RPGBuilderEditorFields.DrawHorizontalEnum("Type", "?", (int)currentEntry.abilityType,
                Enum.GetNames(typeof(RPGAbility.AbilityType)));
            GUILayout.EndVertical();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showRanks =
            RPGBuilderEditorUtility.HandleModuleBanner("RANKS",
                RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showRanks);
        GUILayout.Space(10);

        if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showRanks)
        {
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Rank", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.MinWidth(150), GUILayout.ExpandWidth(true)))
            {
                var newRankData = new RPGAbility.RPGAbilityRankData();
                currentEntry.ranks.Add(newRankData);
            }

            if (currentEntry.ranks.Count > 0)
            {
                GUILayout.Space(20);

                if (GUILayout.Button("Remove Rank",
                    RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalRemoveButton"),
                    GUILayout.MinWidth(150),
                    GUILayout.ExpandWidth(true)))
                {
                    currentEntry.ranks.RemoveAt(currentEntry.ranks.Count - 1);
                    return;
                }
            }

            GUILayout.EndHorizontal();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

            GUILayout.Space(10);

            for (var i = 0; i < currentEntry.ranks.Count; i++)
            {
                GUILayout.Space(6);
                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

                var rankNbr = i + 1;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Rank: " + rankNbr, RPGBuilderEditor.Instance.EditorSkin.GetStyle("GenericButton"),
                    GUILayout.ExpandWidth(true)))
                {
                    currentEntry.ranks[i].ShowedInEditor =
                        !currentEntry.ranks[i].ShowedInEditor;
                    GUI.FocusControl(null);
                }

                if (i > 0)
                {
                    GUILayout.Space(5);
                    if (GUILayout.Button("Copy Above", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(150)))
                    {
                        currentEntry.CopyEntryData(
                            currentEntry.ranks[i],
                            currentEntry.ranks[i - 1]);
                        GUI.FocusControl(null);
                    }
                }

                GUILayout.EndHorizontal();
                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, false);

                if (currentEntry.ranks[i].ShowedInEditor)
                {
                    GUILayout.Space(10);
                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showUnlockSettings =
                        RPGBuilderEditorUtility.HandleModuleBanner("TALENT TREE SETTINGS",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showUnlockSettings);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showUnlockSettings)
                    {
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            false);
                        currentEntry.ranks[i].unlockCost =
                            RPGBuilderEditorFields.DrawHorizontalIntField("Unlock Cost",
                                "The cost to unlock this currentAbilityEntry inside your combat trees",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].unlockCost);
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            false);
                        GUILayout.Space(10);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showUseRequirements =
                        RPGBuilderEditorUtility.HandleModuleBanner("USE REQUIREMENTS",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showUseRequirements);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showUseRequirements)
                    {
                        
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
                        currentEntry.ranks[i].UseRequirementsTemplate = 
                            RPGBuilderEditorFields.DrawHorizontalToggle("Use Template?", "",
                            RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].UseRequirementsTemplate);

                        if (currentEntry.ranks[i].UseRequirementsTemplate)
                        {
                            currentEntry.ranks[i].RequirementsTemplate = (RequirementsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>(
                                "Template", "", currentEntry.ranks[i].RequirementsTemplate);
                        }
                        else
                        {
                            GUILayout.Space(10);
                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Requirement Group", false))
                            {
                                currentEntry.ranks[i].Requirements.Add(new RequirementsData.RequirementGroup());
                            }

                            currentEntry.ranks[i].Requirements = RPGBuilderEditorFields.DrawRequirementGroupsList(currentEntry.ranks[i].Requirements,false);
                        }
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin, true);
                        
                        GUILayout.Space(10);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showActivation =
                        RPGBuilderEditorUtility.HandleModuleBanner("ACTIVATION SETTINGS",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showActivation);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showActivation)
                    {
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                        currentEntry.ranks[i].activationType = 
                            (RPGAbility.AbilityActivationType) RPGBuilderEditorFields.DrawHorizontalEnum("Activation type", "?", 
                                (int)currentEntry.ranks[i].activationType,
                                Enum.GetNames(typeof(RPGAbility.AbilityActivationType)));
                        

                        switch (currentEntry.ranks[i].activationType)
                        {
                            case RPGAbility.AbilityActivationType.Casted:
                                currentEntry.ranks[i].castTime =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Cast Time",
                                        "Duration to cast the currentAbilityEntry",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].castTime);
                                
                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Conditions:", "", true);
                                currentEntry.ranks[i].castInRun =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Move and cast?",
                                        "Can this currentAbilityEntry be casted while moving?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].castInRun);

                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Animation events:", "", true);
                                currentEntry.ranks[i].animationTriggered =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Animation Triggered?",
                                        "Is the effect triggered by an animation event?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].animationTriggered);
                                        
                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Combos:", "", true);
                                currentEntry.ranks[i].comboStarsAfterCastComplete =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Combo After Cast?",
                                        "If this currentAbilityEntry starts a combo, should the combo start only after the cast is completed?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .comboStarsAfterCastComplete);
                                        
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("UI:", "", true);
                                currentEntry.ranks[i].castBarVisible =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Cast Bar Visible?",
                                        "Should this currentAbilityEntry display the cast bar during cast time?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].castBarVisible);
                                
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Top Down:", "", true);
                                currentEntry.ranks[i].faceCursorWhileCasting =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Face Cursor?",
                                        "TOP DOWN ONLY - Should the character face the cursor during the cast?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].faceCursorWhileCasting);

                                currentEntry.ranks[i].faceCursorWhenOnCastStart =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Face Cursor On Start?",
                                        "TOP DOWN ONLY - Should the character face the cursor when the cast starts?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .faceCursorWhenOnCastStart);

                                currentEntry.ranks[i].faceCursorWhenOnCastEnd =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Face Cursor On End?",
                                        "TOP DOWN ONLY - Should the character face the cursor when the cast ends?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].faceCursorWhenOnCastEnd);
                                break;
                            case RPGAbility.AbilityActivationType.Channeled:
                                currentEntry.ranks[i].channelTime =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Channel Time",
                                        "The duration of the channel",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].channelTime);
                                
                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Conditions:", "", true);
                                currentEntry.ranks[i].castInRun =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Move and channel?",
                                        "Can this currentAbilityEntry be channelled while moving?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].castInRun);
                                break;
                            case RPGAbility.AbilityActivationType.Charged:

                                break;
                        }

                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Conditions:", "", true);
                        currentEntry.ranks[i].canUseWhileMounted =
                            RPGBuilderEditorFields.DrawHorizontalToggle("Can use while mounted?",
                                "Can this ability be used while mounted?",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].canUseWhileMounted);
                        currentEntry.ranks[i].canBeUsedStunned =
                            RPGBuilderEditorFields.DrawHorizontalToggle("Stunned and use?",
                                "Can this currentAbilityEntry be used while being stunned?",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].canBeUsedStunned);
                        currentEntry.ranks[i].cancelStealth =
                            RPGBuilderEditorFields.DrawHorizontalToggle("Cancel Stealth?", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].cancelStealth);

                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Stand time:", "", true);
                        currentEntry.ranks[i].standTimeDuration =
                            RPGBuilderEditorFields.DrawHorizontalFloatField("Stand Time",
                                "How long should the caster be locked in place when using this currentAbilityEntry?",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].standTimeDuration);

                        if (currentEntry.ranks[i].standTimeDuration > 0)
                        {
                            currentEntry.ranks[i].canRotateInStandTime =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Mob Rotate In Stand Time?",
                                    "Can the NPC rotate while in stand time?",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].canRotateInStandTime);
                        }

                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Slow time:", "", true);
                        currentEntry.ranks[i].castSpeedSlowAmount =
                            RPGBuilderEditorFields.DrawHorizontalFloatField("Slow Amount",
                                "How much should the caster be slowed while casting this currentAbilityEntry?",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].castSpeedSlowAmount);

                        if (currentEntry.ranks[i].castSpeedSlowAmount > 0)
                        {
                            currentEntry.ranks[i].castSpeedSlowTime =
                                RPGBuilderEditorFields.DrawHorizontalFloatField("Slow Duration",
                                    "How long should the caster be slowed?",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].castSpeedSlowTime);

                            currentEntry.ranks[i].castSpeedSlowRate =
                                RPGBuilderEditorFields.DrawHorizontalFloatField("Slow Rate",
                                    "How fast should the movement speed be reduced?",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].castSpeedSlowRate);
                        }

                        RPGBuilderEditorFields.DrawTitleLabelExpanded("AI Settings :", "", true);
                        currentEntry.ranks[i].AIAttackTime =
                            RPGBuilderEditorFields.DrawHorizontalFloatField("Attack state duration",
                                "How long will the AI be in its attack state for?",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].AIAttackTime);

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                        GUILayout.Space(10);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showAbilityType =
                        RPGBuilderEditorUtility.HandleModuleBanner("ABILITY TYPE",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showAbilityType);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showAbilityType)
                    {
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                            
                        currentEntry.ranks[i].targetType =
                            (RPGAbility.TARGET_TYPES) RPGBuilderEditorFields.DrawHorizontalEnum("Ability Mechanic", "",
                                (int)currentEntry.ranks[i].targetType,
                                Enum.GetNames(typeof(RPGAbility.TARGET_TYPES)));

                        switch (currentEntry.ranks[i].targetType)
                        {
                            case RPGAbility.TARGET_TYPES.CONE:
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Area Settings:", "", true);
                                currentEntry.ranks[i].minRange =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Range",
                                        "The distance at which this cone hits",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].minRange);
                                currentEntry.ranks[i].coneDegree =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Angle",
                                        "The angle of the cone",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].coneDegree);
                                
                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Rules:", "", true);
                                currentEntry.ranks[i].ConeHitCount =
                                    RPGBuilderEditorFields.DrawHorizontalIntField("Hit count",
                                        "How many times does this currentAbilityEntry hit?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].ConeHitCount);
                                if (currentEntry.ranks[i].ConeHitCount > 1)
                                {
                                    currentEntry.ranks[i].ConeHitInterval =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField(
                                            "Hit Interval", "How much time between each hit?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].ConeHitInterval);
                                }

                                break;
                            case RPGAbility.TARGET_TYPES.AOE:
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Area Settings:", "", true);
                                currentEntry.ranks[i].AOERadius =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Radius",
                                        "The radius of the AoE",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].AOERadius);
                                
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Rules:", "", true);
                                currentEntry.ranks[i].AOEHitCount =
                                    RPGBuilderEditorFields.DrawHorizontalIntField("Hits",
                                        "How many times does this currentAbilityEntry hit?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].AOEHitCount);
                                if (currentEntry.ranks[i].AOEHitCount > 1)
                                {
                                    currentEntry.ranks[i].AOEHitInterval =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField(
                                            "Hit Interval", "How much time between each hit?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].AOEHitInterval);
                                }

                                break;
                            case RPGAbility.TARGET_TYPES.LINEAR:
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Area Settings:", "", true);
                                currentEntry.ranks[i].linearWidth =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Width",
                                        "How wide is the linear area?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].linearWidth);
                                currentEntry.ranks[i].linearLength =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Length",
                                        "How long is the linear area?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].linearLength);
                                currentEntry.ranks[i].linearHeight =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Height",
                                        "How high is the linear area?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].linearHeight);
                                        
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Rules:", "", true);
                                currentEntry.ranks[i].ConeHitCount =
                                    RPGBuilderEditorFields.DrawHorizontalIntField("Hits",
                                        "How many times does this currentAbilityEntry hit?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].ConeHitCount);
                                if (currentEntry.ranks[i].ConeHitCount > 1)
                                {
                                    currentEntry.ranks[i].ConeHitInterval =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField(
                                            "Hit Interval", "How much time between each hit?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].ConeHitInterval);
                                }

                                break;
                            case RPGAbility.TARGET_TYPES.PROJECTILE:
                            {
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Visual:", "", true);
                                currentEntry.ranks[i].projectileEffect =
                                    (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>(
                                        "Projectile Prefab", "", currentEntry.ranks[i].projectileEffect);

                                currentEntry.ranks[i].projectileUseNodeSocket =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Use Socket?",
                                        "Is the projectile spawned at a node Socket?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileUseNodeSocket);
                                if (currentEntry.ranks[i].projectileUseNodeSocket)
                                {
                                    currentEntry.ranks[i].projectileNodeSocket = (RPGBNodeSocket) RPGBuilderEditorFields.DrawTypeEntryField("Socket", allNodeSockets, currentEntry.ranks[i].projectileNodeSocket);

                                    currentEntry.ranks[i].projectileParentedToCaster =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Socket?",
                                            "Is the projectile parented to the socket?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .projectileParentedToCaster);
                                }
                                else
                                {
                                    currentEntry.ranks[i].effectPositionOffset =
                                        RPGBuilderEditorFields.DrawHorizontalVector3("Projectile Position Offset", "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .effectPositionOffset);
                                    currentEntry.ranks[i].projectileParentedToCaster =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Caster?",
                                            "Is the projectile parented to the caster?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .projectileParentedToCaster);
                                }

                                    RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Visual:", "", true);
                                currentEntry.ranks[i].hitEffect =
                                    (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Hit Prefab",
                                        "",
                                        currentEntry.ranks[i].hitEffect);
                                currentEntry.ranks[i].hitEffectUseSocket =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Hit Use Socket?",
                                        "Is the hit effect spawned at a node Socket?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].hitEffectUseSocket);
                                if (currentEntry.ranks[i].hitEffectUseSocket)
                                {
                                    
                                    currentEntry.ranks[i].hitEffectNodeSocket = (RPGBNodeSocket) RPGBuilderEditorFields.DrawTypeEntryField("Socket", allNodeSockets, currentEntry.ranks[i].hitEffectNodeSocket);

                                    currentEntry.ranks[i].hitAttachedToNode =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Socket?",
                                            "Is the hit effect parented to the socket?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].hitAttachedToNode);
                                }
                                else
                                {
                                    currentEntry.ranks[i].hitEffectPositionOffset =
                                        RPGBuilderEditorFields.DrawHorizontalVector3("Hit Position Offset", "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .hitEffectPositionOffset);
                                    currentEntry.ranks[i].hitAttachedToNode =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Node?",
                                            "Is the hit effect parented to the node hit?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].hitAttachedToNode);
                                }

                                currentEntry.ranks[i].hitEffectDuration =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Hit Effect Duration",
                                        "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].hitEffectDuration);

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Sound Settings:", "", true);
                                currentEntry.ranks[i].projectileSoundTemplate =
                                    (SoundTemplate) RPGBuilderEditorFields.DrawHorizontalObject<SoundTemplate>(
                                        "Sound Template", "",
                                        currentEntry.ranks[i].projectileSoundTemplate);
                                currentEntry.ranks[i].projectileSoundParentedToEffect =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Attach Sound to Projectile?",
                                        "Is the audio clip attached on the projectile prefab?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .projectileSoundParentedToEffect);


                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Collision Settings:", "", true);
                                currentEntry.ranks[i].useCustomCollision =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Blink Collision System?",
                                        "Is this projectile using rigidbody + collider or the blink custom collision system? Blink collisions are better but only for small sized projectiles",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].useCustomCollision);
                                if (!currentEntry.ranks[i].useCustomCollision)
                                {
                                    
                                    currentEntry.ranks[i].projectileColliderType =
                                        (RPGNpc.NPCColliderType) RPGBuilderEditorFields.DrawHorizontalEnum("Collider Type", "",
                                            (int)currentEntry.ranks[i].projectileColliderType,
                                            Enum.GetNames(typeof(RPGNpc.NPCColliderType)));

                                    switch (currentEntry.ranks[i]
                                        .projectileColliderType)
                                    {
                                        case RPGNpc.NPCColliderType.Capsule:
                                            currentEntry.ranks[i].colliderRadius =
                                                RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                    "Collider Radius", "",
                                                    RPGBuilderEditor.Instance.FieldHeight,
                                                    currentEntry.ranks[i]
                                                        .colliderRadius);
                                            currentEntry.ranks[i].colliderHeight =
                                                RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                    "Collider Height", "",
                                                    RPGBuilderEditor.Instance.FieldHeight,
                                                    currentEntry.ranks[i]
                                                        .colliderHeight);
                                            currentEntry.ranks[i].colliderCenter =
                                                RPGBuilderEditorFields.DrawHorizontalVector3(
                                                    "Collider Center", "",
                                                    RPGBuilderEditor.Instance.FieldHeight,
                                                    currentEntry.ranks[i]
                                                        .colliderCenter);
                                            break;
                                        case RPGNpc.NPCColliderType.Sphere:
                                            currentEntry.ranks[i].colliderRadius =
                                                RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                    "Collider Radius", "",
                                                    RPGBuilderEditor.Instance.FieldHeight,
                                                    currentEntry.ranks[i]
                                                        .colliderRadius);
                                            currentEntry.ranks[i].colliderCenter =
                                                RPGBuilderEditorFields.DrawHorizontalVector3(
                                                    "Collider Center", "",
                                                    RPGBuilderEditor.Instance.FieldHeight,
                                                    currentEntry.ranks[i]
                                                        .colliderCenter);
                                            break;
                                        case RPGNpc.NPCColliderType.Box:
                                            currentEntry.ranks[i].colliderCenter =
                                                RPGBuilderEditorFields.DrawHorizontalVector3(
                                                    "Collider Center", "",
                                                    RPGBuilderEditor.Instance.FieldHeight,
                                                    currentEntry.ranks[i]
                                                        .colliderCenter);
                                            currentEntry.ranks[i].colliderSize =
                                                RPGBuilderEditorFields.DrawHorizontalVector3(
                                                    "Collider Size", "",
                                                    RPGBuilderEditor.Instance.FieldHeight,
                                                    currentEntry.ranks[i]
                                                        .colliderSize);
                                            break;
                                    }
                                }

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Values:", "", true);
                                currentEntry.ranks[i].projectileSpeed =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Speed",
                                        "How fast does the projectile move?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileSpeed);
                                currentEntry.ranks[i].projectileDistance =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField(
                                        "Distance", "How far away can the projectile go?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileDistance);
                                
                                currentEntry.ranks[i].projectileDistanceMaxForNPC =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField(
                                        "Distance to use (AI)", "How far away must the NPC be to be able to cast this ability?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileDistanceMaxForNPC);
                                
                                currentEntry.ranks[i].projectileAngleSpread =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField(
                                        "Angle Spread", "Does the projectiles have an angle spread?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileAngleSpread);
                                currentEntry.ranks[i].projectileCount =
                                    RPGBuilderEditorFields.DrawHorizontalIntField("Counts",
                                        "How many projectiles should be fired?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileCount);
                                
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Time:", "", true);
                                currentEntry.ranks[i].firstProjectileDelay =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Initial Delay",
                                        "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].firstProjectileDelay);
                                currentEntry.ranks[i].projectileDelay =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Delay",
                                        "Is there a delay before each projectile?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileDelay);
                                currentEntry.ranks[i].projectileDuration =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Duration",
                                        "The total duration of the projectile",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileDuration);

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Optional Mechanics:", "", true);
                                currentEntry.ranks[i].isProjectileComeBack =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Come Back To caster?",
                                        "Does the projectile come back to its caster after a certain time?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].isProjectileComeBack);
                                if (currentEntry.ranks[i].isProjectileComeBack)
                                {
                                    currentEntry.ranks[i].projectileComeBackTime =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField(
                                            "Come Back Time",
                                            "After how long does the projectile comes back to the caster?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .projectileComeBackTime);
                                    currentEntry.ranks[i].projectileComeBackSpeed =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField(
                                            "Come Back Speed", "How fast does the projectile comes back to the caster?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .projectileComeBackSpeed);
                                }

                                currentEntry.ranks[i].isProjectileNearbyUnit =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Chain Effect?",
                                        "Does the projectile chain to nearby targets after the first hit?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].isProjectileNearbyUnit);
                                if (currentEntry.ranks[i].isProjectileNearbyUnit)
                                {
                                    currentEntry.ranks[i]
                                            .projectileNearbyUnitDistanceMax =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField("Chain Distance Max",
                                            "How far away can nearby units be hit from?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .projectileNearbyUnitDistanceMax);
                                    currentEntry.ranks[i].projectileNearbyUnitMaxHit =
                                        RPGBuilderEditorFields.DrawHorizontalFloatField("Chain Hit Max",
                                            "How many times maximum can nearby units be hit after the initial hit?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .projectileNearbyUnitMaxHit);
                                }

                                currentEntry.ranks[i].projectileAffectedByGravity =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Gravity?",
                                        "Is the projectile affected by Gravity?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .projectileAffectedByGravity);

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Destruction:", "", true);
                                currentEntry.ranks[i].projectileDestroyedByEnvironment
                                    = RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Destroyed by Environment?",
                                        "Is the projectile destroyed by the environment?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .projectileDestroyedByEnvironment);
                                if (currentEntry.ranks[i]
                                    .projectileDestroyedByEnvironment)
                                {
                                    RPGBuilderEditorFields.DrawHorizontalLabel("Destroyed By Layers",
                                        "What layers can destroy this projectile? on collision");
                                    LayerMask tempMask = EditorGUILayout.MaskField(
                                        InternalEditorUtility.LayerMaskToConcatenatedLayersMask(
                                            currentEntry.ranks[i]
                                                .projectileDestroyLayers),
                                        InternalEditorUtility.layers,
                                        RPGBuilderEditor.Instance.EditorSkin.GetStyle("CustomTextField"),
                                        GUILayout.ExpandWidth(true));
                                    currentEntry.ranks[i].projectileDestroyLayers =
                                        InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
                                    EditorGUILayout.EndHorizontal();
                                }

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Top Down:", "", true);
                                currentEntry.ranks[i].projectileShootOnClickPosition =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Shoot At Click Pos?",
                                        "TOP DOWN ONLY - Is the projectile shot at the initial click position?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .projectileShootOnClickPosition);
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Rules:", "", true);
                                break;
                            }
                            case RPGAbility.TARGET_TYPES.GROUND:
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Visual:", "", true);
                                currentEntry.ranks[i].groundVisualEffect =
                                    (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Ground Effect Prefab", "",
                                        currentEntry.ranks[i].groundVisualEffect);
                                currentEntry.ranks[i].effectPositionOffset =
                                    RPGBuilderEditorFields.DrawHorizontalVector3("Ground Position Offset", "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].effectPositionOffset);

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Area Settings:", "", true);
                                currentEntry.ranks[i].groundRadius =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Radius", "The radius",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundRadius);
                                currentEntry.ranks[i].groundRange =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Range",
                                        "The maximum range at which the ground currentAbilityEntry can be casted from the character",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundRange);
                                        
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Time:", "", true);
                                currentEntry.ranks[i].groundVisualEffectDuration =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Ground Effect Duration", "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .groundVisualEffectDuration);
                                currentEntry.ranks[i].groundHitTime =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Delay",
                                        "The delay before the currentAbilityEntry hits the ground",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundHitTime);
                                        
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Rules:", "", true);
                                currentEntry.ranks[i].groundHitCount =
                                    RPGBuilderEditorFields.DrawHorizontalIntField("Hits",
                                        "How many times does this currentAbilityEntry hits",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundHitCount);
                                currentEntry.ranks[i].groundHitInterval =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Hit Interval",
                                        "How much time between each hit?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundHitInterval);
                                break;
                            case RPGAbility.TARGET_TYPES.GROUND_LEAP:
                                RPGBuilderEditorFields.DrawHorizontalLabel("Stop Leap Layers",
                                    "What layers can stop the leap if collided with");
                                LayerMask tempMask2 = EditorGUILayout.MaskField(
                                    InternalEditorUtility.LayerMaskToConcatenatedLayersMask(
                                        currentEntry.ranks[i].groundLeapBlockLayers),
                                    InternalEditorUtility.layers, RPGBuilderEditorFields.GetTextFieldStyle(), GUILayout.ExpandWidth(true));
                                currentEntry.ranks[i].groundLeapBlockLayers =
                                    InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask2);
                                EditorGUILayout.EndHorizontal();

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Area Settings:", "", true);
                                currentEntry.ranks[i].groundRadius =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Radius", "The radius",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundRadius);
                                currentEntry.ranks[i].groundRange =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Range",
                                        "The maximum range at which the ground currentAbilityEntry can be casted from the character",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundRange);
                                
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Values:", "", true);
                                currentEntry.ranks[i].groundLeapDuration =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Leap Duration",
                                        "How long should the leap last?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundLeapDuration);
                                currentEntry.ranks[i].groundLeapHeight =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Leap Height",
                                        "How high should the leap be?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundLeapHeight);
                                currentEntry.ranks[i].groundLeapSpeed =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Leap Speed",
                                        "The speed of the leap",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundLeapSpeed);
                                        
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Time:", "", true);
                                currentEntry.ranks[i].groundHitTime =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Delay",
                                        "The delay before the currentAbilityEntry hits the ground",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundHitTime);

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Ability Triggered:", "", true);
                                currentEntry.ranks[i].extraAbilityExecutedID = RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.ranks[i].extraAbilityExecutedID, "Ability", "Leap Ability",
                                            "Extra currentAbilityEntry triggered by the leap");

                                currentEntry.ranks[i].extraAbilityExecutedActivationType =
                                    (RPGCombatDATA.CombatVisualActivationType) RPGBuilderEditorFields.DrawHorizontalEnum("Activation Type", "When is the leap currentAbilityEntry triggered",
                                        (int)currentEntry.ranks[i].extraAbilityExecutedActivationType,
                                        Enum.GetNames(typeof(RPGCombatDATA.CombatVisualActivationType)));
                                
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Rules:", "", true);
                                currentEntry.ranks[i].groundHitCount =
                                    RPGBuilderEditorFields.DrawHorizontalIntField("Hits",
                                        "How many times does this currentAbilityEntry hits",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundHitCount);
                                currentEntry.ranks[i].groundHitInterval =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Hit Interval",
                                        "How much time between each hit?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].groundHitInterval);
                                break;
                            case RPGAbility.TARGET_TYPES.TARGET_PROJECTILE:
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Visual:", "", true);
                                currentEntry.ranks[i].projectileEffect =
                                    (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Projectile Prefab", "",
                                        currentEntry.ranks[i].projectileEffect);

                                currentEntry.ranks[i].projectileUseNodeSocket =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Use Socket?",
                                        "Is the projectile spawned at a node Socket?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileUseNodeSocket);
                                if (currentEntry.ranks[i].projectileUseNodeSocket)
                                {
                                    currentEntry.ranks[i].projectileNodeSocket = (RPGBNodeSocket) RPGBuilderEditorFields.DrawTypeEntryField("Socket", allNodeSockets, currentEntry.ranks[i].projectileNodeSocket);

                                    currentEntry.ranks[i].projectileParentedToCaster =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Socket?",
                                            "Is the projectile parented to the socket?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .projectileParentedToCaster);
                                }
                                else
                                {
                                    currentEntry.ranks[i].effectPositionOffset =
                                        RPGBuilderEditorFields.DrawHorizontalVector3("Projectile Position Offset", "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .effectPositionOffset);
                                    currentEntry.ranks[i].projectileParentedToCaster =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Caster?",
                                            "Is the projectile parented to the caster?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .projectileParentedToCaster);
                                }
                                
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Visual:", "", true);
                                currentEntry.ranks[i].hitEffect =
                                    (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Hit Prefab",
                                        "",
                                        currentEntry.ranks[i].hitEffect);
                                currentEntry.ranks[i].hitEffectUseSocket =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Hit Use Socket?",
                                        "Is the hit effect spawned at a node Socket?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].hitEffectUseSocket);
                                if (currentEntry.ranks[i].hitEffectUseSocket)
                                {
                                    currentEntry.ranks[i].hitEffectNodeSocket = (RPGBNodeSocket) RPGBuilderEditorFields.DrawTypeEntryField("Socket", allNodeSockets, currentEntry.ranks[i].hitEffectNodeSocket);

                                    currentEntry.ranks[i].hitAttachedToNode =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Socket?",
                                            "Is the hit effect parented to the socket?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].hitAttachedToNode);
                                }
                                else
                                {
                                    currentEntry.ranks[i].hitEffectPositionOffset =
                                        RPGBuilderEditorFields.DrawHorizontalVector3("Hit Position Offset", "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .hitEffectPositionOffset);
                                    currentEntry.ranks[i].hitAttachedToNode =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Node?",
                                            "Is the hit effect parented to the node hit?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].hitAttachedToNode);
                                }

                                currentEntry.ranks[i].hitEffectDuration =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Hit Effect Duration",
                                        "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].hitEffectDuration);
                                        
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Sound:", "", true);
                                currentEntry.ranks[i].projectileSoundTemplate =
                                    (SoundTemplate) RPGBuilderEditorFields.DrawHorizontalObject<SoundTemplate>(
                                        "Sound Template", "",
                                        currentEntry.ranks[i].projectileSoundTemplate);
                                currentEntry.ranks[i].projectileSoundParentedToEffect =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Attach Sound to Projectile?",
                                        "Is the audio clip attached on the projectile prefab?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .projectileSoundParentedToEffect);

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Guided Settings:", "", true);
                                currentEntry.ranks[i].projectileTargetUseNodeSocket =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Use Target Socket?",
                                        "Is the projectile guided to a target's socket?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .projectileTargetUseNodeSocket);
                                if (currentEntry.ranks[i]
                                    .projectileTargetUseNodeSocket)
                                {
                                    currentEntry.ranks[i].projectileTargetNodeSocket = (RPGBNodeSocket) RPGBuilderEditorFields.DrawTypeEntryField("Socket", allNodeSockets, currentEntry.ranks[i].projectileTargetNodeSocket);
                                }

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Collider Settings:", "", true);
                                
                                currentEntry.ranks[i].projectileColliderType =
                                    (RPGNpc.NPCColliderType) RPGBuilderEditorFields.DrawHorizontalEnum("Collider Type", "",
                                        (int)currentEntry.ranks[i].projectileColliderType,
                                        Enum.GetNames(typeof(RPGNpc.NPCColliderType)));

                                currentEntry.ranks[i].useCustomCollision = false;
                                switch (currentEntry.ranks[i].projectileColliderType)
                                {
                                    case RPGNpc.NPCColliderType.Capsule:
                                        currentEntry.ranks[i].colliderRadius =
                                            RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                "Collider Radius", "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].colliderRadius);
                                        currentEntry.ranks[i].colliderHeight =
                                            RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                "Collider Height", "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].colliderHeight);
                                        currentEntry.ranks[i].colliderCenter =
                                            RPGBuilderEditorFields.DrawHorizontalVector3(
                                                "Collider Center", "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].colliderCenter);
                                        break;
                                    case RPGNpc.NPCColliderType.Sphere:
                                        currentEntry.ranks[i].colliderRadius =
                                            RPGBuilderEditorFields.DrawHorizontalFloatField(
                                                "Collider Radius", "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].colliderRadius);
                                        currentEntry.ranks[i].colliderCenter =
                                            RPGBuilderEditorFields.DrawHorizontalVector3(
                                                "Collider Center", "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].colliderCenter);
                                        break;
                                    case RPGNpc.NPCColliderType.Box:
                                        currentEntry.ranks[i].colliderCenter =
                                            RPGBuilderEditorFields.DrawHorizontalVector3(
                                                "Collider Center", "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].colliderCenter);
                                        currentEntry.ranks[i].colliderSize =
                                            RPGBuilderEditorFields.DrawHorizontalVector3(
                                                "Collider Size", "",
                                                RPGBuilderEditor.Instance.FieldHeight,
                                                currentEntry.ranks[i].colliderSize);
                                        break;
                                }

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Conditions:", "", true);
                                currentEntry.ranks[i].mustLookAtTarget =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Must Face Target?",
                                        "Should the caster be facing the target?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].mustLookAtTarget);
                                currentEntry.ranks[i].minRange =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Min. Range",
                                        "Minimum range from the target to use the currentAbilityEntry",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].minRange);
                                currentEntry.ranks[i].maxRange =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Max. Range",
                                        "Maximum range from the target to use the currentAbilityEntry",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].maxRange);
                                
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Values:", "", true);
                                currentEntry.ranks[i].projectileSpeed =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Speed",
                                        "How fast does the projectile move?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileSpeed);
                                currentEntry.ranks[i].projectileCount =
                                    RPGBuilderEditorFields.DrawHorizontalIntField("Counts",
                                        "How many projectiles should be fired?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileCount);
                                
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Time:", "", true);
                                currentEntry.ranks[i].projectileDelay =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Delay",
                                        "Is there a delay before each projectile?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].projectileDelay);

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Optional Mechanics:", "", true);
                                currentEntry.ranks[i].projectileAffectedByGravity =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Gravity?",
                                        "Is the projectile affected by Gravity?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .projectileAffectedByGravity);

                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Destruction:", "", true);
                                currentEntry.ranks[i]
                                        .projectileDestroyedByEnvironment =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Destroyed by Environment?",
                                        "Is the projectile destroyed by the environment?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i]
                                            .projectileDestroyedByEnvironment);
                                break;
                            case RPGAbility.TARGET_TYPES.TARGET_INSTANT:
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Conditions:", "", true);
                                currentEntry.ranks[i].mustLookAtTarget =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Must Face Target?",
                                        "Should the caster be facing the target?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].mustLookAtTarget);
                                currentEntry.ranks[i].minRange =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Min. Range",
                                        "Minimum range from the target to use the currentAbilityEntry",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].minRange);
                                currentEntry.ranks[i].maxRange =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Max. Range",
                                        "Maximum range from the target to use the currentAbilityEntry",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].maxRange);
                                
                                RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Visual:", "", true);
                                currentEntry.ranks[i].hitEffect =
                                    (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Hit Prefab", "",
                                        currentEntry.ranks[i].hitEffect);
                                currentEntry.ranks[i].hitEffectUseSocket =
                                    RPGBuilderEditorFields.DrawHorizontalToggle(
                                        "Hit Use Socket?",
                                        "Is the hit effect spawned at a node Socket?",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].hitEffectUseSocket);
                                if (currentEntry.ranks[i].hitEffectUseSocket)
                                {
                                    currentEntry.ranks[i].hitEffectNodeSocket = (RPGBNodeSocket) RPGBuilderEditorFields.DrawTypeEntryField("Socket", allNodeSockets, currentEntry.ranks[i].hitEffectNodeSocket);

                                    currentEntry.ranks[i].hitAttachedToNode =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Socket?",
                                            "Is the hit effect parented to the socket?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].hitAttachedToNode);
                                }
                                else
                                {
                                    currentEntry.ranks[i].hitEffectPositionOffset =
                                        RPGBuilderEditorFields.DrawHorizontalVector3("Hit Position Offset", "",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i]
                                                .hitEffectPositionOffset);
                                    currentEntry.ranks[i].hitAttachedToNode =
                                        RPGBuilderEditorFields.DrawHorizontalToggle(
                                            "Attach to Node?",
                                            "Is the hit effect parented to the node hit?",
                                            RPGBuilderEditor.Instance.FieldHeight,
                                            currentEntry.ranks[i].hitAttachedToNode);
                                }

                                currentEntry.ranks[i].hitEffectDuration =
                                    RPGBuilderEditorFields.DrawHorizontalFloatField("Hit Effect Duration",
                                        "",
                                        RPGBuilderEditor.Instance.FieldHeight,
                                        currentEntry.ranks[i].hitEffectDuration);
                                break;
                        }

                        if (currentEntry.ranks[i].targetType !=
                            RPGAbility.TARGET_TYPES.SELF
                            && currentEntry.ranks[i].targetType !=
                            RPGAbility.TARGET_TYPES.TARGET_INSTANT
                            && currentEntry.ranks[i].targetType !=
                            RPGAbility.TARGET_TYPES.TARGET_PROJECTILE
                        )
                        {
                            currentEntry.ranks[i].MaxUnitHit =
                                RPGBuilderEditorFields.DrawHorizontalIntField("Max Hits",
                                    "The maximum amount of units that can be hit by this currentAbilityEntry",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].MaxUnitHit);
                        }
                        
                        if (currentEntry.ranks[i].targetType != RPGAbility.TARGET_TYPES.SELF)
                        {
                            RPGBuilderEditorFields.DrawTitleLabelExpanded("Hit Settings:", "", true);
                            currentEntry.ranks[i].CanHitEnemy =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Can hit Enemies?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].CanHitEnemy);
                            currentEntry.ranks[i].CanHitNeutral =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Can hit Neutral?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].CanHitNeutral);
                            currentEntry.ranks[i].CanHitAlly =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Can hit Allies?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].CanHitAlly);
                            currentEntry.ranks[i].CanHitPlayer =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Can hit Player?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].CanHitPlayer);
                            currentEntry.ranks[i].CanHitPet =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Can hit owned Pets?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].CanHitPet);
                            currentEntry.ranks[i].CanHitOwner =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Can hit Owner?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].CanHitOwner);
                            currentEntry.ranks[i].CanHitSelf =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Can hit Self?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].CanHitSelf);
                            
                            RPGBuilderEditorFields.DrawTitleLabelExpanded("NPC Settings:", "", true);
                            if (currentEntry.ranks[i].targetType == RPGAbility.TARGET_TYPES.TARGET_INSTANT)
                            {
                                currentEntry.ranks[i].UsedOnALly =
                                    RPGBuilderEditorFields.DrawHorizontalToggle("Is used on allies?", "",
                                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].UsedOnALly);
                            }

                            currentEntry.ranks[i].LookAtTargetDuringCombatAction =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Look at target during cast?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].LookAtTargetDuringCombatAction);
                            currentEntry.ranks[i].LookAtTargetOnCombatAction =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Snap to target on use?", "",
                                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].LookAtTargetOnCombatAction);
                        }

                        
                        
                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Toggle Mechanic:", "", true);
                        currentEntry.ranks[i].isToggle =
                            RPGBuilderEditorFields.DrawHorizontalToggle("Is Toggle?", "",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].isToggle);
                        if (currentEntry.ranks[i].isToggle)
                        {
                            currentEntry.ranks[i].toggledTriggerInterval =
                                RPGBuilderEditorFields.DrawHorizontalFloatField("Toggle Interval",
                                    "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].toggledTriggerInterval);
                            currentEntry.ranks[i].isToggleCostOnTrigger =
                                RPGBuilderEditorFields.DrawHorizontalToggle("Cost Pulse?", "",
                                    RPGBuilderEditor.Instance.FieldHeight,
                                    currentEntry.ranks[i].isToggleCostOnTrigger);
                        }

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                            GUILayout.Space(10);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showCooldowns =
                        RPGBuilderEditorUtility.HandleModuleBanner("COOLDOWN SETTINGS",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showCooldowns);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showCooldowns)
                    {
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                        currentEntry.ranks[i].cooldown =
                            RPGBuilderEditorFields.DrawHorizontalFloatField("Cooldown",
                                "How long should the currentAbilityEntry be on cooldown after being used",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].cooldown);
                        currentEntry.ranks[i].startCDOnActivate =
                            RPGBuilderEditorFields.DrawHorizontalToggle("Start on activate",
                                "Is the cooldown started on activation?",
                                RPGBuilderEditor.Instance.FieldHeight,
                                currentEntry.ranks[i].startCDOnActivate);

                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Global Cooldown:", "", true);
                        currentEntry.ranks[i].isGCD = RPGBuilderEditorFields.DrawHorizontalToggle("Start GCD?",
                            "Should this currentAbilityEntry start the Global Cooldown?",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.ranks[i].isGCD);
                        currentEntry.ranks[i].CanUseDuringGCD = RPGBuilderEditorFields.DrawHorizontalToggle(
                            "Can use While GCD?",
                            "Can this currentAbilityEntry be casted during the Global Cooldown?",
                            RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.ranks[i].CanUseDuringGCD);


                        RPGBuilderEditorFields.DrawTitleLabelExpanded("Shared Cooldown:", "", true);
                        currentEntry.ranks[i].isSharingCooldown =
                            RPGBuilderEditorFields.DrawHorizontalToggle("Shared Cooldown?",
                                "Is this currentAbilityEntry cooldown triggered by other abilities with the same tag?",
                                RPGBuilderEditor.Instance.FieldHeight, currentEntry.ranks[i].isSharingCooldown);
                        if (currentEntry.ranks[i].isSharingCooldown)
                        {
                            currentEntry.ranks[i].abilityCooldownTag = (RPGBAbilityCooldownTag) RPGBuilderEditorFields.DrawTypeEntryField("Cooldown Tag", allAbilityCooldownTag, currentEntry.ranks[i].abilityCooldownTag);
                        }

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);
                        GUILayout.Space(10);
                    }

                    if (currentEntry.ranks[i].targetType !=
                        RPGAbility.TARGET_TYPES.SELF)
                    {
                        RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showCasterEffectsApplied =
                            RPGBuilderEditorUtility.HandleModuleBanner("CASTER EFFECTS APPLIED",
                                RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showCasterEffectsApplied);
                        GUILayout.Space(10);

                        if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showCasterEffectsApplied)
                        {
                            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Effect", true))
                            {
                                currentEntry.ranks[i].casterEffectsApplied.Add(new RPGAbility.AbilityEffectsApplied());
                            }

                            currentEntry.ranks[i].casterEffectsApplied =
                                RPGBuilderEditorFields.DrawEffectsAppliedList(
                                    currentEntry.ranks[i].casterEffectsApplied, true);

                            GUILayout.Space(10);
                        }
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showEffectsApplied =
                        RPGBuilderEditorUtility.HandleModuleBanner("EFFECTS APPLIED",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showEffectsApplied);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showEffectsApplied)
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Effect", true))
                        {
                            currentEntry.ranks[i].effectsApplied.Add(new RPGAbility.AbilityEffectsApplied());
                        }

                        currentEntry.ranks[i].effectsApplied =
                            RPGBuilderEditorFields.DrawEffectsAppliedList(currentEntry.ranks[i].effectsApplied, true);

                        GUILayout.Space(10);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showActions =
                        RPGBuilderEditorUtility.HandleModuleBanner("ACTIONS",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showActions);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showActions)
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Action", true))
                        {
                            currentEntry.ranks[i].Actions
                                .Add(new CombatData.AbilityAction());
                        }

                        for (var a = 0; a < currentEntry.ranks[i].Actions.Count; a++)
                        {
                            GUILayout.Space(10);
                            RPGBuilderEditorUtility.StartHorizontalMargin(
                                RPGBuilderEditor.Instance.LongHorizontalMargin,
                                true);
                            EditorGUILayout.BeginHorizontal();
                            currentEntry.ranks[i].Actions[a].RequirementsTarget =
                                (RPGCombatDATA.TARGET_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Required on", "",
                                    (int)currentEntry.ranks[i].Actions[a].RequirementsTarget,
                                    Enum.GetNames(typeof(RPGCombatDATA.TARGET_TYPE)));
                            if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                            {
                                currentEntry.ranks[i].Actions.RemoveAt(a);
                                return;
                            }
                            EditorGUILayout.EndHorizontal();

                            currentEntry.ranks[i].Actions[a].RequirementsTemplate = (RequirementsTemplate)
                                RPGBuilderEditorFields.DrawHorizontalObject<RequirementsTemplate>("Requirements",
                                    "", currentEntry.ranks[i].Actions[a].RequirementsTemplate);

                            currentEntry.ranks[i].Actions[a].ActionsTarget =
                                (RPGCombatDATA.TARGET_TYPE) RPGBuilderEditorFields.DrawHorizontalEnum("Triggered On", "",
                                    (int)currentEntry.ranks[i].Actions[a].ActionsTarget,
                                    Enum.GetNames(typeof(RPGCombatDATA.TARGET_TYPE)));

                            currentEntry.ranks[i].Actions[a].GameActionsTemplate = (GameActionsTemplate)
                                RPGBuilderEditorFields.DrawHorizontalObject<GameActionsTemplate>("Actions",
                                    "", currentEntry.ranks[i].Actions[a].GameActionsTemplate);
                            
                            RPGBuilderEditorUtility.EndHorizontalMargin(
                                RPGBuilderEditor.Instance.LongHorizontalMargin,
                                true);
                        }

                        GUILayout.Space(10);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showTags =
                        RPGBuilderEditorUtility.HandleModuleBanner("TAGS SETTINGS",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showTags);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showTags)
                    {
                        if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Tag", true))
                        {
                            currentEntry.ranks[i].tagsData.Add(new RPGAbility.AbilityTagsData());
                        }

                        for (var a = 0; a < currentEntry.ranks[i].tagsData.Count; a++)
                        {
                            GUILayout.Space(10);
                            RPGBuilderEditorUtility.StartHorizontalMargin(
                                RPGBuilderEditor.Instance.LongHorizontalMargin, true);
                            EditorGUILayout.BeginHorizontal();
                            currentEntry.ranks[i].tagsData[a].tag =
                                (RPGAbility.ABILITY_TAGS) RPGBuilderEditorFields.DrawHorizontalEnum("Applied on", "",
                                    (int)currentEntry.ranks[i].tagsData[a].tag,
                                    Enum.GetNames(typeof(RPGAbility.ABILITY_TAGS)));
                            if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                            {
                                currentEntry.ranks[i].tagsData.RemoveAt(a);
                                return;
                            }

                            EditorGUILayout.EndHorizontal();

                            if (currentEntry.ranks[i].tagsData[a].tag == RPGAbility.ABILITY_TAGS.shapeshifting ||
                                currentEntry.ranks[i].tagsData[a].tag == RPGAbility.ABILITY_TAGS.stealth)
                            {
                                currentEntry.ranks[i].tagsData[a].effectID =
                                    RPGBuilderEditorFields.DrawDatabaseEntryField(
                                        currentEntry.ranks[i].tagsData[a].effectID,
                                        "Effect", "Effect", "");
                            }

                            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                                true);
                        }

                        GUILayout.Space(10);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showTooltip =
                        RPGBuilderEditorUtility.HandleModuleBanner("TOOLTIP",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showTooltip);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showTooltip)
                    {
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);

                        var tooltipTitleStyle = new GUIStyle
                        {
                            alignment = TextAnchor.MiddleLeft,
                            fontSize = 13,
                            fontStyle = FontStyle.Bold,
                            normal =
                            {
                                textColor = Color.white,
                                background = RPGBuilderEditor.Instance.EditorData.AbilityTooltipBackground
                            },
                            hover =
                            {
                                textColor = Color.white,
                                background = RPGBuilderEditor.Instance.EditorData.AbilityTooltipBackground
                            },
                            clipping = TextClipping.Clip,
                            wordWrap = true,
                            padding = new RectOffset(12, 5, 5, 5)

                        };

                        var tooltipTextStyle = new GUIStyle
                        {
                            alignment = TextAnchor.UpperLeft,
                            fontSize = 13,
                            fontStyle = FontStyle.Bold,
                            normal =
                            {
                                textColor = Color.white,
                                background = RPGBuilderEditor.Instance.EditorData.AbilityTooltipBackground
                            },
                            hover =
                            {
                                textColor = Color.white,
                                background = RPGBuilderEditor.Instance.EditorData.AbilityTooltipBackgroundHover
                            },
                            clipping = TextClipping.Clip,
                            wordWrap = true,
                            padding = new RectOffset(10, 10, 10, 10)

                        };

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Box(currentEntry.entryIcon != null ? currentEntry.entryIcon.texture : RPGBuilderEditor.Instance.EditorData.defaultEntryIcon.texture,
                            RPGBuilderEditor.Instance.EditorSkin.GetStyle("CustomImage"), GUILayout.Width(40),
                            GUILayout.Height(40));
                        EditorGUILayout.LabelField(currentEntry.entryName + " Base Text", tooltipTitleStyle,
                            GUILayout.Height(40));
                        EditorGUILayout.EndHorizontal();
                        currentEntry.ranks[i].TooltipText =
                            RPGBuilderEditorFields.DrawHorizontalTooltipField(currentEntry.ranks[i].TooltipText,
                                tooltipTextStyle);

                        for (var index = 0; index < currentEntry.ranks[i].effectsApplied.Count; index++)
                        {
                            GUILayout.Space(10);

                            RPGEffect effect =
                                (RPGEffect) RPGBuilderEditorUtility.GetEntryByID(
                                    currentEntry.ranks[i].effectsApplied[index].effectID, "Effect");
                            if (effect != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Box(effect.entryIcon != null ? effect.entryIcon.texture : RPGBuilderEditor.Instance.EditorData.defaultEntryIcon.texture,
                                    RPGBuilderEditor.Instance.EditorSkin.GetStyle("CustomImage"), GUILayout.Width(40),
                                    GUILayout.Height(40));
                                EditorGUILayout.LabelField(effect.entryName + " (Effect Applied)", tooltipTitleStyle,
                                    GUILayout.Height(40));
                                EditorGUILayout.EndHorizontal();
                                currentEntry.ranks[i].effectsApplied[index].tooltipText =
                                    RPGBuilderEditorFields.DrawHorizontalTooltipField(
                                        currentEntry.ranks[i].effectsApplied[index].tooltipText, tooltipTextStyle);
                            }
                        }

                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin,
                            true);

                        GUILayout.Space(10);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showVisualEffects =
                        RPGBuilderEditorUtility.HandleModuleBanner("VISUAL EFFECTS",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showVisualEffects);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showVisualEffects)
                    {
                        currentEntry.ranks[i].VisualEffectEntries =
                            RPGBuilderEditorFields.DrawVisualEffectsList(currentEntry
                                .ranks[i].VisualEffectEntries, allNodeSockets);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showVisualAnimations =
                        RPGBuilderEditorUtility.HandleModuleBanner("ANIMATIONS",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showVisualAnimations);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showVisualAnimations)
                    {
                        currentEntry.ranks[i].AnimationEntries =
                            RPGBuilderEditorFields.DrawAnimationsList(currentEntry.ranks[i].AnimationEntries);
                    }

                    RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showSounds =
                        RPGBuilderEditorUtility.HandleModuleBanner("SOUNDS",
                            RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showSounds);
                    GUILayout.Space(10);

                    if (RPGBuilderEditor.Instance.EditorFilters.abilityModuleSection.showSounds)
                    {
                        currentEntry.ranks[i].SoundEntries =
                            RPGBuilderEditorFields.DrawSoundsList(currentEntry.ranks[i].SoundEntries);
                    }
                }
                GUILayout.Space(20);
            }
        }

        GUILayout.Space(25);

        EditorGUILayout.EndScrollView();
    }

    public override void ConvertDatabaseEntriesAfterUpdate()
    {
        var allEntries =
            Resources.LoadAll<RPGAbility>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            entry.entryName = entry._name;
            AssetDatabase.RenameAsset(RPGBuilderEditor.Instance.EditorData.ResourcePath + 
                                      RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + entry._fileName + ".asset", entry.entryName + AssetNameSuffix);
            entry.entryFileName = entry.entryName + AssetNameSuffix;
            entry.entryDisplayName = entry.displayName;
            entry.entryIcon = entry.icon;
            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public override void ConvertStringsToTypeEntries()
    {
        var allEntries =
            Resources.LoadAll<RPGAbility>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName);
        RPGBuilderEditorModule nodeSockets = RPGBuilderEditorUtility.GetModuleByName("Node Sockets");
        RPGBuilderEditorModule cooldownTags = RPGBuilderEditorUtility.GetModuleByName("Ability Cooldown Tags");

        foreach (var entry in allEntries)
        {
            EditorUtility.SetDirty(entry);
            foreach (var rank in entry.ranks)
            {
                {
                    RPGBuilderDatabaseEntry entryFile = nodeSockets.GetEntryByName(rank.projectileSocketName);
                    if (entryFile != null)
                    {
                        rank.projectileNodeSocket = (RPGBNodeSocket) entryFile;
                    }
                }
                {
                    RPGBuilderDatabaseEntry entryFile = nodeSockets.GetEntryByName(rank.projectileTargetSocketName);
                    if (entryFile != null)
                    {
                        rank.projectileTargetNodeSocket = (RPGBNodeSocket) entryFile;
                    }
                }
                {
                    RPGBuilderDatabaseEntry entryFile = nodeSockets.GetEntryByName(rank.hitEffectSocketName);
                    if (entryFile != null)
                    {
                        rank.hitEffectNodeSocket = (RPGBNodeSocket) entryFile;
                    }
                }
                {
                    foreach (var visualEffect in rank.visualEffects)
                    {
                        RPGBuilderDatabaseEntry entryFile = nodeSockets.GetEntryByName(visualEffect.SocketName);
                        if (entryFile != null)
                        {
                            visualEffect.NodeSocket = (RPGBNodeSocket) entryFile;
                        }
                    }
                }
                {
                    RPGBuilderDatabaseEntry entryFile = cooldownTags.GetEntryByName(rank.cooldownTag);
                    if (entryFile != null)
                    {
                        rank.abilityCooldownTag = (RPGBAbilityCooldownTag) entryFile;
                    }
                }
            }

            EditorUtility.SetDirty(entry);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
