using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class RPGBuilderEditorCombatSettingsModule : RPGBuilderEditorModule
{
    private RPGBuilderCombatSettings currentEntry;

    public override void Initialize()
    {
        LoadEntries();
    }

    public override void InstantiateCurrentEntry(int index)
    {
        LoadEntries();
    }

    public override void LoadEntries()
    {
        currentEntry = Resources.Load<RPGBuilderCombatSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                                AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderCombatSettings>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderCombatSettings>(
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                AssetFolderName + "/" + EntryType);
        }

        databaseEntries.Clear();
        databaseEntries.Add(currentEntry);

        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void CreateNewEntry()
    {
    }

    public override bool SaveConditionsMet()
    {
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGBuilderCombatSettings entryFile = (RPGBuilderCombatSettings) updatedEntry;
        entryFile.UpdateEntryData(currentEntry);
    }

    public override void ClearEntries()
    {
        databaseEntries.Clear();
        currentEntry = null;
    }

    public override void DrawView()
    {
        RPGBuilderEditorUtility.UpdateViewAndFieldData();

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);

        float panelWidth = RPGBuilderEditorUtility.GetScreenWidth();
        panelWidth -= panelWidth * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent;
        float panelHeight = RPGBuilderEditorUtility.GetScreenHeight();
        Rect panelRect = new Rect(
            RPGBuilderEditorUtility.GetScreenWidth() * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent, 0,
            panelWidth, panelHeight);

        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(panelRect.width),
            GUILayout.MaxWidth(panelRect.width),
            GUILayout.ExpandHeight(true));

        RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showCombatRules =
            RPGBuilderEditorUtility.HandleModuleBanner("RULES",
                RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showCombatRules);

        if (RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showCombatRules)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Stats:", "");
            currentEntry.HealthStatID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.HealthStatID, "Stat", "Stat", "");
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("NPCs:", "", true);
            currentEntry.DefaultAILogicTemplate = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>(
                "Default AI Logic Template", "" , currentEntry.DefaultAILogicTemplate);
            currentEntry.DefaultAIBehaviorTemplate = (AIBehaviorTemplate) RPGBuilderEditorFields.DrawHorizontalObject<AIBehaviorTemplate>(
                "Default Behavior Template", "The default Behavior Template when none is assigned",
                currentEntry.DefaultAIBehaviorTemplate);
            
            
            currentEntry.NPCSpawnerDistanceCheckInterval = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "NPC Spawner Check Interval", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.NPCSpawnerDistanceCheckInterval);

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Cooldown:", "", true);
            currentEntry.GlobalCooldownDuration = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Global Cooldown Duration", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.GlobalCooldownDuration);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Damage Calculation:", "", true);
            currentEntry.CriticalHitBonus = RPGBuilderEditorFields.DrawHorizontalFloatField("Critical Bonus (%)",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.CriticalHitBonus);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Combat State:", "", true);
            currentEntry.ResetCombatDuration = RPGBuilderEditorFields.DrawHorizontalFloatField("Combat Reset Timer",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.ResetCombatDuration);
            currentEntry.AutomaticCombatStates = RPGBuilderEditorFields.DrawHorizontalToggle("Auto. Combat State?",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.AutomaticCombatStates);

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Projectiles:", "", true);
            RPGBuilderEditorFields.DrawHorizontalLabel("Projectile Check",
                "What layer will the projectile check in aim mode");
            LayerMask projCheckMask = EditorGUILayout.MaskField(
                InternalEditorUtility.LayerMaskToConcatenatedLayersMask(
                    currentEntry.ProjectileRaycastLayers),
                InternalEditorUtility.layers, RPGBuilderEditorFields.GetTextFieldStyle(), GUILayout.ExpandWidth(true));
            currentEntry.ProjectileRaycastLayers =
                InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(projCheckMask);
            EditorGUILayout.EndHorizontal();

            RPGBuilderEditorFields.DrawHorizontalLabel("Projectile Destroy",
                "What layer will destroy the projectile on collision");
            LayerMask projDestroyMask = EditorGUILayout.MaskField(
                InternalEditorUtility.LayerMaskToConcatenatedLayersMask(
                    currentEntry.ProjectileDestroyLayers),
                InternalEditorUtility.layers, RPGBuilderEditorFields.GetTextFieldStyle(), GUILayout.ExpandWidth(true));
            currentEntry.ProjectileDestroyLayers =
                InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(projDestroyMask);
            EditorGUILayout.EndHorizontal();

            RPGBuilderEditorFields.DrawHorizontalLabel("Leap Interrupt",
                "What layer will interrupt a leap if getting close to it");
            LayerMask leapBlockMask = EditorGUILayout.MaskField(
                InternalEditorUtility.LayerMaskToConcatenatedLayersMask(
                    currentEntry.InterruptLeapLayers),
                InternalEditorUtility.layers, RPGBuilderEditorFields.GetTextFieldStyle(), GUILayout.ExpandWidth(true));
            currentEntry.InterruptLeapLayers =
                InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(leapBlockMask);
            EditorGUILayout.EndHorizontal();

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        }

        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }

    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var entry = Resources.Load<RPGBuilderCombatSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType);
        var oldSettings = Resources.Load<RPGCombatDATA>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + "CombatSettings");

        if (entry == null)
        {
            Debug.LogError("Could not find " + EntryType);
            return;
        }
        if (oldSettings == null)
        {
            Debug.LogError("Could not find " + "CombatSettings");
            return;
        }
        
        EditorUtility.SetDirty(entry);
        
        entry.FactionStancesList = oldSettings.FactionStancesList;
        entry.AbilityCooldownTagList = oldSettings.AbilityCooldownTagList;
        entry.EffectTagList = oldSettings.EffectTagList;
        entry.CriticalHitBonus = oldSettings.CriticalDamageBonus;
        entry.GlobalCooldownDuration = oldSettings.GCDDuration;
        entry.ResetCombatDuration = oldSettings.outOfCombatDuration;
        entry.AutomaticCombatStates = oldSettings.useAutomaticCombatState;
        entry.HealthStatID = oldSettings.healthStatID;
        entry.ProjectileRaycastLayers = oldSettings.projectileCheckLayer;
        entry.ProjectileDestroyLayers = oldSettings.projectileDestroyLayer;
        entry.InterruptLeapLayers = oldSettings.leapInterruptLayer;
        
        EditorUtility.SetDirty(entry);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
