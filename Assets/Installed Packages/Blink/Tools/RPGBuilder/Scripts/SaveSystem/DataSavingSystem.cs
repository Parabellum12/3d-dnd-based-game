using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using BLINK.RPGBuilder.Characters;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.VersionControl;
#endif
using UnityEngine;
using FileMode = System.IO.FileMode;

public static class DataSavingSystem
{

    public static List<CharacterData> LoadAllCharacters()
    {
        var path = Application.persistentDataPath;
        var di = new DirectoryInfo(path);
        var files = di.GetFiles().Where(o => o.Name.Contains("_RPGBCharacter.txt")).ToArray();
        var allCharacters = new List<CharacterData>();
        foreach (var t in files)
        {
            var charname = t.Name.Replace("_RPGBCharacter.txt", "");
            allCharacters.Add(RPGBuilderJsonSaver.GetCharacterDataByName(charname));
        }

        return allCharacters;
    }

    public static void DeleteAssetIDFile(string IDFileName)
    {
        var path = getPath(IDFileName);
        File.Delete(path);
        
        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }
    
    public static void DeleteOldAssetIDFile(string IDFileName)
    {
        var path = getOldPath(IDFileName);
        File.Delete(path);
    }

    public static void SaveAssetID(AssetIDHandler assetIDHandler)
    {
        var formatter = new BinaryFormatter();
        var path = getPath(assetIDHandler.IDFileName);
        var stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, assetIDHandler);
        stream.Close();
    }

#if UNITY_EDITOR
    public static AssetIDHandler LoadAssetID(string moduleIDFileName)
    {
        var path = getPath(moduleIDFileName);
        
        Asset thisAsset = Provider.GetAssetByPath(path);
        if (!Provider.isActive) return !File.Exists(path) ? null : HandleFileActions(path);
        if(thisAsset == null) return null;
        if (Provider.IsOpenForEdit(thisAsset)) return !File.Exists(path) ? null : HandleFileActions(path);
        Provider.Checkout(thisAsset, CheckoutMode.Both);

        FileInfo fileInfo = new FileInfo(path) {IsReadOnly = false};

        return !File.Exists(path) ? null : HandleFileActions(path);
    }
#endif
    
#if UNITY_EDITOR
    public static AssetIDHandler LoadOldAssetID(string moduleIDFileName)
    {
        var path = getOldPath(moduleIDFileName);
        
        Asset thisAsset = Provider.GetAssetByPath(path);
        if (!Provider.isActive) return !File.Exists(path) ? null : HandleFileActions(path);
        if(thisAsset == null) return null;
        if (Provider.IsOpenForEdit(thisAsset)) return !File.Exists(path) ? null : HandleFileActions(path);
        Provider.Checkout(thisAsset, CheckoutMode.Both);

        FileInfo fileInfo = new FileInfo(path) {IsReadOnly = false};

        return !File.Exists(path) ? null : HandleFileActions(path);
    }
#endif

    private static AssetIDHandler HandleFileActions(string path)
    {
        var formatter = new BinaryFormatter();
        var stream = new FileStream(path, FileMode.Open);
        var data = formatter.Deserialize(stream) as AssetIDHandler;
        stream.Close();
        return data;
    }
    
    private static string getOldPath(string moduleIDFileName)
    {
        RPGBuilderEditorDATA editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/" + moduleIDFileName + ".THMSV";
    }
    private static string getPath(string moduleIDFileName)
    {
        RPGBuilderEditorDATA editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/" + moduleIDFileName + ".RPGBuilderModuleIDs";
    }
}
