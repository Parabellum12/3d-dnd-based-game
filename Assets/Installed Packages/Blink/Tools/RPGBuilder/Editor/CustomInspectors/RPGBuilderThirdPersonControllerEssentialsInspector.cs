using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using UnityEditor;
using UnityEngine;

namespace BLINK.Controller
{
    [CustomEditor(typeof(RPGBThirdPersonCharacterControllerEssentials))]
    public class RPGBuilderThirdPersonControllerEssentialsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:",
                MonoScript.FromMonoBehaviour((RPGBCharacterControllerEssentials) target),
                typeof(RPGBCharacterControllerEssentials),
                false);
            GUI.enabled = true;
        }
    }
}
