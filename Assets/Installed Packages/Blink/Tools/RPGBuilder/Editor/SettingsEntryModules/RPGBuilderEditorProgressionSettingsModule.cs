using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class RPGBuilderEditorProgressionSettingsModule : RPGBuilderEditorModule
{
    private RPGBuilderProgressionSettings currentEntry;

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
        currentEntry = Resources.Load<RPGBuilderProgressionSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
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
            currentEntry = Resources.Load<RPGBuilderProgressionSettings>(
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
        RPGBuilderProgressionSettings entryFile = (RPGBuilderProgressionSettings) updatedEntry;
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

        RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showTalentTrees =
            RPGBuilderEditorUtility.HandleModuleBanner("TALENT TREES",
                RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showTalentTrees);
        if (RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showTalentTrees)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.TalentTreeNodesPerTier = RPGBuilderEditorFields.DrawHorizontalIntField("Nodes Per Tier",
                "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.TalentTreeNodesPerTier);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showLevelUp =
            RPGBuilderEditorUtility.HandleModuleBanner("LEVEL UP",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showLevelUp);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showLevelUp)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.characterLevelUpPrefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Character Level Effect", "",
                currentEntry.characterLevelUpPrefab);
            currentEntry.skillLevelUpPrefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Skill Level Effect", "",
                currentEntry.skillLevelUpPrefab);
            currentEntry.weaponTemplateLevelUpPrefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>(
                "Weapon Level Effect","", currentEntry.weaponTemplateLevelUpPrefab);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var entry = Resources.Load<RPGBuilderProgressionSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType);
        var oldSettings = Resources.Load<RPGGeneralDATA>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + "GeneralSettings");
        var oldSettings2 = Resources.Load<RPGCombatDATA>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + "CombatSettings");

        if (entry == null)
        {
            Debug.LogError("Could not find " + EntryType);
            return;
        }
        if (oldSettings == null)
        {
            Debug.LogError("Could not find " + "GeneralSettings");
            return;
        }
        if (oldSettings2 == null)
        {
            Debug.LogError("Could not find " + "CombatSettings");
            return;
        }
        
        EditorUtility.SetDirty(entry);

        entry.TalentTreeNodesPerTier = oldSettings2.talentTreesNodePerTierCount;
        entry.characterLevelUpPrefab = oldSettings.characterLevelUpPrefab;
        entry.skillLevelUpPrefab = oldSettings.skillLevelUpPrefab;
        entry.weaponTemplateLevelUpPrefab = oldSettings.weaponTemplateLevelUpPrefab;
        
        EditorUtility.SetDirty(entry);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
