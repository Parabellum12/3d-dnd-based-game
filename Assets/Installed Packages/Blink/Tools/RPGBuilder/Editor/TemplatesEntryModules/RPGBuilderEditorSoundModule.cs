using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class RPGBuilderEditorSoundModule : RPGBuilderEditorModule
{
    private Dictionary<int, SoundTemplate> entries = new Dictionary<int, SoundTemplate>();
    private SoundTemplate currentEntry;

    ScriptableObject scriptableObj;
    SerializedObject serialObj;

    private AudioSource currentAudioSource;

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

        InitSerializedObject();
    }

    protected void InitSerializedObject()
    {
        scriptableObj = currentEntry;
        serialObj = new SerializedObject(scriptableObj);
    }

    public override void InstantiateCurrentEntry(int index)
    {
        if (entries.Count == 0) return;
        currentEntry = Instantiate(entries[index]);
        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;

        InitSerializedObject();
    }

    public override void LoadEntries()
    {
        Dictionary<int, SoundTemplate> dictionary = new Dictionary<int, SoundTemplate>();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<SoundTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
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

        currentEntry = CreateInstance<SoundTemplate>();
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
        SoundTemplate entryFile = (SoundTemplate) updatedEntry;
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

        #if !UNITY_2021
        InitSerializedObject();
        #endif

        RPGBuilderEditorUtility.UpdateViewAndFieldData();

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
        currentEntry.entryFileName = currentEntry.entryName + AssetNameSuffix;

        var serialProp = serialObj.FindProperty("Sounds");
        EditorGUILayout.PropertyField(serialProp, true);

        currentEntry.MixerGroup = RPGBuilderEditorFields.DrawHorizontalAudioMixerField("Mixer", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.MixerGroup);

        RPGBuilderEditorFields.DrawTitleLabelExpanded("Play Type:", "", true);
        currentEntry.PlayOneShot = RPGBuilderEditorFields.DrawHorizontalToggle("Play One Shot?", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.PlayOneShot);
        
        RPGBuilderEditorFields.DrawTitleLabelExpanded("Audio Source Values:", "", true);
        currentEntry.BypassEffects = RPGBuilderEditorFields.DrawHorizontalToggle("Bypass Effects", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.BypassEffects);
        currentEntry.BypassListenerEffects = RPGBuilderEditorFields.DrawHorizontalToggle("Bypass Listeners", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.BypassListenerEffects);
        currentEntry.BypassReverbZones = RPGBuilderEditorFields.DrawHorizontalToggle("Bypass Reverb Zones", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.BypassReverbZones);

        currentEntry.Loop = RPGBuilderEditorFields.DrawHorizontalToggle("Looping", "",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.Loop);
        if (currentEntry.Loop)
        {
            currentEntry.LoopDuration = RPGBuilderEditorFields.DrawHorizontalFloatField("Loop Duration", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.LoopDuration);
        }

        currentEntry.Priority =
            RPGBuilderEditorFields.DrawHorizontalRangeSliderField("Priority", "", currentEntry.Priority, 0, 256, 50);
        currentEntry.Volume =
            RPGBuilderEditorFields.DrawHorizontalRangeSliderField("Volume", "", currentEntry.Volume, 0f, 1f, 50);
        currentEntry.Pitch =
            RPGBuilderEditorFields.DrawHorizontalRangeSliderField("Pitch", "", currentEntry.Pitch, -3f, 3f, 50);
        currentEntry.StereoPan =
            RPGBuilderEditorFields.DrawHorizontalRangeSliderField("Stereo Pan", "", currentEntry.StereoPan, -1f, 1f,
                50);
        currentEntry.SpatialBlend =
            RPGBuilderEditorFields.DrawHorizontalRangeSliderField("Spatial Blend", "", currentEntry.SpatialBlend, 0f,
                1f, 50);
        currentEntry.ReverbZoneMix =
            RPGBuilderEditorFields.DrawHorizontalRangeSliderField("Reverb Zone Mix", "", currentEntry.ReverbZoneMix, 0f,
                1.1f, 50);

        currentEntry.rolloffMode = (AudioRolloffMode) RPGBuilderEditorFields.DrawHorizontalEnum("Rolloff Mode", "",
            (int) currentEntry.rolloffMode,
            Enum.GetNames(typeof(AudioRolloffMode)));
        currentEntry.DopplerLevel =
            RPGBuilderEditorFields.DrawHorizontalRangeSliderField("Doppler Level", "", currentEntry.DopplerLevel, 0f,
                5f, 50);
        currentEntry.Spread =
            RPGBuilderEditorFields.DrawHorizontalRangeSliderIntField("Spread", "", currentEntry.Spread, 0, 360, 50);
        currentEntry.Distance =
            RPGBuilderEditorFields.DrawHorizontalRangeSliderField("Distance", "", currentEntry.Distance, 0f, 500f, 50);

        GUILayout.Space(25);
        if (RPGBuilderEditorFields.DrawHorizontalAddButton("PLAY", true))
        {
            if (currentAudioSource == null)
            {
                GameObject audioSourceGO = new GameObject();
                audioSourceGO.hideFlags = HideFlags.HideAndDontSave;
                currentAudioSource = audioSourceGO.AddComponent<AudioSource>();
            }

            if (currentAudioSource != null && currentEntry.Sounds.Count > 0)
            {
                int randomSound = Random.Range(0, currentEntry.Sounds.Count);
                if (currentEntry.Sounds[randomSound] != null)
                {
                    currentAudioSource.outputAudioMixerGroup = currentEntry.MixerGroup;
                    currentAudioSource.bypassEffects = currentEntry.BypassEffects;
                    currentAudioSource.bypassListenerEffects = currentEntry.BypassListenerEffects;
                    currentAudioSource.bypassReverbZones = currentEntry.BypassReverbZones;
                    currentAudioSource.priority = (int) Random.Range(currentEntry.Priority.x, currentEntry.Priority.y);
                    currentAudioSource.volume = Random.Range(currentEntry.Volume.x, currentEntry.Volume.y);
                    currentAudioSource.pitch = Random.Range(currentEntry.Pitch.x, currentEntry.Pitch.y);
                    currentAudioSource.panStereo = Random.Range(currentEntry.StereoPan.x, currentEntry.StereoPan.y);
                    currentAudioSource.reverbZoneMix =
                        Random.Range(currentEntry.ReverbZoneMix.x, currentEntry.ReverbZoneMix.y);
                    currentAudioSource.dopplerLevel =
                        Random.Range(currentEntry.DopplerLevel.x, currentEntry.DopplerLevel.y);
                    currentAudioSource.spread = Random.Range(currentEntry.Spread.x, currentEntry.Spread.y);

                    currentAudioSource.clip = currentEntry.Sounds[randomSound];
                    currentAudioSource.Play();
                }
            }
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();

    }

    public override void OnExitModule()
    {
        DestroyImmediate(currentAudioSource);
    }

    public override void ConvertDatabaseEntriesAfterUpdate()
    {

    }
}
