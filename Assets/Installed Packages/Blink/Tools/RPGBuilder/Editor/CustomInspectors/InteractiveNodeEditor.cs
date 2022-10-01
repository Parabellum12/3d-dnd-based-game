using System;
using BLINK.RPGBuilder;
using BLINK.RPGBuilder.World;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BLINK.RPGBuilder.Managers;
using UnityEditor.SceneManagement;
using UnityEngine.Events;

[CanEditMultipleObjects]
[CustomEditor(typeof(InteractiveNode))]
public class InteractiveNodeEditor : Editor
{
    private InteractiveNode NodeReference;
    
    private SerializedProperty isClick;
    private SerializedProperty isTrigger;
    private SerializedProperty useCount;
    private SerializedProperty cooldown;
    private SerializedProperty interactionTime;
    private SerializedProperty useDistanceMax;
    private SerializedProperty nodeUseAnimation;
    private SerializedProperty nodeInteractableYOffset;
    private SerializedProperty nodeInteractableName;
    
    private void InitSerializedData()
    {
        isClick = serializedObject.FindProperty("isClick");
        isTrigger = serializedObject.FindProperty("isTrigger");
        useCount = serializedObject.FindProperty("useCount");
        cooldown = serializedObject.FindProperty("cooldown");
        interactionTime = serializedObject.FindProperty("interactionTime");
        useDistanceMax = serializedObject.FindProperty("useDistanceMax");
        nodeUseAnimation = serializedObject.FindProperty("nodeUseAnimation");
        nodeInteractableYOffset = serializedObject.FindProperty("interactableUIOffsetY");
        nodeInteractableName = serializedObject.FindProperty("interactableName");
    }
    private void OnEnable()
    {
        NodeReference = (InteractiveNode) target;
        InitSerializedData();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((InteractiveNode) target),
            typeof(InteractiveNode), false);
        GUI.enabled = true;

        var SubTitleStyle = new GUIStyle();
        SubTitleStyle.alignment = TextAnchor.UpperLeft;
        SubTitleStyle.fontSize = 17;
        SubTitleStyle.fontStyle = FontStyle.Bold;
        SubTitleStyle.normal.textColor = Color.white;

        GUIStyle disclaimerStyle = new GUIStyle();
        disclaimerStyle.richText = true;
        disclaimerStyle.fontStyle = FontStyle.Bold;
        GUILayout.Space(5);
        EditorGUILayout.LabelField("<color=red>DEPRECATED COMPONENT</color>", disclaimerStyle);
        EditorGUILayout.LabelField("<color=red>USE <color=green>INTERACTABLE OBJECT</color> INSTEAD</color>", disclaimerStyle);
        
        GUILayout.Space(5);
        GUILayout.Label("Node Type", SubTitleStyle);
        GUILayout.Space(5);
        NodeReference.nodeType = (InteractiveNode.InteractiveNodeType) EditorGUILayout.EnumPopup("Type", NodeReference.nodeType);

        switch (NodeReference.nodeType)
        {
            case InteractiveNode.InteractiveNodeType.resourceNode:
            {
                GUILayout.Space(5);
                GUILayout.Label("Resource Node Specific", SubTitleStyle);
                GUILayout.Space(5);

                NodeReference.resourceNodeData = (RPGResourceNode) EditorGUILayout.ObjectField("Resource Node",
                    NodeReference.resourceNodeData, typeof(RPGResourceNode), false);
                break;
            }
            case InteractiveNode.InteractiveNodeType.effectNode:
            {
                GUILayout.Space(5);
                GUILayout.Label("Effects Specific", SubTitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("+ Add Effect"))
                    NodeReference.effectsData.Add(new InteractiveNode.effectsDATA());

                var ThisList = serializedObject.FindProperty("effectsData");
                NodeReference.effectsData = GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.effectsDATA>;

                for (var a = 0; a < NodeReference.effectsData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        NodeReference.effectsData.RemoveAt(a);
                        serializedObject.Update();
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (NodeReference.effectsData[a].effect != null) effectName = NodeReference.effectsData[a].effect.entryName;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    NodeReference.effectsData[a].effect = (RPGEffect) EditorGUILayout.ObjectField("Effect",
                        NodeReference.effectsData[a].effect, typeof(RPGEffect), false);
                    NodeReference.effectsData[a].chance =
                        EditorGUILayout.Slider("Chance", NodeReference.effectsData[a].chance, 0f, 100f);



                    GUILayout.Space(10);
                }
            }

                break;
            case InteractiveNode.InteractiveNodeType.abilityNode:
            {


            }
                break;
            case InteractiveNode.InteractiveNodeType.questNode:
            {
                GUILayout.Space(5);
                GUILayout.Label("Quest Specific", SubTitleStyle);
                GUILayout.Space(5);

                NodeReference.questsData.quest = (RPGQuest) EditorGUILayout.ObjectField("Quest",
                    NodeReference.questsData.quest, typeof(RPGQuest), false);
                NodeReference.questsData.chance =
                    EditorGUILayout.Slider("Chance", NodeReference.questsData.chance, 0f, 100f);

                break;
            }
            case InteractiveNode.InteractiveNodeType.giveTreePoint:
            {
                GUILayout.Space(5);
                GUILayout.Label("Tree Points Specific", SubTitleStyle);
                GUILayout.Space(5);
                if (GUILayout.Button("+ Add Tree Point"))
                    NodeReference.treePointsData.Add(new InteractiveNode.treePointsDATA());

                var ThisList = serializedObject.FindProperty("treePointsData");
                NodeReference.treePointsData =
                    GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.treePointsDATA>;

                for (var a = 0; a < NodeReference.treePointsData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        NodeReference.treePointsData.RemoveAt(a);
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (NodeReference.treePointsData[a].treePoint != null)
                        effectName = NodeReference.treePointsData[a].treePoint.entryName;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    NodeReference.treePointsData[a].treePoint = (RPGTreePoint) EditorGUILayout.ObjectField(
                        "Tree Point", NodeReference.treePointsData[a].treePoint, typeof(RPGTreePoint), false);
                    NodeReference.treePointsData[a].amount =
                        EditorGUILayout.IntField("Amount", NodeReference.treePointsData[a].amount);
                    NodeReference.treePointsData[a].chance =
                        EditorGUILayout.Slider("Chance", NodeReference.treePointsData[a].chance, 0f, 100f);

                    GUILayout.Space(10);
                }

                break;
            }
            case InteractiveNode.InteractiveNodeType.teachSkill:
            {
                GUILayout.Space(5);
                GUILayout.Label("Skills Specific", SubTitleStyle);
                GUILayout.Space(5);
                break;
            }
            case InteractiveNode.InteractiveNodeType.giveClassEXP:
            {

                GUILayout.Space(5);
                GUILayout.Label("Class EXP Specific", SubTitleStyle);
                GUILayout.Space(5);

                NodeReference.classExpData.expAmount =
                    EditorGUILayout.IntField("Amount", NodeReference.classExpData.expAmount);
                NodeReference.classExpData.chance =
                    EditorGUILayout.Slider("Chance", NodeReference.classExpData.chance, 0f, 100f);

                break;
            }
            case InteractiveNode.InteractiveNodeType.giveSkillEXP:
            {
                GUILayout.Space(5);
                GUILayout.Label("Skills EXP Specific", SubTitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("+ Add Skill"))
                    NodeReference.skillExpData.Add(new InteractiveNode.skillExpDATA());

                var ThisList = serializedObject.FindProperty("skillExpData");
                NodeReference.skillExpData =
                    GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.skillExpDATA>;

                for (var a = 0; a < NodeReference.skillExpData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        NodeReference.skillExpData.RemoveAt(a);
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (NodeReference.skillExpData[a].skill != null) effectName = NodeReference.skillExpData[a].skill.entryName;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    NodeReference.skillExpData[a].skill = (RPGSkill) EditorGUILayout.ObjectField("Skill",
                        NodeReference.skillExpData[a].skill, typeof(RPGSkill), false);
                    NodeReference.skillExpData[a].expAmount =
                        EditorGUILayout.IntField("Amount", NodeReference.skillExpData[a].expAmount);
                    NodeReference.skillExpData[a].chance =
                        EditorGUILayout.Slider("Chance", NodeReference.skillExpData[a].chance, 0f, 100f);

                    GUILayout.Space(10);
                }

                break;
            }
            case InteractiveNode.InteractiveNodeType.completeTask:
            {
                GUILayout.Space(5);
                GUILayout.Label("Tasks Specific", SubTitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("+ Add Task"))
                    NodeReference.taskData.Add(new InteractiveNode.taskDATA());

                var ThisList = serializedObject.FindProperty("taskData");
                NodeReference.taskData =
                    GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.taskDATA>;

                for (var a = 0; a < NodeReference.taskData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        NodeReference.taskData.RemoveAt(a);
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (NodeReference.taskData[a].task != null) effectName = NodeReference.taskData[a].task.entryName;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    NodeReference.taskData[a].task = (RPGTask) EditorGUILayout.ObjectField("Task",
                        NodeReference.taskData[a].task, typeof(RPGTask), false);
                    NodeReference.taskData[a].chance =
                        EditorGUILayout.Slider("Chance", NodeReference.taskData[a].chance, 0f, 100f);

                    GUILayout.Space(10);
                }

                break;
            }
            case InteractiveNode.InteractiveNodeType.container:
            {
                GUILayout.Space(5);
                GUILayout.Label("Container Specific", SubTitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("+ Add Loot Table"))
                    NodeReference.containerTablesData.Add(new InteractiveNode.containerLootTablesDATA());

                var ThisList = serializedObject.FindProperty("containerTablesData");
                NodeReference.containerTablesData =
                    GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.containerLootTablesDATA>;

                for (var a = 0; a < NodeReference.containerTablesData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        NodeReference.containerTablesData.RemoveAt(a);
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (NodeReference.containerTablesData[a].lootTable != null)
                        effectName = NodeReference.containerTablesData[a].lootTable.entryName;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    NodeReference.containerTablesData[a].lootTable =
                        (RPGLootTable) EditorGUILayout.ObjectField("Loot Table",
                            NodeReference.containerTablesData[a].lootTable, typeof(RPGLootTable), false);
                    NodeReference.containerTablesData[a].chance = EditorGUILayout.Slider("Chance",
                        NodeReference.containerTablesData[a].chance, 0f, 100f);

                    GUILayout.Space(10);
                }

                break;
            }
            case InteractiveNode.InteractiveNodeType.UnityEvent:
            {
                GUILayout.Space(5);
                GUILayout.Label("Unity Event Specific", SubTitleStyle);
                GUILayout.Space(5);

                var ThisList = serializedObject.FindProperty("unityEvent");
                serializedObject.Update();
                EditorGUILayout.PropertyField(ThisList, true);
                serializedObject.ApplyModifiedProperties();

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        // REQUIREMENT FIELDS
        GUILayout.Space(5);
        GUILayout.Label("Requirements", SubTitleStyle);
        GUILayout.Space(5);
        isClick.boolValue = EditorGUILayout.Toggle("Is Click?", isClick.boolValue);
        isTrigger.boolValue = EditorGUILayout.Toggle("Is Trigger?", isTrigger.boolValue);


        // GENERAL FIELDS
        GUILayout.Space(5);
        GUILayout.Label("State", SubTitleStyle);
        GUILayout.Space(5);
        NodeReference.nodeState = (InteractiveNode.InteractiveNodeState) EditorGUILayout.EnumPopup("State", NodeReference.nodeState);
        if (NodeReference.nodeType != InteractiveNode.InteractiveNodeType.resourceNode)
        {
            useCount.intValue = EditorGUILayout.IntField("Use Count", useCount.intValue);
            cooldown.floatValue = EditorGUILayout.FloatField("Cooldown", cooldown.floatValue);
            interactionTime.floatValue = EditorGUILayout.FloatField("Interaction Time", interactionTime.floatValue);
            useDistanceMax.floatValue = EditorGUILayout.FloatField("Use Distance Max", useDistanceMax.floatValue);
        }

        GUILayout.Space(5);
        GUILayout.Label("Node Animation", SubTitleStyle);
        GUILayout.Space(5);
        //animationName.stringValue = EditorGUILayout.TextField("Player Animation", animationName.stringValue);
        nodeUseAnimation.stringValue = EditorGUILayout.TextField("Node Animation", nodeUseAnimation.stringValue);
        

        GUILayout.Space(5);
        GUILayout.Label("Player Animation", SubTitleStyle);
        GUILayout.Space(5);
        //NodeReference.animations = RPGBuilderEditorFields.DrawAnimationsInspectorList(NodeReference.animations);

        GUILayout.Space(5);
        GUILayout.Label("Sound", SubTitleStyle);
        GUILayout.Space(5);
        NodeReference.nodeUseSound = (AudioClip) EditorGUILayout.ObjectField("Use Sound",
            NodeReference.nodeUseSound, typeof(AudioClip), false);
        
        GUILayout.Space(5);
        GUILayout.Label("Visuals", SubTitleStyle);
        GUILayout.Space(5);
        NodeReference.readyVisual =
            (GameObject) EditorGUILayout.ObjectField("Ready Visual", NodeReference.readyVisual, typeof(GameObject), true);
        NodeReference.onCooldownVisual = (GameObject) EditorGUILayout.ObjectField("On Cooldown Visual",
            NodeReference.onCooldownVisual, typeof(GameObject), true);
        NodeReference.disabledVisual =
            (GameObject) EditorGUILayout.ObjectField("Disabled Visual", NodeReference.disabledVisual, typeof(GameObject),
                true);
        
        GUILayout.Space(5);
        GUILayout.Label("Interactable UI", SubTitleStyle);
        GUILayout.Space(5);
        if (NodeReference.nodeType != InteractiveNode.InteractiveNodeType.resourceNode)
        {
            nodeInteractableName.stringValue =
                EditorGUILayout.TextField("Interactable Name", nodeInteractableName.stringValue);
        }
        nodeInteractableYOffset.floatValue = EditorGUILayout.FloatField("Y Offset", nodeInteractableYOffset.floatValue);
        
        PrefabUtility.RecordPrefabInstancePropertyModifications(NodeReference);
        serializedObject.ApplyModifiedProperties();
        Undo.RecordObject(NodeReference, "Modified Interactive Node");
        
        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(NodeReference);
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
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (var i = 0; i <= index; i++)
            if (!enm.MoveNext()) return null;
        return enm.Current;
    }
    
    private List<RPGCombatDATA.CombatVisualAnimation> DrawVisualAnimationsList(List<RPGCombatDATA.CombatVisualAnimation> visualAnimations)
    {
        if (GUILayout.Button("+ Add Visual Animation", GUILayout.ExpandWidth(true),
            GUILayout.Height(25)))
        {
            visualAnimations.Add(new RPGCombatDATA.CombatVisualAnimation());
        }

        GUIStyle xButtonStyle = new GUIStyle();
        xButtonStyle.normal.textColor = Color.red;
        xButtonStyle.fontSize = 18;
        
        for (var a = 0; a < visualAnimations.Count; a++)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            var requirementNumber = a + 1;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(20),
                GUILayout.Height(20)))
            {
                visualAnimations.RemoveAt(a);
                return visualAnimations;
            }

            EditorGUILayout.LabelField("Visual:" + requirementNumber + ":");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Activate On", GUILayout.Width(147.5f));
            visualAnimations[a].activationType =
                (RPGCombatDATA.CombatVisualActivationType) EditorGUILayout.EnumPopup(visualAnimations[a].activationType);
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(147.5f));
            visualAnimations[a].parameterType =
                (RPGCombatDATA.CombatVisualAnimationParameterType) EditorGUILayout.EnumPopup(
                    visualAnimations[a].parameterType);
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();

            visualAnimations[a].animationParameter = EditorGUILayout.TextField("Animation Parameter", visualAnimations[a].animationParameter);

            switch (visualAnimations[a].parameterType)
            {
                case RPGCombatDATA.CombatVisualAnimationParameterType.Bool:
                    visualAnimations[a].boolValue =
                        EditorGUILayout.Toggle("Set True?", visualAnimations[a].boolValue);
                    visualAnimations[a].duration = EditorGUILayout.FloatField("Duration", visualAnimations[a].duration);
                    break;
                case RPGCombatDATA.CombatVisualAnimationParameterType.Int:
                    visualAnimations[a].intValue = EditorGUILayout.IntField("Value", visualAnimations[a].intValue);
                    break;
                case RPGCombatDATA.CombatVisualAnimationParameterType.Float:
                        visualAnimations[a].floatValue = EditorGUILayout.FloatField("Value", visualAnimations[a].floatValue);
                    break;
            }
            
            visualAnimations[a].delay = EditorGUILayout.FloatField("Delay", visualAnimations[a].delay);
            
            visualAnimations[a].showWeapons =
                EditorGUILayout.Toggle("Show Weapon?", visualAnimations[a].showWeapons);
            if (visualAnimations[a].showWeapons)
            {
                visualAnimations[a].showWeaponDuration =
                    EditorGUILayout.FloatField("Weapon Duration", visualAnimations[a].showWeaponDuration);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        return visualAnimations;
    }
    
}