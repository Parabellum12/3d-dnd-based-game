using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder._THMSV.RPGBuilder.Scripts.UIElements;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class DevUIManager : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG, getItemCG, spawnNPCCG;
        [SerializeField] private Image GeneralCategory, CombatCategory, WorldCategory;
        [SerializeField] private GameObject GeneralCategoryPanel, CombatCategoryPanel, WorldCategoryPanel;
        [SerializeField] private TMP_Dropdown currencyDropdown, treepointsDropdown, skillsDropdown, gameScenesDropdown, factionDropdown, weaponTemplatesDropdown;
        [SerializeField] private TMP_InputField addCurrencyField, addTreePointField, addSkillEXPField, teleportPosX, teleportPosY, teleportPosZ, alterFactionField, addWeaponTemplateEXPField;
        [SerializeField] private TMP_InputField getItemName;
        [SerializeField] private TMP_InputField getItemCount;
        [SerializeField] private TMP_InputField spawnNPCName, statFieldName;
        [SerializeField] private TMP_InputField spawnNPCCount;
        [SerializeField] private TMP_InputField alterHealthField, alterStatField;
        [SerializeField] private TMP_InputField classXPField;
        [SerializeField] private TextMeshProUGUI currentSceneText, playerPOSText, playerROTText, allNPCsText, selectStatText;
        [SerializeField] private Transform itemsParent;
        [SerializeField] private GameObject getItemSlotPrefab;
        [SerializeField] private Transform npcParent;
        [SerializeField] private GameObject spawnNPCSlotPrefab, statSlotPrefab;
        [SerializeField] private Color selectedColor, NotSelectedColor;
        [SerializeField] private GameObject developerPanelButtonGO;

        private List<GameObject> curGetItemListSlots = new List<GameObject>();
        private List<GameObject> curSpawnNPCListSlots = new List<GameObject>();
        private List<GameObject> curStatListSlot = new List<GameObject>();
        private RPGStat selectedStat;
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen(gameObject.name)) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            Unregister();
        }
        
        protected override void Register()
        {
            UIEvents.Instance.AddPanelEntry(this, gameObject.name, null);
        }

        protected override void Unregister()
        {
            UIEvents.Instance.RemovePanelEntry(this, gameObject.name);
        }
        
        public override bool IsOpen()
        {
            return opened;
        }
        
        public void SetSelectedStat(RPGStat stat)
        {
            selectedStat = stat;
            selectStatText.text = stat.entryName;
        }
        public void selectCategory(string categoryName)
        {
            switch (categoryName)
            {
                case "general":
                    GeneralCategory.color = selectedColor;
                    CombatCategory.color = NotSelectedColor;
                    WorldCategory.color = NotSelectedColor;
                    HideSpawnNPCPanel();
                    break;

                case "combat":
                    GeneralCategory.color = NotSelectedColor;
                    CombatCategory.color = selectedColor;
                    WorldCategory.color = NotSelectedColor;
                    HideGetItemPanel();
                    break;

                case "world":
                    GeneralCategory.color = NotSelectedColor;
                    CombatCategory.color = NotSelectedColor;
                    WorldCategory.color = selectedColor;
                    HideGetItemPanel();
                    HideSpawnNPCPanel();
                    break;
            }

            ShowCategory(categoryName);
        }

        private void ShowCategory(string categoryName)
        {
            switch (categoryName)
            {
                case "general":
                    GeneralCategoryPanel.SetActive(true);
                    CombatCategoryPanel.SetActive(false);
                    WorldCategoryPanel.SetActive(false);
                    PopulateCurrencyDropdown();
                    PopulateTreePointDropdown();
                    PopulateSkillsDropdown();
                    break;

                case "combat":
                    GeneralCategoryPanel.SetActive(false);
                    CombatCategoryPanel.SetActive(true);
                    WorldCategoryPanel.SetActive(false);
                    PopulateFactionDropdown();
                    PopulateWeaponTemplateDropdown();
                    break;

                case "world":
                    GeneralCategoryPanel.SetActive(false);
                    CombatCategoryPanel.SetActive(false);
                    WorldCategoryPanel.SetActive(true);
                    PopulateGameScenesDropdown();
                    break;
            }
        }

        private void PopulateCurrencyDropdown()
        {
            var currencyOptions = new List<TMP_Dropdown.OptionData>();
            foreach (var currency in GameDatabase.Instance.GetCurrencies().Values)
            {
                var newOption = new TMP_Dropdown.OptionData {text = currency.entryName, image = currency.entryIcon};
                currencyOptions.Add(newOption);
            }

            currencyDropdown.ClearOptions();
            currencyDropdown.options = currencyOptions;
        }
        
        private void PopulateTreePointDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var treePoint in GameDatabase.Instance.GetPoints().Values)
            {
                var newOption = new TMP_Dropdown.OptionData {text = treePoint.entryName, image = treePoint.entryIcon};
                options.Add(newOption);
            }

            treepointsDropdown.ClearOptions();
            treepointsDropdown.options = options;
        }
        
        private void PopulateSkillsDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var skill in GameDatabase.Instance.GetSkills().Values)
            {
                var newOption = new TMP_Dropdown.OptionData {text = skill.entryName, image = skill.entryIcon};
                options.Add(newOption);
            }

            skillsDropdown.ClearOptions();
            skillsDropdown.options = options;
        }
        
        private void PopulateGameScenesDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var gameScene in GameDatabase.Instance.GetGameScenes().Values)
            {
                var newOption = new TMP_Dropdown.OptionData();
                newOption.text = gameScene.entryName;
                newOption.image = gameScene.minimapImage;
                options.Add(newOption);
            }

            gameScenesDropdown.ClearOptions();
            gameScenesDropdown.options = options;
        }
        
        private void PopulateFactionDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var faction in GameDatabase.Instance.GetFactions().Values)
            {
                var newOption = new TMP_Dropdown.OptionData {text = faction.entryName, image = faction.entryIcon};
                options.Add(newOption);
            }

            factionDropdown.ClearOptions();
            factionDropdown.options = options;
        }
        
        private void PopulateWeaponTemplateDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var wpTemplate in GameDatabase.Instance.GetWeaponTemplates().Values)
            {
                var newOption = new TMP_Dropdown.OptionData {text = wpTemplate.entryName, image = wpTemplate.entryIcon};
                options.Add(newOption);
            }

            weaponTemplatesDropdown.ClearOptions();
            weaponTemplatesDropdown.options = options;
        }

        public void DEVAlterCurrency()
        {
            RPGCurrency currency =RPGBuilderUtilities.getCurrencyByName(currencyDropdown.options[currencyDropdown.value].text);
            InventoryManager.Instance.AddCurrency(currency.ID,int.Parse(addCurrencyField.text));
        }
        public void DEVAddTreePoint()
        {
            RPGTreePoint point = RPGBuilderUtilities.getTreePointByName(treepointsDropdown.options[treepointsDropdown.value].text);
            TreePointsManager.Instance.AddTreePoint(point.ID,int.Parse(addTreePointField.text));
        }
        public void DEVAddSkillEXP()
        {
            RPGSkill skill = RPGBuilderUtilities.getSkillByName(skillsDropdown.options[skillsDropdown.value].text);
            LevelingManager.Instance.AddSkillEXP(skill.ID,int.Parse(addSkillEXPField.text));
        }
        public void DEVAddWeaponTemplateEXP()
        {
            RPGWeaponTemplate wpTemplate = RPGBuilderUtilities.getWeaponTemplateByName(weaponTemplatesDropdown.options[weaponTemplatesDropdown.value].text);
            LevelingManager.Instance.AddWeaponTemplateEXP(wpTemplate.ID,int.Parse(addWeaponTemplateEXPField.text));
        }

        public void DEVAlterHealth()
        {
            var value = int.Parse(alterHealthField.text);
            if (value < 0) value += Mathf.Abs(value) * 2;
            else value -= value * 2;
            GameState.playerEntity.TakeDamage(CombatCalculations.GenerateDamageResult(GameState.playerEntity, GameState.playerEntity, "NO_DAMAGE_TYPE", value), null, GameDatabase.Instance.GetCombatSettings().HealthStatID);
        }
        
        public void DEVAlterFaction()
        {
            RPGFaction faction = RPGBuilderUtilities.getFactionByName(factionDropdown.options[factionDropdown.value].text);
            int amt = int.Parse(alterFactionField.text);
            if (amt > 0)
            {
                FactionManager.Instance.AddFactionPoint(faction.ID, amt);
            }
            else if(amt < 0)
            {
                FactionManager.Instance.RemoveFactionPoint(faction.ID, Mathf.Abs(amt));
            }
        }

        public void AddClassXP()
        {
            LevelingManager.Instance.AddCharacterEXP(int.Parse(classXPField.text));
        }

        public void GetItem(RPGItem item)
        {
            if (item == null) return;
            int amt;
            if (getItemCount.text == "" || int.Parse(getItemCount.text) == 0 || int.Parse(getItemCount.text) == 1)
                amt = 1;
            else
                amt = int.Parse(getItemCount.text);

            for (int i = 0; i < amt; i++)
            {
                int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(item.ID, -1,1, false, true);
                if (itemsLeftOver != 0)
                {
                    UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                }
            }
        }

        public void ClearInventory()
        {
            for (var i = 0; i < Character.Instance.CharacterData.Inventory.baseSlots.Count; i++)
            {
                if (Character.Instance.CharacterData.Inventory.baseSlots[i].itemID != -1)
                {
                    InventoryManager.Instance.RemoveItem(Character.Instance.CharacterData.Inventory.baseSlots[i].itemID, Character.Instance.CharacterData.Inventory.baseSlots[i].itemDataID,
                        Character.Instance.CharacterData.Inventory.baseSlots[i].itemStack, -1, i, true);
                }
            }
        }

        public void SpawnNPC(RPGNpc npc)
        {
            if (npc == null) return;
            int amt;
            if (spawnNPCCount.text == "" || int.Parse(spawnNPCCount.text) == 0 || int.Parse(spawnNPCCount.text) == 1)
                amt = 1;
            else
                amt = int.Parse(spawnNPCCount.text);
            
            
            for (int i = 0; i < amt; i++)
            {
                CombatManager.Instance.GenerateNPCEntity(npc, false, false, null, GameState.playerEntity.transform.position, Quaternion.identity, null);
            }
        }
        
        public void AlterStat()
        {
            if (selectedStat == null) return;
            foreach (var t in GameState.playerEntity.GetStats())
            {
                if (selectedStat.ID != t.Value.stat.ID) continue;
                float amt = float.Parse(alterStatField.text);
                
                StatCalculator.HandleStat(GameState.playerEntity, selectedStat, t.Value, amt, false, StatCalculator.TemporaryStatSourceType.none);
                
            }
            StatCalculator.TriggerMovementSpeedChange(GameState.playerEntity);
            GameState.playerEntity.appearance.HandleBodyScaleFromStats();
            CombatEvents.Instance.OnStatChanged(GameState.playerEntity, selectedStat);
            CombatEvents.Instance.OnStatValueChanged(GameState.playerEntity, selectedStat, CombatUtilities.GetCurrentStatValue(GameState.playerEntity, selectedStat.ID), CombatUtilities.GetMaxStatValue(GameState.playerEntity, selectedStat.ID));
        }

        public void DEVTeleportToPositon()
        {
            GameState.playerEntity.controllerEssentials.TeleportToTarget( new Vector3(float.Parse(teleportPosX.text), float.Parse(teleportPosY.text), float.Parse(teleportPosZ.text)));
        }
        public void DEVTeleportToGameScene()
        {
            RPGGameScene gameScene = RPGBuilderUtilities.GetGameSceneFromName(gameScenesDropdown.options[gameScenesDropdown.value].text);
            RPGBuilderEssentials.Instance.TeleportToGameScene(gameScene.ID, GameDatabase.Instance.GetWorldPositions()[gameScene.startPositionID].position);
        }

        public void HideGetItemPanel()
        {
            RPGBuilderUtilities.DisableCG(getItemCG);
        }

        public void ShowGetItemPanel()
        {
            HideSpawnNPCPanel();
            if (getItemCG.alpha == 1)
            {
                HideGetItemPanel();
                return;
            }
            RPGBuilderUtilities.EnableCG(getItemCG);
            UpdateGetItemList();
        }

        private void ClearGetItemList()
        {
            foreach (var t in curGetItemListSlots)
                Destroy(t);

            curGetItemListSlots.Clear();
        }

        public void UpdateGetItemList()
        {
            ClearGetItemList();
            var curSearch = getItemName.text;

            var allItems = GameDatabase.Instance.GetItems().Values;
            var validItems = new List<RPGItem>();


            if (curSearch.Length > 0 && !string.IsNullOrEmpty(curSearch) && !string.IsNullOrWhiteSpace(curSearch))
                foreach (var t in allItems)
                {
                    var itemNameToCheck = t.entryName;
                    itemNameToCheck = itemNameToCheck.ToLower();
                    curSearch = curSearch.ToLower();

                    if (itemNameToCheck.Contains(curSearch)) validItems.Add(t);
                }
            else
                validItems = allItems.ToList();


            foreach (var t in validItems)
            {
                var newGetItemSlot = Instantiate(getItemSlotPrefab, itemsParent);
                var newGetItemSlotRef = newGetItemSlot.GetComponent<GetItemSlot>();
                newGetItemSlotRef.thisitem = t;
                newGetItemSlotRef.icon.sprite = t.entryIcon;
                curGetItemListSlots.Add(newGetItemSlot);
            }
        }
        
        public void HideSpawnNPCPanel()
        {
            RPGBuilderUtilities.DisableCG(spawnNPCCG);
        }

        public void ShowSpawnNPCPanel()
        {
            HideGetItemPanel();
            if (spawnNPCCG.alpha == 1)
            {
                HideSpawnNPCPanel();

                if (statFieldName.gameObject.activeInHierarchy)
                {
                    ShowSpawnNPCPanel();
                }
                return;
            }
            statFieldName.gameObject.SetActive(false);
            spawnNPCName.gameObject.SetActive(true);
            RPGBuilderUtilities.EnableCG(spawnNPCCG);
            UpdateSpawnNPCList();
        }

        public void ShowAlterStatPanel()
        {
            HideGetItemPanel();
            if (spawnNPCCG.alpha == 1)
            {
                HideSpawnNPCPanel();

                if (spawnNPCName.gameObject.activeInHierarchy)
                {
                    ShowAlterStatPanel();
                }
                return;
            }
            statFieldName.gameObject.SetActive(true);
            spawnNPCName.gameObject.SetActive(false);
            RPGBuilderUtilities.EnableCG(spawnNPCCG);
            UpdateStatList();
        }

        private void ClearSpawnNPCList()
        {
            foreach (var t in curSpawnNPCListSlots)
                Destroy(t);

            curSpawnNPCListSlots.Clear();
        }

        public void UpdateSpawnNPCList()
        {
            ClearStatList();
            ClearSpawnNPCList();
            var curSearch = spawnNPCName.text;

            var allNPCs = GameDatabase.Instance.GetNPCs().Values;
            var validNPCs = new List<RPGNpc>();


            if (curSearch.Length > 0 && !string.IsNullOrEmpty(curSearch) && !string.IsNullOrWhiteSpace(curSearch))
                foreach (var t in allNPCs)
                {
                    var itemNameToCheck = t.entryName;
                    itemNameToCheck = itemNameToCheck.ToLower();
                    curSearch = curSearch.ToLower();

                    if (itemNameToCheck.Contains(curSearch)) validNPCs.Add(t);
                }
            else
                validNPCs = allNPCs.ToList();


            foreach (var t in validNPCs)
            {
                var newGetItemSlot = Instantiate(spawnNPCSlotPrefab, npcParent);
                var newGetItemSlotRef = newGetItemSlot.GetComponent<NPCSpawnSlotHolder>();
                newGetItemSlotRef.thisNPC = t;
                newGetItemSlotRef.icon.sprite = t.entryIcon;
                newGetItemSlotRef.nameText.text = t.entryName;
                curSpawnNPCListSlots.Add(newGetItemSlot);
            }
        }
        
        
        private void ClearStatList()
        {
            foreach (var t in curStatListSlot)
                Destroy(t);

            curStatListSlot.Clear();
        }
        public void UpdateStatList()
        {
            ClearStatList();
            ClearSpawnNPCList();
            var curSearch = statFieldName.text;

            var allNPCs = GameDatabase.Instance.GetStats().Values;
            var validNPCs = new List<RPGStat>();


            if (curSearch.Length > 0 && !string.IsNullOrEmpty(curSearch) && !string.IsNullOrWhiteSpace(curSearch))
                foreach (var t in allNPCs)
                {
                    var itemNameToCheck = t.entryName;
                    itemNameToCheck = itemNameToCheck.ToLower();
                    curSearch = curSearch.ToLower();

                    if (itemNameToCheck.Contains(curSearch)) validNPCs.Add(t);
                }
            else
                validNPCs = allNPCs.ToList();


            foreach (var t in validNPCs)
            {
                var newGetItemSlot = Instantiate(statSlotPrefab, npcParent);
                var newGetItemSlotRef = newGetItemSlot.GetComponent<DevStatSlotHolder>();
                newGetItemSlotRef.thisStat = t;
                newGetItemSlotRef.nameText.text = t.entryName;
                curStatListSlot.Add(newGetItemSlot);
            }
        }
        
        private void Awake()
        {
            Hide();
            if (Instance != null) return;
            Instance = this;
        }
        private void Start()
        {
            developerPanelButtonGO.SetActive(GameDatabase.Instance.GetGeneralSettings().enableDevPanel);
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            selectCategory("general");
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
        }

        public override void Hide()
        {
            base.Hide();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
            HideSpawnNPCPanel();
            HideGetItemPanel();
        }

        public bool IsTypingInField()
        {
            return addCurrencyField.isFocused || alterHealthField.isFocused || addTreePointField.isFocused
                   || classXPField.isFocused || addSkillEXPField.isFocused || getItemName.isFocused
                   || spawnNPCName.isFocused || statFieldName.isFocused || alterFactionField.isFocused;
        }
        
        private void Update()
        {
            if (!opened || GameState.playerEntity == null) return;
            currentSceneText.text = "Game Scene: <color=white>" + SceneManager.GetActiveScene().name;
            Vector3 pos = GameState.playerEntity.transform.position;
            playerPOSText.text = "Player Position: <color=white>" + (int)pos.x + " / " + (int)pos.y + " / " + (int)pos.z;
            Vector3 rot = GameState.playerEntity.transform.eulerAngles;
            playerROTText.text = "Player Rotation: <color=white>" + (int)rot.x + " / " + (int)rot.y + " / " + (int)rot.z;
            int npccount = GameState.combatEntities.Count - 1;
            allNPCsText.text = "NPC Count: <color=white>" + npccount;
        }


        public static DevUIManager Instance { get; private set; }
    }
}