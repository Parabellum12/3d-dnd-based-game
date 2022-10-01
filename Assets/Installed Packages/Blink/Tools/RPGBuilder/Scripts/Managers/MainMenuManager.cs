using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance { get; private set; }
        
        public CanvasGroup HomeCG, CreateCharCG, ContinueCG, BackHomeButtonCG, errorPopupCG;
        public TMP_InputField characterNameIF;

        public TextMeshProUGUI curSelectedCharacterNameText,
            classTitleText,
            classInfoDescriptionText,
            raceInfoDescriptionText,
            currentAllocationPointsText,
            popupMessageText,
            raceNameText,
            classNameText;

        public GameObject raceSlotPrefab, classSlotPrefab;
        public Transform raceSlotsParent, classSlotsParent;

        private readonly List<RaceSlotHolder> curRaceSlots = new List<RaceSlotHolder>();
        private readonly List<ClassSlotHolder> curClassSlots = new List<ClassSlotHolder>();
        public List<ClassSlotHolder> curGenderSlots = new List<ClassSlotHolder>();
        private readonly List<CharacterSlotHolder> curCharSlots = new List<CharacterSlotHolder>();

        public GameObject GenderSlot;
        public Transform GenderParent;
        
        private int currentlySelectedRace;
        private int currentlySelectedClass;
        private RPGBGender currentlySelectedGender;

        public Color slotSelectedColor, slotNotSelectedColor;

        public Transform characterModelSpot;

        public GameObject curPlayerModel;

        private List<CharacterData> allCharacters = new List<CharacterData>();
        public GameObject characterSlotPrefab;
        public Transform characterSlotsParent;

        public GameObject statAllocationSlotPrefab;
        public Transform statAllocationSlotsParent;
        public List<StatAllocationSlot> curStatAllocationSlots = new List<StatAllocationSlot>();

        public CanvasGroup modifiersCG;
        public TextMeshProUGUI modifiersPointsText;

        public Transform availablePositiveModifiersParent,
            availableNegativeModifiersParent,
            chosenPositiveModifiersParent,
            chosenNegativeModifiersParent;

        public GameObject gameModifierSlot;
        public Color modifierPositiveColor, modifierNegativeColor;

        public List<GameModifierSlotDataHolder> currentAvailablePositiveModifiersSlots = new List<GameModifierSlotDataHolder>();
        public List<GameModifierSlotDataHolder> currentAvailableNegativeModifiersSlots = new List<GameModifierSlotDataHolder>();
        public List<GameModifierSlotDataHolder> currentChosenPositiveModifiersSlots = new List<GameModifierSlotDataHolder>();
        public List<GameModifierSlotDataHolder> currentChosenNegativeModifiersSlots = new List<GameModifierSlotDataHolder>();
        
        private RPGBuilderEditorDATA editorData;
        private RPGBuilderUISettings UISettings;

        private IEnumerator Start()
        {
            if (Instance != null) yield break;
            Instance = this;

            
            
            editorData = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
            if (editorData == null)
            {
                Debug.LogError("EDITOR DATA could not be found");
                yield break;
            }
            UISettings = Resources.Load<RPGBuilderUISettings>(editorData.RPGBDatabasePath + "Settings/" + "UI_Settings");
            if (UISettings == null)
            {
                Debug.LogError("UI SETTINGS could not be found");
                yield break;
            }
            
            if (!PlayerPrefs.HasKey("MasterVolume"))
            {
                PlayerPrefs.SetFloat("MasterVolume", 0.25f);
                AudioListener.volume =  0.25f;
            }
            else AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume");
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            

            if (FindObjectOfType<RPGBuilderEssentials>() == null)
                Instantiate(UISettings.RPGBuilderEssentialsPrefab, Vector3.zero, Quaternion.identity);
            if (FindObjectOfType<LoadingScreenManager>() == null)
                Instantiate(UISettings.LoadingScreenManagerPrefab, Vector3.zero, Quaternion.identity);
            
            disableAllCG();
            RPGBuilderUtilities.EnableCG(HomeCG);

            LoadAllCharacter();

            bool DeletedChar = false;
            foreach (var character in allCharacters)
            {
                if (GameDatabase.Instance.GetRaces().ContainsKey(character.RaceID)) continue;
                RPGBuilderJsonSaver.DeleteCharacter(character.CharacterName);
                DeletedChar = true;
            }
            if(DeletedChar) LoadAllCharacter();
            
            if (!PlayerPrefs.HasKey("CharacterWipe"))
            {
                foreach (var character in allCharacters)
                {
                    RPGBuilderJsonSaver.DeleteCharacter(character.CharacterName);
                }
                LoadAllCharacter();
                PlayerPrefs.SetInt("CharacterWipe", 1);
            }
        }


        private void LoadAllCharacter()
        {
            allCharacters = DataSavingSystem.LoadAllCharacters();
        }

        public void ClickNewChar()
        {
            disableAllCG();
            RPGBuilderUtilities.EnableCG(CreateCharCG);
            RPGBuilderUtilities.EnableCG(BackHomeButtonCG);

            classTitleText.gameObject.SetActive(!GameDatabase.Instance.GetCharacterSettings().NoClasses);
            classInfoDescriptionText.gameObject.SetActive(!GameDatabase.Instance.GetCharacterSettings().NoClasses);
            classNameText.gameObject.SetActive(!GameDatabase.Instance.GetCharacterSettings().NoClasses);
            
            InitCreateNewChar();
        }

        public void ClickContinue()
        {
			if (allCharacters.Count == 0) return;
            disableAllCG();
            RPGBuilderUtilities.EnableCG(ContinueCG);
            RPGBuilderUtilities.EnableCG(BackHomeButtonCG);

            InitContinue();
        }

        public void DeleteCharacter()
        {
            if (allCharacters.Count == 0) return;
            RPGBuilderJsonSaver.DeleteCharacter(Character.Instance.CharacterData.CharacterName);
            allCharacters.Clear();
            LoadAllCharacter();
            if (allCharacters.Count == 0)
                ClickHome();
            else
                InitContinue();
        }

        private void InitContinue()
        {
            clearCharSlots();
            
            foreach (var t in allCharacters)
            {
                var charSlot = Instantiate(characterSlotPrefab, characterSlotsParent);
                var holder = charSlot.GetComponent<CharacterSlotHolder>();
                holder.Init(t);
                curCharSlots.Add(holder);
            }

            if (allCharacters.Count > 0) SelectCharacter(allCharacters[0].CharacterName);
        }

        public void ClickHome()
        {
            RPGBuilderEssentials.Instance.ResetCharacterData(false);
            disableAllCG();
            RPGBuilderUtilities.EnableCG(HomeCG);

            if (curPlayerModel != null) Destroy(curPlayerModel);
        }

        private void disableAllCG()
        {
            RPGBuilderUtilities.DisableCG(HomeCG);
            RPGBuilderUtilities.DisableCG(ContinueCG);
            RPGBuilderUtilities.DisableCG(CreateCharCG);
            RPGBuilderUtilities.DisableCG(BackHomeButtonCG);
        }

        private void InitCreateNewChar()
        {
            clearAllSlots();

            currentlySelectedRace = 0;
            currentlySelectedClass = 0;

            int firstRace = GetFirstRace();
            if (firstRace == -1)
            {
                Debug.LogError("You need at least one race");
                return;
            } 
            Character.Instance.CharacterData.RaceID = GameDatabase.Instance.GetRaces()[firstRace].ID;
            currentlySelectedRace = Character.Instance.CharacterData.RaceID;
            if (GameDatabase.Instance.GetRaces()[firstRace].Genders.Count > 0 && GameDatabase.Instance.GetRaces()[firstRace].Genders[0].Gender != null)
            {
                Character.Instance.CharacterData.Gender = GameDatabase.Instance.GetRaces()[firstRace].Genders[0].Gender.entryName;
            }
            
            foreach (var race in GameDatabase.Instance.GetRaces().Values)
            {
                var raceSlot = Instantiate(raceSlotPrefab, raceSlotsParent);
                var holder = raceSlot.GetComponent<RaceSlotHolder>();
                holder.Init(race, race.ID);
                curRaceSlots.Add(holder);
            }

            GenerateClassSlots();

            resetAllClassBorders();
            resetAllRaceBorders();
            
            RPGBuilderEssentials.Instance.ResetCharacterData(false);
            if(GameDatabase.Instance.GetRaces().Values.Count>0)SelectRace(firstRace);
        }

        private int GetFirstRace()
        {
            foreach (var race in GameDatabase.Instance.GetRaces())
            {
                return race.Value.ID;
            }

            return -1;
        }

        private void GenerateClassSlots()
        {
            clearClassesSlots();
            if (currentlySelectedRace == -1) return;
            if (GameDatabase.Instance.GetCharacterSettings().NoClasses) return;
            if (GameDatabase.Instance.GetRaces().Values.Count == 0) return;
            for (var i = 0; i < GameDatabase.Instance.GetRaces()[currentlySelectedRace].availableClasses.Count; i++)
            {
                var classSlot = Instantiate(classSlotPrefab, classSlotsParent);
                var holder = classSlot.GetComponent<ClassSlotHolder>();
                holder.Init(GameDatabase.Instance.GetClasses()[GameDatabase.Instance.GetRaces()[currentlySelectedRace].availableClasses[i].classID],
                    i);
                curClassSlots.Add(holder);
            }
        }

        private void clearAllSlots()
        {
            foreach (var t in curRaceSlots)
                Destroy(t.gameObject);

            curRaceSlots.Clear();
            foreach (var t in curClassSlots)
                Destroy(t.gameObject);

            curClassSlots.Clear();
        }

        private void clearCharSlots()
        {
            foreach (var t in curCharSlots)
                Destroy(t.gameObject);

            curCharSlots.Clear();
        }

        private void clearClassesSlots()
        {
            foreach (var t in curClassSlots)
                Destroy(t.gameObject);

            curClassSlots.Clear();
        }

        private void resetAllRaceBorders()
        {
            foreach (var t in curRaceSlots)
                t.selectedBorder.color = slotNotSelectedColor;
        }

        private void resetAllClassBorders()
        {
            foreach (var t in curClassSlots)
                t.selectedBorder.color = slotNotSelectedColor;
        }

        private void resetAllGenderBorders()
        {
            foreach (var t in curGenderSlots)
                t.selectedBorder.color = slotNotSelectedColor;
        }

        private void ClearAllGenderBorders()
        {
            foreach (var t in curGenderSlots)
                Destroy(t.gameObject);
            curGenderSlots.Clear();
        }

        

        public void SelectRace(int raceID)
        {
            currentlySelectedRace = raceID;
            Character.Instance.CharacterData.RaceID = raceID;
            
            RPGRace raceREF = GameDatabase.Instance.GetRaces()[raceID];
            
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses) Character.Instance.CharacterData.ClassID = raceREF.availableClasses[0].classID;

            ClearAllGenderBorders();
            for (var index = 0; index < raceREF.Genders.Count; index++)
            {
                var gender = raceREF.Genders[index];
                GameObject genderSlot = Instantiate(GenderSlot, GenderParent);
                ClassSlotHolder slot = genderSlot.GetComponent<ClassSlotHolder>();
                slot.classIndex = index;
                if(slot.icon != null) slot.icon.sprite = gender.Icon;
                if(slot.className != null) slot.className.text = gender.Gender.entryDisplayName;
                curGenderSlots.Add(slot);
            }

            SelectGender(0);
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                SelectClass(0);
                currentlySelectedClass = 0;
                GenerateClassSlots();
                curClassSlots[currentlySelectedClass].selectedBorder.color = slotSelectedColor;
            }
            else
            {
                InitStatAllocation();
            }
            
            HighlightRaceSlot();

            raceInfoDescriptionText.text = raceREF.entryDescription;
            raceNameText.text = raceREF.entryDisplayName;

            if (curPlayerModel != null)
            {
                PlayerAppearance appearanceREF = curPlayerModel.GetComponent<PlayerAppearance>();
                InitStartingItemsVisual(appearanceREF);
            }
        }

        private void HighlightRaceSlot()
        {
            resetAllRaceBorders();
            foreach (var slot in curRaceSlots)
            {
                if(slot.raceID != currentlySelectedRace) continue;
                slot.selectedBorder.color = slotSelectedColor;
            }
        }

        public void SelectGender(int index)
        {
            if (curPlayerModel != null) Destroy(curPlayerModel);
            if (GameDatabase.Instance.GetRaces().Values.Count == 0) return;
            resetAllGenderBorders();
            currentlySelectedGender = GameDatabase.Instance.GetRaces()[currentlySelectedRace].Genders[index].Gender;

            
            curGenderSlots[index].selectedBorder.color = slotSelectedColor;
            curPlayerModel = Instantiate(GameDatabase.Instance.GetRaces()[currentlySelectedRace].Genders[index].Prefab, Vector3.zero,
                Quaternion.identity);

            if (curPlayerModel != null)
            {
                PlayerAppearance appearanceREF = curPlayerModel.GetComponent<PlayerAppearance>();
                InventoryManager.Instance.HideAllItemsMainMenu(appearanceREF);
                InitStartingItemsVisual(appearanceREF);
            }

            Character.Instance.CharacterData.Gender = currentlySelectedGender.entryName;

            curPlayerModel.transform.SetParent(characterModelSpot);
            curPlayerModel.transform.localPosition = Vector3.zero;
            curPlayerModel.transform.localRotation = Quaternion.identity;

            // Preventing combat and movement actions on the character in main menu
            Destroy(curPlayerModel.GetComponent<CombatEntity>());
            Destroy(curPlayerModel.GetComponent<PlayerAnimatorLayer>());

            RPGBCharacterControllerEssentials controllerEssentialREF =
                curPlayerModel.GetComponent<RPGBCharacterControllerEssentials>();
            controllerEssentialREF.MainMenuInit();
        }

        private void InitStartingItemsVisual(PlayerAppearance appearanceREF)
        {
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                for (var i = 0; i < classREF.startItems.Count; i++)
                    if (classREF.startItems[i].itemID != -1 && classREF.startItems[i].equipped)
                        InventoryManager.Instance.InitEquipClassItemMainMenu(
                            GameDatabase.Instance.GetItems()[classREF.startItems[i].itemID],
                            appearanceREF, i);
            }
                
            RPGRace raceREF = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
            for (var i = 0; i < raceREF.startItems.Count; i++)
            {
                if (raceREF.startItems[i].itemID != -1 && raceREF.startItems[i].equipped)
                {
                    InventoryManager.Instance.InitEquipClassItemMainMenu(
                        GameDatabase.Instance.GetItems()[raceREF.startItems[i].itemID],
                        appearanceREF, i);
                }
            }

            foreach (var t in raceREF.weaponTemplates)
            {
                RPGWeaponTemplate weaponTemplateREF = GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID];
                for (var i = 0; i < weaponTemplateREF.startItems.Count; i++)
                    if (weaponTemplateREF.startItems[i].itemID != -1 && weaponTemplateREF.startItems[i].equipped)
                        InventoryManager.Instance.InitEquipClassItemMainMenu(
                            GameDatabase.Instance.GetItems()[weaponTemplateREF.startItems[i].itemID],
                            appearanceREF, i);
            }
        }

        public void SelectClass(int classIndex)
        {
            resetAllClassBorders();
            currentlySelectedClass = classIndex;
            curClassSlots[classIndex].selectedBorder.color = slotSelectedColor;

            Character.Instance.CharacterData.ClassID = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID]
                .availableClasses[currentlySelectedClass].classID;

            if (curPlayerModel == null) return;
            RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
            PlayerAppearance appearanceREF = curPlayerModel.GetComponent<PlayerAppearance>();
            InventoryManager.Instance.HideAllItemsMainMenu(appearanceREF);
            for (var i = 0; i < classREF.startItems.Count; i++)
                if (classREF.startItems[i].itemID != -1 && classREF.startItems[i].equipped)
                    InventoryManager.Instance.InitEquipClassItemMainMenu(
                        GameDatabase.Instance.GetItems()[classREF.startItems[i].itemID],
                        appearanceREF, i);
            
            classInfoDescriptionText.text = classREF.entryDescription;
            classNameText.text = classREF.entryDisplayName;
            
            InitStatAllocation();
        }

        bool IsCharacterNameAvailable(string charName)
        {
            return allCharacters.Any(t => t.CharacterName == charName);
        }

        private void clearAllStatAllocationSlots()
        {
            foreach (var t in curStatAllocationSlots)
                Destroy(t.gameObject);
            
            curStatAllocationSlots.Clear();
        }

        private void InitStatAllocation()
        {
            clearAllStatAllocationSlots();
            Character.Instance.CharacterData.AllocatedStats.Clear();
            Character.Instance.CharacterData.MainMenuStatAllocationPoints = 0;

            RPGRace raceREF = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
            Character.Instance.CharacterData.MainMenuStatAllocationPoints += raceREF.allocationStatPoints;

            RPGClass classREF = null;
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
            }

            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses && classREF != null)
            {
                Character.Instance.CharacterData.MainMenuStatAllocationPoints += classREF.allocationStatPoints;
            }

            Character.Instance.CharacterData.MainMenuStatAllocationMaxPoints =
                Character.Instance.CharacterData.MainMenuStatAllocationPoints;

            UpdateAllocationPointsText();

            foreach (var allocatedStatEntry in raceREF.allocatedStatsEntries)
            {
                StatAllocationManager.Instance.SpawnStatAllocationSlot(allocatedStatEntry, statAllocationSlotPrefab, statAllocationSlotsParent, curStatAllocationSlots, StatAllocationSlot.SlotType.Menu);
            }


            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses && classREF != null)
            {
                foreach (var allocatedStatEntry in classREF.allocatedStatsEntries)
                {
                    StatAllocationManager.Instance.SpawnStatAllocationSlot(allocatedStatEntry, statAllocationSlotPrefab, statAllocationSlotsParent, curStatAllocationSlots, StatAllocationSlot.SlotType.Menu);
                }
            }

            foreach (var allocatedStatSlot in curStatAllocationSlots)
            {
                float max = StatAllocationManager.Instance.getMaxAllocatedStatValue(allocatedStatSlot.thisStat);
                int totalValue = GetCurrentTotalStatValue(allocatedStatSlot.thisStat);
                allocatedStatSlot.curValueText.text = max > 0 ? totalValue + " / " + max : totalValue.ToString();
            }
            
            StatAllocationManager.Instance.HandleStatAllocationButtons(Character.Instance.CharacterData.MainMenuStatAllocationPoints, Character.Instance.CharacterData.MainMenuStatAllocationMaxPoints, curStatAllocationSlots, StatAllocationSlot.SlotType.Menu);
        }

        public int GetCurrentTotalStatValue(RPGStat stat)
        {
            float currentValue = stat.baseValue;
            RPGRace race = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
            foreach (var t in race.CustomStats)
            {
                if (t.statID == stat.ID)
                {
                    currentValue += t.addedValue;
                }
            }

            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                RPGClass classRef = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                foreach (var t in classRef.CustomStats)
                {
                    if (t.statID == stat.ID)
                    {
                        currentValue += t.addedValue;
                    }
                }
            }

            RPGSpecies speciesReference = GameDatabase.Instance.GetSpecies()[race.speciesID];
            if (race.speciesID != -1 &&  speciesReference != null)
            {
                foreach (var t in speciesReference.CustomStats)
                {
                    if (t.statID == stat.ID)
                    {
                        currentValue += t.addedValue;
                    }
                }
            }

            return (int)currentValue;
        }

        public void UpdateAllocationPointsText()
        {
            currentAllocationPointsText.text = "Points: " + Character.Instance.CharacterData.MainMenuStatAllocationPoints;
        }

        public void CreateCharacter()
        {
            
            if (Character.Instance.CharacterData.RaceID == -1)
            {
                ShowPopupMessage("A race must be selected");
                return;
            }
            if (characterNameIF.text == "")
            {
                ShowPopupMessage("The name cannot be empty");
                return;
            }
            if (IsCharacterNameAvailable(characterNameIF.text))
            {
                ShowPopupMessage("This name is already taken");
                return;
            }
            
            if (Character.Instance.CharacterData.MainMenuStatAllocationPoints > 0 && GameDatabase.Instance.GetCharacterSettings().MustSpendAllStatPointsToCreateCharacter)
            {
                ShowPopupMessage("All Stat Points need to be spent");
                return;
            }
            
            Character.Instance.CharacterData.CharacterName = characterNameIF.text;

            foreach (var ab in GameDatabase.Instance.GetAbilities().Values)
            {
                CharacterEntries.AbilityEntry newAb = new CharacterEntries.AbilityEntry
                {
                    name = ab.entryName, ID = ab.ID, rank = -1, known = false
                };
                Character.Instance.CharacterData.Abilities.Add(newAb);
            }
            foreach (var recipe in GameDatabase.Instance.GetRecipes().Values)
            {
                CharacterEntries.RecipeEntry newAb = new CharacterEntries.RecipeEntry
                {
                    name = recipe.entryName, ID = recipe.ID, rank = -1, known = false
                };
                Character.Instance.CharacterData.Recipes.Add(newAb);
            }
            foreach (var resourceNode in GameDatabase.Instance.GetResources().Values)
            {
                CharacterEntries.ResourceNodeEntry newAb = new CharacterEntries.ResourceNodeEntry
                {
                    name = resourceNode.entryName, ID = resourceNode.ID, rank = -1, known = false
                };
                Character.Instance.CharacterData.Resources.Add(newAb);
            }
            foreach (var bonus in GameDatabase.Instance.GetBonuses().Values)
            {
                CharacterEntries.BonusEntry newAb = new CharacterEntries.BonusEntry
                {
                    name = bonus.entryName,
                    ID = bonus.ID,
                    rank = -1,
                    known = false,
                    On = false
                };
                Character.Instance.CharacterData.Bonuses.Add(newAb);
            }

            foreach (var faction in GameDatabase.Instance.GetFactions().Values)
            {
                CharacterEntries.FactionEntry newFaction = new CharacterEntries.FactionEntry
                {
                    name = faction.entryName, ID = faction.ID
                };
                var factionInteractions =
                    GameDatabase.Instance.GetFactions()[
                        GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].factionID].factionInteractions;
                foreach (var interaction in factionInteractions)
                {
                    if (interaction.factionID != faction.ID) continue;
                    newFaction.currentPoint = interaction.startingPoints;

                    for (var stance = 0; stance < faction.factionStances.Count; stance++)
                    {
                        if (faction.factionStances[stance].FactionStance == interaction.DefaultFactionStance)
                        {
                            newFaction.stanceIndex = stance;
                        }
                    }
                }

                Character.Instance.CharacterData.Factions.Add(newFaction);
            }

            Character.Instance.CharacterData.TalentTrees.Clear();
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                var classRef = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                foreach (var t in classRef.talentTrees)
                {
                    CharacterUpdater.AddTalentTree(t.talentTreeID);
                }
            }

            var raceRef = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
            foreach (var t in raceRef.weaponTemplates)
            {
                RPGWeaponTemplate weaponTemplateREF = GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID];
                var newWeaponTemplateDATA = new CharacterEntries.WeaponTemplateEntry();
                newWeaponTemplateDATA.currentWeaponLevel = 1;
                newWeaponTemplateDATA.currentWeaponXP = 0;
                RPGLevelsTemplate lvlTemplateREF = GameDatabase.Instance.GetLevels()[weaponTemplateREF.levelTemplateID];
                newWeaponTemplateDATA.maxWeaponXP = lvlTemplateREF.allLevels[0].XPRequired;
                newWeaponTemplateDATA.weaponTemplateID = t.weaponTemplateID;
                Character.Instance.CharacterData.WeaponTemplates.Add(newWeaponTemplateDATA);
            }

            foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
            {
                var weaponTemplateREF = GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID];
                foreach (var t1 in weaponTemplateREF.talentTrees)
                {
                    CharacterUpdater.AddTalentTree(t1.talentTreeID);
                }
            }

            Character.Instance.CharacterData.Points.Clear();
            foreach (var t in GameDatabase.Instance.GetPoints().Values)
            {
                var newTreePointData = new CharacterEntries.TreePointEntry();
                newTreePointData.treePointID = t.ID;
                newTreePointData.amount = (int)GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.TreePoint + "+" +
                    RPGGameModifier.PointModifierType.Start_At,
                    t.startAmount, t.ID, -1);
                Character.Instance.CharacterData.Points.Add(newTreePointData);
            }

            Character.Instance.CharacterData.Currencies.Clear();

            foreach (var t in GameDatabase.Instance.GetCurrencies().Values)
            {
                var newCurrencyData = new CharacterEntries.CurrencyEntry();
                newCurrencyData.currencyID = t.ID;
                newCurrencyData.amount = t.baseValue;
                Character.Instance.CharacterData.Currencies.Add(newCurrencyData);
            }

            for (var i = 0; i < GameDatabase.Instance.GetEconomySettings().InventorySlots; i++)
            {
                var newInvItemData = new CharacterEntries.InventorySlotEntry {itemID = -1, itemStack = 0, itemDataID = -1};
                Character.Instance.CharacterData.Inventory.baseSlots.Add(newInvItemData);
            }

            Character.Instance.CharacterData.Skills.Clear();
            foreach (var t1 in GameDatabase.Instance.GetSkills().Values)
                if (t1.automaticallyAdded)
                {
                    var newSkillData = new CharacterEntries.SkillEntry();
                    newSkillData.currentSkillLevel = 1;
                    newSkillData.currentSkillXP = 0;
                    newSkillData.skillID = t1.ID;
                    var skillREF = GameDatabase.Instance.GetSkills()[newSkillData.skillID];
                    newSkillData.maxSkillXP = GameDatabase.Instance.GetLevels()[skillREF.levelTemplateID]
                        .allLevels[0].XPRequired;
                    foreach (var t2 in skillREF.talentTrees)
                    {
                        CharacterUpdater.AddTalentTree(t2.talentTreeID);
                    }

                    Character.Instance.CharacterData.Skills.Add(newSkillData);
                }


            Character.Instance.CharacterData.Level = 1;
            Character.Instance.CharacterData.levelTemplateID = GameDatabase.Instance.GetCharacterSettings().NoClasses ? raceRef.levelTemplateID : 
                GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID].levelTemplateID;
            Character.Instance.CharacterData.ExperienceNeeded = GameDatabase.Instance.GetLevels()[Character.Instance.CharacterData.levelTemplateID].allLevels[0].XPRequired;

            foreach (var t in GameDatabase.Instance.GetGeneralSettings().actionKeys)
            {
                var newKeybind = new CharacterEntries.ActionKeyEntry();
                newKeybind.actionKeyName = t.actionName;
                newKeybind.currentKey = t.defaultKey;
                Character.Instance.CharacterData.ActionKeys.Add(newKeybind);
            }
            
            RPGBuilderJsonSaver.GenerateCharacterEquippedtemsData();
            RPGBuilderJsonSaver.SaveCharacterData();
            LoadingScreenManager.Instance.LoadGameScene(raceRef.startingSceneID);
        }

        public void PlaySelectedCharacter()
        {
            CharacterUpdater.UpdateCharacter();
            LoadingScreenManager.Instance.LoadGameScene(Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].GameSceneID);
        }
        

        public void SelectCharacter(string characterName)
        {
            RPGBuilderJsonSaver.InitializeCharacterData(characterName);
            curSelectedCharacterNameText.text = Character.Instance.CharacterData.CharacterName;

            if (curPlayerModel != null) Destroy(curPlayerModel);

            curPlayerModel = Instantiate(
                GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].Genders[RPGBuilderUtilities.GetGenderIndexByName(Character.Instance.CharacterData.Gender)].Prefab,
                Vector3.zero, Quaternion.identity);

            curPlayerModel.transform.SetParent(characterModelSpot);
            curPlayerModel.transform.localPosition = Vector3.zero;
            curPlayerModel.transform.localRotation = Quaternion.identity;
            
            // Preventing combat and movement actions on the character in main menu
            Destroy(curPlayerModel.GetComponent<CombatEntity>());
            Destroy(curPlayerModel.GetComponent<PlayerAnimatorLayer>());
            
            RPGBCharacterControllerEssentials controllerEssentialREF =
                curPlayerModel.GetComponent<RPGBCharacterControllerEssentials>();
            controllerEssentialREF.MainMenuInit();

            var appearanceref = curPlayerModel.GetComponent<PlayerAppearance>();

            for (var i = 0; i < Character.Instance.CharacterData.ArmorPiecesEquipped.Count; i++)
                if (Character.Instance.CharacterData.ArmorPiecesEquipped[i].itemID != -1)
                    InventoryManager.Instance.InitEquipItemMainMenu(
                        GameDatabase.Instance.GetItems()[Character.Instance.CharacterData.ArmorPiecesEquipped[i].itemID],
                        appearanceref, i, Character.Instance.CharacterData.ArmorPiecesEquipped[i].itemDataID);

            for (var i = 0; i < Character.Instance.CharacterData.WeaponsEquipped.Count; i++)
                if (Character.Instance.CharacterData.WeaponsEquipped[i].itemID != -1)
                    InventoryManager.Instance.InitEquipItemMainMenu(
                        GameDatabase.Instance.GetItems()[Character.Instance.CharacterData.WeaponsEquipped[i].itemID],
                        appearanceref, i, Character.Instance.CharacterData.WeaponsEquipped[i].itemDataID);
        }

        private void ShowPopupMessage(string message)
        {
            RPGBuilderUtilities.EnableCG(errorPopupCG);
            popupMessageText.text = message;
        }

        public void HidePopupMessage()
        {
            RPGBuilderUtilities.DisableCG(errorPopupCG);
        }


        private void ClearModifierSlots(List<GameModifierSlotDataHolder> slotList)
        {
            foreach (var slot  in slotList)
            {
                Destroy(slot.gameObject);
            }
            slotList.Clear();
        }
        private void RemoveModifierSlots(List<GameModifierSlotDataHolder> slotList, RPGGameModifier gameModifier)
        {
            for (var index = 0; index < slotList.Count; index++)
            {
                var slot = slotList[index];
                if (slot.thisModifier != gameModifier) continue;
                Destroy(slot.gameObject);
                slotList.Remove(slot);
                return;
            }
        }
        
        public void InitializeModifierPanel()
        {
            ClearAllModifierSlots();
            RPGBuilderUtilities.EnableCG(modifiersCG);

            modifiersPointsText.text = Character.Instance.CharacterData.MenuGameModifierPoints.ToString();
            
            foreach (var gameModifier in GameDatabase.Instance.GetGameModifiers().Values)
            {
                if (RPGBuilderUtilities.isGameModifierOn(gameModifier.ID))
                {
                    if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                    {
                        SpawnGameModifierSlot(chosenNegativeModifiersParent, currentChosenNegativeModifiersSlots, gameModifier);
                    }
                    else
                    {
                        SpawnGameModifierSlot(chosenPositiveModifiersParent, currentChosenPositiveModifiersSlots, gameModifier);
                    }
                }
                else
                {
                    if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                    {
                        SpawnGameModifierSlot(availableNegativeModifiersParent, currentAvailableNegativeModifiersSlots, gameModifier);
                    }
                    else
                    {
                        SpawnGameModifierSlot(availablePositiveModifiersParent, currentAvailablePositiveModifiersSlots, gameModifier);
                    }
                }
            }
        }

        private void SpawnGameModifierSlot(Transform parent, List<GameModifierSlotDataHolder> slotList, RPGGameModifier gameModifierRef)
        {
            GameObject slot = Instantiate(gameModifierSlot, Vector3.zero, Quaternion.identity, parent);
            GameModifierSlotDataHolder slotRef = slot.GetComponent<GameModifierSlotDataHolder>();
            slotList.Add(slotRef);
            slotRef.thisModifier = gameModifierRef;
            slotRef.nameText.text = gameModifierRef.entryDisplayName;
            if (gameModifierRef.entryIcon != null)
            {
                slotRef.icon.sprite = gameModifierRef.entryIcon;
            }
            else
            {
                slotRef.icon.enabled = false;
            }
            if (gameModifierRef.gameModifierType == RPGGameModifier.GameModifierType.Negative)
            {
                slotRef.nameText.color = modifierNegativeColor;
                slotRef.costText.text = "+ " + gameModifierRef.gain;
                slotRef.costText.color = modifierPositiveColor;
            }
            else
            {
                slotRef.nameText.color = modifierPositiveColor;
                slotRef.costText.text = "- " + gameModifierRef.cost;
                slotRef.costText.color = modifierNegativeColor;
            }
        }

        private void ClearAllModifierSlots()
        {
            ClearModifierSlots(currentAvailableNegativeModifiersSlots);
            ClearModifierSlots(currentAvailablePositiveModifiersSlots);
            ClearModifierSlots(currentChosenNegativeModifiersSlots);
            ClearModifierSlots(currentChosenPositiveModifiersSlots);
        }
        
        public void ResetModifierPnanel()
        {
            RPGBuilderUtilities.DisableCG(modifiersCG);
            ClearAllModifierSlots();

            modifiersPointsText.text = "" + 0;
        }

        public void ClickGameModifierSlot(RPGGameModifier gameModifier)
        {
            if (RPGBuilderUtilities.isGameModifierOn(gameModifier.ID))
            {
                RemoveModifierFromCharacterData(gameModifier);
            }
            else
            {
                // THIS IS AN AVAILABLE MODIFIER
                if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                {
                    // ADD TO CHARACTER DATA
                    if (RPGBuilderUtilities.isGameModifierAdded(gameModifier.ID))
                    {
                        RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, true);
                        Character.Instance.CharacterData.MenuGameModifierPoints += gameModifier.gain;
                    }
                    else
                    {
                        AddModifierToCharacterData(gameModifier);
                    }
                }
                else
                {
                    if (Character.Instance.CharacterData.MenuGameModifierPoints < gameModifier.cost)
                    {
                        ShowPopupMessage("You do not have enough points");
                        return;
                    }
                    if (GameDatabase.Instance.GetWorldSettings().checkMaxPositiveModifier &&  RPGBuilderUtilities.getPositiveModifiersCount() >= GameDatabase.Instance.GetWorldSettings()
                        .maximumRequiredPositiveGameModifiers)
                    {
                        ShowPopupMessage("You already have the maximum amounts of positive modifiers");
                        return;
                    }
                    
                    // ADD TO CHARACTER DATA
                    if (RPGBuilderUtilities.isGameModifierAdded(gameModifier.ID))
                    {
                        RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, true);
                        Character.Instance.CharacterData.MenuGameModifierPoints -= gameModifier.cost;
                    }
                    else
                    {
                        AddModifierToCharacterData(gameModifier);
                    }
                }
            }
            
            InitializeModifierPanel();
        }

        private void AddModifierToCharacterData(RPGGameModifier gameModifier)
        {
            // ADD TO CHARACTER DATA
            CharacterEntries.GameModifierEntry newGameModEntry = new CharacterEntries.GameModifierEntry();
            newGameModEntry.name = gameModifier.entryName;
            newGameModEntry.ID = gameModifier.ID;
            Character.Instance.CharacterData.GameModifiers.Add(newGameModEntry);
            RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, true);
            // ADD POINTS
            if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
            {
                Character.Instance.CharacterData.MenuGameModifierPoints += gameModifier.gain;
            }
            else
            {
                Character.Instance.CharacterData.MenuGameModifierPoints -= gameModifier.cost;
            }
        }

        private void RemoveModifierFromCharacterData(RPGGameModifier gameModifier)
        {
            // ADD TO CHARACTER DATA
            foreach (var gameMod in Character.Instance.CharacterData.GameModifiers)
            {
                if(gameMod.ID != gameModifier.ID) continue;
                
                // REFUND POINTS
                if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                {
                    int positivePointsRequired = getTotalPointsRequiredForPositiveModifiers();
                    int negativePointsGain = getTotalGainedPointsFromNegativeModifiers();
                    negativePointsGain -= gameModifier.gain;
                    if (positivePointsRequired == 0 || negativePointsGain >= positivePointsRequired)
                    {
                        Character.Instance.CharacterData.MenuGameModifierPoints -= gameModifier.gain;
                        RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, false);
                    }
                    else
                    {
                        ShowPopupMessage("Positive modifiers require these points");
                        return;
                    }
                }
                else
                {
                    Character.Instance.CharacterData.MenuGameModifierPoints += gameModifier.cost;
                    RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, false);
                }
            }
        }

        private int getTotalPointsRequiredForPositiveModifiers()
        {
            int total = 0;
            foreach (var gameMod in Character.Instance.CharacterData.GameModifiers)
            {
                if(!gameMod.On) continue;
                RPGGameModifier gameModifierRef = GameDatabase.Instance.GetGameModifiers()[gameMod.ID];
                if (gameModifierRef.gameModifierType == RPGGameModifier.GameModifierType.Positive)
                {
                    total += gameModifierRef.cost;
                }
            }

            return total;
        }

        private int getTotalGainedPointsFromNegativeModifiers()
        {
            int total = 0;
            foreach (var gameMod in Character.Instance.CharacterData.GameModifiers)
            {
                if(!gameMod.On) continue;
                RPGGameModifier gameModifierRef = GameDatabase.Instance.GetGameModifiers()[gameMod.ID];
                if (gameModifierRef.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                {
                    total += gameModifierRef.gain;
                }
            }

            return total;
        }
        
        public void OpenBlinkStore() {
            Application.OpenURL("https://assetstore.unity.com/publishers/49855");
        }
    }
}