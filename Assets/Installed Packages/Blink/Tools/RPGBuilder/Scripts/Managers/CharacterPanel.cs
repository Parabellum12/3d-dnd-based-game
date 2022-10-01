using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.DisplayHandler;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public enum CharacterInfoTypes
    {
        GEAR,
        INFO,
        STATS,
        TALENTS,
        FACTIONS
    }
    
    public class CharacterPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG, CharGearCG, CharInfoCG, CharStatsCG, CharTalentsCG, CharFactionsCG, StatTooltipCG;
        [SerializeField] private TextMeshProUGUI CharacterNameText, RaceNameText, ClassNameText, LevelText, ExperienceText, StatTooltipText;
        [SerializeField] private GameObject classTalentTreeButtonGO;
        [SerializeField] private GameObject StatTitlePrefab, StatTextPrefab, CombatTreeSlotPrefab, factionSlotPrefab;
        [SerializeField] private Transform StatTextsParent, CombatTreeSlotsParent, factionSlotsParent;
        [SerializeField] private List<GameObject> statTextGO = new List<GameObject>();
        [SerializeField] private List<GameObject> cbtTreeSlots = new List<GameObject>();
        [SerializeField] private List<GameObject> factionSlots = new List<GameObject>();
        [SerializeField] private Animator talentCategoryAnimator;
        [SerializeField] private Color defaultCategoryColor, selectedCategoryColor;
        [SerializeField] private Sprite defaultCategorySprite, selectedCategorySprite;
        [SerializeField] private Image gearCategoryImage, statsCategoryImage, talentsCategoryImage, factionsCategorryImage;
        [SerializeField] private Button gearCategoryButton, statsCategoryButton, talentsCategoryButton, factionsCategorryButton;
        
        [SerializeField] private List<EquipmentItemSlotDisplayHandler> ArmorSlots;
        [SerializeField] private List<EquipmentItemSlotDisplayHandler> WeaponSlots;
        
        private CharacterInfoTypes curCharInfoType;
        protected static readonly int glowing = Animator.StringToHash("glowing");
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            GameEvents.BackToMainMenu += ResetCategoryButtons;
            GeneralEvents.PlayerEquippedItem += PlayerEquippedItem;
            GeneralEvents.PlayerUnequippedItem += PlayerUnequippedItem;
            UIEvents.ShowCharacterPanelStatTooltip += ShowStatTooltipPanel;
            UIEvents.HideCharacterPanelStatTooltip += HideStatTooltipPanel;
            CombatEvents.StatsChanged += StatsChanged;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Character")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            GameEvents.BackToMainMenu -= ResetCategoryButtons;
            GeneralEvents.PlayerEquippedItem -= PlayerEquippedItem;
            GeneralEvents.PlayerUnequippedItem -= PlayerUnequippedItem;
            UIEvents.ShowCharacterPanelStatTooltip -= ShowStatTooltipPanel;
            UIEvents.HideCharacterPanelStatTooltip -= HideStatTooltipPanel;
            CombatEvents.StatsChanged -= StatsChanged;
            Unregister();
        }
        
        protected override void Register()
        {
            InitCharEquippedItems();
            Dictionary<string, object> panelData = new Dictionary<string, object> {{"ArmorSlots", ArmorSlots}, {"WeaponSlots", WeaponSlots}};
            UIEvents.Instance.AddPanelEntry(this, gameObject.name, panelData);
        }

        protected override void Unregister()
        {
            UIEvents.Instance.RemovePanelEntry(this, gameObject.name);
        }
        
        public override bool IsOpen()
        {
            return opened;
        }

        private void disableAllCharCategoriesCG ()
        {
            RPGBuilderUtilities.DisableCG(CharInfoCG);
            RPGBuilderUtilities.DisableCG(CharGearCG);
            RPGBuilderUtilities.DisableCG(CharStatsCG);
            RPGBuilderUtilities.DisableCG(CharTalentsCG);
            RPGBuilderUtilities.DisableCG(CharFactionsCG);
        }

        private void ResetCategoryButtons()
        {
            setButtonAppearance(gearCategoryButton, gearCategoryImage, false);
            setButtonAppearance(statsCategoryButton, statsCategoryImage,false);
            setButtonAppearance(factionsCategorryButton, factionsCategorryImage, false);
            setButtonAppearance(talentsCategoryButton, talentsCategoryImage,false);
        }

        private void setButtonAppearance(Button button, Image image, bool selected)
        {
            ColorBlock colorblock = button.colors;
            colorblock.normalColor = selected ? selectedCategoryColor : defaultCategoryColor;
            button.colors = colorblock;
            image.sprite = selected ? selectedCategorySprite : defaultCategorySprite;
        }
        
        public void InitCharacterCategory (string newCategory)
        {
            var parsedEnum = (CharacterInfoTypes)System.Enum.Parse(typeof(CharacterInfoTypes), newCategory);
            disableAllCharCategoriesCG();
            ResetCategoryButtons();
            switch(parsedEnum)
            {
                case CharacterInfoTypes.GEAR:
                    curCharInfoType = CharacterInfoTypes.GEAR;
                    RPGBuilderUtilities.EnableCG(CharGearCG);
                    setButtonAppearance(gearCategoryButton, gearCategoryImage, true);
                    InitCharEquippedItems();
                    break;
                case CharacterInfoTypes.INFO:
                    curCharInfoType = CharacterInfoTypes.INFO;
                    RPGBuilderUtilities.EnableCG(CharInfoCG);
                    InitCharacterInfo();
                    break;
                case CharacterInfoTypes.STATS:
                    curCharInfoType = CharacterInfoTypes.STATS;
                    RPGBuilderUtilities.EnableCG(CharStatsCG);
                    setButtonAppearance(statsCategoryButton, statsCategoryImage,true);
                    InitCharStats();
                    break;
                case CharacterInfoTypes.TALENTS:
                    curCharInfoType = CharacterInfoTypes.TALENTS;
                    RPGBuilderUtilities.EnableCG(CharTalentsCG);
                    setButtonAppearance(talentsCategoryButton, talentsCategoryImage,true);
                    InitCharCombatTrees();
                    break;
                case CharacterInfoTypes.FACTIONS:
                    curCharInfoType = CharacterInfoTypes.FACTIONS;
                    RPGBuilderUtilities.EnableCG(CharFactionsCG);
                    setButtonAppearance(factionsCategorryButton,factionsCategorryImage, true);
                    InitCharFactions();
                    break;
            }

            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses && talentCategoryAnimator != null) talentCategoryAnimator.SetBool(glowing, RPGBuilderUtilities.hasPointsToSpendInClassTrees());
        }

        private void InitCharacterInfo ()
        {
            CharacterNameText.text = Character.Instance.CharacterData.CharacterName;
            RaceNameText.text = "Race: " + GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].entryDisplayName;

            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                ClassNameText.text = "Class: " + GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID].entryDisplayName;
            }
            else
            {
                ClassNameText.text = "";
            }

            LevelText.text = "Level: " + Character.Instance.CharacterData.Level;
            ExperienceText.text = "Experience: " + Character.Instance.CharacterData.CurrentExperience + " / " + Character.Instance.CharacterData.ExperienceNeeded;
        }

        private void ClearCombatTreeSlots()
        {
            foreach (var t in cbtTreeSlots) Destroy(t);

            cbtTreeSlots.Clear();
        }

        private void InitCharCombatTrees()
        {
            ClearCombatTreeSlots();
            foreach (var t in GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID].talentTrees)
            {
                var cbtTree = Instantiate(CombatTreeSlotPrefab, CombatTreeSlotsParent);
                cbtTreeSlots.Add(cbtTree);
                var slotREF = cbtTree.GetComponent<CombatTreeSlot>();
                slotREF.InitSlot(GameDatabase.Instance.GetTalentTrees()[t.talentTreeID]);
            }
        }

        private void PlayerEquippedItem(RPGItem itemEquipped)
        {
            InitCharEquippedItems();
        }
        private void PlayerUnequippedItem(RPGItem itemEquipped)
        {
            InitCharEquippedItems();
        }

        private void StatsChanged(CombatEntity statEntity)
        {
            if (!statEntity.IsPlayer()) return;
            if(opened && curCharInfoType == CharacterInfoTypes.STATS) InitCharStats();
        }

        private void InitCharEquippedItems()
        {
            ArmorSlots.Clear();
            WeaponSlots.Clear();

            foreach (var slot in GetComponentsInChildren<EquipmentItemSlotDisplayHandler>())
            {
                if (slot.EquipFunction == EconomyData.EquipFunction.Armor)
                {
                    ArmorSlots.Add(slot);
                }
                else if (slot.EquipFunction == EconomyData.EquipFunction.Weapon)
                {
                    WeaponSlots.Add(slot);
                }
            }

            foreach (var armorSlot in ArmorSlots)
            {
                foreach (var equippedArmorSlot in GameState.playerEntity.equippedArmors)
                {
                    if (armorSlot.ArmorSlot != equippedArmorSlot.ArmorSlot) continue;
                    if (equippedArmorSlot.item != null)
                        armorSlot.InitItem(equippedArmorSlot.item, equippedArmorSlot.temporaryItemDataID);
                    else
                        armorSlot.ResetItem();
                }
            }

            for (var i = 0; i < WeaponSlots.Count; i++)
            {
                var weaponSlot = WeaponSlots[i];
                for (var index = 0; index < GameState.playerEntity.equippedWeapons.Count; index++)
                {
                    var equippedWeaponSlot = GameState.playerEntity.equippedWeapons[index];
                    if (i != index) continue;
                    if (equippedWeaponSlot.item != null)
                        weaponSlot.InitItem(equippedWeaponSlot.item, equippedWeaponSlot.temporaryItemDataID);
                    else
                        weaponSlot.ResetItem();
                }
            }
        }

        private void InitCharStats ()
        {
            ClearStatText();
            foreach (var t in GameDatabase.Instance.GetStatCategories().Values)
            {
                if (t == null) continue;
                var statTitle = Instantiate(StatTitlePrefab, StatTextsParent);
                statTextGO.Add(statTitle);
                statTitle.GetComponent<TextMeshProUGUI>().text = t.entryDisplayName;

                foreach (var t1 in GameState.playerEntity.GetStats())
                {
                    if (t1.Value.stat.StatCategory != t) continue;
                    var statText = Instantiate(StatTextPrefab, StatTextsParent);
                    statTextGO.Add(statText);
                    StatDataHolder statREF = statText.GetComponent<StatDataHolder>();
                    if (statREF != null)
                    {
                        statREF.InitStatText(t1.Value);
                    }
                }
            }
        }

        private void ClearFactionSlots()
        {
            foreach (var t in factionSlots) Destroy(t);
            factionSlots.Clear();
        }

        private void InitCharFactions()
        {
            ClearFactionSlots();
            foreach (var t in Character.Instance.CharacterData.Factions)
            {
                var factionSlot = Instantiate(factionSlotPrefab, factionSlotsParent);
                factionSlots.Add(factionSlot);
                FactionSlotDataHolder factionSlotREF = factionSlot.GetComponent<FactionSlotDataHolder>();
                if (factionSlotREF != null)
                {
                    factionSlotREF.Init(t);
                }
            }
        }

        private void ShowStatTooltipPanel(RPGStat stat)
        {
            StatTooltipCG.alpha = 1;
            StatTooltipText.text = stat.entryDescription;
        }

        private void HideStatTooltipPanel()
        {
            StatTooltipCG.alpha = 0;
            StatTooltipText.text = "";
        }

        private void ClearStatText ()
        {
            foreach (var t in statTextGO) Destroy(t);
            statTextGO.Clear();
        }

        public override void Show()
        {
            base.Show();
            CharacterNameText.text = Character.Instance.CharacterData.CharacterName;
            classTalentTreeButtonGO.SetActive(!GameDatabase.Instance.GetCharacterSettings().NoClasses);
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            InitCharacterCategory(curCharInfoType.ToString());
            UIEvents.Instance.OnClosePanel("Skill_Book");
            UIEvents.Instance.OnClosePanel("Weapon_Templates");
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
            if(PauseGame) Invoke("StartPause", 0);
        }

        public override void Hide()
        {
            base.Hide();
            if(PauseGame) Invoke("EndPause", 0);
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }
    }
}
