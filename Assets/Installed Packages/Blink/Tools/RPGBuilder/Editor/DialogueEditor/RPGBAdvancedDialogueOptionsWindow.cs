using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEditor;
using UnityEngine;

public class RPGBAdvancedDialogueOptionsWindow : EditorWindow
{
    
    private static RPGBAdvancedDialogueOptionsWindow Instance;
    private GUISkin skin;
    private RPGBuilderEditorDATA editorDATA;
    private Vector2 viewScrollPosition;

    public static RPGDialogueTextNode currentNode;
    private static int requirementIndex;
    private static int gameActionIndex;

    private void OnEnable()
    {
        Instance = this;
        editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        skin = Resources.Load<GUISkin>(editorDATA.RPGBEditorDataPath + "RPGBuilderSkin");
    }

    private void OnDestroy()
    {
        currentNode = null;
        Instance = null;
    }

    private void OnDisable()
    {
        currentNode = null;
        Instance = null;
    }
    public static bool IsOpen => Instance != null;

    public void AssignCurrentDialogueNode(RPGDialogueTextNode dialogueNode)
    {
        currentNode = dialogueNode;
    }
    
    private void OnGUI()
    {
        DrawView();
    }

    private void DrawView()
    {

        if (currentNode == null)
        {
            Close();
            return;
        }
        
        Color guiColor = GUI.color;

        int width = Screen.width;
        if (width < 350) width = 350;
        float height = Screen.height;
        if (height < 500) height = 500;


        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(13);
        GUILayout.BeginVertical();
        viewScrollPosition = GUILayout.BeginScrollView(viewScrollPosition, false, false, GUILayout.Width(width),
            GUILayout.Height(height));

        GUILayout.Space(10);
        GUILayout.Label("Node: " + currentNode.message, skin.GetStyle("ViewTitle"),  GUILayout.Height(40));

        currentNode.RequirementsTemplate = (RequirementsTemplate) EditorGUILayout.ObjectField("Requirements",
            currentNode.RequirementsTemplate, typeof(RequirementsTemplate), false);
        currentNode.GameActionsTemplate = (GameActionsTemplate) EditorGUILayout.ObjectField("Game Actions",
            currentNode.GameActionsTemplate, typeof(GameActionsTemplate), false);

        GUILayout.Box(editorDATA.gearSetsSeparator, skin.GetStyle("CustomImage"), GUILayout.Width(450),
            GUILayout.Height(10));
        currentNode.showSettings =
            EditorGUILayout.ToggleLeft("Show Settings?", currentNode.showSettings, GUILayout.Width(300));
        if (currentNode.showSettings)
        {
            
            GUILayout.Space(10);
            GUILayout.Label("Interaction Settings", skin.GetStyle("ViewTitle"), GUILayout.Width(325), GUILayout.Height(40));
            GUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Viewed Endlessly?", GUILayout.Width(150));
            GUILayout.Space(5);
            currentNode.viewedEndless = EditorGUILayout.Toggle(currentNode.viewedEndless, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            if (!currentNode.viewedEndless)
            {
                currentNode.viewCountMax = EditorGUILayout.IntField("View Times",
                    currentNode.viewCountMax, GUILayout.Width(300));
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Clicked Endlessly?", GUILayout.Width(150));
            GUILayout.Space(5);
            currentNode.clickedEndless = EditorGUILayout.Toggle(currentNode.clickedEndless, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            if (!currentNode.clickedEndless)
            {
                currentNode.clickCountMax = EditorGUILayout.IntField("Click Times",
                    currentNode.clickCountMax, GUILayout.Width(300));
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Visual Settings", skin.GetStyle("ViewTitle"), GUILayout.Width(325), GUILayout.Height(40));
            GUILayout.Space(5);
            currentNode.nodeImage = (Sprite)EditorGUILayout.ObjectField("Image", currentNode.nodeImage, typeof(Sprite), false, GUILayout.Width(300), GUILayout.Height(300));
        }
        GUILayout.Box(editorDATA.gearSetsSeparator, skin.GetStyle("CustomImage"), GUILayout.Width(450), GUILayout.Height(10));


        GUI.color = guiColor;

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
}
