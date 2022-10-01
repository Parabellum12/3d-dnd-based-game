using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public static class GameActionsData
{
    [Serializable]
    public class GameAction
    {
        public GameActionType type;
        public float chance = 100;
        
        [AbilityID] public int AbilityID = -1;
        [BonusID] public int BonusID = -1;
        [RecipeID] public int RecipeID = -1;
        [ResourceID] public int ResourceID = -1;
        [EffectID] public int EffectID = -1;
        [NPCID] public int NPCID = -1;
        [FactionID] public int FactionID = -1;
        [ItemID] public int ItemID = -1;
        [CurrencyID] public int CurrencyID = -1;
        [PointID] public int PointID = -1;
        [TalentTreeID] public int TalentTreeID = -1;
        [SkillID] public int SkillID = -1;
        [WeaponTemplateID] public int WeaponTemplateID = -1;
        [QuestID] public int QuestID = -1;
        [DialogueID] public int DialogueID = -1;
        [GameSceneID] public int GameSceneID = -1;

        public AbilityAction AbilityAction;
        public NodeAction NodeAction;
        public ProgressionType ProgressionType;
        public TreeAction TreeAction;
        public LevelAction LevelAction;
        public EffectAction EffectAction;
        public NPCAction NPCAction;
        public FactionAction FactionAction;
        public AlterAction AlterAction;
        public QuestAction QuestAction;
        public CompletionAction CompletionAction;
        public DialogueAction DialogueAction;
        public GameObjectAction GameObjectAction;
        public CombatStateAction CombatStateAction;
        public TimeAction TimeAction;
        public SpawnTypes SpawnTypes;
        public TeleportType TeleportType;

        public RPGBFactionStance FactionStance;
        public RPGDialogueTextNode DialogueNode;

        public GameObject GameObject;
        public string stringValue1;
        public Vector3 Position;
        public Vector3 Rotation;

        public VisualEffectEntry VisualEffectEntry = new VisualEffectEntry();
        public AnimationEntry AnimationEntry = new AnimationEntry();
        public SoundEntry SoundEntry = new SoundEntry();

        public int Amount;
        public int Amount2;
        public float FloatValue1;
        public bool BoolBalue1;
    }
    
    public enum GameActionType
    {
        Ability = 0,
        Bonus = 1,
        Recipe = 2,
        Resource = 3,
        Effect = 4,
        NPC = 5,
        Faction = 6,
        Item = 7,
        Currency = 8,
        Point = 9,
        Skill = 10,
        TalentTree = 11,
        WeaponTemplate = 12,
        Quest = 13,
        Dialogue = 14,
        DialogueNode = 15,
        CombatState = 16,
        Dismount = 17,
        GameObject = 18,
        TriggerVisualEffect = 19,
        TriggerAnimation = 20,
        TriggerSound = 21,
        Teleport = 22,
        SaveCharacter = 23,
        Death = 24,
        ResetSprint = 25,
        ResetBlocking = 26,
        Time = 27,
    }
    
    public enum AbilityAction
    {
        Trigger = 0,
        RankUp = 1,
        RankDown = 2,
        ResetCooldown = 3,
        StartCooldown = 4,
    }
    
    public enum NodeAction
    {
        RankUp = 0,
        RankDown = 1,
    }
    
    public enum ProgressionType
    {
        Unlock = 0,
        Remove = 1,
        GainLevel = 2,
        LoseLevel = 3,
        GainExperience = 4,
    }

    public enum TreeAction
    {
        Unlock = 0,
        Remove = 1,
    }

    public enum LevelAction
    {
        GainExperience = 0,
        LoseExperience = 1,
        GainLevel = 2,
        LoseLevel = 3,
    }
    
    public enum EffectAction
    {
        Trigger = 0,
        Remove = 1,
    }

    public enum NPCAction
    {
        Spawn = 0,
        Kill = 1,
        TriggerPhase = 2,
        Aggro = 3,
    }

    public enum FactionAction
    {
        ChangeFaction = 0,
        ChangeStance = 1,
        GainPoints = 2,
        ResetPoints = 3,
    }

    public enum AlterAction
    {
        Gain = 0,
        Remove = 1,
    }

    public enum QuestAction
    {
        Propose = 0,
        Abandon = 1,
        Complete = 2,
        Reset = 3,
    }

    public enum CompletionAction
    {
        Complete = 0,
        Reset = 1,
    }

    public enum DialogueAction
    {
        Start = 0,
        End = 1,
    }
    
    public enum GameObjectAction
    {
        Spawn = 0,
        Destroy = 1,
        Deactivate = 2,
    }
    
    public enum SpawnTypes
    {
        Caster = 0,
        Target = 1,
        Position = 2,
    }
    
    public enum CombatStateAction
    {
        Set = 0,
        Invert = 1,
    }
    
    public enum TimeAction
    {
        SetYear = 0,
        SetMonth = 1,
        SetWeek = 2,
        SetDay = 3,
        SetHour = 4,
        SetMinute = 5,
        SetSecond = 6,
        SetGlobalSpeed = 7,
        SetTimeScale = 8,
    }
    
    public enum TeleportType
    {
        GameScene = 0,
        Position = 1,
        Target = 2,
    }
}
