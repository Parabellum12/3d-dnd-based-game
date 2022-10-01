using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorCharactersSettingsModule : RPGBuilderEditorModule
{
    private RPGBuilderCharacterSettings currentEntry;

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
        currentEntry = Resources.Load<RPGBuilderCharacterSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                                   AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderCharacterSettings>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderCharacterSettings>(
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
        RPGBuilderCharacterSettings entryFile = (RPGBuilderCharacterSettings) updatedEntry;
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

            currentEntry.NoClasses = RPGBuilderEditorFields.DrawHorizontalToggle("No Classes", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.NoClasses);
            
            currentEntry.CanTargetPlayerOnClick = RPGBuilderEditorFields.DrawHorizontalToggle(
                "Target self on click?", "", RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.CanTargetPlayerOnClick);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showStatSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("STAT",
                RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showStatSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showStatSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            currentEntry.StatAllocationPointID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.StatAllocationPointID, "Point", "Point", "");

            currentEntry.MustSpendAllStatPointsToCreateCharacter =
                RPGBuilderEditorFields.DrawHorizontalToggle("Must spend all stat points?",
                    "Must spend all stat points to create character?",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.MustSpendAllStatPointsToCreateCharacter);
            currentEntry.CanRefundStatPointInGame =
                RPGBuilderEditorFields.DrawHorizontalToggle("Refunding points allowed?",
                    "Can the player decrease points once spent?",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.CanRefundStatPointInGame);

            GUILayout.Space(10);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        }


        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showSprint =
            RPGBuilderEditorUtility.HandleModuleBanner("SPRINT", RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showSprint);
        if (RPGBuilderEditor.Instance.EditorFilters.combatSettingsModuleSection.showSprint)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.SprintStatDrainID =
                RPGBuilderEditorFields.DrawDatabaseEntryField(currentEntry.SprintStatDrainID, "Stat", "Stat", "");
            currentEntry.SprintStatDrainAmount =
                RPGBuilderEditorFields.DrawHorizontalIntField("Amount", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.SprintStatDrainAmount);
            currentEntry.SprintStatDrainInterval =
                RPGBuilderEditorFields.DrawHorizontalFloatField("Interval", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.SprintStatDrainInterval);
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            GUILayout.Space(10);
        }


        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }
    
    public override void ConvertDatabaseEntriesAfterUpdate ()
    {
        var entry = Resources.Load<RPGBuilderCharacterSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType);
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

        entry.StatFunctionsList = oldSettings.StatFunctionsList;
        entry.nodeSocketNames = oldSettings.nodeSocketNames;
        entry.NoClasses = !oldSettings.useClasses;
        entry.CanTargetPlayerOnClick = oldSettings.targetPlayerOnClick;
        entry.StatAllocationPointID = oldSettings.pointID;
        entry.MustSpendAllStatPointsToCreateCharacter = oldSettings.spendAllStatPointsToCreateChar;
        entry.CanRefundStatPointInGame = oldSettings.canDescreaseGameStatPoints;
        entry.SprintStatDrainID = oldSettings.sprintStatDrainID;
        entry.SprintStatDrainAmount = oldSettings.sprintStatDrainAmount;
        entry.SprintStatDrainInterval = oldSettings.sprintStatDrainInterval;
        
        EditorUtility.SetDirty(entry);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
