using BLINK.RPGBuilder.Combat;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlayerCombatEntity))]
public class PlayerCombatEntityInspector : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((PlayerCombatEntity) target),
            typeof(PlayerCombatEntity),
            false);
        GUI.enabled = true;
    }
}
