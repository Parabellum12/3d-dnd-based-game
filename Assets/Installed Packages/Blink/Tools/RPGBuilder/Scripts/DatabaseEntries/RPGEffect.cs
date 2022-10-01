using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using UnityEngine.Events;

public class RPGEffect : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public Sprite icon;

    public enum EFFECT_TYPE
    {
        InstantDamage,
        InstantHeal,
        DamageOverTime,
        HealOverTime,
        Stat,
        Stun,
        Sleep,
        Immune,
        Shapeshifting,
        Dispel,
        Teleport,
        Taunt,
        Root,
        Silence,
        Pet,
        RollLootTable,
        Knockback,
        Motion,
        Blocking,
        Flying,
        Stealth,
        Mount,
        GameAction
    }

    public enum MainDamageTypes
    {
        Neutral,
        Physical,
        Magical
    }

    [Serializable]
    public class STAT_EFFECTS_DATA
    {
        [StatID] public int statID = -1;
        public float statEffectModification;
        public bool isPercent;
    }

    public enum TELEPORT_TYPE
    {
        gameScene,
        position,
        target,
        directional
    }

    public enum TELEPORT_DIRECTIONAL_TYPE
    {
        forward,
        left,
        right,
        backward,
        diagonalTopLeft,
        diagonalTopRight,
        diagonalBackwardLeft,
        diagonalBackwardRight
    }

    public EFFECT_TYPE effectType;
    [HideInInspector] public string effectTag;
    public RPGBEffectTag EffectTag;
    public bool isState, isBuffOnSelf;
    public int stackLimit = 1;
    public bool allowMultiple, allowMixedCaster;
    public int pulses = 1;
    public float duration;
    public bool endless;
    public bool canBeManuallyRemoved = true;
    public bool showUIState = true;
    public bool IsPersistent;

    public enum BLOCK_DURATION_TYPE
    {
        Time,
        HoldKey
    }

    public enum BLOCK_END_TYPE
    {
        HitCount,
        MaxDamageBlocked,
        Stat
    }

    public enum DISPEL_TYPE
    {
        EffectType,
        EffectTag,
        Effect
    }

    [Serializable]
    public class RPGEffectRankData
    {
        public bool ShowedInEditor;

        public MainDamageTypes mainDamageType;
        [HideInInspector] public string secondaryDamageType;
        public RPGBDamageType customDamageType;
        public RPGBHealingType customHealingType;

        public int Damage;
        [StatID] public int alteredStatID = -1;
        public bool FlatCalculation;
        public bool CannotCrit;
        public bool removeStealth = true;
        public float skillModifier, weaponDamageModifier;
        public bool useWeapon1Damage = true, useWeapon2Damage = true;
        [SkillID] public int skillModifierID = -1;
        public float lifesteal;
        public float maxHealthModifier;
        public float missingHealthModifier;
        public float UltimateGain;
        public float delay;

        public int projectilesReflectedCount;

        [EffectID] public int requiredEffectID = -1;
        public float requiredEffectDamageModifier;

        [StatID] public int damageStatID = -1;
        public float damageStatModifier;
        public RPGAbility.COST_TYPES hitValueType;

        public TELEPORT_TYPE teleportType;
        [GameSceneID] public int gameSceneID = -1;
        public Vector3 teleportPOS;
        public TELEPORT_DIRECTIONAL_TYPE teleportDirectionalType;
        public float teleportDirectionalDistance;
        public LayerMask teleportDirectionalBlockLayers;

        [LootTableID] public int lootTableID = -1;

        [NPCID] public int petNPCDataID = -1;
        public float petDuration = 60;
        public int petSPawnCount = 1;
        public bool petScaleWithCharacter;

        public float knockbackDistance;

        public float motionDistance = 5;
        public float motionSpeed = 0.5f;
        public Vector3 motionDirection;
        public bool motionIgnoreUseCondition, isImmuneDuringMotion;

        public bool blockAnyDamage = true,
            blockPhysicalDamage,
            blockMagicalDamage,
            isBlockChargeTime,
            isBlockLimitedDuration,
            isBlockPowerDecay,
            isBlockKnockback,
            blockStatDecay;

        public float blockChargeTime = 0.5f,
            blockDuration = 1,
            blockPowerModifier = 100,
            blockPowerDecay = 0.1f,
            blockAngle = 90f,
            blockStatDecayInterval = 1;

        public int blockPowerFlat, blockHitCount = 1, blockMaxDamage, blockStatDecayAmount = 1;
        public BLOCK_DURATION_TYPE blockDurationType;
        public BLOCK_END_TYPE blockEndType;
        [StatID] public int blockStatID = -1;

        [RPGDataList] public List<string> blockedDamageTypes = new List<string>();
        [RPGDataList] public List<RPGBDamageType> blockedCustomDamageTypes = new List<RPGBDamageType>();


        public DISPEL_TYPE dispelType;
        public EFFECT_TYPE dispelEffectType;
        [HideInInspector] public string dispelEffectTag;
        public RPGBEffectTag DispelEffectTag;
        [EffectID] public int dispelEffectID = -1;

        public GameObject shapeshiftingModel;
        public Vector3 shapeshiftingmodelPosition, shapeshiftingmodelScale = Vector3.one;
        public RuntimeAnimatorController shapeshiftingAnimatorController;
        public RuntimeAnimatorController shapeshiftingAnimatorControllerCombat;
        public Avatar shapeshiftingAnimatorAvatar;
        public bool shapeshiftingAnimatorUseRootMotion;
        public AnimatorUpdateMode shapeshiftingAnimatorUpdateMode = AnimatorUpdateMode.Normal;
        public AnimatorCullingMode shapeshiftingAnimatorCullingMode = AnimatorCullingMode.AlwaysAnimate;
        public bool shapeshiftingOverrideMainActionBar, canCameraAim;
        [RPGDataList] public List<int> shapeshiftingActiveAbilities = new List<int>();
        public bool shapeshiftingNoActionAbilities = true;

        public GameObject MountPrefab;
        public Vector3 MountPosition, MountScale = Vector3.one;
        public Vector3 RiderPosition, RiderRotation;
        public bool ReParentCharacterArmature;
        public string RiderReParentName;
        public bool CanUseAbilitiesMounted;
        public string MountAnimationParameter;
        public RuntimeAnimatorController MountAnimatorController;
        public Avatar MountAvatar;
        public bool mountCanAim;


        public bool showStealthActionBar = true;

        [RPGDataList]
        public List<RPGAbility.AbilityEffectsApplied> nestedEffects = new List<RPGAbility.AbilityEffectsApplied>();


        [RPGDataList] public List<VisualEffectEntry> VisualEffectEntries = new List<VisualEffectEntry>();
        [RPGDataList] public List<AnimationEntry> AnimationEntries = new List<AnimationEntry>();
        [RPGDataList] public List<SoundEntry> SoundEntries = new List<SoundEntry>();


        [RPGDataList]
        public List<RPGCombatDATA.CombatVisualEffect> visualEffects = new List<RPGCombatDATA.CombatVisualEffect>();

        [RPGDataList]
        public List<RPGCombatDATA.CombatVisualAnimation> visualAnimations =
            new List<RPGCombatDATA.CombatVisualAnimation>();

        [RPGDataList] public List<STAT_EFFECTS_DATA> statEffectsData = new List<STAT_EFFECTS_DATA>();
        [RPGDataList] public List<GameActionsData.GameAction> GameActions = new List<GameActionsData.GameAction>();
        public bool UseGameActionsTemplate;
        public GameActionsTemplate GameActionsTemplate;
    }

    [RPGDataList] public List<RPGEffectRankData> ranks = new List<RPGEffectRankData>();


    public void UpdateEntryData(RPGEffect newEntryData)
    {
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;

        ranks = newEntryData.ranks;
        effectType = newEntryData.effectType;
        EffectTag = newEntryData.EffectTag;
        isState = newEntryData.isState;
        isBuffOnSelf = newEntryData.isBuffOnSelf;
        endless = newEntryData.endless;
        canBeManuallyRemoved = newEntryData.canBeManuallyRemoved;
        stackLimit = newEntryData.stackLimit;
        allowMultiple = newEntryData.allowMultiple;
        allowMixedCaster = newEntryData.allowMixedCaster;
        pulses = newEntryData.pulses;
        duration = newEntryData.duration;
        showUIState = newEntryData.showUIState;
        IsPersistent = newEntryData.IsPersistent;
    }

    public void CopyEntryData(RPGEffectRankData original, RPGEffectRankData copied)
    {

        original.mainDamageType = copied.mainDamageType;
        original.customDamageType = copied.customDamageType;
        original.customHealingType = copied.customHealingType;
        original.Damage = copied.Damage;
        original.lifesteal = copied.lifesteal;
        original.maxHealthModifier = copied.maxHealthModifier;
        original.missingHealthModifier = copied.missingHealthModifier;
        original.UltimateGain = copied.UltimateGain;
        original.delay = copied.delay;

        original.teleportType = copied.teleportType;
        original.gameSceneID = copied.gameSceneID;
        original.teleportPOS = copied.teleportPOS;
        original.teleportDirectionalType = copied.teleportDirectionalType;
        original.teleportDirectionalDistance = copied.teleportDirectionalDistance;
        original.teleportDirectionalBlockLayers = copied.teleportDirectionalBlockLayers;

        original.petNPCDataID = copied.petNPCDataID;
        original.petDuration = copied.petDuration;
        original.petSPawnCount = copied.petSPawnCount;
        original.projectilesReflectedCount = copied.projectilesReflectedCount;
        original.petScaleWithCharacter = copied.petScaleWithCharacter;

        original.knockbackDistance = copied.knockbackDistance;

        original.motionDistance = copied.motionDistance;
        original.motionSpeed = copied.motionSpeed;
        original.motionDirection = copied.motionDirection;
        original.isImmuneDuringMotion = copied.isImmuneDuringMotion;
        original.motionIgnoreUseCondition = copied.motionIgnoreUseCondition;

        original.blockChargeTime = copied.blockChargeTime;
        original.blockDuration = copied.blockDuration;
        original.blockHitCount = copied.blockHitCount;
        original.blockPowerFlat = copied.blockPowerFlat;
        original.blockPowerModifier = copied.blockPowerModifier;
        original.blockPowerDecay = copied.blockPowerDecay;
        original.blockAngle = copied.blockAngle;
        original.blockedCustomDamageTypes = copied.blockedCustomDamageTypes;
        original.blockAnyDamage = copied.blockAnyDamage;
        original.blockPhysicalDamage = copied.blockPhysicalDamage;
        original.blockMagicalDamage = copied.blockMagicalDamage;
        original.isBlockChargeTime = copied.isBlockChargeTime;
        original.isBlockLimitedDuration = copied.isBlockLimitedDuration;
        original.isBlockPowerDecay = copied.isBlockPowerDecay;
        original.isBlockKnockback = copied.isBlockKnockback;
        original.blockDurationType = copied.blockDurationType;
        original.blockEndType = copied.blockEndType;
        original.blockMaxDamage = copied.blockMaxDamage;
        original.blockStatID = copied.blockStatID;
        original.blockStatDecay = copied.blockStatDecay;
        original.blockStatDecayAmount = copied.blockStatDecayAmount;
        original.blockStatDecayInterval = copied.blockStatDecayInterval;

        original.dispelType = copied.dispelType;
        original.DispelEffectTag = copied.DispelEffectTag;
        original.dispelEffectType = copied.dispelEffectType;
        original.dispelEffectID = copied.dispelEffectID;

        original.skillModifier = copied.skillModifier;
        original.skillModifierID = copied.skillModifierID;

        original.alteredStatID = copied.alteredStatID;
        original.FlatCalculation = copied.FlatCalculation;
        original.weaponDamageModifier = copied.weaponDamageModifier;
        original.useWeapon1Damage = copied.useWeapon1Damage;
        original.useWeapon2Damage = copied.useWeapon2Damage;

        original.CannotCrit = copied.CannotCrit;
        original.removeStealth = copied.removeStealth;

        original.requiredEffectID = copied.requiredEffectID;
        original.requiredEffectDamageModifier = copied.requiredEffectDamageModifier;

        original.damageStatID = copied.damageStatID;
        original.damageStatModifier = copied.damageStatModifier;
        original.hitValueType = copied.hitValueType;

        original.lootTableID = copied.lootTableID;

        original.shapeshiftingModel = copied.shapeshiftingModel;
        original.shapeshiftingmodelPosition = copied.shapeshiftingmodelPosition;
        original.shapeshiftingmodelScale = copied.shapeshiftingmodelScale;
        original.shapeshiftingAnimatorController = copied.shapeshiftingAnimatorController;
        original.shapeshiftingAnimatorControllerCombat = copied.shapeshiftingAnimatorControllerCombat;
        original.shapeshiftingAnimatorAvatar = copied.shapeshiftingAnimatorAvatar;
        original.shapeshiftingAnimatorUseRootMotion = copied.shapeshiftingAnimatorUseRootMotion;
        original.shapeshiftingNoActionAbilities = copied.shapeshiftingNoActionAbilities;
        original.shapeshiftingAnimatorUpdateMode = copied.shapeshiftingAnimatorUpdateMode;
        original.shapeshiftingAnimatorCullingMode = copied.shapeshiftingAnimatorCullingMode;
        original.shapeshiftingOverrideMainActionBar = copied.shapeshiftingOverrideMainActionBar;
        original.canCameraAim = copied.canCameraAim;
        original.shapeshiftingActiveAbilities = copied.shapeshiftingActiveAbilities;
        original.showStealthActionBar = copied.showStealthActionBar;

        original.MountPrefab = copied.MountPrefab;
        original.MountPosition = copied.MountPosition;
        original.MountScale = copied.MountScale;
        original.RiderPosition = copied.RiderPosition;
        original.RiderRotation = copied.RiderRotation;
        original.CanUseAbilitiesMounted = copied.CanUseAbilitiesMounted;
        original.MountAnimatorController = copied.MountAnimatorController;
        original.MountAnimationParameter = copied.MountAnimationParameter;
        original.MountAvatar = copied.MountAvatar;
        original.ReParentCharacterArmature = copied.ReParentCharacterArmature;
        original.RiderReParentName = copied.RiderReParentName;

        original.nestedEffects = new List<RPGAbility.AbilityEffectsApplied>();
        for (var index = 0; index < copied.nestedEffects.Count; index++)
        {
            RPGAbility.AbilityEffectsApplied newRef = new RPGAbility.AbilityEffectsApplied();
            newRef.chance = copied.nestedEffects[index].chance;
            newRef.target = copied.nestedEffects[index].target;
            newRef.effectID = copied.nestedEffects[index].effectID;
            newRef.effectRank = copied.nestedEffects[index].effectRank;
            newRef.isSpread = copied.nestedEffects[index].isSpread;
            newRef.spreadDistanceMax = copied.nestedEffects[index].spreadDistanceMax;
            newRef.spreadUnitMax = copied.nestedEffects[index].spreadUnitMax;
            original.nestedEffects.Add(newRef);
        }

        original.VisualEffectEntries = new List<VisualEffectEntry>();
        for (var index = 0; index < copied.VisualEffectEntries.Count; index++)
        {
            VisualEffectEntry newVisualEffectEntry = new VisualEffectEntry
            {
                ActivationType = copied.VisualEffectEntries[index].ActivationType,
                Template = copied.VisualEffectEntries[index].Template,
                NodeSocket = copied.VisualEffectEntries[index].NodeSocket,
                ParentedToCaster = copied.VisualEffectEntries[index].ParentedToCaster,
                UseNodeSocket = copied.VisualEffectEntries[index].UseNodeSocket,
                Duration = copied.VisualEffectEntries[index].Duration,
                Delay = copied.VisualEffectEntries[index].Delay,
                Scale = copied.VisualEffectEntries[index].Scale,
                PositionOffset = copied.VisualEffectEntries[index].PositionOffset,
            };

            original.VisualEffectEntries.Add(newVisualEffectEntry);
        }

        original.AnimationEntries = new List<AnimationEntry>();
        for (var index = 0; index < copied.AnimationEntries.Count; index++)
        {
            AnimationEntry newAnimationEntry = new AnimationEntry
            {
                ActivationType = copied.AnimationEntries[index].ActivationType,
                Template = copied.AnimationEntries[index].Template,
                Delay = copied.AnimationEntries[index].Delay,
                ShowWeapons = copied.AnimationEntries[index].ShowWeapons,
                ShowWeaponsDuration = copied.AnimationEntries[index].ShowWeaponsDuration,
                ModifySpeed = copied.AnimationEntries[index].ModifySpeed,
                ModifierSpeed = copied.AnimationEntries[index].ModifierSpeed,
                SpeedParameterName = copied.AnimationEntries[index].SpeedParameterName,
            };
            original.AnimationEntries.Add(newAnimationEntry);
        }

        original.SoundEntries = new List<SoundEntry>();
        for (var index = 0; index < copied.SoundEntries.Count; index++)
        {
            SoundEntry newAnimationEntry = new SoundEntry
            {
                ActivationType = copied.SoundEntries[index].ActivationType,
                Template = copied.SoundEntries[index].Template,
                Delay = copied.SoundEntries[index].Delay,
                Parented = copied.SoundEntries[index].Parented,
            };
            original.SoundEntries.Add(newAnimationEntry);
        }

        original.statEffectsData = new List<STAT_EFFECTS_DATA>();
        for (var index = 0; index < copied.statEffectsData.Count; index++)
        {
            STAT_EFFECTS_DATA newRef = new STAT_EFFECTS_DATA();
            newRef.isPercent = copied.statEffectsData[index].isPercent;
            newRef.statEffectModification = copied.statEffectsData[index].statEffectModification;
            newRef.statID = copied.statEffectsData[index].statID;
            original.statEffectsData.Add(newRef);
        }

        original.UseGameActionsTemplate = copied.UseGameActionsTemplate;
        original.GameActionsTemplate = copied.GameActionsTemplate;
        original.GameActions = new List<GameActionsData.GameAction>();
        for (var index = 0; index < copied.GameActions.Count; index++)
        {
            GameActionsData.GameAction newAction = new GameActionsData.GameAction
            {
                type = copied.GameActions[index].type,
                AbilityID = copied.GameActions[index].AbilityID,
                BonusID = copied.GameActions[index].BonusID,
                RecipeID = copied.GameActions[index].RecipeID,
                ResourceID = copied.GameActions[index].ResourceID,
                EffectID = copied.GameActions[index].EffectID,
                NPCID = copied.GameActions[index].NPCID,
                FactionID = copied.GameActions[index].FactionID,
                ItemID = copied.GameActions[index].ItemID,
                CurrencyID = copied.GameActions[index].CurrencyID,
                PointID = copied.GameActions[index].PointID,
                TalentTreeID = copied.GameActions[index].TalentTreeID,
                SkillID = copied.GameActions[index].SkillID,
                WeaponTemplateID = copied.GameActions[index].WeaponTemplateID,
                QuestID = copied.GameActions[index].QuestID,
                DialogueID = copied.GameActions[index].DialogueID,
                AbilityAction = copied.GameActions[index].AbilityAction,
                NodeAction = copied.GameActions[index].NodeAction,
                ProgressionType = copied.GameActions[index].ProgressionType,
                TreeAction = copied.GameActions[index].TreeAction,
                LevelAction = copied.GameActions[index].LevelAction,
                EffectAction = copied.GameActions[index].EffectAction,
                NPCAction = copied.GameActions[index].NPCAction,
                FactionAction = copied.GameActions[index].FactionAction,
                AlterAction = copied.GameActions[index].AlterAction,
                QuestAction = copied.GameActions[index].QuestAction,
                CompletionAction = copied.GameActions[index].CompletionAction,
                DialogueAction = copied.GameActions[index].DialogueAction,
                GameObjectAction = copied.GameActions[index].GameObjectAction,
                CombatStateAction = copied.GameActions[index].CombatStateAction,
                TimeAction = copied.GameActions[index].TimeAction,
                Amount = copied.GameActions[index].Amount,
                BoolBalue1 = copied.GameActions[index].BoolBalue1,
            };
            original.GameActions.Add(newAction);
        }
    }
}