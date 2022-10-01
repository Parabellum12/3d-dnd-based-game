using UnityEngine;

public class RPGTask : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string displayName;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string description;

    
    public enum TASK_TYPE
    {
        enterScene,
        enterRegion,
        learnAbility,
        learnRecipe,
        killNPC,
        getItem,
        reachLevel,
        reachSkillLevel,
        useItem,
        talkToNPC,
        reachWeaponTemplateLevel,
        killNPCFamily
    }

    public TASK_TYPE taskType;

    public string sceneName;
    [AbilityID] public int abilityToLearnID = -1;
    [NPCID] public int npcToKillID = -1;
    [ItemID] public int itemToGetID = -1;
    public bool keepItems;
    [ClassID] public int classRequiredID = -1;
    [SkillID] public int skillRequiredID = -1;
    [ItemID] public int itemToUseID = -1;
    [NPCID] public int npcToTalkToID = -1;
    [WeaponTemplateID] public int weaponTemplateRequiredID = -1;
    public int taskValue;

    public RPGBNPCFamily NPCFamily;

    public void UpdateEntryData(RPGTask newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        taskType = newEntryData.taskType;
        sceneName = newEntryData.sceneName;
        abilityToLearnID = newEntryData.abilityToLearnID;
        npcToKillID = newEntryData.npcToKillID;
        taskValue = newEntryData.taskValue;
        itemToGetID = newEntryData.itemToGetID;
        classRequiredID = newEntryData.classRequiredID;
        skillRequiredID = newEntryData.skillRequiredID;
        itemToUseID = newEntryData.itemToUseID;
        npcToTalkToID = newEntryData.npcToTalkToID;
        weaponTemplateRequiredID = newEntryData.weaponTemplateRequiredID;
        keepItems = newEntryData.keepItems;
        NPCFamily = newEntryData.NPCFamily;
    }
}