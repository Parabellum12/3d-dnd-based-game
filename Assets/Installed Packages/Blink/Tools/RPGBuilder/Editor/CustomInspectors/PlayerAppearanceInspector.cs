using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BLINK.RPGBuilder.Characters;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlayerAppearance))]
public class PlayerAppearanceInspector : Editor
{
    
    private PlayerAppearance REF;
    private RPGItemDATA itemSettings;
    private RPGGeneralDATA generalSettings;
    private RPGBuilderEditorDATA editorDATA;
    private GUISkin EditorSkin;
    private readonly List<RPGBuilderDatabaseEntry> allWeaponTypes = new List<RPGBuilderDatabaseEntry>();

    private GUIStyle removeButtonStyle;
    private GUIStyle genericButton;
    
    private void OnEnable()
    {
        editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        itemSettings = Resources.Load<RPGItemDATA>(editorDATA.RPGBDatabasePath +"Settings/ItemSettings");
        generalSettings = Resources.Load<RPGGeneralDATA>(editorDATA.RPGBDatabasePath +"Settings/GeneralSettings");
        EditorSkin = Resources.Load<GUISkin>("EditorData/" + "RPGBuilderSkin");
        REF = (PlayerAppearance) target;
        
        foreach (var typeEntry in Resources.LoadAll<RPGBWeaponType>(editorDATA.RPGBDatabasePath + "Types/"))
        {
            allWeaponTypes.Add(typeEntry);
        }
        removeButtonStyle = EditorSkin.GetStyle("SquareRemoveButtonSmallInspector");
        genericButton = EditorSkin.GetStyle("GenericButtonSmallInspector");
    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((PlayerAppearance) target),
            typeof(PlayerAppearance),
            false);
        GUI.enabled = true;
        EditorGUI.BeginChangeCheck();
        var SubTitleStyle = new GUIStyle();
        SubTitleStyle.alignment = TextAnchor.UpperLeft;
        SubTitleStyle.fontSize = 17;
        SubTitleStyle.fontStyle = FontStyle.Bold;
        SubTitleStyle.normal.textColor = Color.white;

        GUILayout.Space(5);
        GUILayout.Label("Body", SubTitleStyle);
        REF.cachedBodyParent =
            (GameObject) EditorGUILayout.ObjectField("Body Parent", REF.cachedBodyParent, typeof(GameObject),
                true);
        var tps5 = serializedObject.FindProperty("BodyParts");
        EditorGUILayout.PropertyField(tps5, true);

        GUILayout.Space(5);
        if (GUILayout.Button("Find Body Parts"))
        {
            foreach (var bodyPart in REF.gameObject.GetComponentsInChildren<BodyPart>())
            {
                if (!REF.BodyParts.Contains(bodyPart))
                {
                    REF.BodyParts.Add(bodyPart);
                }
            }
        }
        
        
        GUILayout.Space(10);
        GUILayout.Label("Armor", SubTitleStyle);

        var tps2 = serializedObject.FindProperty("armorPieces");
        EditorGUILayout.PropertyField(tps2, true);
        
        REF.cachedArmorsParent =
            (GameObject) EditorGUILayout.ObjectField("Armors Parent", REF.cachedArmorsParent, typeof(GameObject),
                true);
        
        GUILayout.Space(10);
        GUILayout.Label("Armature", SubTitleStyle);

        var tps3 = serializedObject.FindProperty("armatureReferences");
        EditorGUILayout.PropertyField(tps3, true);
        REF.armatureParentGO =
            (GameObject) EditorGUILayout.ObjectField("Armature Parent", REF.armatureParentGO, typeof(GameObject),
                true);
        REF.armatureParentOffset =
            EditorGUILayout.Vector3Field("Armature Offset", REF.armatureParentOffset);

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Weapon Slots", SubTitleStyle, GUILayout.MaxWidth(125));
        if (GUILayout.Button("+", GUILayout.MaxHeight(20), GUILayout.MaxWidth(20))) REF.WeaponSlots.Add(new PlayerAppearance.WeaponSlot());
        EditorGUILayout.EndHorizontal();

        

        var ThisList5 = serializedObject.FindProperty("WeaponSlots");
        REF.WeaponSlots = GetTargetObjectOfProperty(ThisList5) as List<PlayerAppearance.WeaponSlot>;

        for (var a = 0; a < REF.WeaponSlots.Count; a++)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            int weaponTypeIndex = EditorGUILayout.Popup(
                RPGBuilderEditorUtility.GetTypeEntryIndex(allWeaponTypes,
                    REF.WeaponSlots[a].WeaponType),
                RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allWeaponTypes.ToArray()));
            REF.WeaponSlots[a].WeaponType = (RPGBWeaponType) allWeaponTypes[weaponTypeIndex];
            if (GUILayout.Button(REF.WeaponSlots[a].collapsed ? "+" : "-", genericButton, GUILayout.Width(25), GUILayout.Height(25)))
            {
                REF.WeaponSlots[a].collapsed = !REF.WeaponSlots[a].collapsed;
                return;
            }
            GUILayout.Space(5);
            if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(25), GUILayout.Height(25)))
            {
                REF.WeaponSlots.RemoveAt(a);
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (!REF.WeaponSlots[a].collapsed)
            {
                REF.WeaponSlots[a].RightHandCombat = (Transform)
                    EditorGUILayout.ObjectField("Right Hand Combat", REF.WeaponSlots[a].RightHandCombat,
                        typeof(Transform), true);
                REF.WeaponSlots[a].RightHandRest = (Transform)
                    EditorGUILayout.ObjectField("Right Hand Rest", REF.WeaponSlots[a].RightHandRest, typeof(Transform),
                        true);
                REF.WeaponSlots[a].LeftHandCombat = (Transform)
                    EditorGUILayout.ObjectField("Left Hand Combat", REF.WeaponSlots[a].LeftHandCombat,
                        typeof(Transform), true);
                REF.WeaponSlots[a].LeftHandRest = (Transform)
                    EditorGUILayout.ObjectField("Left Hand Rest", REF.WeaponSlots[a].LeftHandRest, typeof(Transform),
                        true);
                GUILayout.Space(10);
            }
        }
        
        PrefabUtility.RecordPrefabInstancePropertyModifications(REF);
        serializedObject.ApplyModifiedProperties();
        Undo.RecordObject(REF, "Modified Player Appearance Handler" + REF.gameObject.name);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(REF);
        }
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
