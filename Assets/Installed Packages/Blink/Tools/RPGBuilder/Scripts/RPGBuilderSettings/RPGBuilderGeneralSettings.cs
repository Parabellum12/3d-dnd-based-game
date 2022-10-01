using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBuilderGeneralSettings : RPGBuilderDatabaseEntry
{
    public bool automaticSave;
    public float automaticSaveDelay;

    public bool clickToLoadScene;
    public float DelayAfterSceneLoad;
    public float LoadingScreenEndDelay;
    public bool enableDevPanel = true;

    public Sprite mainMenuLoadingImage;
    public string mainMenuSceneName, mainMenuLoadingName, mainMenuLoadingDescription;

    public bool useOldController;
    
    public List<string> ActionKeyCategoryList = new List<string>();
    public List<RPGGeneralDATA.ActionKey> actionKeys = new List<RPGGeneralDATA.ActionKey>();

    public Texture2D defaultCursor, merchantCursor, questGiverCursor, interactiveObjectCursor, craftingStationCursor, enemyCursor;
    
    public enum ControllerTypes
    {
        ThirdPerson,
        ThirdPersonShooter,
        TopDownClickToMove,
        TopDownWASD,
        FirstPerson
    }
    
    public void UpdateEntryData(RPGBuilderGeneralSettings newEntryData)
    {
        automaticSave = newEntryData.automaticSave;
        automaticSaveDelay = newEntryData.automaticSaveDelay;
        clickToLoadScene = newEntryData.clickToLoadScene;
        mainMenuSceneName = newEntryData.mainMenuSceneName;
        mainMenuLoadingImage = newEntryData.mainMenuLoadingImage;
        mainMenuLoadingName = newEntryData.mainMenuLoadingName;
        mainMenuLoadingDescription = newEntryData.mainMenuLoadingDescription;
        enableDevPanel = newEntryData.enableDevPanel;
        useOldController = newEntryData.useOldController;
        actionKeys = newEntryData.actionKeys;
        ActionKeyCategoryList = newEntryData.ActionKeyCategoryList;
        DelayAfterSceneLoad = newEntryData.DelayAfterSceneLoad;
        LoadingScreenEndDelay = newEntryData.LoadingScreenEndDelay;
        defaultCursor = newEntryData.defaultCursor;
        merchantCursor = newEntryData.merchantCursor;
        questGiverCursor = newEntryData.questGiverCursor;
        interactiveObjectCursor = newEntryData.interactiveObjectCursor;
        craftingStationCursor = newEntryData.craftingStationCursor;
        enemyCursor = newEntryData.enemyCursor;
    }
}
