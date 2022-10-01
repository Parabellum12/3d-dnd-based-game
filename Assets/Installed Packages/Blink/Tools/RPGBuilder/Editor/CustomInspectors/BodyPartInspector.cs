using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BodyPart))]
public class BodyPartInspector : Editor
{
    private RPGBuilderEditorSettings editorSettings;
    private BodyPart[] BodyPart;
    private readonly List<RPGBuilderDatabaseEntry> allBodyParts = new List<RPGBuilderDatabaseEntry>();

    private void OnEnable()
    {
        editorSettings = Resources.Load<RPGBuilderEditorSettings>("Database/Settings/Editor_Settings");

        foreach (var typeEntry in Resources.LoadAll<RPGBBodyPart>(editorSettings.DatabasePath + "Types/"))
        {
            allBodyParts.Add(typeEntry);
        }

        Object[] references = targets;
        BodyPart = new BodyPart[references.Length];
        for (int i = 0; i < references.Length; i++) {
            BodyPart[i] = references[i] as BodyPart;
        }
        
        foreach (var bodyPart in BodyPart)
        {
            if (bodyPart.bodyRenderer == null)
            {
                bodyPart.bodyRenderer = bodyPart.GetComponent<Renderer>();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        
        
        int bodyPartIndex = EditorGUILayout.Popup("Part", RPGBuilderEditorUtility.GetTypeEntryIndex(allBodyParts, BodyPart[0].bodyPart),
            RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allBodyParts.ToArray()));
        
        foreach (var bodyPart in BodyPart)
        {
            bodyPart.bodyPart = (RPGBBodyPart) allBodyParts[bodyPartIndex];
        }
        
        EditorGUILayout.BeginHorizontal();
        BodyPart[0].bodyRenderer = (Renderer) EditorGUILayout.ObjectField("Renderer", BodyPart[0].bodyRenderer, typeof(Renderer), true);
        foreach (var bodyPart in BodyPart)
        {
            if (bodyPart.bodyRenderer == null)
            {
                if (GUILayout.Button("Find Renderer"))
                {
                    bodyPart.bodyRenderer = bodyPart.GetComponent<Renderer>();
                }
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        foreach (var bodyPart in BodyPart)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(bodyPart);
        }
        
        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            foreach (var bodyPart in BodyPart)
            {
                EditorUtility.SetDirty(bodyPart);
            }
        }
    }
}
