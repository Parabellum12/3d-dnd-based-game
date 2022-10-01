
using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;

public static class RequirementsData
{
    [Serializable]
    public class RequirementGroup
    {
        public List<Requirement> Requirements = new List<Requirement>();
        public bool checkCount;
        public int requiredCount;
    }
    
    [Serializable]
    public class Requirement
    {
        public RequirementType type;
        public Rule condition;

        [AbilityID] public int AbilityID = -1;
        [BonusID] public int BonusID = -1;
        [RecipeID] public int RecipeID = -1;
        [ResourceID] public int ResourceID = -1;
        [EffectID] public int EffectID = -1;
        [NPCID] public int NPCID = -1;
        [StatID] public int StatID = -1;
        [FactionID] public int FactionID = -1;
        [ComboID] public int ComboID = -1;
        [RaceID] public int RaceID = -1;
        [LevelsID] public int LevelsID = -1;
        [ClassID] public int ClassID = -1;
        [SpeciesID] public int SpeciesID = -1;
        [ItemID] public int ItemID = -1;
        [CurrencyID] public int CurrencyID = -1;
        [PointID] public int PointID = -1;
        [TalentTreeID] public int TalentTreeID = -1;
        [SkillID] public int SkillID = -1;
        [SpellbookID] public int SpellbookID = -1;
        [WeaponTemplateID] public int WeaponTemplateID = -1;
        [EnchantmentID] public int EnchantmentID = -1;
        [GearSetID] public int GearSetID = -1;
        [GameSceneID] public int GameSceneID = -1;
        [QuestID] public int QuestID = -1;
        [DialogueID] public int DialogueID = -1;

        public Knowledge Knowledge;
        public State State;
        public Comparison Comparison;
        public Value Value;
        public Ownership Ownership;
        public ItemCondition ItemCondition;
        public Progression Progression;
        public Entity Entity;
        public PointType PointType;
        public DialogueNodeState DialogueNodeState;
        public EffectCondition EffectCondition;
        public AmountType AmountType;
        public TimeType TimeType;
        public TimeValue TimeValue;

        public int Amount1;
        public int Amount2;
        public float Float1;
        public bool Consume;
        public bool BoolBalue1;
        public bool BoolBalue2;
        public bool BoolBalue3;
        public bool IsPercent;

        public RPGBEffectTag EffectTag;
        public RPGEffect.EFFECT_TYPE EffectType;
        public RPGBFactionStance FactionStance;
        public RPGBItemType ItemType;
        public RPGBWeaponType WeaponType;
        public RPGBWeaponHandSlot WeaponSlot;
        public RPGBArmorType ArmorType;
        public RPGBArmorSlot ArmorSlot;
        public RPGBGender Gender;
        public QuestManager.questState QuestState;
        public RPGDialogueTextNode DialogueNode;
        public RPGBNPCFamily NPCFamily;
        public RegionTemplate Region;

        public TimeRequirement TimeRequirement1 = new TimeRequirement();
        public TimeRequirement TimeRequirement2 = new TimeRequirement();
    }
    
    public enum Rule
    {
        Mandatory = 0,
        Optional = 1
    }
    
    public enum RequirementType
    {
        Ability = 0,
        Bonus = 1,
        Recipe = 2,
        Resource = 3,
        Effect = 4,
        NPCKilled = 5,
        NPCFamily = 6,
        Stat = 7,
        StatCost = 8,
        Faction = 9,
        FactionStance = 10,
        Combo = 11,
        Race = 12,
        Level = 13,
        Gender = 14,
        Class = 15,
        Species = 16,
        Item = 17,
        Currency = 18,
        Point = 19,
        TalentTree = 20,
        Skill = 21,
        Spellbook = 22,
        WeaponTemplate = 23,
        Enchantment = 24,
        GearSet = 25,
        GameScene = 26,
        Quest = 27,
        DialogueNode = 28,
        Region = 29,
        CombatState = 30,
        Stealth = 31,
        Mounted = 32,
        Grounded = 33,
        Time = 34,
    }

    public enum Knowledge
    {
        Known = 0,
        Unknown = 1
    }
        
    public enum State
    {
        Active = 0,
        Inactive = 1
    }
        
    public enum Comparison
    {
        Equal = 0,
        Different = 1 
    }
        
    public enum Value
    {
        Equal = 0,
        EqualOrBelow = 1,
        EqualOrAbove = 2,
        Below = 3,
        Above = 4
    }
        
    public enum Ownership
    {
        Owned = 0,
        NotOwned = 1,
        Equipped = 2,
        NotEquipped = 3,
    }
        
    public enum ItemCondition
    {
        Item = 0,
        ItemType = 1,
        WeaponType = 2,
        WeaponSlot = 3,
        ArmorType = 4,
        ArmorSlot = 5
    }
        
    public enum Progression
    {
        Known = 0,
        Unknown = 1,
        Level = 2,
    }
    
    public enum Entity
    {
        Caster = 0,
        Target = 1
    }
    
    public enum AmountType
    {
        Value = 0,
        PercentOfCurrent = 1,
        PercentOfMax = 2,
    }
    
    public enum PointType
    {
        Available = 0,
        Spent = 1,
        Total = 2,
    }
    
    public enum DialogueNodeState
    {
        Viewed = 0,
        Clicked = 1,
        Completed = 2
    }
    
    public enum EffectCondition
    {
        Effect = 0,
        EffectType = 1,
        EffectTag = 2,
    }

    public enum TimeType
    {
        GameTime = 0,
        SystemTime = 1,
        CombatTime
    }
    
    public enum TimeValue
    {
        Equal = 0,
        EqualOrBelow = 1,
        EqualOrAbove = 2,
        Below = 3,
        Above = 4,
        Between = 5
    }

    [Serializable]
    public class TimeRequirement
    {
        public bool CheckYear;
        public bool CheckMonth;
        public bool CheckWeek;
        public bool CheckDay;
        public bool CheckHour;
        public bool CheckMinute;
        public bool CheckSecond;
        public bool CheckGlobalSpeed;
        
        public int Year;
        public int Month;
        public int Week;
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;
        public int GlobalSpeed;
    }
}
