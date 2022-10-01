using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.World;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(ContainerObject))]
public class ContainerObjectInspector : Editor
{
    private ContainerObject Container;
    private RPGBuilderEditorSettings editorSettings;
    private GUISkin EditorSkin;

    private float marginValue = 30;
    private bool showRequirements, showActions, showVisualEffects, showAnimations, showSounds;
    private readonly List<RPGBuilderDatabaseEntry> allNodeSockets = new List<RPGBuilderDatabaseEntry>();

    private void OnEnable()
    {
        editorSettings = Resources.Load<RPGBuilderEditorSettings>("Database/Settings/Editor_Settings");
        Container = (ContainerObject) target;
        EditorSkin = Resources.Load<GUISkin>("EditorData/" + "RPGBuilderSkin");
        
        foreach (var typeEntry in Resources.LoadAll<RPGBNodeSocket>(editorSettings.DatabasePath + "Types/"))
        {
            allNodeSockets.Add(typeEntry);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((ContainerObject) target),
            typeof(ContainerObject), false);
        GUI.enabled = true;

        var SubTitleStyle = new GUIStyle
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = {textColor = Color.white}
        };
        var NumberStyle = new GUIStyle
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = {textColor = Color.white}
        };
        var SmallTitle = new GUIStyle
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            normal = {textColor = Color.white}
        };

        GUILayout.Space(5);
        GUILayout.Label("Persistence", SubTitleStyle);
        GUILayout.Space(5);
        Container.Saver =
            (ContainerObjectSaver) EditorGUILayout.ObjectField("Saver", Container.Saver, typeof(ContainerObjectSaver),
                true);

        GUILayout.Space(15);
        GUILayout.Label("Settings", SubTitleStyle);
        GUILayout.Space(5);
        Container.SlotAmount = EditorGUILayout.IntField("Slots", Container.SlotAmount);

        GUIStyle removeButtonStyle = EditorSkin.GetStyle("SquareRemoveButtonSmall");
        GUIStyle addButtonStyle = EditorSkin.GetStyle("SquareAddButtonSmall");
        GUIStyle genericButtonStyle = EditorSkin.GetStyle("GenericButtonSmall");

        GUILayout.Space(15);
        GUILayout.Label("Requirements:", SubTitleStyle, GUILayout.MaxWidth(150));
        GUILayout.Space(5);
        Container.RequirementsTemplate = (RequirementsTemplate) EditorGUILayout.ObjectField("Requirements",
            Container.RequirementsTemplate, typeof(RequirementsTemplate), false);

        GUILayout.Space(15);
        RPGBuilderEditorUtility.StartHorizontalMargin(0, false);
        GUILayout.Label("Visual Effects:", SubTitleStyle, GUILayout.MaxWidth(150));
        GUILayout.Space(40);
        if (GUILayout.Button(showVisualEffects ? "Hide" : "Show", genericButtonStyle, GUILayout.MaxWidth(60),
            GUILayout.MaxHeight(20)))
        {
            showVisualEffects = !showVisualEffects;
        }

        GUILayout.Space(5);
        if (GUILayout.Button("+", addButtonStyle, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            Container.VisualEfects.Add(new InteractableObjectData.InteractableObjectVisualEffect());
            if (showVisualEffects == false) showVisualEffects = true;
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(0, false);

        var visualEffectsList = serializedObject.FindProperty("VisualEfects");
        Container.VisualEfects =
            GetTargetObjectOfProperty(visualEffectsList) as List<InteractableObjectData.InteractableObjectVisualEffect>;

        if (showVisualEffects)
        {
            for (var a = 0; a < Container.VisualEfects.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(a + 1 + ":", NumberStyle, GUILayout.MaxWidth(15));
                Container.VisualEfects[a].TargetType =
                    (InteractableObjectData.InteractableObjectTemplateTarget) EditorGUILayout.EnumPopup("Spawn On:",
                        Container.VisualEfects[a].TargetType);

                if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Container.VisualEfects.RemoveAt(a);
                    serializedObject.Update();
                    return;
                }

                EditorGUILayout.EndHorizontal();

                RPGBuilderEditorUtility.StartHorizontalMargin(marginValue, true);
                EditorGUILayout.LabelField(Container.VisualEfects[a].VisualEntry.Template == null
                    ? "- Select Template -"
                    : Container.VisualEfects[a].VisualEntry.Template.entryName, SmallTitle);
                EditorGUILayout.BeginHorizontal();
                Container.VisualEfects[a].ActivationType =
                    (ActivationType) EditorGUILayout.EnumPopup("Activate On",
                        Container.VisualEfects[a].ActivationType);
                EditorGUILayout.EndHorizontal();

                Container.VisualEfects[a].VisualEntry.Template =
                    (VisualEffectTemplate) EditorGUILayout.ObjectField("Template",
                        Container.VisualEfects[a].VisualEntry.Template, typeof(VisualEffectTemplate), false);

                switch (Container.VisualEfects[a].TargetType)
                {
                    case InteractableObjectData.InteractableObjectTemplateTarget.Object:
                        EditorGUILayout.LabelField("Spawn Position:", SmallTitle);
                        Container.VisualEfects[a].VisualEntry.UseNodeSocket = false;

                        Container.VisualEfects[a].VisualEntry.PositionOffset = EditorGUILayout.Vector3Field(
                            "Position Offset",
                            Container.VisualEfects[a].VisualEntry.PositionOffset);
                        Container.VisualEfects[a].VisualEntry.ParentedToCaster = EditorGUILayout.Toggle(
                            "Attach to Object?",
                            Container.VisualEfects[a].VisualEntry.ParentedToCaster);
                        break;
                    case InteractableObjectData.InteractableObjectTemplateTarget.User:
                        EditorGUILayout.LabelField("Spawn Position:", SmallTitle);

                        Container.VisualEfects[a].VisualEntry.UseNodeSocket = EditorGUILayout.Toggle(
                            "Use Node Socket?",
                            Container.VisualEfects[a].VisualEntry.UseNodeSocket);
                        if (Container.VisualEfects[a].VisualEntry.UseNodeSocket)
                        {
                            int projSocketIndex = EditorGUILayout.Popup("Socket",
                                RPGBuilderEditorUtility.GetTypeEntryIndex(allNodeSockets,
                                    Container.VisualEfects[a].VisualEntry.NodeSocket),
                                RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allNodeSockets.ToArray()));
                            Container.VisualEfects[a].VisualEntry.NodeSocket =
                                (RPGBNodeSocket) allNodeSockets[projSocketIndex];
                            Container.VisualEfects[a].VisualEntry.PositionOffset = EditorGUILayout.Vector3Field(
                                "Position Offset",
                                Container.VisualEfects[a].VisualEntry.PositionOffset);
                            Container.VisualEfects[a].VisualEntry.ParentedToCaster = EditorGUILayout.Toggle(
                                "Attach to Socket?",
                                Container.VisualEfects[a].VisualEntry.ParentedToCaster);
                        }
                        else
                        {
                            Container.VisualEfects[a].VisualEntry.PositionOffset = EditorGUILayout.Vector3Field(
                                "Position Offset",
                                Container.VisualEfects[a].VisualEntry.PositionOffset);
                            Container.VisualEfects[a].VisualEntry.ParentedToCaster = EditorGUILayout.Toggle(
                                "Attach to Object?",
                                Container.VisualEfects[a].VisualEntry.ParentedToCaster);
                        }

                        break;
                }

                Container.VisualEfects[a].VisualEntry.Scale =
                    EditorGUILayout.Vector3Field("Scale", Container.VisualEfects[a].VisualEntry.Scale);

                EditorGUILayout.LabelField("Settings:", SmallTitle);
                Container.VisualEfects[a].VisualEntry.Duration =
                    EditorGUILayout.FloatField("Duration", Container.VisualEfects[a].VisualEntry.Duration);
                Container.VisualEfects[a].VisualEntry.Delay =
                    EditorGUILayout.FloatField("Delay", Container.VisualEfects[a].VisualEntry.Delay);
                RPGBuilderEditorUtility.EndHorizontalMargin(0, true);
                GUILayout.Space(10);
            }
        }

        GUILayout.Space(15);
        RPGBuilderEditorUtility.StartHorizontalMargin(0, false);
        GUILayout.Label("Animations:", SubTitleStyle, GUILayout.MaxWidth(150));
        GUILayout.Space(40);
        if (GUILayout.Button(showAnimations ? "Hide" : "Show", genericButtonStyle, GUILayout.MaxWidth(60),
            GUILayout.MaxHeight(20)))
        {
            showAnimations = !showAnimations;
        }

        GUILayout.Space(5);
        if (GUILayout.Button("+", addButtonStyle, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            Container.Animations.Add(new InteractableObjectData.InteractableObjectAnimation());
            if (showAnimations == false) showAnimations = true;
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(0, false);

        var animationsList = serializedObject.FindProperty("Animations");
        Container.Animations =
            GetTargetObjectOfProperty(animationsList) as List<InteractableObjectData.InteractableObjectAnimation>;

        if (showAnimations)
        {
            Container.anim = (Animator) EditorGUILayout.ObjectField("Animator", Container.anim, typeof(Animator), true);
            for (var a = 0; a < Container.Animations.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(a + 1 + ":", NumberStyle, GUILayout.MaxWidth(15));
                Container.Animations[a].TargetType =
                    (InteractableObjectData.InteractableObjectTemplateTarget) EditorGUILayout.EnumPopup("Play On:",
                        Container.Animations[a].TargetType);

                if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Container.Animations.RemoveAt(a);
                    serializedObject.Update();
                    return;
                }

                EditorGUILayout.EndHorizontal();
                RPGBuilderEditorUtility.StartHorizontalMargin(marginValue, true);

                EditorGUILayout.LabelField(Container.Animations[a].AnimationEntry.Template == null
                    ? "- Select Template -"
                    : Container.Animations[a].AnimationEntry.Template.entryName, SmallTitle);
                Container.Animations[a].ActivationType =
                    (ActivationType) EditorGUILayout.EnumPopup("Activate On",
                        Container.Animations[a].ActivationType);

                Container.Animations[a].AnimationEntry.Template =
                    (AnimationTemplate) EditorGUILayout.ObjectField("Template",
                        Container.Animations[a].AnimationEntry.Template, typeof(AnimationTemplate), false);
                EditorGUILayout.LabelField("Settings:", SmallTitle);
                Container.Animations[a].AnimationEntry.Delay =
                    EditorGUILayout.FloatField("Delay", Container.Animations[a].AnimationEntry.Delay);
                RPGBuilderEditorUtility.EndHorizontalMargin(0, true);
                GUILayout.Space(10);
            }
        }

        GUILayout.Space(15);
        RPGBuilderEditorUtility.StartHorizontalMargin(0, false);
        GUILayout.Label("Sounds:", SubTitleStyle, GUILayout.MaxWidth(150));
        GUILayout.Space(40);
        if (GUILayout.Button(showSounds ? "Hide" : "Show", genericButtonStyle, GUILayout.MaxWidth(60),
            GUILayout.MaxHeight(20)))
        {
            showSounds = !showSounds;
        }

        GUILayout.Space(5);
        if (GUILayout.Button("+", addButtonStyle, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            Container.Sounds.Add(new InteractableObjectData.InteractableObjectSound());
            if (showSounds == false) showSounds = true;
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(0, false);

        var soundsList = serializedObject.FindProperty("Sounds");
        Container.Sounds =
            GetTargetObjectOfProperty(soundsList) as List<InteractableObjectData.InteractableObjectSound>;

        if (showSounds)
        {
            for (var a = 0; a < Container.Sounds.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(a + 1 + ":", NumberStyle, GUILayout.MaxWidth(15));
                Container.Sounds[a].TargetType =
                    (InteractableObjectData.InteractableObjectTemplateTarget) EditorGUILayout.EnumPopup("Play On:",
                        Container.Sounds[a].TargetType);

                if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Container.Sounds.RemoveAt(a);
                    serializedObject.Update();
                    return;
                }

                EditorGUILayout.EndHorizontal();
                RPGBuilderEditorUtility.StartHorizontalMargin(marginValue, true);

                EditorGUILayout.LabelField(Container.Sounds[a].SoundEntry.Template == null
                    ? "- Select Template -"
                    : Container.Sounds[a].SoundEntry.Template.entryName, SmallTitle);
                Container.Sounds[a].ActivationType =
                    (ActivationType) EditorGUILayout.EnumPopup("Activate On",
                        Container.Sounds[a].ActivationType);

                Container.Sounds[a].SoundEntry.Template =
                    (SoundTemplate) EditorGUILayout.ObjectField("Template",
                        Container.Sounds[a].SoundEntry.Template, typeof(SoundTemplate), false);
                EditorGUILayout.LabelField("Settings:", SmallTitle);
                Container.Sounds[a].SoundEntry.Delay =
                    EditorGUILayout.FloatField("Delay", Container.Sounds[a].SoundEntry.Delay);
                RPGBuilderEditorUtility.EndHorizontalMargin(0, true);
                GUILayout.Space(10);
            }
        }

        GUILayout.Space(15);
        GUILayout.Label("State", SubTitleStyle);
        GUILayout.Space(5);

        Container.MaxDistance = EditorGUILayout.FloatField("Use Distance Max", Container.MaxDistance);

        GUILayout.Space(15);
        GUILayout.Label("Interactable UI", SubTitleStyle);
        GUILayout.Space(5);
        Container.InteractableName = EditorGUILayout.TextField("Interactable Name", Container.InteractableName);
        Container.UIOffsetY = EditorGUILayout.FloatField("Y Offset", Container.UIOffsetY);

        PrefabUtility.RecordPrefabInstancePropertyModifications(Container);
        serializedObject.ApplyModifiedProperties();
        Undo.RecordObject(Container, "Modified Interactive Node");

        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(Container);
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

            var p = type.GetProperty(name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
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
            if (!enm.MoveNext())
                return null;
        return enm.Current;
    }
}
