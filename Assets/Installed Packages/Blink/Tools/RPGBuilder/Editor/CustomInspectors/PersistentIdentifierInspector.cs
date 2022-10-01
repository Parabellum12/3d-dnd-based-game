using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [CustomEditor(typeof(SaverIdentifier))]
    public class PersistentIdentifierInspector : Editor
    {
        private SaverIdentifier reference;

        private void OnEnable()
        {
            reference = (SaverIdentifier) target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            reference.Dynamic = EditorGUILayout.Toggle("Dynamic", reference.Dynamic);
            if (reference.Dynamic)
            {
                if (reference.GetIdentifier() != "-1")
                {
                    if (GUILayout.Button("Reset Unique Identifier", GUILayout.ExpandWidth(true)))
                    {
                        reference.ResetIdentifier();
                    }
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField("Identifier: " + reference.GetIdentifier());
                EditorGUI.EndDisabledGroup();

                if (reference.GetIdentifier() == "-1")
                {
                    if (GUILayout.Button("Generate Unique Identifier", GUILayout.ExpandWidth(true)))
                    {
                        reference.GenerateUniqueIdentifier();
                    }
                }
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(reference);
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(reference);
        }
    }
}
