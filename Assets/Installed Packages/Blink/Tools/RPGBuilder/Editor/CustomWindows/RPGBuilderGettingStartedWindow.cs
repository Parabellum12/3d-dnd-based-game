using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RPGBuilderGettingStartedWindow : EditorWindow
{
    private ScriptableObject scriptableObj;
    private SerializedObject serialObj;
    public RPGBuilderEditorDATA EditorData;
    private RPGBuilderEditorSettings EditorSettings;
    public GUISkin EditorSkin;
    private Vector2 scrollPos;
    private GUIStyle titleStyle;

    private bool SwitchToLinear = true;
    
    [MenuItem("BLINK/Getting Started")]
    private static void Init()
    {
        var window = (RPGBuilderGettingStartedWindow) GetWindow(typeof(RPGBuilderGettingStartedWindow));
        window.Show();
    }

    public void OnEnable()
    {
        EditorData = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        EditorSettings = Resources.Load<RPGBuilderEditorSettings>("Database/Settings/Editor_Settings");
        EditorSkin = Resources.Load<GUISkin>(EditorData.RPGBEditorDataPath + "RPGBuilderSkin");
        scriptableObj = this;
        serialObj = new SerializedObject(scriptableObj);


        titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            normal = {textColor = Color.white}
        };
    }

    private void Update()
    {
        Repaint();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Welcome to RPG Builder by Blink!", titleStyle);
        GUILayout.Space(15);
        
        EditorGUILayout.LabelField("Join us on:", titleStyle);
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("Discord", EditorSkin.GetStyle("SquareAddButtonSmallInspector"), GUILayout.ExpandWidth(true),
            GUILayout.Height(40)))
        {
            Application.OpenURL("https://discord.gg/fYzpuYwPwJ");
        }
        GUILayout.Space(6);
        if (GUILayout.Button("YouTube", EditorSkin.GetStyle("SquareAddButtonSmallInspector"), GUILayout.ExpandWidth(true),
            GUILayout.Height(40)))
        {
            Application.OpenURL("https://www.youtube.com/c/BlinkStudiosYoutube");
        }
        GUILayout.Space(6);
        if (GUILayout.Button("Store", EditorSkin.GetStyle("SquareAddButtonSmallInspector"), GUILayout.ExpandWidth(true),
            GUILayout.Height(40)))
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/49855");
        }
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
        
        
        GUILayout.Space(25);
        EditorGUILayout.LabelField("Ready?", titleStyle);
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        SwitchToLinear = EditorGUILayout.ToggleLeft("(Optional) Switch to Linear", SwitchToLinear, GUILayout.ExpandWidth(true));
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("Setup RPG Builder", EditorSkin.GetStyle("SquareAddButtonSmallInspector"), GUILayout.ExpandWidth(true),
            GUILayout.Height(40)))
        {
            
            if (Application.isPlaying)
            {
                Debug.LogError("Can only be done outside of play mode");
                return;
            }
            
            if (SwitchToLinear)
            {
                if(PlayerSettings.colorSpace != ColorSpace.Linear) PlayerSettings.colorSpace = ColorSpace.Linear;
            }

            EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            
            if (EditorData.DemoScene != null)
            {
                EditorBuildSettingsScene DemoScene = new EditorBuildSettingsScene();
                DemoScene.path = AssetDatabase.GetAssetPath(EditorData.DemoScene);
                DemoScene.enabled = true;
                scenes.Add(DemoScene);
            }
            else
            {
                Debug.LogError("The Demo scene could not be found");
            }
            
            if (EditorData.MainMenuScene != null)
            {
                EditorBuildSettingsScene MainMenuScene = new EditorBuildSettingsScene();
                MainMenuScene.path = AssetDatabase.GetAssetPath(EditorData.MainMenuScene);
                MainMenuScene.enabled = true;
                scenes.Insert(0, MainMenuScene);
                
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(EditorData.MainMenuScene));
            }
            else
            {
                Debug.LogError("The Main Menu scene could not be found");
            }
            
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.EndScrollView();
        serialObj.ApplyModifiedProperties();
    }
}
