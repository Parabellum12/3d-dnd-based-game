using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Managers;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class RPGBuilderEditorUISettingsModule : RPGBuilderEditorModule
{
     private RPGBuilderUISettings currentEntry;

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
        currentEntry = Resources.Load<RPGBuilderUISettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                            AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderUISettings>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderUISettings>(
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
        RPGBuilderUISettings entryFile = (RPGBuilderUISettings) updatedEntry;
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

        RPGBuilderEditor.Instance.EditorFilters.gameUISettingsModuleSection.showEssentials =
            RPGBuilderEditorUtility.HandleModuleBanner("ESSENTIALS",
                RPGBuilderEditor.Instance.EditorFilters.gameUISettingsModuleSection.showEssentials);
        if (RPGBuilderEditor.Instance.EditorFilters.gameUISettingsModuleSection.showEssentials)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            currentEntry.RPGBuilderEssentialsPrefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Essentials Prefab", "",
                currentEntry.RPGBuilderEssentialsPrefab);
            currentEntry.LoadingScreenManagerPrefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>(
                "Loading Screen Prefab", "", currentEntry.LoadingScreenManagerPrefab);

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);

        RPGBuilderEditor.Instance.EditorFilters.gameUISettingsModuleSection.showAbilityTooltips =
            RPGBuilderEditorUtility.HandleModuleBanner("TEXT COLORS",
                RPGBuilderEditor.Instance.EditorFilters.gameUISettingsModuleSection.showAbilityTooltips);
        if (RPGBuilderEditor.Instance.EditorFilters.gameUISettingsModuleSection.showAbilityTooltips)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Combat Texts", "", false);
                currentEntry.PhysicalDamageColor = RPGBuilderEditorFields.DrawHorizontalColorField("Physical Damage", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.PhysicalDamageColor);
                currentEntry.PhysicalCriticalDamageColor = RPGBuilderEditorFields.DrawHorizontalColorField("Physical Damage (Critical)", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.PhysicalCriticalDamageColor);
                
                currentEntry.MagicalDamageColor = RPGBuilderEditorFields.DrawHorizontalColorField("Magical Damage", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.MagicalDamageColor);
                currentEntry.MagicalCriticalDamageColor = RPGBuilderEditorFields.DrawHorizontalColorField("Magical Damage (Critical)", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.MagicalCriticalDamageColor);
                
                currentEntry.NeutralDamageColor = RPGBuilderEditorFields.DrawHorizontalColorField("Neutral Damage", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.NeutralDamageColor);
                currentEntry.NeutralCriticalDamageColor = RPGBuilderEditorFields.DrawHorizontalColorField("Neutral Damage (Critical)", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.NeutralCriticalDamageColor);
                
                
                currentEntry.HealingColor = RPGBuilderEditorFields.DrawHorizontalColorField("Healing", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.HealingColor);
                currentEntry.HealingCriticalColor = RPGBuilderEditorFields.DrawHorizontalColorField("Healing (Critical)", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.HealingCriticalColor);
                
                currentEntry.SelfDamageColor = RPGBuilderEditorFields.DrawHorizontalColorField("Damage Taken", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.SelfDamageColor);
                currentEntry.SelfHealColor = RPGBuilderEditorFields.DrawHorizontalColorField("Heal Received", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.SelfHealColor);
                currentEntry.SelfDamageCriticalColor = RPGBuilderEditorFields.DrawHorizontalColorField("Damage Taken (Critical)", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.SelfDamageCriticalColor);
                currentEntry.SelfHealCriticalColor = RPGBuilderEditorFields.DrawHorizontalColorField("Heal Received (Critical)", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.SelfHealCriticalColor);
                
                currentEntry.ThornDamageColor = RPGBuilderEditorFields.DrawHorizontalColorField("Thorn Damage", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ThornDamageColor);
                currentEntry.ImmuneColor = RPGBuilderEditorFields.DrawHorizontalColorField("Immune", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.ImmuneColor);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Events Text", "", true);
                currentEntry.EXPColor = RPGBuilderEditorFields.DrawHorizontalColorField("Experience Gain", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.EXPColor);
                currentEntry.LevelUpColor = RPGBuilderEditorFields.DrawHorizontalColorField("Level Gain", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.LevelUpColor);
                currentEntry.FactionColor = RPGBuilderEditorFields.DrawHorizontalColorField("Faction Point Gain", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.FactionColor);
                
                RPGBuilderEditorFields.DrawTitleLabelExpanded("Tooltips", "", true);
                currentEntry.RequirementMetColor = RPGBuilderEditorFields.DrawHorizontalColorField("Requirement Met",
                    "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.RequirementMetColor);
                currentEntry.RequirementNotMetColor = RPGBuilderEditorFields.DrawHorizontalColorField(
                    "Requirement Not Met", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.RequirementNotMetColor);
                
            GUILayout.Space(10);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }
}
