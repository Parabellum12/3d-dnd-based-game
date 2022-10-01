using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

//[CanEditMultipleObjects]
[CustomEditor(typeof(NPCSpawner))]
public class NPCSpawnerInspector : Editor
{
    private GUISkin EditorSkin;
    private NPCSpawner Spawner;
    
    private void OnEnable()
    {
        Spawner = (NPCSpawner) target;
        EditorSkin = Resources.Load<GUISkin>("EditorData/" + "RPGBuilderSkin");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var SubTitleStyle = new GUIStyle
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = {textColor = Color.white}
        };
        
        GUIStyle removeButtonStyle = EditorSkin.GetStyle("SquareRemoveButtonSmallInspector");
        GUIStyle addButtonStyle = EditorSkin.GetStyle("SquareAddButtonSmallInspector");
        
        GUILayout.Space(5);
        GUILayout.Label("Spawner Type", SubTitleStyle);
        GUILayout.Space(5);
        Spawner.spawnerType = (AIData.SpawnerType) EditorGUILayout.EnumPopup("Type", Spawner.spawnerType);

        if (Spawner.spawnerType == AIData.SpawnerType.Manual)
        {
            if (Application.isPlaying)
                if (GUILayout.Button("Spawn"))
                    Spawner.ManualSpawnNPC();
        }

        if (Spawner.spawnerType == AIData.SpawnerType.Limited)
        {
            Spawner.spawnedCountMax = EditorGUILayout.IntField("Max. Spawn", Spawner.spawnedCountMax);
        }
        
        
        GUILayout.Space(5);
        GUILayout.Label("Persistence", SubTitleStyle);
        GUILayout.Space(5);
        Spawner.Saver = (NPCSpawnerSaver) EditorGUILayout.ObjectField("Saver", Spawner.Saver, typeof(NPCSpawnerSaver), true);
        
        GUILayout.Space(5);
        GUILayout.Label("Spawn Settings", SubTitleStyle);
        GUILayout.Space(5);
        Spawner.usePosition = EditorGUILayout.Toggle("Use Position?", Spawner.usePosition);
        Spawner.areaHeight = EditorGUILayout.FloatField("Area Height", Spawner.areaHeight);
        if (!Spawner.usePosition)
        {
            Spawner.areaRadius = EditorGUILayout.FloatField("Area Radius", Spawner.areaRadius);
            LayerMask tempMask = EditorGUILayout.MaskField("Ground Layers",
                InternalEditorUtility.LayerMaskToConcatenatedLayersMask(Spawner.groundLayers), InternalEditorUtility.layers);
            Spawner.groundLayers = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
        }
        Spawner.npcCountMax = EditorGUILayout.IntField("Max NPCs", Spawner.npcCountMax);
        Spawner.RequirementsTemplate = (RequirementsTemplate) EditorGUILayout.ObjectField("Requirements", Spawner.RequirementsTemplate,
            typeof(RequirementsTemplate), false);
        Spawner.PlayerDistanceMax = EditorGUILayout.FloatField("Max Distance from Player", Spawner.PlayerDistanceMax);
        Spawner.gizmoColor = EditorGUILayout.ColorField("Spawn Circle Color", Spawner.gizmoColor);
        if (!Spawner.usePosition)
        {
            Spawner.lineColor = EditorGUILayout.ColorField("Spawn Line Color", Spawner.lineColor);
        }

        GUILayout.Space(5);
        GUILayout.Label("Overrides", SubTitleStyle);
        GUILayout.Space(5);
        Spawner.OverrideLevels = EditorGUILayout.Toggle("Levels?", Spawner.OverrideLevels);
        if (Spawner.OverrideLevels)
        {
            Spawner.ScaleWithPlayer = EditorGUILayout.Toggle("Scale with player?", Spawner.ScaleWithPlayer);
            if (!Spawner.ScaleWithPlayer)
            {
                Spawner.MinLevel = EditorGUILayout.IntField("Min. Level", Spawner.MinLevel);
                Spawner.MaxLevel = EditorGUILayout.IntField("Max. Level", Spawner.MaxLevel);
            }

            GUILayout.Space(5);
        }
        
        Spawner.OverrideRespawn = EditorGUILayout.Toggle("Respawn?", Spawner.OverrideRespawn);
        if (Spawner.OverrideRespawn)
        {
            Spawner.MinRespawn = EditorGUILayout.FloatField("Min. Respawn", Spawner.MinRespawn);
            Spawner.MaxRespawn = EditorGUILayout.FloatField("Max. Respawn", Spawner.MaxRespawn);
            GUILayout.Space(5);
        }
        
        Spawner.OverrideFaction = EditorGUILayout.Toggle("Faction?", Spawner.OverrideFaction);
        if (Spawner.OverrideFaction)
        {
            Spawner.Faction = (RPGFaction) EditorGUILayout.ObjectField("Faction", Spawner.Faction, typeof(RPGFaction), false);
            GUILayout.Space(5);
        }
        
        Spawner.OverrideSpecies = EditorGUILayout.Toggle("Species?", Spawner.OverrideSpecies);
        if (Spawner.OverrideSpecies)
        {
            Spawner.Species = (RPGSpecies) EditorGUILayout.ObjectField("Species", Spawner.Species, typeof(RPGSpecies), false);
            GUILayout.Space(5);
        }

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("NPC List", SubTitleStyle, GUILayout.MaxWidth(100));
        GUILayout.Space(5);
        if (GUILayout.Button("+", addButtonStyle, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            Spawner.spawnData.Add(new NPCSpawner.NPC_SPAWN_DATA());
        }
        EditorGUILayout.EndHorizontal();

        var ThisList = serializedObject.FindProperty("spawnData");
        Spawner.spawnData = GetTargetObjectOfProperty(ThisList) as List<NPCSpawner.NPC_SPAWN_DATA>;

        for (var a = 0; a < Spawner.spawnData.Count; a++)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            Spawner.spawnData[a].npc = (RPGNpc) EditorGUILayout.ObjectField("NPC", Spawner.spawnData[a].npc, typeof(RPGNpc), false);
            if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                Spawner.spawnData.RemoveAt(a);
                return;
            }
            EditorGUILayout.EndHorizontal();
            
            Spawner.spawnData[a].spawnChance = EditorGUILayout.Slider("Chance to spawn", Spawner.spawnData[a].spawnChance, 0f, 100f);
            Spawner.spawnData[a].IsPersistent = EditorGUILayout.Toggle("Is Persistent?", Spawner.spawnData[a].IsPersistent);
            
            GUILayout.Space(10);
        }
        
        float chanceLeft = 100;
        for (var index = 0; index < Spawner.spawnData.Count; index++)
        {
            var npc = Spawner.spawnData[index];
            if (index == 0)
            {
                chanceLeft -= npc.spawnChance;
                continue;
            }

            if (npc.spawnChance > chanceLeft) npc.spawnChance = chanceLeft;
            chanceLeft -= npc.spawnChance;
            if (index + 1 == Spawner.spawnData.Count && chanceLeft > 0)
            {
                npc.spawnChance += chanceLeft;
            }
        }
        
        PrefabUtility.RecordPrefabInstancePropertyModifications(Spawner);
        serializedObject.ApplyModifiedProperties();
        Undo.RecordObject(Spawner, "Modified NPC Spawner");
    }

    private object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }

        return obj;
    }
    private object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }

    private object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (var i = 0; i <= index; i++)
            if (!enm.MoveNext()) return null;
        return enm.Current;
    }
    
    
}