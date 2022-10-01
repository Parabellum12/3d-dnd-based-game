using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(PatrolPath))]
public class PatrolPathInspector : Editor
{
    private PatrolPath patrolPath;
    private RPGBuilderEditorSettings editorSettings;
    private GUISkin EditorSkin;
    
    private void OnEnable()
    {
        editorSettings = Resources.Load<RPGBuilderEditorSettings>("Database/Settings/Editor_Settings");
        patrolPath = (PatrolPath) target;
        EditorSkin = Resources.Load<GUISkin>("EditorData/" + "RPGBuilderSkin");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((PatrolPath) target),
            typeof(PatrolPath), false);
        GUI.enabled = true;

        var SubTitleStyle = new GUIStyle
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = {textColor = Color.white}
        };

        GUILayout.Space(5);
        GUILayout.Label("Settings", SubTitleStyle);
        GUILayout.Space(5);
        patrolPath.Looping = EditorGUILayout.Toggle("Is Looping?", patrolPath.Looping);
        
        GUILayout.Space(5);
        GUILayout.Label("Points", SubTitleStyle);
        GUILayout.Space(5);

        patrolPath.SelectPointAfterAdd = EditorGUILayout.Toggle("Select new points?", patrolPath.SelectPointAfterAdd);

        if (HasMissingPoints().Count > 0 && GUILayout.Button("Remove Missing Points"))
        {
            foreach (var missingPoint in HasMissingPoints())
            {
                patrolPath.Points.Remove(missingPoint);
            }

            for (int i = 0; i < patrolPath.Points.Count; i++)
            {
                patrolPath.Points[i].gameObject.name = "Point" + (i+1) + "_" + patrolPath.name;
            }
        }

        if (GUILayout.Button("Add Point"))
        {
            GameObject newPoint = new GameObject();
            patrolPath.Points.Add(newPoint.transform);
            newPoint.name = "Point" + patrolPath.Points.Count + "_" + patrolPath.name;
            if (patrolPath.SelectPointAfterAdd)
            {
                Selection.objects = new Object[] {newPoint};
            }
            
            var totalX = 0f;
            var totalZ = 0f;
            foreach(var point in patrolPath.Points)
            {
                totalX += point.transform.position.x;
                totalZ += point.transform.position.z;
            }
            var centerX = totalX / patrolPath.Points.Count;
            var centerZ = totalZ / patrolPath.Points.Count;
            newPoint.transform.position = new Vector3(centerX, patrolPath.transform.position.y, centerZ);
            newPoint.transform.SetParent(patrolPath.transform);
        }
        
        var points = serializedObject.FindProperty("Points");
        EditorGUILayout.PropertyField(points, true);
        
        GUILayout.Space(5);
        GUILayout.Label("Gizmos", SubTitleStyle);
        GUILayout.Space(5);
        patrolPath.FirstPointColor = EditorGUILayout.ColorField("First Point Color", patrolPath.FirstPointColor);
        patrolPath.PointColor = EditorGUILayout.ColorField("Points Color", patrolPath.PointColor);
        patrolPath.LastPointColor = EditorGUILayout.ColorField("Last Point Color", patrolPath.LastPointColor);
        patrolPath.LineColor = EditorGUILayout.ColorField("Lines Color", patrolPath.LineColor);
        
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            
        }
    }

    private List<Transform> HasMissingPoints()
    {
        List<Transform> missingPoints = new List<Transform>();
        foreach (var point in patrolPath.Points)
        {
            if (point == null)
            {
                missingPoints.Add(point);
            }
        }

        return missingPoints;
    }
}
