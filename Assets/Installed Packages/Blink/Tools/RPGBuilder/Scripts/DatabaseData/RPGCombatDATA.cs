using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using UnityEngine;

public class RPGCombatDATA : ScriptableObject
{
    public List<string> StatFunctionsList = new List<string>();
    public List<string> UIStatsCategoriesList = new List<string>();
    public List<string> FactionStancesList = new List<string>();
    public List<string> AbilityCooldownTagList = new List<string>();
    public List<string> EffectTagList = new List<string>();
    public List<string> nodeSocketNames = new List<string>();
    
    public float CriticalDamageBonus = 2;
    public float GCDDuration = 0.75f;
    
    public int talentTreesNodePerTierCount = 6;
    public float outOfCombatDuration = 15;

    public bool useClasses = true;
    public bool targetPlayerOnClick = true;

    public bool useAutomaticCombatState = true;
    
    [PointID] public int pointID = -1;
    public bool spendAllStatPointsToCreateChar;
    public bool canDescreaseGameStatPoints;
    
    [StatID] public int healthStatID = -1;

    [StatID] public int sprintStatDrainID = -1;
    public int sprintStatDrainAmount;
    public float sprintStatDrainInterval;
    
    public LayerMask projectileCheckLayer;
    public LayerMask projectileDestroyLayer;
    public LayerMask leapInterruptLayer;
    
    public enum TARGET_TYPE
    {
        Target,
        Caster
    }
    public enum ALIGNMENT_TYPE
    {
        ALLY,
        NEUTRAL,
        ENEMY
    }
    
    
    public enum INTERACTABLE_TYPE
    {
        None,
        AlliedUnit,
        NeutralUnit,
        EnemyUnit,
        InteractableObject,
        CraftingStation,
        LootBag,
        WorldDroppedItem,
        Container
    }

    [System.Serializable]
    public class Faction_Reward_DATA
    {
        [FactionID] public int factionID = -1;

        public int amount;
    }

    [System.Serializable]
    public class ActionAbilityDATA
    {
        public ActionAbilityKeyType keyType;
        public KeyCode key;
        public string actionKeyName;
        [AbilityID] public int abilityID = -1;
    }

    public enum ActionAbilityKeyType
    {
        OverrideKey,
        ActionKey
    }

    public enum CombatVisualActivationType
    {
        Activate,
        CastCompleted,
        Completed,
        Interrupted
    }
    
    [System.Serializable]
    public class CombatVisualEffect
    {
        public CombatVisualActivationType activationType;
        public GameObject EffectGO;
        [HideInInspector] public string SocketName;
        public RPGBNodeSocket NodeSocket;
        public bool UseNodeSocket;
        public bool ParentedToCaster;
        public AudioClip Sound;
        public bool SoundParentedToEffect;
        public Vector3 positionOffset;
        public Vector3 effectScale = Vector3.one;
        public float duration = 5, delay;
        public bool isDestroyedOnDeath = true;
        public bool isDestroyedOnStun = true;
        public bool isDestroyedOnStealth;
        public bool isDestroyedOnStealthEnd;
    }
    
    public enum CombatVisualAnimationParameterType
    {
        Bool,
        Int,
        Float,
        Trigger
    }
    
    [System.Serializable]
    public class CombatVisualAnimation
    {
        public CombatVisualActivationType activationType;
        public CombatVisualAnimationParameterType parameterType;
        public string animationParameter;
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public float duration = 1;
        public float delay = 0;
        public bool showWeapons;
        public float showWeaponDuration;
    }
    
    public void UpdateEntryData(RPGCombatDATA newEntryData)
    {
        CriticalDamageBonus = newEntryData.CriticalDamageBonus;
        healthStatID = newEntryData.healthStatID;
        outOfCombatDuration = newEntryData.outOfCombatDuration;
        useClasses = newEntryData.useClasses;
        useAutomaticCombatState = newEntryData.useAutomaticCombatState;
        pointID = newEntryData.pointID;
        spendAllStatPointsToCreateChar = newEntryData.spendAllStatPointsToCreateChar;
        canDescreaseGameStatPoints = newEntryData.canDescreaseGameStatPoints;
        talentTreesNodePerTierCount = newEntryData.talentTreesNodePerTierCount;
        
        StatFunctionsList = newEntryData.StatFunctionsList;
        UIStatsCategoriesList = newEntryData.UIStatsCategoriesList;
        FactionStancesList = newEntryData.FactionStancesList;
        nodeSocketNames = newEntryData.nodeSocketNames;
        GCDDuration = newEntryData.GCDDuration;
        AbilityCooldownTagList = newEntryData.AbilityCooldownTagList;
        EffectTagList = newEntryData.EffectTagList;
        
        targetPlayerOnClick = newEntryData.targetPlayerOnClick;
        
        sprintStatDrainID = newEntryData.sprintStatDrainID;
        sprintStatDrainInterval = newEntryData.sprintStatDrainInterval;
        sprintStatDrainAmount = newEntryData.sprintStatDrainAmount;
        
        projectileCheckLayer = newEntryData.projectileCheckLayer;
        projectileDestroyLayer = newEntryData.projectileDestroyLayer;
        leapInterruptLayer = newEntryData.leapInterruptLayer;
    }
}