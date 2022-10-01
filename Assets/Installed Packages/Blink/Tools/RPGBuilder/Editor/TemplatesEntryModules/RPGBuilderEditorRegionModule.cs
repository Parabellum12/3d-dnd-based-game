using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorRegionModule : RPGBuilderEditorModule
{
    private Dictionary<int, RegionTemplate> entries = new Dictionary<int, RegionTemplate>();
    private RegionTemplate currentEntry;

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
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void LoadEntries()
    {
        Dictionary<int, RegionTemplate> dictionary = new Dictionary<int, RegionTemplate>();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<RegionTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                              AssetFolderName);
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

        currentEntry = CreateInstance<RegionTemplate>();
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
        RegionTemplate entryFile = (RegionTemplate) updatedEntry;
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

        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        currentEntry.entryName = RPGBuilderEditorFields.DrawHorizontalTextField("Name",
            "", RPGBuilderEditor.Instance.FieldHeight + 15, currentEntry.entryName);
        currentEntry.entryDisplayName = RPGBuilderEditorFields.DrawHorizontalTextField("Display Name",
            "", RPGBuilderEditor.Instance.FieldHeight + 15, currentEntry.entryDisplayName);
        currentEntry.entryFileName = currentEntry.entryName + AssetNameSuffix;
        GUILayout.Space(10);

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Fog", "", true);
        currentEntry.fogChange = RPGBuilderEditorFields.DrawHorizontalToggle("Modify Fog?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.fogChange);
        if (currentEntry.fogChange)
        {
            currentEntry.fogEnabled = RPGBuilderEditorFields.DrawHorizontalToggle("Enable Fog?", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.fogEnabled);
            currentEntry.fogColor = RPGBuilderEditorFields.DrawHorizontalColorField("Color", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.fogColor);

            currentEntry.fogMode =
                (FogMode) RPGBuilderEditorFields.DrawHorizontalEnum("Mode", "",
                    (int) currentEntry.fogMode,
                    Enum.GetNames(typeof(FogMode)));

            if (currentEntry.fogMode == FogMode.Linear)
            {
                currentEntry.fogStartDistance = RPGBuilderEditorFields.DrawHorizontalFloatField(
                    "Start Distance", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.fogStartDistance);
                currentEntry.fogEndDistance = RPGBuilderEditorFields.DrawHorizontalFloatField(
                    "End Distance",
                    "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.fogEndDistance);
            }
            else
            {
                currentEntry.fogDensity = RPGBuilderEditorFields.DrawHorizontalFloatField("Density", "",
                    RPGBuilderEditor.Instance.FieldHeight, currentEntry.fogDensity);
            }

            currentEntry.fogTransitionSpeed = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Transition Speed", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.fogTransitionSpeed);
        }

        GUILayout.Space(10);

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Lightning", "", true);
        currentEntry.lightningChange = RPGBuilderEditorFields.DrawHorizontalToggle("Modify Lightning?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.lightningChange);
        if (currentEntry.lightningChange)
        {
            currentEntry.lightGameobjectName = RPGBuilderEditorFields.DrawHorizontalTextField(
                "Light Name", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.lightGameobjectName);
            currentEntry.lightEnabled = RPGBuilderEditorFields.DrawHorizontalToggle("Enable Light?", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.lightEnabled);
            currentEntry.lightColor = RPGBuilderEditorFields.DrawHorizontalColorField("Color", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.lightColor);
            currentEntry.lightIntensity = RPGBuilderEditorFields.DrawHorizontalFloatField("Intensity", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.lightIntensity);
            currentEntry.lightTransitionSpeed = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Transition Speed", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.lightTransitionSpeed);
        }

        GUILayout.Space(10);

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Skybox", "", true);
        currentEntry.skyboxChange = RPGBuilderEditorFields.DrawHorizontalToggle("Modify Skybox?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.skyboxChange);
        if (currentEntry.skyboxChange)
        {
            currentEntry.skyboxCubemap = (Texture) EditorGUILayout.ObjectField(
                "Cubemap Texture",
                currentEntry.skyboxCubemap, typeof(Texture), false,
                GUILayout.Height(15));
            currentEntry.skyboxTransitionSpeed = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Blend Speed", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.skyboxTransitionSpeed);
        }

        GUILayout.Space(10);

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Particles", "", true);
        currentEntry.cameraParticleChange = RPGBuilderEditorFields.DrawHorizontalToggle(
            "Modify Camera Particle?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.cameraParticleChange);
        if (currentEntry.cameraParticleChange)
        {
            currentEntry.cameraParticle = (GameObject) EditorGUILayout.ObjectField(
                "Particle Prefab",
                currentEntry.cameraParticle, typeof(GameObject), false,
                GUILayout.Height(15));
        }

        GUILayout.Space(10);

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Combat", "", true);
        currentEntry.combatModeChange = RPGBuilderEditorFields.DrawHorizontalToggle("Modify Combat Mode?",
            "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.combatModeChange);
        if (currentEntry.combatModeChange)
        {
            currentEntry.combatEnabled = RPGBuilderEditorFields.DrawHorizontalToggle("Enable Combat?", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.combatEnabled);
        }

        GUILayout.Space(10);

        currentEntry.combatStateChange = RPGBuilderEditorFields.DrawHorizontalToggle("Modify Combat State?",
            "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.combatStateChange);
        if (currentEntry.combatStateChange)
        {
            currentEntry.inCombat = RPGBuilderEditorFields.DrawHorizontalToggle("Override in Combat?", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.inCombat);
        }

        GUILayout.Space(10);

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Text", "", true);
        currentEntry.welcomeText = RPGBuilderEditorFields.DrawHorizontalToggle("Display Welcome Text?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.welcomeText);
        if (currentEntry.welcomeText)
        {
            currentEntry.welcomeMessageText = RPGBuilderEditorFields.DrawHorizontalTextField("Message", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.welcomeMessageText);
            currentEntry.welcomeMessageDuration = RPGBuilderEditorFields.DrawHorizontalFloatField(
                "Duration", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.welcomeMessageDuration);
        }

        GUILayout.Space(10);

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Music", "", true);
        currentEntry.musicChange = RPGBuilderEditorFields.DrawHorizontalToggle("Modify Music?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.musicChange);
        if (currentEntry.musicChange)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Music", true))
            {
                currentEntry.musicClips.Add(null);
            }

            GUILayout.Space(10);

            for (var index = 0; index < currentEntry.musicClips.Count; index++)
            {
                EditorGUILayout.BeginHorizontal();
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.musicClips.RemoveAt(index);
                    return;
                }

                currentEntry.musicClips[index] =
                    (AudioClip) EditorGUILayout.ObjectField(
                        "Audio Clip", currentEntry.musicClips[index],
                        typeof(AudioClip),
                        false, GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Actions", "", true);
            currentEntry.EnterGameActionsTemplate =
                (GameActionsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<GameActionsTemplate>("Enter Actions",
                    "", currentEntry.EnterGameActionsTemplate);
            currentEntry.ExitGameActionsTemplate =
                (GameActionsTemplate) RPGBuilderEditorFields.DrawHorizontalObject<GameActionsTemplate>("Exit Actions",
                    "", currentEntry.ExitGameActionsTemplate);

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }
}
