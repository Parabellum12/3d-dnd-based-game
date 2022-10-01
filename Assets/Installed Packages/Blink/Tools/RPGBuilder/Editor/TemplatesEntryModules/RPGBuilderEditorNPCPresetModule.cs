using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class RPGBuilderEditorNPCPresetModule : RPGBuilderEditorModule
{
    private Dictionary<int, NPCPresetTemplate> entries = new Dictionary<int, NPCPresetTemplate>();
    private NPCPresetTemplate currentEntry;

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
        Dictionary<int, NPCPresetTemplate> dictionary = new Dictionary<int, NPCPresetTemplate>();
        databaseEntries.Clear();
        var allEntries =
            Resources.LoadAll<NPCPresetTemplate>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
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

        currentEntry = CreateInstance<NPCPresetTemplate>();
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
        NPCPresetTemplate entryFile = (NPCPresetTemplate) updatedEntry;
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
        EditorGUILayout.BeginHorizontal();
        currentEntry.entryIcon =
            RPGBuilderEditorFields.DrawIcon(currentEntry.entryIcon, 45, 45);
        currentEntry.entryName = RPGBuilderEditorFields.DrawHorizontalTextField("",
            "", RPGBuilderEditor.Instance.FieldHeight + 15, currentEntry.entryName);
        EditorGUILayout.EndHorizontal();
            
        currentEntry.entryFileName = currentEntry.entryName + AssetNameSuffix;
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Visual:", "", true);
            currentEntry.Prefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Prefab", "",
                currentEntry.Prefab);
            
            currentEntry.Position = RPGBuilderEditorFields.DrawHorizontalVector3("Position", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.Position);
            currentEntry.Scale = RPGBuilderEditorFields.DrawHorizontalVector3("Scale", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.Scale);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("UI:", "", true);
            currentEntry.NameplateYOffset = RPGBuilderEditorFields.DrawHorizontalFloatField("Nameplate Y offset", "How far on the Y axis should the nameplate be from the NPC origin",
            RPGBuilderEditor.Instance.FieldHeight, currentEntry.NameplateYOffset);
            currentEntry.NameplateDistance = RPGBuilderEditorFields.DrawHorizontalFloatField("Nameplate Distance", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.NameplateDistance);
            currentEntry.RendererName = RPGBuilderEditorFields.DrawHorizontalTextField("Renderer name", "The name of the renderer used to check the NPC's visibility",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.RendererName);
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Animations:", "", true);
            currentEntry.AnimatorController = (RuntimeAnimatorController) 
                RPGBuilderEditorFields.DrawHorizontalObject<RuntimeAnimatorController>("Animator Controller", "", currentEntry.AnimatorController);
            
            currentEntry.AnimatorAvatar = (Avatar) 
                RPGBuilderEditorFields.DrawHorizontalObject<Avatar>("Avatar", "", currentEntry.AnimatorAvatar);
            
            currentEntry.AnimatorUseRootMotion = RPGBuilderEditorFields.DrawHorizontalToggle("Animator Root Motion", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.AnimatorUseRootMotion);
            
            currentEntry.AnimatorUpdateMode =
                (AnimatorUpdateMode) RPGBuilderEditorFields.DrawHorizontalEnum("Animator Update Mode", "",
                    (int)currentEntry.AnimatorUpdateMode,
                    Enum.GetNames(typeof(AnimatorUpdateMode)));
            
            currentEntry.AnimatorCullingMode =
                (AnimatorCullingMode) RPGBuilderEditorFields.DrawHorizontalEnum("Animator Culling Mode", "",
                    (int)currentEntry.AnimatorCullingMode,
                    Enum.GetNames(typeof(AnimatorCullingMode)));
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Navmesh Settings:", "", true);
            currentEntry.NavmeshAgentRadius = RPGBuilderEditorFields.DrawHorizontalFloatField("NavMesh Agent Radius", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.NavmeshAgentRadius);
            currentEntry.NavmeshAgentHeight = RPGBuilderEditorFields.DrawHorizontalFloatField("NavMesh Agent Height", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.NavmeshAgentHeight);
            currentEntry.NavmeshAgentAngularSpeed = RPGBuilderEditorFields.DrawHorizontalFloatField("NavMesh Agent Rotation Speed", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.NavmeshAgentAngularSpeed);
            
            currentEntry.NavmeshObstacleAvoidance =
                (ObstacleAvoidanceType) RPGBuilderEditorFields.DrawHorizontalEnum("Obstacle Avoidance", "",
                    (int)currentEntry.NavmeshObstacleAvoidance,
                    Enum.GetNames(typeof(ObstacleAvoidanceType)));
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Collider Settings:", "", true);
            
            currentEntry.ColliderType =
                (AIData.NPCColliderType) RPGBuilderEditorFields.DrawHorizontalEnum("Collider Type", "",
                    (int)currentEntry.ColliderType,
                    Enum.GetNames(typeof(RPGNpc.NPCColliderType)));
            
            switch (currentEntry.ColliderType)
            {
                case AIData.NPCColliderType.Capsule:
                    currentEntry.ColliderRadius = RPGBuilderEditorFields.DrawHorizontalFloatField("Collider Radius", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.ColliderRadius);
                    currentEntry.ColliderHeight = RPGBuilderEditorFields.DrawHorizontalFloatField("Collider Height", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.ColliderHeight);
                    currentEntry.ColliderCenter = RPGBuilderEditorFields.DrawHorizontalVector3("Collider Center", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.ColliderCenter);
                    break;
                case AIData.NPCColliderType.Sphere:
                    currentEntry.ColliderRadius = RPGBuilderEditorFields.DrawHorizontalFloatField("Collider Radius", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.ColliderRadius);
                    currentEntry.ColliderCenter = RPGBuilderEditorFields.DrawHorizontalVector3("Collider Center", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.ColliderCenter);
                    break;
                case AIData.NPCColliderType.Box:
                    currentEntry.ColliderCenter = RPGBuilderEditorFields.DrawHorizontalVector3("Collider Center", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.ColliderCenter);
                    currentEntry.ColliderSize = RPGBuilderEditorFields.DrawHorizontalVector3("Collider Size", "",
                        RPGBuilderEditor.Instance.FieldHeight, currentEntry.ColliderSize);
                    break;
            }
            
            RPGBuilderEditorFields.DrawTitleLabelExpanded("Interaction Settings:", "", true);
            currentEntry.InteractionDistanceMax = RPGBuilderEditorFields.DrawHorizontalFloatField("Distance Max", "",
                RPGBuilderEditor.Instance.FieldHeight, currentEntry.InteractionDistanceMax);
        
        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

        serialObj.ApplyModifiedProperties();

        GUILayout.Space(25);
        GUILayout.EndScrollView();
    }
}
