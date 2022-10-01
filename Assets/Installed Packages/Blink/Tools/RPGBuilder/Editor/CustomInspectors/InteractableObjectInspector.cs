using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.World;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(InteractableObject))]
public class InteractableObjectInspector : Editor
{
    private InteractableObject Interactable;
    private RPGBuilderEditorSettings editorSettings;
    private GUISkin EditorSkin;

    private float marginValue = 30;
    private bool showRequirements, showActions, showVisualEffects, showAnimations, showSounds;
    
    private readonly List<RPGBuilderDatabaseEntry> allNodeSockets = new List<RPGBuilderDatabaseEntry>();

    private void OnEnable()
    {
        editorSettings = Resources.Load<RPGBuilderEditorSettings>("Database/Settings/Editor_Settings");
        Interactable = (InteractableObject) target;
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
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((InteractableObject) target),
            typeof(InteractableObject), false);
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
        Interactable.IsPersistent = EditorGUILayout.Toggle("Is Persistent", Interactable.IsPersistent);
        if (Interactable.IsPersistent)
        {
            Interactable.Saver = (InteractableObjectSaver) EditorGUILayout.ObjectField("Saver",
                Interactable.Saver, typeof(InteractableObjectSaver), true);
        }

        GUILayout.Space(15);
        GUILayout.Label("Activation", SubTitleStyle);
        GUILayout.Space(5);
        Interactable.IsClick = EditorGUILayout.Toggle("Click Enabled?", Interactable.IsClick);
        Interactable.IsTrigger = EditorGUILayout.Toggle("Trigger Enabled?", Interactable.IsTrigger);

        GUIStyle removeButtonStyle = EditorSkin.GetStyle("SquareRemoveButtonSmallInspector");
        GUIStyle addButtonStyle = EditorSkin.GetStyle("SquareAddButtonSmallInspector");
        GUIStyle genericButtonStyle = EditorSkin.GetStyle("GenericButtonSmallInspector");
        
        GUILayout.Space(15);
        GUILayout.Label("Requirements:", SubTitleStyle, GUILayout.MaxWidth(150));
        GUILayout.Space(5);
        Interactable.RequirementsTemplate = (RequirementsTemplate) EditorGUILayout.ObjectField("Requirements",
            Interactable.RequirementsTemplate, typeof(RequirementsTemplate), false);

        GUILayout.Space(15);
        RPGBuilderEditorUtility.StartHorizontalMargin(0, false);
        GUILayout.Label("Actions:", SubTitleStyle, GUILayout.MaxWidth(150));
        GUILayout.Space(40);
        if (GUILayout.Button(showActions ? "Hide" : "Show", genericButtonStyle, GUILayout.MaxWidth(60), GUILayout.MaxHeight(20)))
        {
            showActions = !showActions;
        }
        GUILayout.Space(5);
        if (GUILayout.Button("+", addButtonStyle, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            Interactable.Actions.Add(new InteractableObjectData.InteractableObjectAction());
            if (showActions == false) showActions = true;
        }
        RPGBuilderEditorUtility.EndHorizontalMargin(0, false);

        
        var actionList = serializedObject.FindProperty("Actions");
        Interactable.Actions = GetTargetObjectOfProperty(actionList) as List<InteractableObjectData.InteractableObjectAction>;

        if (showActions)
        {
            Interactable.MaxActions = EditorGUILayout.IntField("Maximum Actions", Interactable.MaxActions); 
            for (var a = 0; a < Interactable.Actions.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(a + 1 + ":", NumberStyle, GUILayout.MaxWidth(15));
                Interactable.Actions[a].type =
                    (InteractableObjectData.InteractableObjectActionType) EditorGUILayout.EnumPopup(Interactable
                        .Actions[a].type);

                if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Interactable.Actions.RemoveAt(a);
                    serializedObject.Update();
                    return;
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3);
                RPGBuilderEditorUtility.StartHorizontalMargin(marginValue, true);
                Interactable.Actions[a].chance =
                    EditorGUILayout.Slider("Chance", Interactable.Actions[a].chance, 0f, 100f);
                switch (Interactable.Actions[a].type)
                {
                    case InteractableObjectData.InteractableObjectActionType.Effect:
                        Interactable.Actions[a].Effect = (RPGEffect) EditorGUILayout.ObjectField("Effect",
                            Interactable.Actions[a].Effect, typeof(RPGEffect), false);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.Quest:
                        Interactable.Actions[a].Quest = (RPGQuest) EditorGUILayout.ObjectField("Quest",
                            Interactable.Actions[a].Quest, typeof(RPGQuest), false);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.Point:
                        Interactable.Actions[a].Point = (RPGTreePoint) EditorGUILayout.ObjectField(
                            "Point", Interactable.Actions[a].Point, typeof(RPGTreePoint), false);
                        Interactable.Actions[a].amount =
                            EditorGUILayout.IntField("Amount", Interactable.Actions[a].amount);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.GiveCharacterExperience:
                        Interactable.Actions[a].amount =
                            EditorGUILayout.IntField("Amount", Interactable.Actions[a].amount);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.GiveSkillExperience:
                        Interactable.Actions[a].Skill = (RPGSkill) EditorGUILayout.ObjectField("Skill",
                            Interactable.Actions[a].Skill, typeof(RPGSkill), false);
                        Interactable.Actions[a].amount =
                            EditorGUILayout.IntField("Amount", Interactable.Actions[a].amount);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.GiveWeaponTemplateExperience:
                        Interactable.Actions[a].WeaponTemplate = (RPGWeaponTemplate) EditorGUILayout.ObjectField(
                            "Weapon Template",
                            Interactable.Actions[a].WeaponTemplate, typeof(RPGWeaponTemplate), false);
                        Interactable.Actions[a].amount =
                            EditorGUILayout.IntField("Amount", Interactable.Actions[a].amount);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.CompleteTask:
                        Interactable.Actions[a].Task = (RPGTask) EditorGUILayout.ObjectField("Task",
                            Interactable.Actions[a].Task, typeof(RPGTask), false);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.UnityEvent:
                        var ActionRef = actionList.GetArrayElementAtIndex(a);
                        var events = ActionRef.FindPropertyRelative("unityEvents");
                        EditorGUILayout.PropertyField(events);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    case InteractableObjectData.InteractableObjectActionType.SaveCharacter:
                        break;
                    case InteractableObjectData.InteractableObjectActionType.Resource:
                        Interactable.Actions[a].Resource = (RPGResourceNode) EditorGUILayout.ObjectField("Resource",
                            Interactable.Actions[a].Resource, typeof(RPGResourceNode), false);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.Chest:
                        Interactable.Actions[a].LootTable = (RPGLootTable) EditorGUILayout.ObjectField("Loot Table",
                            Interactable.Actions[a].LootTable, typeof(RPGLootTable), false);
                        break;
                    case InteractableObjectData.InteractableObjectActionType.GameActions:
                        Interactable.Actions[a].GameActionsTemplate = (GameActionsTemplate) EditorGUILayout.ObjectField("Game Actions",
                            Interactable.Actions[a].GameActionsTemplate, typeof(GameActionsTemplate), false);
                        break;
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(0, true);

                GUILayout.Space(10);
            }
        }

        GUILayout.Space(15);
        RPGBuilderEditorUtility.StartHorizontalMargin(0, false);
        GUILayout.Label("Visual Effects:", SubTitleStyle, GUILayout.MaxWidth(150));
        GUILayout.Space(40);
        if (GUILayout.Button(showVisualEffects ? "Hide" : "Show", genericButtonStyle, GUILayout.MaxWidth(60), GUILayout.MaxHeight(20)))
        {
            showVisualEffects = !showVisualEffects;
        }
        GUILayout.Space(5);
        if (GUILayout.Button("+", addButtonStyle, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            Interactable.VisualEfects.Add(new InteractableObjectData.InteractableObjectVisualEffect());
            if (showVisualEffects == false) showVisualEffects = true;
        }
        RPGBuilderEditorUtility.EndHorizontalMargin(0, false);

        var visualEffectsList = serializedObject.FindProperty("VisualEfects");
        Interactable.VisualEfects =
            GetTargetObjectOfProperty(visualEffectsList) as List<InteractableObjectData.InteractableObjectVisualEffect>;

        if (showVisualEffects)
        {
            for (var a = 0; a < Interactable.VisualEfects.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(a + 1 + ":", NumberStyle, GUILayout.MaxWidth(15));
                Interactable.VisualEfects[a].TargetType =
                    (InteractableObjectData.InteractableObjectTemplateTarget) EditorGUILayout.EnumPopup("Spawn On:",
                        Interactable
                            .VisualEfects[a].TargetType);

                if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Interactable.VisualEfects.RemoveAt(a);
                    serializedObject.Update();
                    return;
                }

                EditorGUILayout.EndHorizontal();

                RPGBuilderEditorUtility.StartHorizontalMargin(marginValue, true);
                EditorGUILayout.LabelField(Interactable.VisualEfects[a].VisualEntry.Template == null
                    ? "- Select Template -"
                    : Interactable.VisualEfects[a].VisualEntry.Template.entryName, SmallTitle);
                EditorGUILayout.BeginHorizontal();
                Interactable.VisualEfects[a].ActivationType =
                    (ActivationType) EditorGUILayout.EnumPopup("Activate On",
                        Interactable.VisualEfects[a].ActivationType);
                EditorGUILayout.EndHorizontal();

                Interactable.VisualEfects[a].VisualEntry.Template =
                    (VisualEffectTemplate) EditorGUILayout.ObjectField("Template",
                        Interactable.VisualEfects[a].VisualEntry.Template, typeof(VisualEffectTemplate), false);

                switch (Interactable.VisualEfects[a].TargetType)
                {
                    case InteractableObjectData.InteractableObjectTemplateTarget.Object:
                        EditorGUILayout.LabelField("Spawn Position:", SmallTitle);
                        Interactable.VisualEfects[a].VisualEntry.UseNodeSocket = false;

                        Interactable.VisualEfects[a].VisualEntry.PositionOffset = EditorGUILayout.Vector3Field(
                            "Position Offset",
                            Interactable.VisualEfects[a].VisualEntry.PositionOffset);
                        Interactable.VisualEfects[a].VisualEntry.ParentedToCaster = EditorGUILayout.Toggle(
                            "Attach to Object?",
                            Interactable.VisualEfects[a].VisualEntry.ParentedToCaster);
                        break;
                    case InteractableObjectData.InteractableObjectTemplateTarget.User:
                        EditorGUILayout.LabelField("Spawn Position:", SmallTitle);

                        Interactable.VisualEfects[a].VisualEntry.UseNodeSocket = EditorGUILayout.Toggle(
                            "Use Node Socket?",
                            Interactable.VisualEfects[a].VisualEntry.UseNodeSocket);
                        if (Interactable.VisualEfects[a].VisualEntry.UseNodeSocket)
                        {
                            int projSocketIndex = EditorGUILayout.Popup("Socket",
                                RPGBuilderEditorUtility.GetTypeEntryIndex(allNodeSockets,
                                    Interactable.VisualEfects[a].VisualEntry.NodeSocket),
                                RPGBuilderEditorUtility.GetTypeEntriesAsStringArray(allNodeSockets.ToArray()));
                            Interactable.VisualEfects[a].VisualEntry.NodeSocket =
                                (RPGBNodeSocket) allNodeSockets[projSocketIndex];
                            Interactable.VisualEfects[a].VisualEntry.PositionOffset = EditorGUILayout.Vector3Field(
                                "Position Offset",
                                Interactable.VisualEfects[a].VisualEntry.PositionOffset);
                            Interactable.VisualEfects[a].VisualEntry.ParentedToCaster = EditorGUILayout.Toggle(
                                "Attach to Socket?",
                                Interactable.VisualEfects[a].VisualEntry.ParentedToCaster);
                        }
                        else
                        {
                            Interactable.VisualEfects[a].VisualEntry.PositionOffset = EditorGUILayout.Vector3Field(
                                "Position Offset",
                                Interactable.VisualEfects[a].VisualEntry.PositionOffset);
                            Interactable.VisualEfects[a].VisualEntry.ParentedToCaster = EditorGUILayout.Toggle(
                                "Attach to Object?",
                                Interactable.VisualEfects[a].VisualEntry.ParentedToCaster);
                        }

                        break;
                }

                Interactable.VisualEfects[a].VisualEntry.Scale =
                    EditorGUILayout.Vector3Field("Scale", Interactable.VisualEfects[a].VisualEntry.Scale);

                EditorGUILayout.LabelField("Settings:", SmallTitle);
                Interactable.VisualEfects[a].VisualEntry.Duration =
                    EditorGUILayout.FloatField("Duration", Interactable.VisualEfects[a].VisualEntry.Duration);
                Interactable.VisualEfects[a].VisualEntry.Delay =
                    EditorGUILayout.FloatField("Delay", Interactable.VisualEfects[a].VisualEntry.Delay);
                RPGBuilderEditorUtility.EndHorizontalMargin(0, true);
                GUILayout.Space(10);
            }
        }

        GUILayout.Space(15);
        RPGBuilderEditorUtility.StartHorizontalMargin(0, false);
        GUILayout.Label("Animations:", SubTitleStyle, GUILayout.MaxWidth(150));
        GUILayout.Space(40);
        if (GUILayout.Button(showAnimations ? "Hide" : "Show", genericButtonStyle, GUILayout.MaxWidth(60), GUILayout.MaxHeight(20)))
        {
            showAnimations = !showAnimations;
        }
        GUILayout.Space(5);
        if (GUILayout.Button("+", addButtonStyle, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            Interactable.Animations.Add(new InteractableObjectData.InteractableObjectAnimation());
            if (showAnimations == false) showAnimations = true;
        }
        RPGBuilderEditorUtility.EndHorizontalMargin(0, false);

        var animationsList = serializedObject.FindProperty("Animations");
        Interactable.Animations =
            GetTargetObjectOfProperty(animationsList) as List<InteractableObjectData.InteractableObjectAnimation>;

        if (showAnimations)
        {
            Interactable.anim = (Animator) EditorGUILayout.ObjectField("Animator", Interactable.anim, typeof(Animator), true);
            for (var a = 0; a < Interactable.Animations.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(a + 1 + ":", NumberStyle, GUILayout.MaxWidth(15));
                Interactable.Animations[a].TargetType =
                    (InteractableObjectData.InteractableObjectTemplateTarget) EditorGUILayout.EnumPopup("Play On:",
                        Interactable.Animations[a].TargetType);

                if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Interactable.Animations.RemoveAt(a);
                    serializedObject.Update();
                    return;
                }

                EditorGUILayout.EndHorizontal();
                RPGBuilderEditorUtility.StartHorizontalMargin(marginValue, true);

                EditorGUILayout.LabelField(Interactable.Animations[a].AnimationEntry.Template == null
                    ? "- Select Template -"
                    : Interactable.Animations[a].AnimationEntry.Template.entryName, SmallTitle);
                Interactable.Animations[a].ActivationType =
                    (ActivationType) EditorGUILayout.EnumPopup("Activate On",
                        Interactable.Animations[a].ActivationType);

                Interactable.Animations[a].AnimationEntry.Template =
                    (AnimationTemplate) EditorGUILayout.ObjectField("Template",
                        Interactable.Animations[a].AnimationEntry.Template, typeof(AnimationTemplate), false);
                EditorGUILayout.LabelField("Settings:", SmallTitle);
                Interactable.Animations[a].AnimationEntry.Delay =
                    EditorGUILayout.FloatField("Delay", Interactable.Animations[a].AnimationEntry.Delay);
                RPGBuilderEditorUtility.EndHorizontalMargin(0, true);
                GUILayout.Space(10);
            }
        }

        GUILayout.Space(15);
        RPGBuilderEditorUtility.StartHorizontalMargin(0, false);
        GUILayout.Label("Sounds:", SubTitleStyle, GUILayout.MaxWidth(150));
        GUILayout.Space(40);
        if (GUILayout.Button(showSounds ? "Hide" : "Show", genericButtonStyle, GUILayout.MaxWidth(60), GUILayout.MaxHeight(20)))
        {
            showSounds = !showSounds;
        }
        GUILayout.Space(5);
        if (GUILayout.Button("+", addButtonStyle, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            Interactable.Sounds.Add(new InteractableObjectData.InteractableObjectSound());
            if (showSounds == false) showSounds = true;
        }
        RPGBuilderEditorUtility.EndHorizontalMargin(0, false);

        var soundsList = serializedObject.FindProperty("Sounds");
        Interactable.Sounds =
            GetTargetObjectOfProperty(soundsList) as List<InteractableObjectData.InteractableObjectSound>;

        if (showSounds)
        {
            for (var a = 0; a < Interactable.Sounds.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(a + 1 + ":", NumberStyle, GUILayout.MaxWidth(15));
                Interactable.Sounds[a].TargetType =
                    (InteractableObjectData.InteractableObjectTemplateTarget) EditorGUILayout.EnumPopup("Play On:",
                        Interactable.Sounds[a].TargetType);

                if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Interactable.Sounds.RemoveAt(a);
                    serializedObject.Update();
                    return;
                }

                EditorGUILayout.EndHorizontal();
                RPGBuilderEditorUtility.StartHorizontalMargin(marginValue, true);

                EditorGUILayout.LabelField(Interactable.Sounds[a].SoundEntry.Template == null
                    ? "- Select Template -"
                    : Interactable.Sounds[a].SoundEntry.Template.entryName, SmallTitle);
                Interactable.Sounds[a].ActivationType =
                    (ActivationType) EditorGUILayout.EnumPopup("Activate On",
                        Interactable.Sounds[a].ActivationType);

                Interactable.Sounds[a].SoundEntry.Template =
                    (SoundTemplate) EditorGUILayout.ObjectField("Template",
                        Interactable.Sounds[a].SoundEntry.Template, typeof(SoundTemplate), false);
                EditorGUILayout.LabelField("Settings:", SmallTitle);
                Interactable.Sounds[a].SoundEntry.Delay =
                    EditorGUILayout.FloatField("Delay", Interactable.Sounds[a].SoundEntry.Delay);
                RPGBuilderEditorUtility.EndHorizontalMargin(0, true);
                GUILayout.Space(10);
            }
        }

        GUILayout.Space(15);
        GUILayout.Label("State", SubTitleStyle);
        GUILayout.Space(5);
        Interactable.State =
            (InteractableObjectData.InteractableObjectState) EditorGUILayout.EnumPopup("State", Interactable.State);

        if (Interactable.UseResourceValues)
        {
            Interactable.Resource = (RPGResourceNode) EditorGUILayout.ObjectField("Resource",
                Interactable.Resource, typeof(RPGResourceNode), false);
        }
        else
        {
            if (Interactable.IsPersistent)
            {
                Interactable.LimitedUseAmount =
                    EditorGUILayout.Toggle("Limited Use Amount?", Interactable.LimitedUseAmount);
                if (Interactable.LimitedUseAmount)
                {
                    Interactable.MaxUseAmount = EditorGUILayout.IntField("Max Use Amount", Interactable.MaxUseAmount);
                }
            }

            Interactable.Cooldown = EditorGUILayout.FloatField("Cooldown", Interactable.Cooldown);
            Interactable.InteractionTime = EditorGUILayout.FloatField("Interaction Time", Interactable.InteractionTime);
            Interactable.MaxDistance = EditorGUILayout.FloatField("Use Distance Max", Interactable.MaxDistance);
        }
        
        GUILayout.Space(15);
        GUILayout.Label("Appearances", SubTitleStyle);
        GUILayout.Space(5);
        Interactable.ReadyAppearance = (GameObject) EditorGUILayout.ObjectField("Ready",
            Interactable.ReadyAppearance, typeof(GameObject), true);
        Interactable.OnCooldownAppearance = (GameObject) EditorGUILayout.ObjectField("Cooldown",
            Interactable.OnCooldownAppearance, typeof(GameObject), true);
        Interactable.UnavailableAppearance = (GameObject) EditorGUILayout.ObjectField("Consumed",
            Interactable.UnavailableAppearance, typeof(GameObject), true);

        GUILayout.Space(15);
        GUILayout.Label("Interactable UI", SubTitleStyle);
        GUILayout.Space(5);
        if (Interactable.UseResourceValues)
        {
            Interactable.InteractableName = Interactable.Resource != null ? Interactable.Resource.displayName : "";
        }
        else
        {
            Interactable.InteractableName =
                EditorGUILayout.TextField("Interactable Name", Interactable.InteractableName);
        }

        Interactable.UIOffsetY = EditorGUILayout.FloatField("Y Offset", Interactable.UIOffsetY);

        PrefabUtility.RecordPrefabInstancePropertyModifications(Interactable);
        serializedObject.ApplyModifiedProperties();
        Undo.RecordObject(Interactable, "Modified Interactable Object");

        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(Interactable);
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

