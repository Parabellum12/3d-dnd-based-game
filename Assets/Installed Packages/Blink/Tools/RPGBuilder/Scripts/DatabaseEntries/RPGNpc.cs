using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using UnityEngine.AI;

public class RPGNpc : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public Sprite icon;
    
    public enum NPC_TYPE
    {
        MOB,
        RARE,
        BOSS,
        MERCHANT,
        BANK,
        QUEST_GIVER,
        DIALOGUE
    }

    public NPC_TYPE npcType;

    public GameObject AILogicTemplate;

    public List<AIData.AIPhase> Phases = new List<AIData.AIPhase>();
    public bool ResetPhaseAfterCombat = true;

    public bool InstantlyHealAfterCombat = true;
    
    public bool isDummyTarget;

    [FactionID] public int factionID = -1;
    
    [SpeciesID] public int speciesID = -1;
    
    [MerchantTableID] public int merchantTableID = -1;
    [RPGDataList] public List<AIData.NPCMerchantTable> MerchantTables = new List<AIData.NPCMerchantTable>();
    
    [DialogueID] public int dialogueID = -1;

    [Serializable]
    public class NPC_QUEST_DATA
    {
        public int questID = -1;
    }

    [RPGDataList] public List<NPC_QUEST_DATA> questGiven = new List<NPC_QUEST_DATA>();
    [RPGDataList] public List<NPC_QUEST_DATA> questCompleted = new List<NPC_QUEST_DATA>();

    public float MinRespawn = 60;
    public float MaxRespawn = 120;
    
    public float corpseDespawnTime = 15;

    public int MinEXP = 1;
    public int MaxEXP = 2;
    public float LowerLevelEXPModifier;
    public float HigherLevelEXPModifier;
    public int EXPBonusPerLevel;
    public GameObject lootBagPrefab;
    public float LootBagDuration = 60;

    public List<RPGCombatDATA.Faction_Reward_DATA> factionRewards = new List<RPGCombatDATA.Faction_Reward_DATA>();

    public int MinLevel = 1;
    public int MaxLevel = 2;
    public bool isScalingWithPlayer;


    [Serializable]
    public class NPC_STATS_DATA
    {
        [StatID] public int statID = -1;
        public float minValue;
        public float maxValue;
        public float baseValue;
        public float bonusPerLevel;
        
        [RPGDataList] public List<RPGStat.VitalityActions> vitalityActions = new List<RPGStat.VitalityActions>();
    }
    [RPGDataList] public List<NPC_STATS_DATA> stats = new List<NPC_STATS_DATA>();
    
    [RPGDataList] public List<CombatData.CustomStatValues> CustomStats = new List<CombatData.CustomStatValues>();
    public bool UseStatListTemplate;
    public StatListTemplate StatListTemplate;

    [Serializable]
    public class LOOT_TABLES
    {
        [LootTableID] public int lootTableID = -1;
        public float dropRate = 100f;
    }

    [RPGDataList] public List<LOOT_TABLES> lootTables = new List<LOOT_TABLES>();

    public bool isCombatEnabled = true;
    public bool isMovementEnabled = true;
    public bool isCollisionEnabled;
    public bool isTargetable = true;
    public bool isNameplateEnabled = true;
    public bool isPlayerInteractable = true;
    public bool isMerchant;
    public bool isQuestGiver;
    public bool isDialogue;

    public string MerchantText = "Show me your goods.";
    public string QuestText = "Can I do something for you?";
    public string DialogueText = "Tell me your story.";
    
    // PREFAB DATA
    public GameObject NPCVisual;
    public Vector3 modelPosition, modelScale = Vector3.one;

    public float nameplateYOffset = 1.5f;
    public string RendererName;
    
    public RuntimeAnimatorController animatorController;
    public Avatar animatorAvatar;

    public bool animatorUseRootMotion;
    public AnimatorUpdateMode animatorUpdateMode = AnimatorUpdateMode.Normal;
    public AnimatorCullingMode AnimatorCullingMode = AnimatorCullingMode.AlwaysAnimate;

    public float navmeshAgentRadius = 0.5f, navmeshAgentHeight = 2f, navmeshAgentAngularSpeed = 400;
    public ObstacleAvoidanceType navmeshObstacleAvoidance;
    
    public enum NPCColliderType
    {
        Capsule,
        Sphere,
        Box
    }

    public NPCColliderType colliderType;

    public Vector3 colliderCenter, colliderSize;
    public float colliderRadius = 1, colliderHeight = 2;
    
    [Serializable]
    public class NPC_AGGRO_LINK
    {
        public AIData.AggroLinkType type;
        [NPCID] public int npcID = -1;
        public RPGBNPCFamily npcFamily;
        public float maxDistance;
    }

    [RPGDataList] public List<NPC_AGGRO_LINK> aggroLinks = new List<NPC_AGGRO_LINK>();

    public bool SetLayer;
    public bool SetTag;
    public int GameObjectLayer;
    public string GameObjectTag = "Untagged";

    public RPGBNPCFamily npcFamily;
    
    public void UpdateEntryData(RPGNpc newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        CustomStats = newEntryData.CustomStats;
        UseStatListTemplate = newEntryData.UseStatListTemplate;
        StatListTemplate = newEntryData.StatListTemplate;
        isDummyTarget = newEntryData.isDummyTarget;
        lootTables = newEntryData.lootTables;
        aggroLinks = newEntryData.aggroLinks;
        MinRespawn = newEntryData.MinRespawn;
        MaxRespawn = newEntryData.MaxRespawn;
        npcType = newEntryData.npcType;
        MinEXP = newEntryData.MinEXP;
        MaxEXP = newEntryData.MaxEXP;
        LowerLevelEXPModifier = newEntryData.LowerLevelEXPModifier;
        HigherLevelEXPModifier = newEntryData.HigherLevelEXPModifier;
        EXPBonusPerLevel = newEntryData.EXPBonusPerLevel;
        lootBagPrefab = newEntryData.lootBagPrefab;
        MinLevel = newEntryData.MinLevel;
        MaxLevel = newEntryData.MaxLevel;
        isScalingWithPlayer = newEntryData.isScalingWithPlayer;
        MerchantTables = newEntryData.MerchantTables;
        questGiven = newEntryData.questGiven;
        questCompleted = newEntryData.questCompleted;
        isCombatEnabled = newEntryData.isCombatEnabled;
        isMovementEnabled = newEntryData.isMovementEnabled;
        isCollisionEnabled = newEntryData.isCollisionEnabled;
        factionID = newEntryData.factionID;
        factionRewards = newEntryData.factionRewards;
        dialogueID = newEntryData.dialogueID;
        speciesID = newEntryData.speciesID;
        NPCVisual = newEntryData.NPCVisual;
        nameplateYOffset = newEntryData.nameplateYOffset;
        RendererName = newEntryData.RendererName;
        animatorController = newEntryData.animatorController;
        animatorAvatar = newEntryData.animatorAvatar;
        animatorUseRootMotion = newEntryData.animatorUseRootMotion;
        animatorUpdateMode = newEntryData.animatorUpdateMode;
        AnimatorCullingMode = newEntryData.AnimatorCullingMode;
        navmeshAgentRadius = newEntryData.navmeshAgentRadius;
        navmeshAgentHeight = newEntryData.navmeshAgentHeight;
        navmeshAgentAngularSpeed = newEntryData.navmeshAgentAngularSpeed;
        navmeshObstacleAvoidance = newEntryData.navmeshObstacleAvoidance;
        colliderType = newEntryData.colliderType;
        colliderCenter = newEntryData.colliderCenter;
        colliderSize = newEntryData.colliderSize;
        colliderRadius = newEntryData.colliderRadius;
        colliderHeight = newEntryData.colliderHeight;
        modelPosition = newEntryData.modelPosition;
        modelScale = newEntryData.modelScale;
        corpseDespawnTime = newEntryData.corpseDespawnTime;
        isTargetable = newEntryData.isTargetable;
        isNameplateEnabled = newEntryData.isNameplateEnabled;
        isPlayerInteractable = newEntryData.isPlayerInteractable;
        isMerchant = newEntryData.isMerchant;
        isQuestGiver = newEntryData.isQuestGiver;
        isDialogue = newEntryData.isDialogue;
        MerchantText = newEntryData.MerchantText;
        QuestText = newEntryData.QuestText;
        DialogueText = newEntryData.DialogueText;
        AILogicTemplate = newEntryData.AILogicTemplate;
        Phases = newEntryData.Phases;
        GameObjectLayer = newEntryData.GameObjectLayer;
        GameObjectTag = newEntryData.GameObjectTag;
        SetLayer = newEntryData.SetLayer;
        SetTag = newEntryData.SetTag;
        npcFamily = newEntryData.npcFamily;
        ResetPhaseAfterCombat = newEntryData.ResetPhaseAfterCombat;
        InstantlyHealAfterCombat = newEntryData.InstantlyHealAfterCombat;
        LootBagDuration = newEntryData.LootBagDuration;
    }
}