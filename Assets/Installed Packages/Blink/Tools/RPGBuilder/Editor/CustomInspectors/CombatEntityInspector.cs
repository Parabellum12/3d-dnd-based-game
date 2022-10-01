using BLINK.RPGBuilder.Combat;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(CombatEntity))]
public class CombatEntityInspector : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((CombatEntity) target),
            typeof(CombatEntity),
            false);
        GUI.enabled = true;
    }
}
