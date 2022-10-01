using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class GameDatabase : MonoBehaviour
    {
        public static GameDatabase Instance { get; private set; }
        
        // EDITOR DATA
        private RPGBuilderEditorDATA editorDATA;
        
        // SETTINGS
        private RPGBuilderCharacterSettings CharacterSettings;
        public RPGBuilderCharacterSettings GetCharacterSettings()
        {
            return CharacterSettings;
        }
        
        private RPGBuilderCombatSettings CombatSettings;
        public RPGBuilderCombatSettings GetCombatSettings()
        {
            return CombatSettings;
        }
        
        private RPGBuilderEconomySettings EconomySettings;
        public RPGBuilderEconomySettings GetEconomySettings()
        {
            return EconomySettings;
        }
        
        private RPGBuilderUISettings UISettings;
        public RPGBuilderUISettings GetUISettings()
        {
            return UISettings;
        }
        
        private RPGBuilderGeneralSettings GeneralSettings;
        public RPGBuilderGeneralSettings GetGeneralSettings()
        {
            return GeneralSettings;
        }
        
        private RPGBuilderProgressionSettings ProgressionSettings;
        public RPGBuilderProgressionSettings GetProgressionSettings()
        {
            return ProgressionSettings;
        }
        
        private RPGBuilderWorldSettings WorldSettings;
        public RPGBuilderWorldSettings GetWorldSettings()
        {
            return WorldSettings;
        }
        
        private RPGBuilderPersistenceSettings PersistenceSettings;

        public RPGBuilderPersistenceSettings GetPersistenceSettings()
        {
            return PersistenceSettings;
        }

        // COMBAT
        private Dictionary<int, RPGAbility> Abilities = new Dictionary<int, RPGAbility>();
        public Dictionary<int, RPGAbility> GetAbilities()
        {
            return Abilities;
        }
        
        private Dictionary<int, RPGEffect> Effects = new Dictionary<int, RPGEffect>();
        public Dictionary<int, RPGEffect> GetEffects()
        {
            return Effects;
        }
        
        private Dictionary<int, RPGNpc> NPCs = new Dictionary<int, RPGNpc>();
        public Dictionary<int, RPGNpc> GetNPCs()
        {
            return NPCs;
        }

        private Dictionary<int, RPGStat> Stats = new Dictionary<int, RPGStat>();
        public Dictionary<int, RPGStat> GetStats()
        {
            return Stats;
        }

        private Dictionary<int, RPGTreePoint> TreePoints = new Dictionary<int, RPGTreePoint>();
        public Dictionary<int, RPGTreePoint> GetPoints()
        {
            return TreePoints;
        }

        private Dictionary<int, RPGSpellbook> Spellbooks = new Dictionary<int, RPGSpellbook>();
        public Dictionary<int, RPGSpellbook> GetSpellbooks()
        {
            return Spellbooks;
        }
        
        private Dictionary<int, RPGFaction> Factions = new Dictionary<int, RPGFaction>();
        public Dictionary<int, RPGFaction> GetFactions()
        {
            return Factions;
        }
        
        private Dictionary<int, RPGWeaponTemplate> WeaponTemplates = new Dictionary<int, RPGWeaponTemplate>();
        public Dictionary<int, RPGWeaponTemplate> GetWeaponTemplates()
        {
            return WeaponTemplates;
        }
        
        private Dictionary<int, RPGSpecies> Species = new Dictionary<int, RPGSpecies>();
        public Dictionary<int, RPGSpecies> GetSpecies()
        {
            return Species;
        }
        
        private Dictionary<int, RPGCombo> Combos = new Dictionary<int, RPGCombo>();
        public Dictionary<int, RPGCombo> GetCombos()
        {
            return Combos;
        }
        

        // GENERAL
        private Dictionary<int, RPGItem> Items = new Dictionary<int, RPGItem>();
        public Dictionary<int, RPGItem> GetItems()
        {
            return Items;
        }
        
        private Dictionary<int, RPGSkill> Skills = new Dictionary<int, RPGSkill>();
        public Dictionary<int, RPGSkill> GetSkills()
        {
            return Skills;
        }
        
        private Dictionary<int, RPGLevelsTemplate> Levels = new Dictionary<int, RPGLevelsTemplate>();
        public Dictionary<int, RPGLevelsTemplate> GetLevels()
        {
            return Levels;
        }
        
        private Dictionary<int, RPGRace> Races = new Dictionary<int, RPGRace>();
        public Dictionary<int, RPGRace> GetRaces()
        {
            return Races;
        }
        
        private Dictionary<int, RPGClass> Classes = new Dictionary<int, RPGClass>();
        public Dictionary<int, RPGClass> GetClasses()
        {
            return Classes;
        }
        
        private Dictionary<int, RPGLootTable> LootTables = new Dictionary<int, RPGLootTable>();
        public Dictionary<int, RPGLootTable> GetLootTables()
        {
            return LootTables;
        }
        
        private Dictionary<int, RPGMerchantTable> MerchantTables = new Dictionary<int, RPGMerchantTable>();
        public Dictionary<int, RPGMerchantTable> GetMerchantTables()
        {
            return MerchantTables;
        }
        
        private Dictionary<int, RPGCurrency> Currencies = new Dictionary<int, RPGCurrency>();
        public Dictionary<int, RPGCurrency> GetCurrencies()
        {
            return Currencies;
        }
        
        private Dictionary<int, RPGCraftingRecipe> CraftingRecipes = new Dictionary<int, RPGCraftingRecipe>();
        public Dictionary<int, RPGCraftingRecipe> GetRecipes()
        {
            return CraftingRecipes;
        }
        
        private Dictionary<int, RPGCraftingStation> CraftingStation = new Dictionary<int, RPGCraftingStation>();
        public Dictionary<int, RPGCraftingStation> GetCraftingStations()
        {
            return CraftingStation;
        }
        
        private Dictionary<int, RPGTalentTree> TalentTrees = new Dictionary<int, RPGTalentTree>();
        public Dictionary<int, RPGTalentTree> GetTalentTrees()
        {
            return TalentTrees;
        }
        
        private Dictionary<int, RPGBonus> Bonuses = new Dictionary<int, RPGBonus>();
        public Dictionary<int, RPGBonus> GetBonuses()
        {
            return Bonuses;
        }
        
        private Dictionary<int, RPGGearSet> GearSets = new Dictionary<int, RPGGearSet>();
        public Dictionary<int, RPGGearSet> GetGearSets()
        {
            return GearSets;
        }
        
        private Dictionary<int, RPGEnchantment> Enchantments = new Dictionary<int, RPGEnchantment>();
        public Dictionary<int, RPGEnchantment> GetEnchantments()
        {
            return Enchantments;
        }
        

        // WORLD
        private Dictionary<int, RPGTask> Tasks = new Dictionary<int, RPGTask>();
        public Dictionary<int, RPGTask> GetTasks()
        {
            return Tasks;
        }
        
        private Dictionary<int, RPGQuest> Quests = new Dictionary<int, RPGQuest>();
        public Dictionary<int, RPGQuest> GetQuests()
        {
            return Quests;
        }
        
        private Dictionary<int, RPGWorldPosition> WorldPositions = new Dictionary<int, RPGWorldPosition>();
        public Dictionary<int, RPGWorldPosition> GetWorldPositions()
        {
            return WorldPositions;
        }
        
        private Dictionary<int, RPGResourceNode> ResourceNodes = new Dictionary<int, RPGResourceNode>();
        public Dictionary<int, RPGResourceNode> GetResources()
        {
            return ResourceNodes;
        }
        
        private Dictionary<int, RPGGameScene> GameScenes = new Dictionary<int, RPGGameScene>();
        public Dictionary<int, RPGGameScene> GetGameScenes()
        {
            return GameScenes;
        }
        
        private Dictionary<int, RPGDialogue> Dialogues = new Dictionary<int, RPGDialogue>();
        public Dictionary<int, RPGDialogue> GetDialogues()
        {
            return Dialogues;
        }
        
        private Dictionary<int, RPGGameModifier> GameModifiers = new Dictionary<int, RPGGameModifier>();
        public Dictionary<int, RPGGameModifier> GetGameModifiers()
        {
            return GameModifiers;
        }

        private Dictionary<string, RPGBGender> Genders = new Dictionary<string, RPGBGender>();
        public Dictionary<string, RPGBGender> GetGenders()
        {
            return Genders;
        }
        
        Dictionary<string, RPGBArmorSlot> ArmorSlots = new Dictionary<string, RPGBArmorSlot>();
        public Dictionary<string, RPGBArmorSlot> GetArmorSlots()
        {
            return ArmorSlots;
        }
        
        Dictionary<string, RPGBWeaponSlot> WeaponSlots = new Dictionary<string, RPGBWeaponSlot>();
        public Dictionary<string, RPGBWeaponSlot> GetWeaponSlots()
        {
            return WeaponSlots;
        }
        
        Dictionary<string, RPGBFactionStance> FactionStances = new Dictionary<string, RPGBFactionStance>();
        public Dictionary<string, RPGBFactionStance> GetFactionStances()
        {
            return FactionStances;
        }
        
        Dictionary<string, RPGBTextKeyword> TextKeywords = new Dictionary<string, RPGBTextKeyword>();
        public Dictionary<string, RPGBTextKeyword> GetTextKeywords()
        {
            return TextKeywords;
        }
        
        Dictionary<string, RPGBGemSocketType> GemSocketTypes = new Dictionary<string, RPGBGemSocketType>();
        public Dictionary<string, RPGBGemSocketType> GetGemSocketTypes()
        {
            return GemSocketTypes;
        }
        
        Dictionary<string, RPGBStatCategory> StatCategories = new Dictionary<string, RPGBStatCategory>();
        public Dictionary<string, RPGBStatCategory> GetStatCategories()
        {
            return StatCategories;
        }
        
        Dictionary<string, RPGBActionKeyCategory> ActionKeyCategories = new Dictionary<string, RPGBActionKeyCategory>();
        public Dictionary<string, RPGBActionKeyCategory> GetActionKeyCategories()
        {
            return ActionKeyCategories;
        }
        
        // CACHED REFERENCES
        private RPGStat HealthStat;
        public RPGStat GetHealthStat()
        {
            return HealthStat;
        }
        
        private RPGStat SprintDrainStat;
        public RPGStat GetSprintDrainStat()
        {
            return SprintDrainStat;
        }
        
        
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
            
            LoadGameData();
        }

        private void LoadGameData()
        {
            editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
            
            CharacterSettings = Resources.Load<RPGBuilderCharacterSettings>(editorDATA.RPGBDatabasePath + "Settings/Character_Settings");
            CombatSettings = Resources.Load<RPGBuilderCombatSettings>(editorDATA.RPGBDatabasePath + "Settings/Combat_Settings");
            EconomySettings = Resources.Load<RPGBuilderEconomySettings>(editorDATA.RPGBDatabasePath + "Settings/Economy_Settings");
            UISettings = Resources.Load<RPGBuilderUISettings>(editorDATA.RPGBDatabasePath + "Settings/UI_Settings");
            GeneralSettings = Resources.Load<RPGBuilderGeneralSettings>(editorDATA.RPGBDatabasePath + "Settings/General_Settings");
            ProgressionSettings = Resources.Load<RPGBuilderProgressionSettings>(editorDATA.RPGBDatabasePath + "Settings/Progression_Settings");
            WorldSettings = Resources.Load<RPGBuilderWorldSettings>(editorDATA.RPGBDatabasePath + "Settings/World_Settings");
            PersistenceSettings = Resources.Load<RPGBuilderPersistenceSettings>(editorDATA.RPGBDatabasePath + "Settings/Persistence_Settings");
            
            Abilities = Resources.LoadAll<RPGAbility>(  editorDATA.RPGBDatabasePath + "Abilities").ToDictionary(t => t.ID, t => t);
            Effects = Resources.LoadAll<RPGEffect>(editorDATA.RPGBDatabasePath + "Effects").ToDictionary(t => t.ID, t => t);
            NPCs = Resources.LoadAll<RPGNpc>(editorDATA.RPGBDatabasePath + "NPCs").ToDictionary(t => t.ID, t => t);
            Stats = Resources.LoadAll<RPGStat>(editorDATA.RPGBDatabasePath + "Stats").ToDictionary(t => t.ID, t => t);
            TreePoints = Resources.LoadAll<RPGTreePoint>(editorDATA.RPGBDatabasePath + "Points").ToDictionary(t => t.ID, t => t);
            Spellbooks = Resources.LoadAll<RPGSpellbook>(editorDATA.RPGBDatabasePath + "Spellbooks").ToDictionary(t => t.ID, t => t);
            Factions = Resources.LoadAll<RPGFaction>(editorDATA.RPGBDatabasePath + "Factions").ToDictionary(t => t.ID, t => t);
            WeaponTemplates = Resources.LoadAll<RPGWeaponTemplate>(editorDATA.RPGBDatabasePath + "WeaponTemplates").ToDictionary(t => t.ID, t => t);
            Species = Resources.LoadAll<RPGSpecies>(editorDATA.RPGBDatabasePath + "Species").ToDictionary(t => t.ID, t => t);
            Combos = Resources.LoadAll<RPGCombo>(editorDATA.RPGBDatabasePath + "Combos").ToDictionary(t => t.ID, t => t);
            Items = Resources.LoadAll<RPGItem>(editorDATA.RPGBDatabasePath + "Items").ToDictionary(t => t.ID, t => t);
            Skills = Resources.LoadAll<RPGSkill>(editorDATA.RPGBDatabasePath + "Skills").ToDictionary(t => t.ID, t => t);
            Levels = Resources.LoadAll<RPGLevelsTemplate>(editorDATA.RPGBDatabasePath + "Levels").ToDictionary(t => t.ID, t => t);
            Races = Resources.LoadAll<RPGRace>(editorDATA.RPGBDatabasePath + "Races").ToDictionary(t => t.ID, t => t);
            Classes = Resources.LoadAll<RPGClass>(editorDATA.RPGBDatabasePath + "Classes").ToDictionary(t => t.ID, t => t);
            LootTables = Resources.LoadAll<RPGLootTable>(editorDATA.RPGBDatabasePath + "LootTables").ToDictionary(t => t.ID, t => t);
            MerchantTables = Resources.LoadAll<RPGMerchantTable>(editorDATA.RPGBDatabasePath + "MerchantTables").ToDictionary(t => t.ID, t => t);
            Currencies = Resources.LoadAll<RPGCurrency>(editorDATA.RPGBDatabasePath + "Currencies").ToDictionary(t => t.ID, t => t);
            CraftingRecipes = Resources.LoadAll<RPGCraftingRecipe>(editorDATA.RPGBDatabasePath + "Recipes").ToDictionary(t => t.ID, t => t);
            CraftingStation = Resources.LoadAll<RPGCraftingStation>(editorDATA.RPGBDatabasePath + "CraftingStations").ToDictionary(t => t.ID, t => t);
            TalentTrees = Resources.LoadAll<RPGTalentTree>(editorDATA.RPGBDatabasePath + "TalentTrees").ToDictionary(t => t.ID, t => t);
            Bonuses = Resources.LoadAll<RPGBonus>(editorDATA.RPGBDatabasePath + "Bonuses").ToDictionary(t => t.ID, t => t);
            GearSets = Resources.LoadAll<RPGGearSet>(editorDATA.RPGBDatabasePath + "GearSets").ToDictionary(t => t.ID, t => t);
            Enchantments = Resources.LoadAll<RPGEnchantment>(editorDATA.RPGBDatabasePath + "Enchantments").ToDictionary(t => t.ID, t => t);
            Tasks = Resources.LoadAll<RPGTask>(editorDATA.RPGBDatabasePath + "Tasks").ToDictionary(t => t.ID, t => t);
            Quests = Resources.LoadAll<RPGQuest>(editorDATA.RPGBDatabasePath + "Quests").ToDictionary(t => t.ID, t => t);
            WorldPositions = Resources.LoadAll<RPGWorldPosition>(editorDATA.RPGBDatabasePath + "Coordinates").ToDictionary(t => t.ID, t => t);
            ResourceNodes = Resources.LoadAll<RPGResourceNode>(editorDATA.RPGBDatabasePath + "ResourceNodes").ToDictionary(t => t.ID, t => t);
            GameScenes = Resources.LoadAll<RPGGameScene>(editorDATA.RPGBDatabasePath + "GameScenes").ToDictionary(t => t.ID, t => t);
            Dialogues = Resources.LoadAll<RPGDialogue>(editorDATA.RPGBDatabasePath + "Dialogues").ToDictionary(t => t.ID, t => t);
            GameModifiers = Resources.LoadAll<RPGGameModifier>(editorDATA.RPGBDatabasePath + "GameModifiers").ToDictionary(t => t.ID, t => t);
            
            Genders = Resources.LoadAll<RPGBGender>(editorDATA.RPGBDatabasePath + "Types").ToDictionary(t => t.entryName, t => t);
            ArmorSlots = Resources.LoadAll<RPGBArmorSlot>(editorDATA.RPGBDatabasePath + "Types").ToDictionary(t => t.entryName, t => t);
            WeaponSlots = Resources.LoadAll<RPGBWeaponSlot>(editorDATA.RPGBDatabasePath + "Types").ToDictionary(t => t.entryName, t => t);
            FactionStances = Resources.LoadAll<RPGBFactionStance>(editorDATA.RPGBDatabasePath + "Types").ToDictionary(t => t.entryName, t => t);
            TextKeywords = Resources.LoadAll<RPGBTextKeyword>(editorDATA.RPGBDatabasePath + "Types").ToDictionary(t => t.entryName, t => t);
            GemSocketTypes = Resources.LoadAll<RPGBGemSocketType>(editorDATA.RPGBDatabasePath + "Types").ToDictionary(t => t.entryName, t => t);
            StatCategories = Resources.LoadAll<RPGBStatCategory>(editorDATA.RPGBDatabasePath + "Types").ToDictionary(t => t.entryName, t => t);
            ActionKeyCategories = Resources.LoadAll<RPGBActionKeyCategory>(editorDATA.RPGBDatabasePath + "Types").ToDictionary(t => t.entryName, t => t);

            HealthStat = GetStats()[CombatSettings.HealthStatID];
            if(CharacterSettings.SprintStatDrainID != -1) SprintDrainStat = GetStats()[CharacterSettings.SprintStatDrainID];
        }
        
    }
}
