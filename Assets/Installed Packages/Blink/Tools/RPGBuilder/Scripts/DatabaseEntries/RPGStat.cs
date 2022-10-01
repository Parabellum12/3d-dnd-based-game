using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGStat : RPGBuilderDatabaseEntry
{
    [Header("-----BASE DATA-----")]
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public string description;
    
    
    public bool minCheck;
    public float minValue;
    public bool maxCheck;
    public float maxValue;
    public float baseValue;
    
    public bool isShiftingInSprint = true;
    public bool isShiftingInBlock = true;
    public bool isShiftingOutsideCombat;
    public bool isShiftingInCombat;
    public float shiftAmountOutsideCombat;
    public float shiftIntervalOutsideCombat;
    public float shiftAmountInCombat;
    public float shiftIntervalInCombat;

    public float startPercentage = 100;
    
    public bool isPercentStat;
    public bool IsPersistent;
    
    public enum VitalityActionsValueType
    {
        Equal,
        EqualOrBelow,
        EqualOrAbove,
        Below,
        Above
    }
    
    [Serializable]
    public class VitalityActions
    {
        public float value;
        public VitalityActionsValueType valueType;
        public bool isPercent;

        public GameActionsTemplate GameActionsTemplate;
    }
    [RPGDataList] public List<VitalityActions> vitalityActions = new List<VitalityActions>();
    
    [Serializable]
    public class OnHitEffectsData
    {
        [EffectID] public int effectID = -1;
        public int effectRank;
        public RPGCombatDATA.TARGET_TYPE targetType;
        public RPGAbility.ABILITY_TAGS tagType;
        public float chance = 100f;
    }
    
    [RPGDataList] public List<OnHitEffectsData> onHitEffectsData = new List<OnHitEffectsData>();


    public bool isVitalityStat;
    [Serializable]
    public class StatBonusData
    {
        public STAT_TYPE statType;
        public float modifyValue = 1;
        public RPGEffect.MainDamageTypes MainDamageType;
        public RPGBDamageType CustomDamageType;
        public RPGBHealingType CustomHealingType;
        [StatID] public int ResistanceStatID = -1;
        [StatID] public int PenetrationStatID = -1;
        [StatID] public int statID = -1;
    }
    [RPGDataList] public List<StatBonusData> statBonuses = new List<StatBonusData>();
    
    public enum STAT_TYPE
    {
        NONE,
        DAMAGE,
        RESISTANCE,
        PENETRATION,
        HEALING,
        ABSORBTION,
        CC_POWER,
        CC_RESISTANCE,
        DMG_TAKEN,
        DMG_DEALT,
        HEAL_RECEIVED,
        HEAL_DONE,
        CAST_SPEED,
        CRIT_CHANCE,
        BASE_DAMAGE_TYPE,
        BASE_RESISTANCE_TYPE,
        SUMMON_COUNT,
        CD_RECOVERY_SPEED,
        GLOBAL_HEALING,
        LIFESTEAL,
        THORN,
        BLOCK_CHANCE,
        BLOCK_FLAT,
        BLOCK_MODIFIER,
        DODGE_CHANCE,
        GLANCING_BLOW_CHANCE,
        CRIT_POWER,
        DOT_BONUS,
        HOT_BONUS,
        HEALTH_ON_HIT,
        HEALTH_ON_KILL,
        HEALTH_ON_BLOCK,
        EFFECT_TRIGGER,
        LOOT_CHANCE_MODIFIER,
        EXPERIENCE_MODIFIER,
        VITALITY_REGEN,
        MINION_DAMAGE,
        MINION_PHYSICAL_DAMAGE,
        MINION_MAGICAL_DAMAGE,
        MINION_HEALTH,
        MINION_CRIT_CHANCE,
        MINION_CRIT_POWER,
        MINION_DURATION,
        MINION_LIFESTEAL,
        MINION_THORN,
        MINION_DODGE_CHANCE,
        MINION_GLANCING_BLOW_CHANCE,
        MINION_HEALTH_ON_HIT,
        MINION_HEALTH_ON_KILL,
        MINION_HEALTH_ON_BLOCK,
        PROJECTILE_SPEED,
        PROJECTILE_ANGLE_SPREAD,
        PROJECTILE_RANGE,
        PROJECTILE_COUNT,
        AOE_RADIUS,
        ABILITY_MAX_HIT,
        ABILITY_TARGET_MAX_RANGE,
        ABILITY_TARGET_MIN_RANGE,
        ATTACK_SPEED,
        BODY_SCALE,
        GCD_DURATION,
        BLOCK_ACTIVE_ANGLE,
        BLOCK_ACTIVE_COUNT,
        BLOCK_ACTIVE_CHARGE_TIME_SPEED_MODIFIER,
        BLOCK_ACTIVE_DURATION_MODIFIER,
        BLOCK_ACTIVE_DECAY_MODIFIER,
        BLOCK_ACTIVE_FLAT_AMOUNT,
        BLOCK_ACTIVE_PERCENT_AMOUNT,
        MOVEMENT_SPEED,
        VITALITY_BONUS
    }

    [HideInInspector] public string StatUICategory;
    public RPGBStatCategory StatCategory;
    
    public void UpdateEntryData(RPGStat newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        isShiftingInSprint = newEntryData.isShiftingInSprint;
        isShiftingInBlock = newEntryData.isShiftingInBlock;
        isShiftingInCombat = newEntryData.isShiftingInCombat;
        isShiftingOutsideCombat = newEntryData.isShiftingOutsideCombat;
        shiftAmountInCombat = newEntryData.shiftAmountInCombat;
        shiftIntervalInCombat = newEntryData.shiftIntervalInCombat;
        shiftAmountOutsideCombat = newEntryData.shiftAmountOutsideCombat;
        shiftIntervalOutsideCombat = newEntryData.shiftIntervalOutsideCombat;
        minValue = newEntryData.minValue;
        maxValue = newEntryData.maxValue;
        baseValue = newEntryData.baseValue;
        StatCategory = newEntryData.StatCategory;
        minCheck = newEntryData.minCheck;
        maxCheck = newEntryData.maxCheck;
        onHitEffectsData = newEntryData.onHitEffectsData;
        isPercentStat = newEntryData.isPercentStat;
        vitalityActions = newEntryData.vitalityActions;
        statBonuses = newEntryData.statBonuses;
        isVitalityStat = newEntryData.isVitalityStat;
        startPercentage = newEntryData.startPercentage;
        IsPersistent = newEntryData.IsPersistent;
    }
}