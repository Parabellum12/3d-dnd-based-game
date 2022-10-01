using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEngine.Screen;

public class Blink_EditorSceneLoader : EditorWindow
{
    private RPGBuilderEditorSettings EditorSettings;
    
    [MenuItem("BLINK/Scene Loader")]
    private static void Init()
    {
        // Get existing open window or if none, make a new one:
        var window = (Blink_EditorSceneLoader) GetWindow(typeof(Blink_EditorSceneLoader));
        window.Show();
    }

    public void OnEnable()
    {
        EditorSettings = Resources.Load<RPGBuilderEditorSettings>("Database/Settings/Editor_Settings");
    }

    private Vector2 scrollPos;
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUILayout.BeginHorizontal();
        GUILayout.Space(width / 4);
        GUILayout.BeginVertical();

        foreach (var scene in EditorSettings.sceneLoaderList.Where(scene => GUILayout.Button(scene.sceneName, GUILayout.Height(22))))
        {
            if (Application.isPlaying)
            {
                return;
            }
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scene.scene));
        }

        GUILayout.EndVertical();
        GUILayout.Space(width / 4);
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
    }
}