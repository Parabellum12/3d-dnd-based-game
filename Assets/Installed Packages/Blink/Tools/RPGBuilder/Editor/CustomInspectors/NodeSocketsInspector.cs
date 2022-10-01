using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.LogicMono;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(NodeSockets))]
public class NodeSocketsInspector : Editor
{
    private NodeSockets nodeSocketsREF;
    private RPGBuilderEditorSettings editorSettings;
    
    private readonly List<RPGBuilderDatabaseEntry> allNodeSockets = new List<RPGBuilderDatabaseEntry>();
    
    private void OnEnable()
    {
        editorSettings = Resources.Load<RPGBuilderEditorSettings>("Database/Settings/Editor_Settings");
        nodeSocketsREF = (NodeSockets) target;
        
        foreach (var typeEntry in Resources.LoadAll<RPGBNodeSocket>(editorSettings.DatabasePath))
        {
            allNodeSockets.Add(typeEntry);
        }
    }
    
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((NodeSockets) target), typeof(NodeSockets),
            false);
        GUI.enabled = true;
        EditorGUI.BeginChangeCheck();
        var SubTitleStyle = new GUIStyle();
        SubTitleStyle.alignment = TextAnchor.UpperLeft;
        SubTitleStyle.fontSize = 17;
        SubTitleStyle.fontStyle = FontStyle.Bold;
        SubTitleStyle.normal.textColor = Color.white;

        GUILayout.Space(5);
        GUILayout.Label("Sockets", SubTitleStyle);
        GUILayout.Space(5);
        
        if (GUILayout.Button("+ Add Socket")) nodeSocketsREF.sockets.Add(new NodeSockets.NodeSocket());

        var ThisList = serializedObject.FindProperty("sockets");
        nodeSocketsREF.sockets = GetTargetObjectOfProperty(ThisList) as List<NodeSockets.NodeSocket>;

        for (var a = 0; a < nodeSocketsREF.sockets.Count; a++)
        {
            GUILayout.Space(10);
            var requirementNumber = a + 1;
            EditorGUILayout.BeginHorizontal();
            
            GUIStyle xButtonStyle = new GUIStyle();
            xButtonStyle.normal.textColor = Color.red;
            xButtonStyle.fontSize = 25;
            if (GUILayout.Button("X", xButtonStyle,GUILayout.Width(25), GUILayout.Height(25)))
            {
                nodeSocketsREF.sockets.RemoveAt(a);
                return;
            }
            
            EditorGUILayout.LabelField("" + requirementNumber + ":", GUILayout.Width(25));
            
            EditorGUILayout.BeginVertical();
            
            int projSocketIndex = EditorGUILayout.Popup("Socket", RPGBuilderEditorUtility.GetTypeEntryIndex(allNodeSockets, nodeSocketsREF.sockets[a].nodeSocket),
                RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allNodeSockets.ToArray()));
            nodeSocketsREF.sockets[a].nodeSocket = (RPGBNodeSocket) allNodeSockets[projSocketIndex];
            
            nodeSocketsREF.sockets[a].socketTransform = (Transform) EditorGUILayout.ObjectField("Transform", nodeSocketsREF.sockets[a].socketTransform, typeof(Transform), true);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
        
        PrefabUtility.RecordPrefabInstancePropertyModifications(nodeSocketsREF);
        serializedObject.ApplyModifiedProperties();
        Undo.RecordObject(nodeSocketsREF, "Modified Node Sockets");


        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(nodeSocketsREF);
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

        for (var i = 0; i <= index; i++)
            if (!enm.MoveNext()) return null;
        return enm.Current;
    }
}
