using System.Collections.Generic;
using UnityEngine;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.WorldPersistence;

public class CharacterEntries
{
    [System.Serializable]
    public class AllocatedStatEntry
    {
        public int statID = -1;
        public int maxValue = -1;
        public int cost = 1;
        public int valueAdded = 1;
    }

    [System.Serializable]
    public class AllocatedStatData
    {
        public string statName;
        public int statID = -1;
        public int value;
        public int maxValue;
        public int valueGame;
        public int maxValueGame;
    }

    [System.Serializable]
    public class ActionKeyEntry
    {
        public string actionKeyName;
        public KeyCode currentKey;
    }

    [System.Serializable]
    public class WeaponTemplateEntry
    {
        public int weaponTemplateID;
        public int currentWeaponLevel;
        public int currentWeaponXP, maxWeaponXP;
    }

    [System.Serializable]
    public class TalentTreeEntry
    {
        public int treeID;
        public int pointsSpent;

        public List<TalentTreeNodeEntry> nodes = new List<TalentTreeNodeEntry>();
    }

    [System.Serializable]
    public class TalentTreeNodeEntry
    {
        public RPGTalentTree.Node_DATA nodeData;
    }

    [System.Serializable]
    public class AbilityEntry
    {
        public string name;
        public int ID;
        public bool known;
        public int rank;
        public float NextTimeUse, CDLeft;
        public bool comboActive;
    }

    [System.Serializable]
    public class RecipeEntry
    {
        public string name;
        public int ID;
        public bool known;
        public int rank;
    }

    [System.Serializable]
    public class ResourceNodeEntry
    {
        public string name;
        public int ID;
        public bool known;
        public int rank;
    }

    [System.Serializable]
    public class BonusEntry
    {
        public string name;
        public int ID;
        public bool known;
        public int rank;
        public bool On;
    }

    [System.Serializable]
    public class GameModifierEntry
    {
        public string name;
        public int ID;
        public bool On;
    }

    [System.Serializable]
    public class FactionEntry
    {
        public string name;
        public int ID;
        [HideInInspector] public string currentStance;
        public int stanceIndex;
        public int currentPoint;
    }

    public enum ActionBarSlotContentType
    {
        None,
        Ability,
        Item
    }

    public enum ActionBarType
    {
        Main,
        Extra
    }

    public class AbilityCDState
    {
        public float NextUse;
        public float CDLeft;
        public bool canUseDuringGCD;
    }

    [System.Serializable]
    public class ActionBarSlotEntry
    {
        public ActionBarSlotContentType contentType;
        public ActionBarType slotType;
        public int ID = -1;
    }

    [System.Serializable]
    public class SkillEntry
    {
        public int skillID;
        public int currentSkillLevel;
        public int currentSkillXP, maxSkillXP;
    }

    [System.Serializable]
    public class InventoryBagEntry
    {
        public int bagItemID;
        public List<InventorySlotEntry> slots = new List<InventorySlotEntry>();

    }

    [System.Serializable]
    public class InventorySlotEntry
    {
        public int itemID;
        public int itemStack;
        public int itemDataID = -1;
    }

    [System.Serializable]
    public class InventoryData
    {
        public List<InventorySlotEntry> baseSlots = new List<InventorySlotEntry>();
        public List<InventoryBagEntry> bags = new List<InventoryBagEntry>();
    }

    [System.Serializable]
    public class ArmorEquippedEntry
    {
        public int itemID = -1;
        public int itemDataID = -1;
    }

    [System.Serializable]
    public class WeaponEquippedEntry
    {
        public int itemID = -1;
        public int itemDataID = -1;
    }

    [System.Serializable]
    public class QuestEntry
    {
        public int questID;
        public QuestManager.questState state;

        public List<QuestObjectiveEntry> objectives = new List<QuestObjectiveEntry>();
    }

    [System.Serializable]
    public class QuestObjectiveEntry
    {
        public int taskID;
        public QuestManager.questObjectiveState state;
        public int currentProgressValue, maxProgressValue;
    }

    [System.Serializable]
    public class TreePointEntry
    {
        public int treePointID;
        public int amount;
    }

    [System.Serializable]
    public class CurrencyEntry
    {
        public int currencyID;
        public int amount;
    }

    [System.Serializable]
    public class NPCKilledEntry
    {
        public int npcID;
        public int killedAmount;
    }

    [System.Serializable]
    public class SceneEnteredEntry
    {
        public string sceneName;
    }

    [System.Serializable]
    public class RegionEnteredEntry
    {
        public string regionName;
    }

    [System.Serializable]
    public class AbilityLearnedEntry
    {
        public int abilityID;
    }

    [System.Serializable]
    public class BonusLearnedEntry
    {
        public int bonusID;
    }

    [System.Serializable]
    public class RecipeLearnedEntry
    {
        public int recipeID;
    }

    [System.Serializable]
    public class ResourceNodeLearnedEntry
    {
        public int resourceNodeID;
    }

    [System.Serializable]
    public class ItemGainedEntry
    {
        public int itemID;
    }

    [System.Serializable]
    public class ItemEntry
    {
        public string itemName = "";
        public int itemID = -1;
        public int id = -1;
        public int rdmItemID = -1;
        public ItemEntryState state;

        // Enchanting Data
        public int enchantmentID = -1;
        public int enchantmentTierIndex = -1;

        // Socketing Data
        public List<ItemSocketEntry> sockets = new List<ItemSocketEntry>();
    }

    [System.Serializable]
    public class ItemSocketEntry
    {
        [HideInInspector] public string socketType;
        public string GemSocketType;
        public int gemItemID = -1;
    }

    public enum ItemEntryState
    {
        InWorld,
        InBag,
        Equipped
    }

    [System.Serializable]
    public class RandomizedItem
    {
        public string itemName = "";
        public int itemID = -1;
        public int id = 0;
        public List<RPGItemDATA.RandomizedStat> randomStats = new List<RPGItemDATA.RandomizedStat>();
    }

    [System.Serializable]
    public class ActionAbilityEntry
    {
        public ActionAbilityEntryType entryType;
        public int sourceID;
        public RPGAbility ability;
        public RPGCombatDATA.ActionAbilityKeyType keyType;
        public KeyCode key;
        public string actionKeyName;
        public float NextTimeUse, CDLeft;
    }

    public enum ActionAbilityEntryType
    {
        Race,
        Skill,
        Class,
        Item
    }

    [System.Serializable]
    public class DialogueEntry
    {
        public int ID;
        public List<DialogueNodeEntry> nodes = new List<DialogueNodeEntry>();
    }


    [System.Serializable]
    public class DialogueNodeEntry
    {
        public RPGDialogueTextNode textNode;
        public int currentlyViewedCount;
        public int currentlyClickedCount;
        public bool lineCompleted;
    }

    [System.Serializable]
    public class GameSceneEntry
    {
        public string GameSceneName;
        public int GameSceneID;

        public Vector3 LastPosition;
        public Vector3 LastRotation;

        public List<TransformSaverTemplate> SavedTransforms = new List<TransformSaverTemplate>();
        public List<AnimatorSaverTemplate> SavedAnimators = new List<AnimatorSaverTemplate>();
        public List<RigidbodySaverTemplate> SavedRigidbodies = new List<RigidbodySaverTemplate>();
        public List<ColliderSaverTemplate> SavedColliders = new List<ColliderSaverTemplate>();

        public List<InteractableObjectSaverTemplate> SavedInteractableObjects =
            new List<InteractableObjectSaverTemplate>();

        public List<NPCSpawnerSaverTemplate> SavedNPCSpawners = new List<NPCSpawnerSaverTemplate>();
        public List<ContainerObjectSaverTemplate> SavedContainerObjects = new List<ContainerObjectSaverTemplate>();
        public List<string> DestroyedObjects = new List<string>();
    }

    [System.Serializable]
    public class StateEntry
    {
        public string EffectName;
        public int EffectID;
        public int EffectRank;
        public int maxPulses;
        public int curPulses;
        public float nextPulse;
        public float pulseInterval;
        public float stateMaxDuration;
        public float stateCurDuration;
        public int curStack;
        public int maxStack;
    }

    [System.Serializable]
    public class TimeData
    {
        public int CurrentYear;
        public int CurrentMonth;
        public int CurrentWeek;
        public int CurrentDay;
        public int CurrentHour;
        public int CurrentMinute;
        public int CurrentSecond;
        public float GlobalSpeed;
    }
}
