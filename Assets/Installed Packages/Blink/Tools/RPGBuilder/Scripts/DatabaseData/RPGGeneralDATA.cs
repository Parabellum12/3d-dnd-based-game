using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGGeneralDATA : ScriptableObject
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
    public List<string> dialogueKeywordsList = new List<string>();

    public bool useGameModifiers;
    public int negativePointsRequired;
    public bool checkMinNegativeModifier, checkMaxPositiveModifier;
    public int minimumRequiredNegativeGameModifiers;
    public int maximumRequiredPositiveGameModifiers;
    public int baseGameModifierPointsInMenu;
    public int baseGameModifierPointsInWorld;

    
    
    public List<string> ActionKeyCategoryList = new List<string>();
    
    [Serializable]
    public class ActionKey
    {
        public string actionName;
        public string actionDisplayName;
        public KeyCode defaultKey;
        public bool isUnique;
        [HideInInspector] public string category;
        public RPGBActionKeyCategory Category;
    }

    public List<ActionKey> actionKeys = new List<ActionKey>();

    public LayerMask worldInteractableLayer;

    public Texture2D defaultCursor, merchantCursor, questGiverCursor, interactiveObjectCursor, craftingStationCursor, enemyCursor;

    public GameObject characterLevelUpPrefab, skillLevelUpPrefab, weaponTemplateLevelUpPrefab;
    
    public void UpdateThis(RPGGeneralDATA newEntryData)
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
        dialogueKeywordsList = newEntryData.dialogueKeywordsList;
        useGameModifiers = newEntryData.useGameModifiers;
        negativePointsRequired = newEntryData.negativePointsRequired;
        minimumRequiredNegativeGameModifiers = newEntryData.minimumRequiredNegativeGameModifiers;
        maximumRequiredPositiveGameModifiers = newEntryData.maximumRequiredPositiveGameModifiers;
        baseGameModifierPointsInMenu = newEntryData.baseGameModifierPointsInMenu;
        baseGameModifierPointsInWorld = newEntryData.baseGameModifierPointsInWorld;
        checkMinNegativeModifier = newEntryData.checkMinNegativeModifier;
        checkMaxPositiveModifier = newEntryData.checkMaxPositiveModifier;
        actionKeys = newEntryData.actionKeys;
        ActionKeyCategoryList = newEntryData.ActionKeyCategoryList;
        worldInteractableLayer = newEntryData.worldInteractableLayer;
        DelayAfterSceneLoad = newEntryData.DelayAfterSceneLoad;
        LoadingScreenEndDelay = newEntryData.LoadingScreenEndDelay;
        defaultCursor = newEntryData.defaultCursor;
        merchantCursor = newEntryData.merchantCursor;
        questGiverCursor = newEntryData.questGiverCursor;
        interactiveObjectCursor = newEntryData.interactiveObjectCursor;
        craftingStationCursor = newEntryData.craftingStationCursor;
        enemyCursor = newEntryData.enemyCursor;
        characterLevelUpPrefab = newEntryData.characterLevelUpPrefab;
        skillLevelUpPrefab = newEntryData.skillLevelUpPrefab;
        weaponTemplateLevelUpPrefab = newEntryData.weaponTemplateLevelUpPrefab;
    }
}