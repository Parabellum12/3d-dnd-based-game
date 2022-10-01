using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RPGBuilderEditorWorldSettingsModule : RPGBuilderEditorModule
{
    private RPGBuilderWorldSettings currentEntry;

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
        currentEntry = Resources.Load<RPGBuilderWorldSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                               AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderWorldSettings>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderWorldSettings>(
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
        RPGBuilderWorldSettings entryFile = (RPGBuilderWorldSettings) updatedEntry;
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

        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showTimeSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("WORLD TIME SETTINGS",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showTimeSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showTimeSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Years", "");
            currentEntry.UseYears = RPGBuilderEditorFields.DrawHorizontalToggle("Use years?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.UseYears);
            if (currentEntry.UseYears)
            {
                currentEntry.StartingYear = RPGBuilderEditorFields.DrawHorizontalIntField("Starting year", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.StartingYear);
                currentEntry.MonthsPerYear = RPGBuilderEditorFields.DrawHorizontalIntField("Months per year", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.MonthsPerYear);
            }
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Months", "", true);
            currentEntry.UseMonths = RPGBuilderEditorFields.DrawHorizontalToggle("Use months?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.UseMonths);
            if (currentEntry.UseMonths)
            {
                currentEntry.StartingMonth = RPGBuilderEditorFields.DrawHorizontalIntField("Starting month", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.StartingMonth);
                currentEntry.WeeksPerMonth = RPGBuilderEditorFields.DrawHorizontalIntField("Weeks per month", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.WeeksPerMonth);
            }
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Weeks", "", true);
            currentEntry.UseWeeks = RPGBuilderEditorFields.DrawHorizontalToggle("Use weeks?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.UseWeeks);
            if (currentEntry.UseWeeks)
            {
                currentEntry.StartingWeek = RPGBuilderEditorFields.DrawHorizontalIntField("Starting week", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.StartingWeek);
                currentEntry.DaysPerWeek = RPGBuilderEditorFields.DrawHorizontalIntField("Days per week", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.DaysPerWeek);
            }
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Days", "", true);
            currentEntry.UseDays = RPGBuilderEditorFields.DrawHorizontalToggle("Use days?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.UseDays);
            if (currentEntry.UseDays)
            {
                currentEntry.StartingDay = RPGBuilderEditorFields.DrawHorizontalIntField("Starting day", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.StartingDay);
                currentEntry.HoursPerDay = RPGBuilderEditorFields.DrawHorizontalIntField("Hours per day", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.HoursPerDay);
            }
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Hours", "", true);
            currentEntry.UseHours = RPGBuilderEditorFields.DrawHorizontalToggle("Use hours?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.UseHours);
            if (currentEntry.UseHours)
            {
                currentEntry.StartingHour = RPGBuilderEditorFields.DrawHorizontalIntField("Starting hour", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.StartingHour);
                currentEntry.MinutesPerHour = RPGBuilderEditorFields.DrawHorizontalIntField("Minutes per hour", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.MinutesPerHour);
            }
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Minutes", "", true);
            currentEntry.UseMinutes = RPGBuilderEditorFields.DrawHorizontalToggle("Use minutes?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.UseMinutes);
            if (currentEntry.UseMinutes)
            {
                currentEntry.StartingMinute = RPGBuilderEditorFields.DrawHorizontalIntField("Starting minute", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.StartingMinute);
                currentEntry.SecondsPerMinutes = RPGBuilderEditorFields.DrawHorizontalIntField("Seconds per minute", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.SecondsPerMinutes);
            }
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Seconds", "", true);
            currentEntry.UseSeconds = RPGBuilderEditorFields.DrawHorizontalToggle("Use seconds?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.UseSeconds);
            if (currentEntry.UseSeconds)
            {
                currentEntry.StartingSecond = RPGBuilderEditorFields.DrawHorizontalIntField("Starting second", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.StartingSecond);
            }

            RPGBuilderEditorFields.DrawTitleLabelExpanded("Settings", "", true);
            currentEntry.GlobalTimeSpeed = RPGBuilderEditorFields.DrawHorizontalFloatField("Global Time Speed", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.GlobalTimeSpeed);
            currentEntry.HourDuration = RPGBuilderEditorFields.DrawHorizontalFloatField("Hour Duration (seconds)", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.HourDuration);
            currentEntry.SecondDuration = currentEntry.HourDuration / (currentEntry.SecondsPerMinutes * currentEntry.MinutesPerHour);
                
            float minuteDuration = currentEntry.HourDuration / currentEntry.MinutesPerHour;
            float secondDuration = minuteDuration / currentEntry.SecondsPerMinutes;
            float hourDuration = currentEntry.HourDuration;
            float dayDuration = currentEntry.HourDuration * currentEntry.HoursPerDay;
            float weekDuration = dayDuration * currentEntry.DaysPerWeek;
            float monthDuration = weekDuration * currentEntry.WeeksPerMonth;
            float yearDuration = monthDuration * currentEntry.MonthsPerYear;
            secondDuration /= currentEntry.GlobalTimeSpeed;
            secondDuration /= 60;
            minuteDuration /= currentEntry.GlobalTimeSpeed;
            minuteDuration /= 60;
            hourDuration /= currentEntry.GlobalTimeSpeed;
            hourDuration /= 60;
            dayDuration /= currentEntry.GlobalTimeSpeed;
            dayDuration /= 60;
            weekDuration /= currentEntry.GlobalTimeSpeed;
            weekDuration /= 60;
            monthDuration /= currentEntry.GlobalTimeSpeed;
            monthDuration /= 60;
            yearDuration /= currentEntry.GlobalTimeSpeed;
            yearDuration /= 60;
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Preview", "", true);
            RPGBuilderEditorFields.DrawLabelExpanded("Second duration: " + secondDuration + " hours", "");
            RPGBuilderEditorFields.DrawLabelExpanded("Minute duration: " + minuteDuration + " hours", "");
            RPGBuilderEditorFields.DrawLabelExpanded("Hour duration: " + hourDuration + " hours", "");
            RPGBuilderEditorFields.DrawLabelExpanded("Day duration: " + dayDuration + " hours", "");
            RPGBuilderEditorFields.DrawLabelExpanded("Week duration: " + weekDuration + " hours", "");
            RPGBuilderEditorFields.DrawLabelExpanded("Month duration: " + monthDuration + " hours", "");
            RPGBuilderEditorFields.DrawLabelExpanded("Year duration: " + yearDuration + " hours", "");
            
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showGameModifierSettings =
            RPGBuilderEditorUtility.HandleModuleBanner("GAME MODIFIERS",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showGameModifierSettings);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showGameModifierSettings)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            currentEntry.useGameModifiers = RPGBuilderEditorFields.DrawHorizontalToggle("Use Game Modifiers?", "",
                RPGBuilderEditor.Instance.FieldHeight,
                currentEntry.useGameModifiers);
            if (currentEntry.useGameModifiers)
            {
                currentEntry.baseGameModifierPointsInMenu = RPGBuilderEditorFields.DrawHorizontalIntField(
                    "Starting Points (Menu)", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.baseGameModifierPointsInMenu);
                currentEntry.baseGameModifierPointsInWorld = RPGBuilderEditorFields.DrawHorizontalIntField(
                    "Starting Points (World)", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.baseGameModifierPointsInWorld);

                currentEntry.negativePointsRequired = RPGBuilderEditorFields.DrawHorizontalIntField(
                    "Negative Points Required?", "",
                    RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.negativePointsRequired);

                currentEntry.checkMinNegativeModifier = RPGBuilderEditorFields.DrawHorizontalToggle(
                    "Min Negative Modifiers?", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.checkMinNegativeModifier);
                if (currentEntry.checkMinNegativeModifier)
                {
                    currentEntry.minimumRequiredNegativeGameModifiers =
                        RPGBuilderEditorFields.DrawHorizontalIntField(
                            "Negative Modifiers",
                            "", RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.minimumRequiredNegativeGameModifiers);
                }

                currentEntry.checkMaxPositiveModifier = RPGBuilderEditorFields.DrawHorizontalToggle(
                    "Max Positive Modifiers?", "", RPGBuilderEditor.Instance.FieldHeight,
                    currentEntry.checkMaxPositiveModifier);
                if (currentEntry.checkMaxPositiveModifier)
                {
                    currentEntry.maximumRequiredPositiveGameModifiers =
                        RPGBuilderEditorFields.DrawHorizontalIntField(
                            "Positive Modifiers",
                            "", RPGBuilderEditor.Instance.FieldHeight,
                            currentEntry.maximumRequiredPositiveGameModifiers);
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showLayers =
            RPGBuilderEditorUtility.HandleModuleBanner("LAYERS",
                RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showLayers);
        if (RPGBuilderEditor.Instance.EditorFilters.generalSettingsModuleSection.showLayers)
        {
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            RPGBuilderEditorFields.DrawHorizontalLabel("Interactable Layer", "");
            currentEntry.worldInteractableLayer =
                EditorGUILayout.LayerField(currentEntry.worldInteractableLayer, RPGBuilderEditorFields.GetTextFieldStyle());
            EditorGUILayout.EndHorizontal();
            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }

    public override void ConvertDatabaseEntriesAfterUpdate()
    {
        var entry = Resources.Load<RPGBuilderWorldSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                            AssetFolderName + "/" + EntryType);
        var oldSettings = Resources.Load<RPGGeneralDATA>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                         AssetFolderName + "/" + "GeneralSettings");

        if (entry == null)
        {
            Debug.LogError("Could not find " + EntryType);
            return;
        }

        if (oldSettings == null)
        {
            Debug.LogError("Could not find " + "currentEntry");
            return;
        }

        EditorUtility.SetDirty(entry);

        entry.dialogueKeywordsList = oldSettings.dialogueKeywordsList;
        entry.useGameModifiers = oldSettings.useGameModifiers;
        entry.maximumRequiredPositiveGameModifiers = oldSettings.maximumRequiredPositiveGameModifiers;
        entry.minimumRequiredNegativeGameModifiers = oldSettings.minimumRequiredNegativeGameModifiers;
        entry.baseGameModifierPointsInMenu = oldSettings.baseGameModifierPointsInMenu;
        entry.baseGameModifierPointsInWorld = oldSettings.baseGameModifierPointsInWorld;
        entry.worldInteractableLayer = oldSettings.worldInteractableLayer;

        EditorUtility.SetDirty(entry);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
