using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.World;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BLINK.RPGBuilder.LogicMono
{
    public class RPGBuilderEssentials : MonoBehaviour
    {
        private static RPGBuilderEssentials instance;
        
        public Canvas mainGameCanvas;
        private float nextAutomaticSave = 0;

        public bool isInGame;
    
        public static RPGBuilderEssentials Instance => instance;
        
        private void Start()
        {
            mainGameCanvas.enabled = false;
        }

        private void Awake()
        {
            if (instance != null) return;
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    
        public Scene getCurrentScene()
        {
            return SceneManager.GetActiveScene();
        }

        public void TeleportToGameScene(int GameSceneID, Vector3 teleportPosition)
        {
            int sceneIndex = Character.Instance.GetGameSceneIndexByID(GameSceneID);
            if (sceneIndex == -1)
            {
                CharacterEntries.GameSceneEntry gameSceneEntry = new CharacterEntries.GameSceneEntry
                {
                    GameSceneName = GameDatabase.Instance.GetGameScenes()[GameSceneID].entryName, GameSceneID = GameSceneID
                };
                Character.Instance.CharacterData.GameScenes.Add(gameSceneEntry);
                Character.Instance.CharacterData.GameSceneEntryIndex = Character.Instance.GetGameSceneIndexByID(GameSceneID);
                Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].LastPosition = teleportPosition;
            }
            else
            {
                Character.Instance.CharacterData.GameScenes[sceneIndex].LastPosition = teleportPosition;
            }
            LoadingScreenManager.Instance.LoadGameScene(GameSceneID);
        }

        public void HandleDATAReset ()
        {
            GameEvents.Instance.OnBackToMainMenu();
            
            GameState.playerEntity.ResetTarget();
            GameState.playerEntity = null;
            
            GameState.combatEntities.Clear();

            UIEvents.Instance.OnSetCursorToDefault();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private bool isGameScene(string sceneName)
        {
            foreach (var gameScene in GameDatabase.Instance.GetGameScenes().Values)
            {
                if (gameScene.entryName == sceneName) return true;
            }

            return false;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(InitializeGameState(scene));
        }

        public void LoadCharacterInstantly()
        {
            StartCoroutine(InitializeGameState(SceneManager.GetActiveScene()));
        }
        
        private IEnumerator InitializeGameState(Scene scene)
        {
            yield return new WaitForSeconds(GameDatabase.Instance.GetGeneralSettings().DelayAfterSceneLoad);
            if (isGameScene(scene.name))
            {
                mainGameCanvas.enabled = true;
                
                RPGGameScene gameScene = RPGBuilderUtilities.GetGameSceneFromName(scene.name);
                Character.Instance.CharacterData.GameSceneEntryIndex = Character.Instance.GetCurrentGameSceneIndex();
                Vector3 spawnPosition = Vector3.zero;
                Vector3 spawnRotation = Vector3.zero;
                if (Character.Instance.CharacterData.GameSceneEntryIndex == -1)
                {
                    CharacterEntries.GameSceneEntry gameSceneEntry = new CharacterEntries.GameSceneEntry
                    {
                        GameSceneName = gameScene.entryName, GameSceneID = gameScene.ID
                    };
                    Character.Instance.CharacterData.GameScenes.Add(gameSceneEntry);
                    Character.Instance.CharacterData.GameSceneEntryIndex = Character.Instance.GetCurrentGameSceneIndex();
                    
                    if (gameScene.isProceduralScene && !string.IsNullOrEmpty(gameScene.SpawnPointName))
                    {
                        GameObject spawnPoint = GameObject.Find(gameScene.SpawnPointName);
                        spawnPosition = spawnPoint.transform.position;
                        spawnRotation = spawnPoint.transform.eulerAngles;
                    }
                    else
                    {
                        if (Character.Instance.CharacterData.IsCreated)
                        {
                            spawnPosition = GameDatabase.Instance.GetWorldPositions()[gameScene.startPositionID]
                                .position;
                            if (GameDatabase.Instance.GetWorldPositions()[gameScene.startPositionID].useRotation)
                            {
                                spawnRotation = GameDatabase.Instance.GetWorldPositions()[gameScene.startPositionID]
                                    .rotation;
                            }
                        }
                        else
                        {
                            spawnPosition = GameDatabase.Instance.GetWorldPositions()[GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].startingPositionID]
                                .position;
                            if (GameDatabase.Instance.GetWorldPositions()[GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].startingPositionID].useRotation)
                            {
                                spawnRotation = GameDatabase.Instance.GetWorldPositions()[GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].startingPositionID]
                                    .rotation;
                            }
                        }
                    }
                }
                else
                {
                    if (gameScene.AlwaysSpawnAtPoint)
                    {
                        GameObject spawnPoint = GameObject.Find(gameScene.SpawnPointName);
                        spawnPosition = spawnPoint.transform.position;
                        spawnRotation = spawnPoint.transform.eulerAngles;
                    }
                    else
                    {
                        spawnPosition = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].LastPosition;
                        spawnRotation = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].LastRotation;
                    }
                }
                    
                GameObject playerCharacter = Instantiate(GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].Genders[RPGBuilderUtilities.GetGenderIndexByName(Character.Instance.CharacterData.Gender)].Prefab,
                    spawnPosition, Quaternion.Euler(spawnRotation));
                    
                GameState.playerEntity = playerCharacter.GetComponent<PlayerCombatEntity>();

                if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
                {
                    GameState.playerEntity.autoAttackData.CurrentAutoAttackAbilityID = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID].autoAttackAbilityID;
                }
                GameState.Instance.RegisterEntity(GameState.playerEntity);
                GameState.playerEntity.InitStats();

                ScreenSpaceWorldDroppedItems.Instance.InitCamera();

                ActionBarManager.Instance.InitActionBar();
                if (Character.Instance.CharacterData.ActionBarSlots.Count == 0)
                {
                    foreach (var t in ActionBarManager.Instance.actionBarSlots)
                    {
                        Character.Instance.CharacterData.ActionBarSlots.Add(new CharacterEntries.ActionBarSlotEntry());
                        if (t.actionBarType ==
                            CharacterEntries.ActionBarType.Main)
                        {
                            Character.Instance.CharacterData.StealthedActionBarSlots.Add(new CharacterEntries.ActionBarSlotEntry());
                        }
                    }
                }
                else
                {
                    if (ActionBarManager.Instance.actionBarSlots.Count > Character.Instance.CharacterData.ActionBarSlots.Count)
                    {
                        int diff = ActionBarManager.Instance.actionBarSlots.Count - Character.Instance.CharacterData.ActionBarSlots.Count;
                        for (int i = 0; i < diff; i++)
                        {
                            Character.Instance.CharacterData.ActionBarSlots.Add(new CharacterEntries.ActionBarSlotEntry());
                        }
                    }
                    else if(ActionBarManager.Instance.actionBarSlots.Count < Character.Instance.CharacterData.ActionBarSlots.Count)
                    {
                        int diff = Character.Instance.CharacterData.ActionBarSlots.Count - ActionBarManager.Instance.actionBarSlots.Count;
                        for (int i = 0; i < diff; i++)
                        {
                            Character.Instance.CharacterData.ActionBarSlots.RemoveAt(Character.Instance.CharacterData.ActionBarSlots.Count-1);
                        }  
                    }

                    int currentMainSlotCount = ActionBarManager.Instance.actionBarSlots.Count(t => t.actionBarType == CharacterEntries.ActionBarType.Main);

                    if (currentMainSlotCount != Character.Instance.CharacterData.StealthedActionBarSlots.Count)
                    {
                        if (currentMainSlotCount > Character.Instance.CharacterData.StealthedActionBarSlots.Count)
                        {
                            int diff = currentMainSlotCount - Character.Instance.CharacterData.StealthedActionBarSlots.Count;
                            for (int i = 0; i < diff; i++)
                            {
                                Character.Instance.CharacterData.StealthedActionBarSlots.Add(new CharacterEntries.ActionBarSlotEntry());
                            }
                        } else if (currentMainSlotCount < Character.Instance.CharacterData.StealthedActionBarSlots.Count)
                        {
                            int diff = Character.Instance.CharacterData.StealthedActionBarSlots.Count - currentMainSlotCount;
                            for (int i = 0; i < diff; i++)
                            {
                                Character.Instance.CharacterData.StealthedActionBarSlots.RemoveAt(Character.Instance.CharacterData.StealthedActionBarSlots.Count-1);
                            }  
                        }
                    }

                    for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
                    {
                        switch (Character.Instance.CharacterData.ActionBarSlots[i].contentType)
                        {
                            case CharacterEntries.ActionBarSlotContentType.None:
                                ActionBarManager.Instance.actionBarSlots[i].Reset();
                                continue;
                            case CharacterEntries.ActionBarSlotContentType.Ability:
                                if(!CombatUtilities.IsAbilityKnown((Character.Instance.CharacterData.ActionBarSlots[i].ID))) continue;
                                ActionBarManager.Instance.actionBarSlots[i].contentType = CharacterEntries.ActionBarSlotContentType.Ability;
                                ActionBarManager.Instance.actionBarSlots[i].ThisAbility = GameDatabase.Instance.GetAbilities()[Character.Instance.CharacterData.ActionBarSlots[i].ID];
                                break;
                            case CharacterEntries.ActionBarSlotContentType.Item:
                                if(RPGBuilderUtilities.getItemCount(GameDatabase.Instance.GetItems()[Character.Instance.CharacterData.ActionBarSlots[i].ID]) == 0)continue;
                                ActionBarManager.Instance.actionBarSlots[i].contentType = CharacterEntries.ActionBarSlotContentType.Item;
                                ActionBarManager.Instance.actionBarSlots[i].ThisItem = GameDatabase.Instance.GetItems()[Character.Instance.CharacterData.ActionBarSlots[i].ID];
                                break;
                        }
                    }
                }
                
                ActionBarManager.Instance.InitializeSlots();
                
                MusicManager.Instance.InitializeSceneMusic();

                GameEvents.Instance.OnSceneEntered(scene.name);
                
                BonusManager.Instance.ResetAllOnBonuses();
                BonusManager.Instance.InitBonuses();

                Toolbar.Instance.UpdateToolbar();
                
                GameState.playerEntity.ResetTarget();
                
                foreach (var armorSlot in GameDatabase.Instance.GetArmorSlots().Values)
                {
                    EconomyData.EquippedArmor ArmorSlot = new EconomyData.EquippedArmor {ArmorSlot = armorSlot};
                    GameState.playerEntity.equippedArmors.Add(ArmorSlot);
                }
                
                EconomyData.EquippedWeapon WeaponSlot1 = new EconomyData.EquippedWeapon ();
                GameState.playerEntity.equippedWeapons.Add(WeaponSlot1);
                EconomyData.EquippedWeapon WeaponSlot2 = new EconomyData.EquippedWeapon ();
                GameState.playerEntity.equippedWeapons.Add(WeaponSlot2);
                
                foreach (var ab in Character.Instance.CharacterData.Abilities)
                {
                    ab.comboActive = false;
                }

                bool isNewCharacter = !Character.Instance.CharacterData.IsCreated;
                if (!Character.Instance.CharacterData.IsCreated)
                {
                    InitializeNewCharacter();
                }
                else
                {
                    InventoryManager.Instance.InitEquippedItems();
                    LevelingManager.Instance.HandleSpellbookAfterLevelUp();
                }
                
                StatCalculator.SetVitalityToMax();

                
                
                GameState.playerEntity.appearance.HandleAnimatorOverride();
                InitActionAbilities();
                ShapeshiftingSlotsDisplayManager.Instance.DisplaySlots();
                GameState.playerEntity.controllerEssentials.StartCoroutine(GameState.playerEntity.controllerEssentials.InitControllers());
                if(isNewCharacter) GameEvents.Instance.OnNewCharacterEnteredGame();
                
                isInGame = true;
                GameEvents.Instance.OnNewGameSceneLoaded();
                RegionManager.Instance.StartCoroutine(RegionManager.Instance.InitializeDefaultRegion());
            } else
            {
                isInGame = false;
            }
        }


        private void InitActionAbilities()
        {
            Character.Instance.CharacterData.ActionAbilities.Clear();

            foreach (var actionAb in GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].actionAbilities)
            {
                AddCurrentActionAb(actionAb, CharacterEntries.ActionAbilityEntryType.Race, Character.Instance.CharacterData.RaceID);
            }

            foreach (var skill in Character.Instance.CharacterData.Skills)
            {
                foreach (var actionAb in GameDatabase.Instance.GetSkills()[skill.skillID].actionAbilities)
                {
                    AddCurrentActionAb(actionAb, CharacterEntries.ActionAbilityEntryType.Skill, skill.skillID);
                }
            }

            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                foreach (var actionAb in GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID]
                    .actionAbilities)
                {
                    AddCurrentActionAb(actionAb, CharacterEntries.ActionAbilityEntryType.Class,
                        Character.Instance.CharacterData.ClassID);
                }
            }

            foreach (var equippedItem in Character.Instance.CharacterData.ArmorPiecesEquipped)
            {
                if(equippedItem.itemID == -1) continue;
                foreach (var actionAb in GameDatabase.Instance.GetItems()[equippedItem.itemID].actionAbilities)
                {
                    AddCurrentActionAb(actionAb, CharacterEntries.ActionAbilityEntryType.Item, equippedItem.itemID);
                }
            }

            foreach (var equippedItem in Character.Instance.CharacterData.WeaponsEquipped)
            {
                if(equippedItem.itemID == -1) continue;
                foreach (var actionAb in GameDatabase.Instance.GetItems()[equippedItem.itemID].actionAbilities)
                {
                    AddCurrentActionAb(actionAb, CharacterEntries.ActionAbilityEntryType.Item, equippedItem.itemID);
                }
            }
        }

        public void AddCurrentActionAb(RPGCombatDATA.ActionAbilityDATA actionAb, CharacterEntries.ActionAbilityEntryType entryType, int sourceID)
        {
            CharacterEntries.ActionAbilityEntry newActionAb = new CharacterEntries.ActionAbilityEntry
            {
                entryType = entryType, sourceID = sourceID, ability = GameDatabase.Instance.GetAbilities()[actionAb.abilityID]
            };
            if (actionAb.keyType == RPGCombatDATA.ActionAbilityKeyType.ActionKey)
            {
                newActionAb.keyType = actionAb.keyType;
                newActionAb.actionKeyName = actionAb.actionKeyName;
            }
            else
            {
                newActionAb.keyType = actionAb.keyType;
                newActionAb.key = actionAb.key;
            }
            newActionAb.CDLeft = 0;
            newActionAb.NextTimeUse = 0;
            Character.Instance.CharacterData.ActionAbilities.Add(newActionAb);
        }

        private void InitializeNewCharacter()
        {
            Character.Instance.CharacterData.IsCreated = true;

            RPGRace raceREF = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
            foreach (var t in raceREF.startItems)
            {
                RPGBuilderUtilities.HandleItemLooting(t.itemID, -1, t.count, t.equipped, false);
            }

            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                foreach (var t in classREF.startItems)
                {
                    RPGBuilderUtilities.HandleItemLooting(t.itemID, -1, t.count, t.equipped, false);
                }
            }

            foreach (var t1 in Character.Instance.CharacterData.Skills)
            {
                RPGSkill skillREF = GameDatabase.Instance.GetSkills()[t1.skillID];
                foreach (var t in skillREF.startItems)
                {
                    RPGBuilderUtilities.HandleItemLooting(t.itemID, -1, t.count, t.equipped, false);
                }
            }
            
            foreach (var t1 in Character.Instance.CharacterData.WeaponTemplates)
            {
                RPGWeaponTemplate weaponTemplateREF = GameDatabase.Instance.GetWeaponTemplates()[t1.weaponTemplateID];
                foreach (var t in weaponTemplateREF.startItems)
                {
                    RPGBuilderUtilities.HandleItemLooting(t.itemID,  -1,t.count, t.equipped, false);
                }
            }

            List<RPGAbility> knownAbilities = new List<RPGAbility>();
            List<int> slots = new List<int>();
            int curAb = 0;

            LevelingManager.Instance.HandleSpellbookAfterLevelUp();
            CraftingManager.Instance.HandleStartingRecipes();
            GatheringManager.Instance.HandleStartingResourceNodes();

            foreach (CharacterEntries.AbilityEntry t in Character.Instance.CharacterData.Abilities)
            {
                RPGAbility abREF = GameDatabase.Instance.GetAbilities()[t.ID];
                if (!t.known) continue;
                knownAbilities.Add(abREF);
                slots.Add(curAb);
                curAb++;
            }

            for (int i = 0; i < knownAbilities.Count; i++)
            {
                ActionBarManager.Instance.SetAbilityToSlot(knownAbilities[i], slots[i]);
            }

            Character.Instance.CharacterData.Time.CurrentYear = GameDatabase.Instance.GetWorldSettings().StartingYear;
            Character.Instance.CharacterData.Time.CurrentMonth = GameDatabase.Instance.GetWorldSettings().StartingMonth;
            Character.Instance.CharacterData.Time.CurrentWeek = GameDatabase.Instance.GetWorldSettings().StartingWeek;
            Character.Instance.CharacterData.Time.CurrentDay = GameDatabase.Instance.GetWorldSettings().StartingDay;
            Character.Instance.CharacterData.Time.CurrentHour = GameDatabase.Instance.GetWorldSettings().StartingHour;
            Character.Instance.CharacterData.Time.CurrentMinute = GameDatabase.Instance.GetWorldSettings().StartingMinute;
            Character.Instance.CharacterData.Time.CurrentSecond = GameDatabase.Instance.GetWorldSettings().StartingSecond;
        }

        private void FixedUpdate()
        {
            if (GameDatabase.Instance.GetCombatSettings() == null || GameState.playerEntity == null) return;
            if (!GameDatabase.Instance.GetGeneralSettings().automaticSave) return;
            if (!(Time.time >= nextAutomaticSave)) return;
            nextAutomaticSave += GameDatabase.Instance.GetGeneralSettings().automaticSaveDelay;
            RPGBuilderJsonSaver.SaveCharacterData();
        }

        public void ClearAllWorldItemData()
        {
            List<CharacterEntries.ItemEntry> worldItems = new List<CharacterEntries.ItemEntry>();
            foreach (var itemData in Character.Instance.CharacterData.ItemEntries)
            {
                if (itemData.state == CharacterEntries.ItemEntryState.InWorld)
                    worldItems.Add(itemData);
            }

            foreach (var worldItem in worldItems)
            {
                if (Character.Instance.CharacterData.ItemEntries.Contains(worldItem))
                    Character.Instance.CharacterData.ItemEntries.Remove(worldItem);
            }
        }

        public void ResetCharacterData(bool deleteEssentials)
        {
            if (deleteEssentials)
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(GetComponent<Character>());
                gameObject.AddComponent<Character>();
            }
        }

    }
}
