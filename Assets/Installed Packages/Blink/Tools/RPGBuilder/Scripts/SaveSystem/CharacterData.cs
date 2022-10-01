using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using UnityEngine;

namespace BLINK.RPGBuilder.Characters
{
    [System.Serializable]
    public class CharacterData
    {
        [Header("GENERAL")] public bool IsCreated;
        public int RaceID = -1;
        public int ClassID = -1;
        public string CharacterName;
        public string Gender;

        [Header("LEVEL")] public int Level;
        public int CurrentExperience, ExperienceNeeded;
        public int levelTemplateID = -1;

        [Header("ABILITIES")]
        public List<CharacterEntries.AbilityEntry> Abilities = new List<CharacterEntries.AbilityEntry>();

        [Header("RECIPES")]
        public List<CharacterEntries.RecipeEntry> Recipes = new List<CharacterEntries.RecipeEntry>();

        [Header("RESOURCE NODES")]
        public List<CharacterEntries.ResourceNodeEntry> Resources = new List<CharacterEntries.ResourceNodeEntry>();

        [Header("BONUSES")] public List<CharacterEntries.BonusEntry> Bonuses = new List<CharacterEntries.BonusEntry>();

        [Header("TALENT TREES")]
        public List<CharacterEntries.TalentTreeEntry> TalentTrees = new List<CharacterEntries.TalentTreeEntry>();

        [Header("SKILLS")] public List<CharacterEntries.SkillEntry> Skills = new List<CharacterEntries.SkillEntry>();

        [Header("WEAPON TEMPLATES")]
        public List<CharacterEntries.WeaponTemplateEntry> WeaponTemplates =
            new List<CharacterEntries.WeaponTemplateEntry>();

        [Header("FACTIONS")]
        public List<CharacterEntries.FactionEntry> Factions = new List<CharacterEntries.FactionEntry>();

        [Header("INVENTORY")] public CharacterEntries.InventoryData Inventory = new CharacterEntries.InventoryData();

        [Header("CURRENCIES")]
        public List<CharacterEntries.CurrencyEntry> Currencies = new List<CharacterEntries.CurrencyEntry>();

        [Header("EQUIPPED ITEMS")] public List<CharacterEntries.ArmorEquippedEntry>
            ArmorPiecesEquipped = new List<CharacterEntries.ArmorEquippedEntry>();

        public List<CharacterEntries.WeaponEquippedEntry> WeaponsEquipped =
            new List<CharacterEntries.WeaponEquippedEntry>();

        [Header("RANDOMIZED ITEMS")] public int nextAvailableItemID = 0;
        public List<CharacterEntries.ItemEntry> ItemEntries = new List<CharacterEntries.ItemEntry>();
        public int nextAvailableRandomItemID = 0;
        public List<CharacterEntries.RandomizedItem> RandomizedItems = new List<CharacterEntries.RandomizedItem>();

        [Header("STAT ALLOCATION")] public int MainMenuStatAllocationPoints = 0;
        public int MainMenuStatAllocationMaxPoints = 0;
        public List<CharacterEntries.AllocatedStatData> AllocatedStats = new List<CharacterEntries.AllocatedStatData>();

        [Header("POINTS")]
        public List<CharacterEntries.TreePointEntry> Points = new List<CharacterEntries.TreePointEntry>();

        [Header("ACTION ABILITIES")]
        public List<CharacterEntries.ActionAbilityEntry> ActionAbilities =
            new List<CharacterEntries.ActionAbilityEntry>();

        [Header("QUESTS")] public List<CharacterEntries.QuestEntry> Quests = new List<CharacterEntries.QuestEntry>();

        [Header("DIALOGUES")]
        public List<CharacterEntries.DialogueEntry> Dialogues = new List<CharacterEntries.DialogueEntry>();

        [Header("KEYS")]
        public List<CharacterEntries.ActionKeyEntry> ActionKeys = new List<CharacterEntries.ActionKeyEntry>();

        [Header("ACTION BARS")]
        public List<CharacterEntries.ActionBarSlotEntry> ActionBarSlots =
            new List<CharacterEntries.ActionBarSlotEntry>();

        public List<CharacterEntries.ActionBarSlotEntry> ShapeshiftingActionBarSlots =
            new List<CharacterEntries.ActionBarSlotEntry>();

        public List<CharacterEntries.ActionBarSlotEntry> StealthedActionBarSlots =
            new List<CharacterEntries.ActionBarSlotEntry>();

        [Header("GAME MODIFIERS")]
        public List<CharacterEntries.GameModifierEntry> GameModifiers = new List<CharacterEntries.GameModifierEntry>();

        public int MenuGameModifierPoints;
        public int WorldGameModifierPoints;

        [Header("ACHIEVEMENTS")]
        public List<CharacterEntries.NPCKilledEntry> KilledNPCs = new List<CharacterEntries.NPCKilledEntry>();

        public List<CharacterEntries.SceneEnteredEntry> EnteredScenes = new List<CharacterEntries.SceneEnteredEntry>();

        public List<CharacterEntries.RegionEnteredEntry> EnteredRegions =
            new List<CharacterEntries.RegionEnteredEntry>();

        public List<CharacterEntries.AbilityLearnedEntry> LearnedAbilities =
            new List<CharacterEntries.AbilityLearnedEntry>();

        public List<CharacterEntries.BonusLearnedEntry> LearnedBonuses = new List<CharacterEntries.BonusLearnedEntry>();

        public List<CharacterEntries.RecipeLearnedEntry> LearnedRecipes =
            new List<CharacterEntries.RecipeLearnedEntry>();

        public List<CharacterEntries.ResourceNodeLearnedEntry> LearnedResourceNodes =
            new List<CharacterEntries.ResourceNodeLearnedEntry>();

        public List<CharacterEntries.ItemGainedEntry> GainedItems = new List<CharacterEntries.ItemGainedEntry>();

        [Header("WORLD PERSISTENCE")] public int GameSceneEntryIndex = -1;
        public List<CharacterEntries.GameSceneEntry> GameScenes = new List<CharacterEntries.GameSceneEntry>();
        public List<CharacterEntries.StateEntry> States = new List<CharacterEntries.StateEntry>();
        public List<CombatData.VitalityStatEntry> VitalityStats = new List<CombatData.VitalityStatEntry>();

        public CharacterEntries.TimeData Time = new CharacterEntries.TimeData();


    }
}
