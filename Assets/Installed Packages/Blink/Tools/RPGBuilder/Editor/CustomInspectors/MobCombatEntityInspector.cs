using BLINK.RPGBuilder.Combat;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MobCombatEntity))]
public class MobCombatEntityInspector : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((MobCombatEntity) target),
            typeof(MobCombatEntity),
            false);
        GUI.enabled = true;
    }
}