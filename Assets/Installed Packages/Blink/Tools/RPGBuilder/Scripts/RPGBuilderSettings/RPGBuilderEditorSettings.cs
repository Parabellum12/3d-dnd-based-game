using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorSettings : RPGBuilderDatabaseEntry
{
    public string GetCurrentRPGBuilderVersion()
    {
        return "2.0.6";
    }

    public bool IncreasedEditorRepaint = true;
    public bool ShowFileNames;
    public string ResourcePath = "Assets/Blink/Tools/RPGBuilder/Resources/", DatabasePath = "Database/", RPGBEditorDataPath = "EditorData/";
    public RPGBuilderEditorTheme EditorTheme;
    
    [Serializable]
    public class SceneLoaderData
    {
        public string sceneName;

#if UNITY_EDITOR
        public SceneAsset scene;
#endif
    }
    public List<SceneLoaderData> sceneLoaderList = new List<SceneLoaderData>();
    
    public string characterSelectedName;
    public void UpdateEntryData(RPGBuilderEditorSettings newEntryData)
    {
        IncreasedEditorRepaint = newEntryData.IncreasedEditorRepaint;
        ResourcePath = newEntryData.ResourcePath;
        DatabasePath = newEntryData.DatabasePath;
        RPGBEditorDataPath = newEntryData.RPGBEditorDataPath;
        EditorTheme = newEntryData.EditorTheme;
        sceneLoaderList = newEntryData.sceneLoaderList;
        characterSelectedName = newEntryData.characterSelectedName;
        ShowFileNames = newEntryData.ShowFileNames;
    }
}