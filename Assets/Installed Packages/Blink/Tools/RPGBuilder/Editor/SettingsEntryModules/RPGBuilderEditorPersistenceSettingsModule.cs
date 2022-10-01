using BLINK.RPGBuilder.WorldPersistence;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RPGBuilderEditorPersistenceSettingsModule : RPGBuilderEditorModule
{
    private RPGBuilderPersistenceSettings currentEntry;
    private SaverIdentifier[] persistentObjects;

    public override void Initialize()
    {
        LoadEntries();
    }

    public override void InstantiateCurrentEntry(int index)
    {
        LoadEntries();
    }

    public override void LoadEntries()
    {
        currentEntry = Resources.Load<RPGBuilderPersistenceSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                                     AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }
        else
        {
            AssetDatabase.CreateAsset(CreateInstance<RPGBuilderPersistenceSettings>(),
                RPGBuilderEditor.Instance.EditorData.ResourcePath +
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath + AssetFolderName + "/" + EntryType + ".asset");
            currentEntry = Resources.Load<RPGBuilderPersistenceSettings>(
                RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                AssetFolderName + "/" + EntryType);
        }

        databaseEntries.Clear();
        databaseEntries.Add(currentEntry);

        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void CreateNewEntry()
    {
    }

    public override bool SaveConditionsMet()
    {
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        RPGBuilderPersistenceSettings entryFile = (RPGBuilderPersistenceSettings) updatedEntry;
        entryFile.UpdateEntryData(currentEntry);
        RPGBuilderEditor.Instance.LoadSettings();
    }

    public override void ClearEntries()
    {
        databaseEntries.Clear();
        currentEntry = null;
    }

    public override void DrawView()
    {
        RPGBuilderEditorUtility.UpdateViewAndFieldData();

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);

        float panelWidth = RPGBuilderEditorUtility.GetScreenWidth();
        panelWidth -= panelWidth * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent;
        float panelHeight = RPGBuilderEditorUtility.GetScreenHeight();
        Rect panelRect = new Rect(
            RPGBuilderEditorUtility.GetScreenWidth() * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent, 0,
            panelWidth, panelHeight);

        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(panelRect.width),
            GUILayout.MaxWidth(panelRect.width),
            GUILayout.ExpandHeight(true));

        RPGBuilderEditor.Instance.EditorFilters.persistenceSettingsModuleSection.showPersistenceUtility =
            RPGBuilderEditorUtility.HandleModuleBanner("GENERATE IDENTIFIERS",
                RPGBuilderEditor.Instance.EditorFilters.persistenceSettingsModuleSection.showPersistenceUtility);
        if (RPGBuilderEditor.Instance.EditorFilters.persistenceSettingsModuleSection.showPersistenceUtility)
        {
            
            if (!Application.isPlaying)
            {
                GUILayout.Space(10);
                RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

                RPGGameScene currentGameScene = null;
                RPGBuilderEditorModule gameScenesModule = RPGBuilderEditorUtility.GetModuleByName("Game Scenes");
                foreach (var gameScene in gameScenesModule.databaseEntries)
                {
                    if (gameScene.entryName != SceneManager.GetActiveScene().name) continue;
                    currentGameScene = (RPGGameScene) gameScene;
                }

                if (currentGameScene != null)
                {

                    RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin - 25, false);
                    if (GUILayout.Button("Load Persistent Objects",
                        RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(250),
                        GUILayout.ExpandWidth(true)))
                    {
                        persistentObjects = FindObjectsOfType<SaverIdentifier>();
                    }

                    if (persistentObjects != null)
                    {
                        GUILayout.Space(25);
                        if (GUILayout.Button("Generate Unique Identifiers",
                            RPGBuilderEditor.Instance.EditorSkin.GetStyle("GenericButton"), GUILayout.MinWidth(250),
                            GUILayout.ExpandWidth(true)))
                        {
                            foreach (var persistenObject in persistentObjects)
                            {
                                if (persistenObject.GetIdentifier() == "-1")
                                {
                                    persistenObject.GenerateUniqueIdentifier();
                                }
                            }
                        }
                    }

                    RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin - 25, false);

                    if (persistentObjects != null)
                    {
                        GUILayout.Space(10);
                        RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin - 25,
                            false);
                        
                        EditorGUILayout.LabelField("<color=cyan>" +
                            currentGameScene.entryName + "</color> has " + persistentObjects.Length + " persistent objects",
                            RPGBuilderEditor.Instance.EditorSkin.GetStyle("CenteredText"));
                        RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.CenteredMargin - 25,
                            false);
                        GUILayout.Space(5);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("The current scene is not a valid game scene", RPGBuilderEditor.Instance.EditorSkin.GetStyle("BoldText"));
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
            }
            else
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Not available during play mode", RPGBuilderEditor.Instance.EditorSkin.GetStyle("BoldText"));
            }
        }

        GUILayout.Space(10);
        RPGBuilderEditor.Instance.EditorFilters.persistenceSettingsModuleSection.showSpawnedPersistentPrefabs =
            RPGBuilderEditorUtility.HandleModuleBanner("DYNAMIC OBJECTS LIST",
                RPGBuilderEditor.Instance.EditorFilters.persistenceSettingsModuleSection.showSpawnedPersistentPrefabs);
        if (RPGBuilderEditor.Instance.EditorFilters.persistenceSettingsModuleSection.showSpawnedPersistentPrefabs)
        {
            GUILayout.Space(10);
            if (RPGBuilderEditorFields.DrawHorizontalAddButton("Add Prefab", true))
            {
                currentEntry.prefabList.Add(new RPGBuilderPersistenceSettings.PersistentPrefab());
            }
            
            GUILayout.Space(10);
            RPGBuilderEditorUtility.StartHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);

            for (var a = 0; a < currentEntry.prefabList.Count; a++)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                string prefabName = currentEntry.prefabList[a].prefab != null
                    ? currentEntry.prefabList[a].prefab.name
                    : "- Select a Prefab -";
                RPGBuilderEditorFields.DrawTitleLabelExpanded(prefabName, "");
                if (RPGBuilderEditorFields.DrawSmallRemoveButton())
                {
                    currentEntry.prefabList.RemoveAt(a);
                    return;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                currentEntry.prefabList[a].prefab = (GameObject) RPGBuilderEditorFields.DrawHorizontalObject<GameObject>("Prefab", "",
                    currentEntry.prefabList[a].prefab);
                if (EditorGUI.EndChangeCheck() && currentEntry.prefabList[a].prefab != null)
                {
                    if (!PrefabNameIsAllowed(currentEntry.prefabList[a].prefab))
                    {
                        currentEntry.prefabList[a].prefab = null;
                    }
                    if (ContainsPrefab(currentEntry.prefabList[a].prefab, a))
                    {
                        currentEntry.prefabList[a].prefab = null;
                    }
                }

                if (currentEntry.prefabList[a].prefab != null)
                {
                    currentEntry.prefabList[a].name = currentEntry.prefabList[a].prefab.name;
                }
                
                EditorGUI.BeginDisabledGroup(true);
                currentEntry.prefabList[a].ID = RPGBuilderEditorFields.DrawHorizontalIntField("ID", "", 0, currentEntry.prefabList[a].ID);
                EditorGUI.EndDisabledGroup();
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(RPGBuilderEditor.Instance.LongHorizontalMargin, true);
        }

        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }

    public override void OnSave()
    {
        RPGBuilderPersistenceSettings persistenceFile = Resources.Load<RPGBuilderPersistenceSettings>(RPGBuilderEditor.Instance.EditorData.RPGBDatabasePath +
                                                                     AssetFolderName + "/" + EntryType);
        foreach (var prefab in persistenceFile.prefabList)
        {
            if (prefab.prefab != null && prefab.ID == -1)
            {
                prefab.ID = GenerateNewPrefabID();
            }
        }
        EditorUtility.SetDirty(persistenceFile);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        LoadEntries();
    }

    private int GenerateNewPrefabID()
    {
        var prefabID = -1;
        var currentIDFile = DataSavingSystem.LoadAssetID(IDFileName);
        if (currentIDFile != null)
        {
            prefabID = currentIDFile.id;
            prefabID++;
            currentIDFile.id = prefabID;
            DataSavingSystem.SaveAssetID(currentIDFile);
        }
        else
        {
            var file = new AssetIDHandler(IDFileName, 0);
            DataSavingSystem.SaveAssetID(file);
            prefabID = 0;
        }

        return prefabID;
    }

    private bool PrefabNameIsAllowed(GameObject newPrefab)
    {
        foreach (var prefab in currentEntry.prefabList)
        {
            if (prefab.prefab != null && prefab.prefab != newPrefab && prefab.prefab.name == newPrefab.name) return false;
        }

        return true;
    }
    
    private bool ContainsPrefab (GameObject newPrefab, int newIndex)
    {
        for (var index = 0; index < currentEntry.prefabList.Count; index++)
        {
            var prefab = currentEntry.prefabList[index];
            if (prefab.prefab != null && prefab.prefab == newPrefab &&  index != newIndex) return true;
        }

        return false;
    }
    
    private void OnEnable()
    {
        EditorSceneManager.sceneOpened += EditorSceneManager_SceneOpened;
    }
    
    private void OnDestroy()
    {
        EditorSceneManager.sceneOpened -= EditorSceneManager_SceneOpened;
    }
    
    private void EditorSceneManager_SceneOpened(Scene arg0, OpenSceneMode mode)
    {
        persistentObjects = null;
    }
}
